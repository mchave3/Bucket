using System.Diagnostics;

namespace Bucket.Updater.Services
{
    /// <summary>
    /// Interface for high-level update operations including checking, downloading, and installing updates
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Checks for available updates based on current configuration
        /// </summary>
        /// <returns>Update information if an update is available, otherwise null</returns>
        Task<Bucket.Updater.Models.UpdateInfo> CheckForUpdatesAsync();

        /// <summary>
        /// Downloads an update to a temporary location with progress reporting
        /// </summary>
        /// <param name="updateInfo">Information about the update to download</param>
        /// <param name="progress">Optional progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token to cancel the download</param>
        /// <returns>Path to the downloaded file</returns>
        Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)> progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Installs an update from the specified MSI file path
        /// </summary>
        /// <param name="msiFilePath">Path to the MSI file to install</param>
        /// <param name="progress">Optional progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token to cancel the installation</param>
        /// <returns>True if installation was successful, otherwise false</returns>
        Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string> progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans up downloaded files at the specified path
        /// </summary>
        /// <param name="downloadPath">Path to the downloaded file to clean up</param>
        void CleanupFiles(string downloadPath);

        /// <summary>
        /// Cleans up all temporary files created during update operations
        /// </summary>
        void CleanupAllTemporaryFiles();

        /// <summary>
        /// Gets the current updater configuration
        /// </summary>
        /// <returns>The current updater configuration</returns>
        UpdaterConfiguration GetConfiguration();
    }

    /// <summary>
    /// High-level service that orchestrates update operations by coordinating between
    /// configuration, GitHub API, and installation services.
    /// </summary>
    public class UpdateService : IUpdateService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IGitHubService _gitHubService;
        private readonly IInstallationService _installationService;

        /// <summary>
        /// Initializes a new instance of UpdateService with the required dependencies
        /// </summary>
        /// <param name="configurationService">Service for managing configuration</param>
        /// <param name="gitHubService">Service for GitHub API operations</param>
        /// <param name="installationService">Service for installing updates</param>
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

        /// <summary>
        /// Checks for available updates based on current configuration
        /// </summary>
        /// <returns>Update information if an update is available, otherwise null</returns>
        public async Task<Bucket.Updater.Models.UpdateInfo> CheckForUpdatesAsync()
        {
            using (Logger?.BeginOperationScope("CheckForUpdates"))
            {
                Logger?.LogMethodEntry(nameof(CheckForUpdatesAsync));

                try
                {
                    // Load configuration with performance monitoring
                    var configuration = await PerformanceLogger.MeasureAndLogAsync(
                        "ConfigurationService.LoadConfiguration",
                        () => _configurationService.LoadConfigurationAsync()).ConfigureAwait(false);

                    Logger?.Debug("Configuration loaded: {@Configuration}", new
                    {
                        CurrentVersion = configuration.CurrentVersion,
                        Channel = configuration.UpdateChannel.ToString(),
                        Architecture = configuration.GetArchitectureString()
                    });

                    // Check for updates using GitHub service with performance monitoring
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

        /// <summary>
        /// Downloads an update to a temporary location with progress reporting
        /// </summary>
        /// <param name="updateInfo">Information about the update to download</param>
        /// <param name="progress">Optional progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token to cancel the download</param>
        /// <returns>Path to the downloaded file</returns>
        public async Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)> progress = null, CancellationToken cancellationToken = default)
        {
            using (Logger?.BeginOperationScope("DownloadUpdate", new { Version = updateInfo.Version, FileSize = updateInfo.FileSize }))
            {
                Logger?.LogMethodEntry(nameof(DownloadUpdateAsync), new { updateInfo.Version, updateInfo.FileSize, Url = updateInfo.DownloadUrl });

                try
                {
                    // Set up temporary directory for downloads
                    var tempDirectory = Path.Combine(Path.GetTempPath(), "BucketUpdater");
                    if (!Directory.Exists(tempDirectory))
                    {
                        Directory.CreateDirectory(tempDirectory);
                        Logger?.Information("Created temp directory: {Directory}", tempDirectory);
                    }

                    // Extract filename from download URL and create full path
                    var fileName = Path.GetFileName(new Uri(updateInfo.DownloadUrl).LocalPath);
                    var downloadPath = Path.Combine(tempDirectory, fileName);
                    Logger?.Debug("Download configuration: {@DownloadConfig}", new
                    {
                        TempDirectory = tempDirectory,
                        FileName = fileName,
                        DownloadPath = downloadPath,
                        Url = updateInfo.DownloadUrl
                    });

                    // Clean up any existing file at the download path
                    if (File.Exists(downloadPath))
                    {
                        var existingSize = new FileInfo(downloadPath).Length;
                        File.Delete(downloadPath);
                        Logger?.Information("Deleted existing file at download path (was {Size} bytes)", existingSize);
                    }

                    var downloadAndSaveStopwatch = Stopwatch.StartNew();

                    // Download file stream with performance monitoring
                    using var downloadStream = await PerformanceLogger.MeasureAndLogAsync(
                        "GitHubService.DownloadUpdate",
                        () => _gitHubService.DownloadUpdateAsync(updateInfo.DownloadUrl, progress, cancellationToken)).ConfigureAwait(false);

                    // Save downloaded content to file with performance monitoring
                    using var fileStream = File.Create(downloadPath);
                    await PerformanceLogger.MeasureAndLogAsync(
                        "FileStream.CopyTo",
                        () => downloadStream.CopyToAsync(fileStream, cancellationToken)).ConfigureAwait(false);

                    downloadAndSaveStopwatch.Stop();
                    var finalFileInfo = new FileInfo(downloadPath);

                    // Validate downloaded file size matches expected size
                    if (finalFileInfo.Length != updateInfo.FileSize)
                    {
                        Logger?.Warning("Downloaded file size mismatch. Expected: {ExpectedSize} bytes, Actual: {ActualSize} bytes, Difference: {Difference} bytes",
                            updateInfo.FileSize, finalFileInfo.Length, updateInfo.FileSize - finalFileInfo.Length);

                        // Fail if difference is significant (more than 1% or 1MB)
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

        /// <summary>
        /// Installs an update from the specified MSI file path
        /// </summary>
        /// <param name="msiFilePath">Path to the MSI file to install</param>
        /// <param name="progress">Optional progress reporting callback</param>
        /// <param name="cancellationToken">Cancellation token to cancel the installation</param>
        /// <returns>True if installation was successful, otherwise false</returns>
        public async Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            // Delegate installation to the specialized installation service
            return await _installationService.InstallUpdateAsync(msiFilePath, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Cleans up downloaded files at the specified path
        /// </summary>
        /// <param name="downloadPath">Path to the downloaded file to clean up</param>
        public void CleanupFiles(string downloadPath)
        {
            Logger?.Information("UpdateService cleaning up files at {Path}", downloadPath);
            _installationService.CleanupDownloadedFiles(downloadPath);
        }

        /// <summary>
        /// Cleans up all temporary files created during update operations
        /// </summary>
        public void CleanupAllTemporaryFiles()
        {
            Logger?.Information("UpdateService cleaning up all temporary files");
            _installationService.CleanupAllTemporaryFiles();
        }

        /// <summary>
        /// Gets the current updater configuration
        /// </summary>
        /// <returns>The current updater configuration</returns>
        public UpdaterConfiguration GetConfiguration()
        {
            return _configurationService.GetConfiguration();
        }

    }
}