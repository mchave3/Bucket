using Bucket.App.Services;

namespace Bucket.App.ViewModels
{
    public partial class AppUpdateSettingViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;

        [ObservableProperty]
        public string currentVersion;

        [ObservableProperty]
        public string lastUpdateCheck;

        [ObservableProperty]
        public bool isUpdateAvailable;

        [ObservableProperty]
        public bool isLoading;

        [ObservableProperty]
        public bool isCheckButtonEnabled = true;

        [ObservableProperty]
        public string loadingStatus = "Status";

        [ObservableProperty]
        public bool isUpdaterAvailable;

        private AppUpdateInfo? _currentUpdateInfo;

        public AppUpdateSettingViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            CurrentVersion = $"Current Version {ProcessInfoHelper.VersionWithPrefix}";
            LastUpdateCheck = Settings.LastUpdateCheck;
            IsUpdaterAvailable = _updateService.IsUpdaterAvailable();
        }

        /// <summary>
        /// Initializes the view model and performs initial update check
        /// </summary>
        public async Task InitializeAsync()
        {
            // Automatically check for updates when the page loads
            await CheckForUpdateAsync();
        }

        [RelayCommand]
        private async Task CheckForUpdateAsync()
        {
            IsLoading = true;
            IsUpdateAvailable = false;
            IsCheckButtonEnabled = false;
            LoadingStatus = "Checking for new version";
            _currentUpdateInfo = null;

            try
            {
                if (!NetworkHelper.IsNetworkAvailable())
                {
                    LoadingStatus = "Error Connection";
                    return;
                }

                LastUpdateCheck = DateTime.Now.ToShortDateString();
                Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();

                var updateInfo = await _updateService.GetUpdateInfoAsync();
                if (updateInfo != null)
                {
                    IsUpdateAvailable = true;
                    _currentUpdateInfo = updateInfo;

                    var updateType = updateInfo.IsPreRelease ? "Pre-release" : "Stable release";
                    LoadingStatus = $"We found a new {updateType.ToLower()} version {updateInfo.Version} " +
                                  $"Created at {updateInfo.CreatedAt:yyyy-MM-dd} and Published at {updateInfo.PublishedAt:yyyy-MM-dd}";
                }
                else
                {
                    LoadingStatus = "You are using latest version";
                }
            }
            catch (Exception ex)
            {
                LoadingStatus = $"Error checking for updates: {ex.Message}";
                Logger?.Error(ex, "Error checking for updates in settings page");
            }
            finally
            {
                IsLoading = false;
                IsCheckButtonEnabled = true;
            }
        }

        [RelayCommand]
        private async Task LaunchUpdaterAsync()
        {
            if (!IsUpdaterAvailable)
            {
                LoadingStatus = "Updater not available. Please download manually from GitHub.";
                Logger?.Warning("Updater not available when trying to launch from settings");
                return;
            }

            try
            {
                LoadingStatus = "Launching updater...";
                var success = await _updateService.LaunchUpdaterAsync();

                if (!success)
                {
                    LoadingStatus = "Failed to launch updater. Please try again or download manually.";
                    Logger?.Error("Failed to launch updater from settings page");
                }
            }
            catch (Exception ex)
            {
                LoadingStatus = $"Error launching updater: {ex.Message}";
                Logger?.Error(ex, "Error launching updater from settings page");
            }
        }

        [RelayCommand]
        private async Task OpenReleaseNotesAsync()
        {
            try
            {
                var version = _currentUpdateInfo?.Version;
                if (string.IsNullOrEmpty(version))
                {
                    // Fallback to general releases page if no specific version
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/mchave3/Bucket/releases"));
                    Logger?.Information("Opened GitHub releases page (no specific version)");
                    return;
                }

                // Open specific release page
                var releaseUrl = $"https://github.com/mchave3/Bucket/releases/tag/{version}";
                await Windows.System.Launcher.LaunchUriAsync(new Uri(releaseUrl));
                Logger?.Information("Opened GitHub release page for version {Version}", version);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error opening GitHub release notes page");

                // Fallback: try to open general releases page
                try
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/mchave3/Bucket/releases"));
                    Logger?.Information("Opened fallback GitHub releases page");
                }
                catch (Exception fallbackEx)
                {
                    Logger?.Error(fallbackEx, "Failed to open fallback releases page");
                }
            }
        }
    }
}
