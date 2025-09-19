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

        private UpdateInfo? _currentUpdateInfo;

        public AppUpdateSettingViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            CurrentVersion = $"Current Version {ProcessInfoHelper.VersionWithPrefix}";
            LastUpdateCheck = Settings.LastUpdateCheck;
            IsUpdaterAvailable = _updateService.IsUpdaterAvailable();
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
        private async Task GetReleaseNotesAsync()
        {
            try
            {
                var releaseNotes = _currentUpdateInfo?.Changelog ?? "No release notes available.";
                var version = _currentUpdateInfo?.Version ?? "Unknown";

                ContentDialog dialog = new ContentDialog()
                {
                    Title = $"Release Notes - {version}",
                    CloseButtonText = "Close",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = releaseNotes,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(10)
                        },
                        MaxHeight = 400,
                        Margin = new Thickness(10)
                    },
                    Margin = new Thickness(10),
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = App.MainWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error showing release notes dialog in settings");
            }
        }
    }
}
