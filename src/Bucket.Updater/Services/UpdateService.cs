namespace Bucket.Updater.Services
{
    public interface IUpdateService
    {
        Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync();
        Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default);
        Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
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
            Logger?.Information("UpdateService initialized (read-only configuration)");
        }

        public async Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync()
        {
            Logger?.Information("UpdateService checking for updates");
            try
            {
                var configuration = await _configurationService.LoadConfigurationAsync();
                var updateInfo = await _gitHubService.CheckForUpdatesAsync(configuration);

                if (updateInfo != null)
                {
                    Logger?.Information("Update check completed, update available: {Version}", updateInfo.Version);

                    // NOTE: LastUpdateCheck is no longer updated here as Bucket.Updater is read-only
                    // This responsibility could be delegated to Bucket.App if necessary
                }
                else
                {
                    Logger?.Information("Update check completed, no updates available");
                }

                return updateInfo;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "UpdateService failed to check for updates");
                return null;
            }
        }

        public async Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default)
        {
            Logger?.Information("UpdateService starting download for version {Version}", updateInfo.Version);
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
                Logger?.Information("Download path: {Path}", downloadPath);

                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                    Logger?.Information("Deleted existing file at download path");
                }

                using var downloadStream = await _gitHubService.DownloadUpdateAsync(updateInfo.DownloadUrl, progress, cancellationToken);
                using var fileStream = File.Create(downloadPath);
                await downloadStream.CopyToAsync(fileStream, cancellationToken);

                Logger?.Information("Download completed successfully to {Path}", downloadPath);
                return downloadPath;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "UpdateService failed to download update");
                throw;
            }
        }

        public async Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            return await _installationService.InstallUpdateAsync(msiFilePath, progress, cancellationToken);
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