using System.Net.Http.Headers;

namespace Bucket.Updater.Services
{
    public interface IGitHubService
    {
        Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync(UpdaterConfiguration configuration);
        Task<Stream> DownloadUpdateAsync(string downloadUrl, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default);
        Task<List<GitHubRelease>> GetReleasesAsync(string owner, string repository, bool includePrerelease = true);
    }

    public class GitHubService : IGitHubService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public GitHubService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bucket.Updater", "1.0.0"));
            _httpClient.Timeout = TimeSpan.FromMinutes(30);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        public async Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync(UpdaterConfiguration configuration)
        {
            Logger?.Information("Checking for updates: Channel={Channel}, Architecture={Architecture}", 
                configuration.UpdateChannel, configuration.Architecture);
            
            try
            {
                var releases = await GetReleasesAsync(configuration.GitHubOwner, configuration.GitHubRepository, true);
                
                GitHubRelease? targetRelease = null;
                
                if (configuration.UpdateChannel == UpdateChannel.Release)
                {
                    targetRelease = releases.FirstOrDefault(r => !r.Prerelease);
                }
                else
                {
                    targetRelease = releases.FirstOrDefault(r => r.Prerelease && r.Name.Contains("Nightly"));
                }

                if (targetRelease == null)
                {
                    Logger?.Information("No suitable release found for channel {Channel}", configuration.UpdateChannel);
                    return null;
                }

                var architectureString = configuration.GetArchitectureString();
                var msiAsset = targetRelease.Assets.FirstOrDefault(a => 
                    a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) && 
                    a.Name.Contains(architectureString, StringComparison.OrdinalIgnoreCase));

                if (msiAsset == null)
                {
                    Logger?.Warning("No MSI asset found for architecture {Architecture} in release {Version}", 
                        architectureString, targetRelease.TagName);
                    return null;
                }

                var updateInfo = new Bucket.Updater.Models.UpdateInfo
                {
                    Version = targetRelease.TagName.TrimStart('v'),
                    TagName = targetRelease.TagName,
                    Name = targetRelease.Name,
                    Body = targetRelease.Body,
                    PublishedAt = targetRelease.PublishedAt,
                    IsPrerelease = targetRelease.Prerelease,
                    DownloadUrl = msiAsset.BrowserDownloadUrl,
                    FileSize = msiAsset.Size,
                    Channel = configuration.UpdateChannel,
                    Architecture = configuration.Architecture,
                    Assets = targetRelease.Assets.Select(a => new UpdateAsset
                    {
                        Name = a.Name,
                        DownloadUrl = a.BrowserDownloadUrl,
                        Size = a.Size,
                        ContentType = a.ContentType
                    }).ToList()
                };

                var isNewer = IsNewerVersion(updateInfo.Version, configuration.CurrentVersion);
                if (isNewer)
                {
                    Logger?.Information("Update available: {Version} > {CurrentVersion}", 
                        updateInfo.Version, configuration.CurrentVersion);
                }
                else
                {
                    Logger?.Information("No update needed: Current version {CurrentVersion} is up to date", 
                        configuration.CurrentVersion);
                }

                return isNewer ? updateInfo : null;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to check for updates");
                return null;
            }
        }

        public async Task<Stream> DownloadUpdateAsync(string downloadUrl, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default)
        {
            Logger?.Information("Starting download from {Url}", downloadUrl);
            
            try
            {
                var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                Logger?.Information("Download started, size: {Size} bytes", totalBytes);
                
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                
                if (progress == null)
                    return stream;

                var progressStream = new ProgressStream(stream, totalBytes, progress);
                return progressStream;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to download from {Url}", downloadUrl);
                throw;
            }
        }

        public async Task<List<GitHubRelease>> GetReleasesAsync(string owner, string repository, bool includePrerelease = true)
        {
            try
            {
                var url = $"https://api.github.com/repos/{owner}/{repository}/releases";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(json, _jsonOptions) ?? new List<GitHubRelease>();

                return includePrerelease ? releases : releases.Where(r => !r.Prerelease).ToList();
            }
            catch (Exception)
            {
                return new List<GitHubRelease>();
            }
        }

        private static bool IsNewerVersion(string newVersion, string currentVersion)
        {
            try
            {
                var cleanNewVersion = newVersion.TrimStart('v').Split('-')[0];
                var cleanCurrentVersion = currentVersion.TrimStart('v').Split('-')[0];

                var newVer = new Version(cleanNewVersion);
                var currentVer = new Version(cleanCurrentVersion);

                return newVer > currentVer;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ProgressStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _totalBytes;
        private readonly IProgress<(long downloaded, long total)> _progress;
        private long _downloadedBytes;

        public ProgressStream(Stream baseStream, long totalBytes, IProgress<(long downloaded, long total)> progress)
        {
            _baseStream = baseStream;
            _totalBytes = totalBytes;
            _progress = progress;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
            _downloadedBytes += read;
            _progress.Report((_downloadedBytes, _totalBytes));
            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _baseStream.Read(buffer, offset, count);
            _downloadedBytes += read;
            _progress.Report((_downloadedBytes, _totalBytes));
            return read;
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;
        public override long Position 
        { 
            get => _baseStream.Position; 
            set => _baseStream.Position = value; 
        }

        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        public override void SetLength(long value) => _baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _baseStream?.Dispose();
            base.Dispose(disposing);
        }
    }
}