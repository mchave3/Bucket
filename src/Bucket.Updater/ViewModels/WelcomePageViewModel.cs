namespace Bucket.Updater.ViewModels
{
    public partial class WelcomePageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;

        [ObservableProperty]
        private string currentVersion = "1.0.0.0";

        [ObservableProperty]
        private string architecture = "x64";

        [ObservableProperty]
        private int selectedChannelIndex = 0;

        [ObservableProperty]
        private bool isChecking = false;

        [ObservableProperty]
        private string statusMessage = "Ready to check for updates";

        [ObservableProperty]
        private bool hasUpdate = false;

        [ObservableProperty]
        private string updateMessage = string.Empty;

        [ObservableProperty]
        private bool canCheckUpdates = true;

        private Bucket.Updater.Models.UpdateInfo? _availableUpdate;

        public WelcomePageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            LoadConfiguration();
            Logger?.Information("WelcomePageViewModel initialized");
        }

        [RelayCommand]
        private async Task CheckUpdatesAsync()
        {
            Logger?.Information("Starting update check from Welcome page");
            try
            {
                IsChecking = true;
                CanCheckUpdates = false;
                StatusMessage = "Checking for updates...";
                HasUpdate = false;

                await SaveConfigurationAsync();

                _availableUpdate = await _updateService.CheckForUpdatesAsync();

                if (_availableUpdate != null)
                {
                    HasUpdate = true;
                    StatusMessage = "Update found!";
                    UpdateMessage = $"Version {_availableUpdate.Version} is available";
                    Logger?.Information("Update found: {Version}", _availableUpdate.Version);
                }
                else
                {
                    StatusMessage = "You have the latest version";
                    Logger?.Information("No update found - current version is up to date");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error checking for updates: {ex.Message}";
                Logger?.Error(ex, "Error checking for updates from Welcome page");
            }
            finally
            {
                IsChecking = false;
                CanCheckUpdates = true;
            }
        }

        [RelayCommand]
        private void Next()
        {
            if (_availableUpdate != null && HasUpdate)
            {
                Logger?.Information("Navigating to changelog page for version {Version}", _availableUpdate.Version);
                var mainWindow = App.MainWindow as MainWindow;
                var frame = mainWindow?.ContentFrame;
                frame?.Navigate(typeof(ChangelogPage), _availableUpdate);
            }
        }

        private void LoadConfiguration()
        {
            var config = _updateService.GetConfiguration();
            CurrentVersion = config.CurrentVersion;
            Architecture = config.GetArchitectureString();
            SelectedChannelIndex = config.UpdateChannel == UpdateChannel.Release ? 0 : 1;
        }

        private async Task SaveConfigurationAsync()
        {
            var config = _updateService.GetConfiguration();
            config.UpdateChannel = SelectedChannelIndex == 0 ? UpdateChannel.Release : UpdateChannel.Nightly;
            await _updateService.SaveConfigurationAsync(config);
        }
    }
}