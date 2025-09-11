using System.Diagnostics;

namespace Bucket.Updater.Services
{
    public interface IUpdateService
    {
        Task<Bucket.Updater.Models.UpdateInfo> CheckForUpdatesAsync();
        Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)> progress = null, CancellationToken cancellationToken = default);
        Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string> progress = null, CancellationToken cancellationToken = default);
        void CleanupFiles(string downloadPath);
        void CleanupAllTemporaryFiles();
        UpdaterConfiguration GetConfiguration();
    }

    public class UpdateService : IUpdateService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IGitHubService _gitHubService;
        private readonly IInstallationService _installationService;

        public UpdateService(
            IConfigurationService configurationService,
            IGitHubService gitHubService,
            IInstallationService installationService)
        {
            _configurationService = configurationService;
            _gitHubService = gitHubService;
            _installationService = installationService;
            Logger?.LogMethodEntry(nameof(UpdateService), new
            {
                ConfigurationService = configurationService?.GetType().Name,
                GitHubService = gitHubService?.GetType().Name,
                InstallationService = installationService?.GetType().Name
            });
            Logger?.Information("UpdateService initialized (read-only configuration) with injected services");
        }

        public async Task<Bucket.Updater.Models.UpdateInfo> CheckForUpdatesAsync()
        {
            using (Logger?.BeginOperationScope("CheckForUpdates"))
            {
                Logger?.LogMethodEntry(nameof(CheckForUpdatesAsync));

                try
                {
                    var configuration = await PerformanceLogger.MeasureAndLogAsync(
                        "ConfigurationService.LoadConfiguration",
                        () => _configurationService.LoadConfigurationAsync()).ConfigureAwait(false);

                    Logger?.Debug("Configuration loaded: {@Configuration}", new
                    {
                        CurrentVersion = configuration.CurrentVersion,
                        Channel = configuration.UpdateChannel.ToString(),
                        Architecture = configuration.GetArchitectureString()
                    });

                    var updateInfo = await PerformanceLogger.MeasureAndLogAsync(
                        "GitHubService.CheckForUpdates",
                        () => _gitHubService.CheckForUpdatesAsync(configuration)).ConfigureAwait(false);

                    if (updateInfo != null)
                    {
                        Logger?.Information("Update check completed, update available: {CurrentVersion} → {NewVersion} ({FileSize} bytes)",
                            configuration.CurrentVersion, updateInfo.Version, updateInfo.FileSize);
                        Logger?.Debug("Update details: {@UpdateInfo}", new
                        {
                            updateInfo.Version,
                            updateInfo.FileSize,
                            updateInfo.DownloadUrl
                        });
                    }
                    else
                    {
                        Logger?.Information("Update check completed, no updates available for version {CurrentVersion}",
                            configuration.CurrentVersion);
                    }

                    return updateInfo;
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "UpdateService failed to check for updates");
                    return null;
                }
            }
        }

        public async Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)> progress = null, CancellationToken cancellationToken = default)
        {
            using (Logger?.BeginOperationScope("DownloadUpdate", new { Version = updateInfo.Version, FileSize = updateInfo.FileSize }))
            {
                Logger?.LogMethodEntry(nameof(DownloadUpdateAsync), new { updateInfo.Version, updateInfo.FileSize, Url = updateInfo.DownloadUrl });

                try
                {
                    var tempDirectory = Path.Combine(Path.GetTempPath(), "BucketUpdater");
                    if (!Directory.Exists(tempDirectory))
                    {
                        Directory.CreateDirectory(tempDirectory);
                        Logger?.Information("Created temp directory: {Directory}", tempDirectory);
                    }

                    var fileName = Path.GetFileName(new Uri(updateInfo.DownloadUrl).LocalPath);
                    var downloadPath = Path.Combine(tempDirectory, fileName);
                    Logger?.Debug("Download configuration: {@DownloadConfig}", new
                    {
                        TempDirectory = tempDirectory,
                        FileName = fileName,
                        DownloadPath = downloadPath,
                        Url = updateInfo.DownloadUrl
                    });

                    if (File.Exists(downloadPath))
                    {
                        var existingSize = new FileInfo(downloadPath).Length;
                        File.Delete(downloadPath);
                        Logger?.Information("Deleted existing file at download path (was {Size} bytes)", existingSize);
                    }

                    var downloadAndSaveStopwatch = Stopwatch.StartNew();

                    using var downloadStream = await PerformanceLogger.MeasureAndLogAsync(
                        "GitHubService.DownloadUpdate",
                        () => _gitHubService.DownloadUpdateAsync(updateInfo.DownloadUrl, progress, cancellationToken)).ConfigureAwait(false);

                    using var fileStream = File.Create(downloadPath);
                    await PerformanceLogger.MeasureAndLogAsync(
                        "FileStream.CopyTo",
                        () => downloadStream.CopyToAsync(fileStream, cancellationToken)).ConfigureAwait(false);

                    downloadAndSaveStopwatch.Stop();
                    var finalFileInfo = new FileInfo(downloadPath);

                    // Validate file size integrity
                    if (finalFileInfo.Length != updateInfo.FileSize)
                    {
                        Logger?.Warning("Downloaded file size mismatch. Expected: {ExpectedSize} bytes, Actual: {ActualSize} bytes, Difference: {Difference} bytes",
                            updateInfo.FileSize, finalFileInfo.Length, updateInfo.FileSize - finalFileInfo.Length);

                        // If difference is significant (more than 1% or 1MB), consider it an error
                        var sizeDifference = Math.Abs(updateInfo.FileSize - finalFileInfo.Length);
                        var percentageDifference = (double)sizeDifference / updateInfo.FileSize * 100;

                        if (sizeDifference > 1024 * 1024 || percentageDifference > 1.0) // 1MB or 1%
                        {
                            File.Delete(downloadPath);
                            Logger?.Error("Download integrity check failed. File size difference too large: {Difference} bytes ({Percentage:F2}%)",
                                sizeDifference, percentageDifference);
                            throw new InvalidOperationException($"Download integrity check failed. Expected {updateInfo.FileSize} bytes, got {finalFileInfo.Length} bytes");
                        }
                    }

                    Logger?.LogPerformance("DownloadAndSave", downloadAndSaveStopwatch.Elapsed, finalFileInfo.Length);
                    Logger?.Information("Download completed successfully to {Path} ({Size} bytes)", downloadPath, finalFileInfo.Length);

                    return downloadPath;
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "UpdateService failed to download update for version {Version}", updateInfo.Version);
                    throw;
                }
            }
        }

        public async Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            return await _installationService.InstallUpdateAsync(msiFilePath, progress, cancellationToken).ConfigureAwait(false);
        }

        public void CleanupFiles(string downloadPath)
        {
            Logger?.Information("UpdateService cleaning up files at {Path}", downloadPath);
            _installationService.CleanupDownloadedFiles(downloadPath);
        }

        public void CleanupAllTemporaryFiles()
        {
            Logger?.Information("UpdateService cleaning up all temporary files");
            _installationService.CleanupAllTemporaryFiles();
        }

        public UpdaterConfiguration GetConfiguration()
        {
            return _configurationService.GetConfiguration();
        }

    }
}