using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bucket.App.Services
{
    /// <summary>
    /// Interface for managing application updates and updater launching
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Checks if an update is available asynchronously
        /// </summary>
        /// <returns>True if update is available, false otherwise</returns>
        Task<bool> CheckForUpdateAsync();

        /// <summary>
        /// Gets detailed update information including version and changelog
        /// </summary>
        /// <returns>Update information or null if no update available</returns>
        Task<UpdateInfo?> GetUpdateInfoAsync();

        /// <summary>
        /// Launches the updater application if available
        /// </summary>
        /// <returns>True if updater was launched successfully, false otherwise</returns>
        Task<bool> LaunchUpdaterAsync();

        /// <summary>
        /// Checks if the updater executable exists in the installation directory
        /// </summary>
        /// <returns>True if updater exists, false otherwise</returns>
        bool IsUpdaterAvailable();

        /// <summary>
        /// Gets the path to the updater executable
        /// </summary>
        /// <returns>Full path to Bucket.Updater.exe or null if not found</returns>
        string? GetUpdaterPath();
    }

    /// <summary>
    /// Simple update information container
    /// </summary>
    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Changelog { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsPreRelease { get; set; }
    }

    /// <summary>
    /// Service for managing application updates and launching the updater
    /// </summary>
    public class UpdateService : IUpdateService
    {
        private const string UPDATER_FOLDER = "Updater";
        private const string UPDATER_EXECUTABLE = "Bucket.Updater.exe";
        private const string GITHUB_OWNER = "mchave3";
        private const string GITHUB_REPO = "Bucket";

        /// <summary>
        /// Checks if an update is available asynchronously
        /// </summary>
        /// <returns>True if update is available, false otherwise</returns>
        public async Task<bool> CheckForUpdateAsync()
        {
            try
            {
                if (!NetworkHelper.IsNetworkAvailable())
                {
                    Logger?.Warning("Network not available for update check");
                    return false;
                }

                var currentVersion = new Version(ProcessInfoHelper.Version);
                var update = await UpdateHelper.CheckUpdateAsync(GITHUB_OWNER, GITHUB_REPO, currentVersion);

                return update.StableRelease.IsExistNewVersion || update.PreRelease.IsExistNewVersion;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error checking for updates");
                return false;
            }
        }

        /// <summary>
        /// Gets detailed update information including version and changelog
        /// </summary>
        /// <returns>Update information or null if no update available</returns>
        public async Task<UpdateInfo?> GetUpdateInfoAsync()
        {
            try
            {
                if (!NetworkHelper.IsNetworkAvailable())
                {
                    Logger?.Warning("Network not available for update info");
                    return null;
                }

                var currentVersion = new Version(ProcessInfoHelper.Version);
                var update = await UpdateHelper.CheckUpdateAsync(GITHUB_OWNER, GITHUB_REPO, currentVersion);

                // Prefer stable release over pre-release
                if (update.StableRelease.IsExistNewVersion)
                {
                    return new UpdateInfo
                    {
                        Version = update.StableRelease.TagName,
                        Changelog = update.StableRelease.Changelog,
                        CreatedAt = update.StableRelease.CreatedAt,
                        PublishedAt = update.StableRelease.PublishedAt,
                        IsPreRelease = false
                    };
                }
                else if (update.PreRelease.IsExistNewVersion)
                {
                    return new UpdateInfo
                    {
                        Version = update.PreRelease.TagName,
                        Changelog = update.PreRelease.Changelog,
                        CreatedAt = update.PreRelease.CreatedAt,
                        PublishedAt = update.PreRelease.PublishedAt,
                        IsPreRelease = true
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error getting update information");
                return null;
            }
        }

        /// <summary>
        /// Launches the updater application if available
        /// </summary>
        /// <returns>True if updater was launched successfully, false otherwise</returns>
        public async Task<bool> LaunchUpdaterAsync()
        {
            try
            {
                var updaterPath = GetUpdaterPath();
                if (string.IsNullOrEmpty(updaterPath))
                {
                    Logger?.Warning("Updater not found, cannot launch");
                    return false;
                }

                Logger?.Information("Launching updater: {UpdaterPath}", updaterPath);

                var startInfo = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(updaterPath)
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    Logger?.Information("Updater launched successfully (PID: {ProcessId})", process.Id);

                    // Give the updater a moment to start
                    await Task.Delay(1000);

                    // Exit the current application to allow update
                    Logger?.Information("Exiting application for update");
                    Application.Current.Exit();

                    return true;
                }

                Logger?.Error("Failed to start updater process");
                return false;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error launching updater");
                return false;
            }
        }

        /// <summary>
        /// Checks if the updater executable exists in the installation directory
        /// </summary>
        /// <returns>True if updater exists, false otherwise</returns>
        public bool IsUpdaterAvailable()
        {
            var updaterPath = GetUpdaterPath();
            return !string.IsNullOrEmpty(updaterPath) && File.Exists(updaterPath);
        }

        /// <summary>
        /// Gets the path to the updater executable
        /// </summary>
        /// <returns>Full path to Bucket.Updater.exe or null if not found</returns>
        public string? GetUpdaterPath()
        {
            try
            {
                // Get the installation directory (where the main app is running from)
                var installDirectory = GetInstallationDirectory();
                if (string.IsNullOrEmpty(installDirectory))
                {
                    Logger?.Warning("Could not determine installation directory");
                    return null;
                }

                // Build path to updater
                var updaterPath = Path.Combine(installDirectory, UPDATER_FOLDER, UPDATER_EXECUTABLE);

                Logger?.Debug("Checking for updater at: {UpdaterPath}", updaterPath);

                return File.Exists(updaterPath) ? updaterPath : null;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error getting updater path");
                return null;
            }
        }

        /// <summary>
        /// Gets the installation directory of the current application
        /// </summary>
        /// <returns>Installation directory path or null if unable to determine</returns>
        private string? GetInstallationDirectory()
        {
            try
            {
                // Try different methods to get the installation directory

                // Method 1: Use AppContext.BaseDirectory (most reliable for published apps)
                var baseDirectory = AppContext.BaseDirectory;
                if (!string.IsNullOrEmpty(baseDirectory) && Directory.Exists(baseDirectory))
                {
                    Logger?.Debug("Using AppContext.BaseDirectory: {Directory}", baseDirectory);
                    return baseDirectory;
                }

                // Method 2: Use executing assembly location
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(assemblyLocation))
                {
                    var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                    if (!string.IsNullOrEmpty(assemblyDirectory) && Directory.Exists(assemblyDirectory))
                    {
                        Logger?.Debug("Using Assembly location: {Directory}", assemblyDirectory);
                        return assemblyDirectory;
                    }
                }

                // Method 3: Use current directory as fallback
                var currentDirectory = Environment.CurrentDirectory;
                if (!string.IsNullOrEmpty(currentDirectory) && Directory.Exists(currentDirectory))
                {
                    Logger?.Debug("Using current directory: {Directory}", currentDirectory);
                    return currentDirectory;
                }

                Logger?.Warning("All methods to determine installation directory failed");
                return null;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error determining installation directory");
                return null;
            }
        }
    }
}