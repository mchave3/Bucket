using System.Net.Http.Headers;

namespace Bucket.Updater.Services
{
    /// <summary>
    /// Interface for GitHub API operations related to releases and downloads
    /// </summary>
    public interface IGitHubService
    {
        /// <summary>
        /// Checks for available updates based on the provided configuration
        /// </summary>
        /// <param name="configuration">The updater configuration containing channel, architecture, and repository information</param>
        /// <returns>Update information if an update is available, otherwise null</returns>
        Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync(UpdaterConfiguration configuration);

        /// <summary>
        /// Downloads an update file from the specified URL with optional progress reporting
        /// </summary>
        /// <param name="downloadUrl">The URL to download from</param>
        /// <param name="progress">Optional progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token to cancel the download</param>
        /// <returns>A stream containing the downloaded content</returns>
        Task<Stream> DownloadUpdateAsync(string downloadUrl, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves releases from a GitHub repository
        /// </summary>
        /// <param name="owner">The repository owner</param>
        /// <param name="repository">The repository name</param>
        /// <param name="includePrerelease">Whether to include prerelease versions</param>
        /// <returns>List of GitHub releases</returns>
        Task<List<GitHubRelease>> GetReleasesAsync(string owner, string repository, bool includePrerelease = true);
    }

    /// <summary>
    /// Service for interacting with GitHub API to check for updates and download releases.
    /// Handles version comparison, architecture filtering, and progress tracking for downloads.
    /// </summary>
    public class GitHubService : IGitHubService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of GitHubService with configured HTTP client
        /// </summary>
        public GitHubService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bucket.Updater", "1.0.0"));
            _httpClient.Timeout = TimeSpan.FromMinutes(30); // Extended timeout for large downloads

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        /// <summary>
        /// Checks for available updates based on the provided configuration
        /// </summary>
        /// <param name="configuration">The updater configuration containing channel, architecture, and repository information</param>
        /// <returns>Update information if an update is available, otherwise null</returns>
        public async Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync(UpdaterConfiguration configuration)
        {
            Logger?.Information("Checking for updates: Channel={Channel}, Architecture={Architecture}",
                configuration.UpdateChannel, configuration.Architecture);

            try
            {
                // Get all releases from the GitHub repository
                var releases = await GetReleasesAsync(configuration.GitHubOwner, configuration.GitHubRepository, true);

                GitHubRelease? targetRelease = null;

                // Filter releases based on update channel
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

                // Find the appropriate MSI asset for the target architecture
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

                // Create update information object with release details
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

                // Compare versions to determine if an update is available
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

        /// <summary>
        /// Downloads an update file from the specified URL with optional progress reporting
        /// </summary>
        /// <param name="downloadUrl">The URL to download from</param>
        /// <param name="progress">Optional progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token to cancel the download</param>
        /// <returns>A stream containing the downloaded content</returns>
        public async Task<Stream> DownloadUpdateAsync(string downloadUrl, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default)
        {
            Logger?.Information("Starting download from {Url}", downloadUrl);

            try
            {
                // Send HTTP request for file download with response headers read first
                var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                Logger?.Information("Download started, size: {Size} bytes", totalBytes);

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                // Return plain stream if no progress reporting is needed
                if (progress == null)
                    return stream;

                // Wrap stream with progress tracking capability
                var progressStream = new ProgressStream(stream, totalBytes, progress);
                return progressStream;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to download from {Url}", downloadUrl);
                throw;
            }
        }

        /// <summary>
        /// Retrieves releases from a GitHub repository
        /// </summary>
        /// <param name="owner">The repository owner</param>
        /// <param name="repository">The repository name</param>
        /// <param name="includePrerelease">Whether to include prerelease versions</param>
        /// <returns>List of GitHub releases</returns>
        public async Task<List<GitHubRelease>> GetReleasesAsync(string owner, string repository, bool includePrerelease = true)
        {
            try
            {
                // Build GitHub API URL for releases
                var url = $"https://api.github.com/repos/{owner}/{repository}/releases";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Deserialize GitHub API response into release objects
                var json = await response.Content.ReadAsStringAsync();
                var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(json, _jsonOptions) ?? new List<GitHubRelease>();

                // Filter releases based on prerelease preference
                return includePrerelease ? releases : releases.Where(r => !r.Prerelease).ToList();
            }
            catch (Exception)
            {
                // Return empty list on API failure rather than throwing
                return new List<GitHubRelease>();
            }
        }

        /// <summary>
        /// Compares version strings to determine if the new version is newer than the current version
        /// </summary>
        /// <param name="newVersion">The new version string to compare</param>
        /// <param name="currentVersion">The current version string to compare against</param>
        /// <returns>True if the new version is newer, otherwise false</returns>
        private static bool IsNewerVersion(string newVersion, string currentVersion)
        {
            try
            {
                // Clean version strings by removing 'v' prefix and prerelease suffixes
                var cleanNewVersion = newVersion.TrimStart('v').Split('-')[0];
                var cleanCurrentVersion = currentVersion.TrimStart('v').Split('-')[0];

                // Parse versions and compare using built-in Version comparison
                var newVer = new Version(cleanNewVersion);
                var currentVer = new Version(cleanCurrentVersion);

                return newVer > currentVer;
            }
            catch
            {
                // Return false if version parsing fails
                return false;
            }
        }

        /// <summary>
        /// Disposes the HTTP client and releases associated resources
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Stream wrapper that provides progress reporting for download operations
    /// </summary>
    public class ProgressStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _totalBytes;
        private readonly IProgress<(long downloaded, long total)> _progress;
        private long _downloadedBytes;

        /// <summary>
        /// Initializes a new progress tracking stream wrapper
        /// </summary>
        /// <param name="baseStream">The underlying stream to wrap</param>
        /// <param name="totalBytes">Total bytes expected to be read</param>
        /// <param name="progress">Progress reporting callback</param>
        public ProgressStream(Stream baseStream, long totalBytes, IProgress<(long downloaded, long total)> progress)
        {
            _baseStream = baseStream;
            _totalBytes = totalBytes;
            _progress = progress;
        }

        /// <summary>
        /// Reads data asynchronously and reports progress
        /// </summary>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
            _downloadedBytes += read;
            _progress.Report((_downloadedBytes, _totalBytes));
            return read;
        }

        /// <summary>
        /// Reads data synchronously and reports progress
        /// </summary>
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