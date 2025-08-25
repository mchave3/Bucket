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

        /// <summary>
        /// Initializes the ViewModel by automatically checking for updates.
        /// </summary>
        public async Task InitializeAsync()
        {
            Logger.Information("Initializing AppUpdateSettingViewModel - checking for updates");

            // Automatically check for updates when the page loads
            await CheckForUpdateAsync();
        }

        [RelayCommand]
        private async Task CheckForUpdateAsync()
        {
            Logger.Information("Starting update check");
            IsLoading = true;
            IsUpdateAvailable = false;
            IsCheckButtonEnabled = false;
            LoadingStatus = "Checking for new version";
            Logger.Information("Update check status: {Status}", LoadingStatus);
            if (NetworkHelper.IsNetworkAvailable())
            {
                Logger.Debug("Network is available, proceeding with update check");
                try
                {
                    string username = "mchave3";
                    string repo = "Bucket";
                    LastUpdateCheck = DateTime.Now.ToShortDateString();
                    Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();
                    Logger.Debug("Checking for updates from repository: {Username}/{Repo}", username, repo);
                    var update = await UpdateHelper.CheckUpdateAsync(username, repo, new Version(ProcessInfoHelper.Version));
                    if (update.StableRelease.IsExistNewVersion)
                    {
                        IsUpdateAvailable = true;
                        ChangeLog = update.StableRelease.Changelog;
                        LoadingStatus = $"We found a new version {update.StableRelease.TagName} Created at {update.StableRelease.CreatedAt} and Published at {update.StableRelease.PublishedAt}";
                        Logger.Information("New stable version found: {Version}, Status: {Status}", update.StableRelease.TagName, LoadingStatus);
                    }
                    else if (update.PreRelease.IsExistNewVersion)
                    {
                        IsUpdateAvailable = true;
                        ChangeLog = update.PreRelease.Changelog;
                        LoadingStatus = $"We found a new PreRelease Version {update.PreRelease.TagName} Created at {update.PreRelease.CreatedAt} and Published at {update.PreRelease.PublishedAt}";
                        Logger.Information("New pre-release version found: {Version}, Status: {Status}", update.PreRelease.TagName, LoadingStatus);
                    }
                    else
                    {
                        LoadingStatus = "You are using latest version";
                        Logger.Information("No updates available, Status: {Status}", LoadingStatus);
                    }
                }
                catch (Exception ex)
                {
                    LoadingStatus = ex.Message;
                    Logger.Error(ex, "Error occurred while checking for updates, Status: {Status}", LoadingStatus);
                    IsLoading = false;
                    IsCheckButtonEnabled = true;
                }
            }
            else
            {
                LoadingStatus = "Error Connection";
                Logger.Warning("Network is not available, cannot check for updates, Status: {Status}", LoadingStatus);
            }
            IsLoading = false;
            IsCheckButtonEnabled = true;
        }

        [RelayCommand]
        private async Task GoToUpdateAsync()
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/mchave3/Bucket/releases"));
        }

        [RelayCommand]
        private async Task GetReleaseNotesAsync()
        {
            await MessageBox.ShowInfoAsync(ChangeLog, "Release Notes", MessageBoxButtons.OK);
        }
    }
}
