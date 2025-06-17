using Windows.System;

namespace Bucket.ViewModels
{
    public partial class AppUpdateSettingViewModel : ObservableObject
    {
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

        private string ChangeLog = string.Empty;

        public AppUpdateSettingViewModel()
        {
            Logger.Debug("AppUpdateSettingViewModel initialized");
            CurrentVersion = $"Current Version {ProcessInfoHelper.VersionWithPrefix}";
            LastUpdateCheck = Settings.LastUpdateCheck;
            Logger.Information("Current version: {Version}, Last update check: {LastCheck}",
                ProcessInfoHelper.Version, Settings.LastUpdateCheck);
        }

        [RelayCommand]
        private async Task CheckForUpdateAsync()
        {
            Logger.Information("Starting update check");
            IsLoading = true;
            IsUpdateAvailable = false;
            IsCheckButtonEnabled = false;
            LoadingStatus = "Checking for new version";
            if (NetworkHelper.IsNetworkAvailable())
            {
                Logger.Debug("Network is available, proceeding with update check");
                try
                {
                    //Todo: Fix UserName and Repo
                    string username = "";
                    string repo = "";
                    LastUpdateCheck = DateTime.Now.ToShortDateString();
                    Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();
                    Logger.Debug("Checking for updates from repository: {Username}/{Repo}", username, repo);
                    var update = await UpdateHelper.CheckUpdateAsync(username, repo, new Version(ProcessInfoHelper.Version));
                    if (update.StableRelease.IsExistNewVersion)
                    {
                        IsUpdateAvailable = true;
                        ChangeLog = update.StableRelease.Changelog;
                        LoadingStatus = $"We found a new version {update.StableRelease.TagName} Created at {update.StableRelease.CreatedAt} and Published at {update.StableRelease.PublishedAt}";
                    }
                    else if (update.PreRelease.IsExistNewVersion)
                    {
                        IsUpdateAvailable = true;
                        ChangeLog = update.PreRelease.Changelog;
                        LoadingStatus = $"We found a new PreRelease Version {update.PreRelease.TagName} Created at {update.PreRelease.CreatedAt} and Published at {update.PreRelease.PublishedAt}";
                    }
                    else
                    {
                        LoadingStatus = "You are using latest version";
                    }
                }
                catch (Exception ex)
                {
                    LoadingStatus = ex.Message;
                    Logger.Error(ex, "Error occurred while checking for updates");
                    IsLoading = false;
                    IsCheckButtonEnabled = true;
                }
            }
            else
            {
                LoadingStatus = "Error Connection";
                Logger.Warning("Network is not available, cannot check for updates");
            }
            IsLoading = false;
            IsCheckButtonEnabled = true;
        }

        [RelayCommand]
        private async Task GoToUpdateAsync()
        {
            //Todo: Change Uri
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Ghost1372/DevWinUI/releases"));
        }

        [RelayCommand]
        private async Task GetReleaseNotesAsync()
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Release Note",
                CloseButtonText = "Close",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = ChangeLog,
                        Margin = new Thickness(10)
                    },
                    Margin = new Thickness(10)
                },
                Margin = new Thickness(10),
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
