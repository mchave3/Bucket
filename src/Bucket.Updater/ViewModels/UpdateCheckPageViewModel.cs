namespace Bucket.Updater.ViewModels
{
    public partial class UpdateCheckPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private Bucket.Updater.Models.UpdateInfo? _availableUpdate;

        [ObservableProperty]
        private string headerText = "🔍 Checking for Updates...";

        [ObservableProperty]
        private string subHeaderText = string.Empty;

        [ObservableProperty]
        private Visibility subHeaderVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string currentVersion = string.Empty;

        [ObservableProperty]
        private string newVersion = string.Empty;

        [ObservableProperty]
        private Visibility newVersionVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string updateChannel = string.Empty;

        [ObservableProperty]
        private string architecture = string.Empty;

        [ObservableProperty]
        private bool isChecking = true;

        [ObservableProperty]
        private string statusMessage = "Checking for updates...";

        [ObservableProperty]
        private bool canCheckUpdates = false;

        [ObservableProperty]
        private bool canDownloadInstall = false;


        [ObservableProperty]
        private Visibility checkButtonVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility downloadInstallButtonVisibility = Visibility.Collapsed;

        public UpdateCheckPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.LogMethodEntry(nameof(UpdateCheckPageViewModel));
            LoadSystemInfo();
            Logger?.Information("UpdateCheckPageViewModel initialized with system info: {CurrentVersion}, {Channel}, {Architecture}",
                CurrentVersion, UpdateChannel, Architecture);

            // Start update check automatically
            _ = Task.Run(CheckForUpdatesAutomaticallyAsync);
        }

        private void LoadSystemInfo()
        {
            var config = _updateService.GetConfiguration();
            CurrentVersion = config.CurrentVersion;
            UpdateChannel = config.UpdateChannel == Models.UpdateChannel.Release ? "Release" : "Nightly";
            Architecture = config.GetArchitectureString();
        }

        private async Task CheckForUpdatesAutomaticallyAsync()
        {
            using (Logger?.BeginOperationScope("AutomaticUpdateCheck"))
            {
                await Task.Delay(500); // Small delay for UI

                try
                {
                    Logger?.Information("Starting automatic update check for {CurrentVersion} ({Channel})", CurrentVersion, UpdateChannel);

                    _availableUpdate = await PerformanceLogger.MeasureAndLogAsync(
                        "UpdateService.CheckForUpdates",
                        () => _updateService.CheckForUpdatesAsync());

                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (_availableUpdate != null)
                        {
                            // Update available
                            HeaderText = "🎉 Update Available!";
                            SubHeaderText = $"A new version of Bucket is ready to install";
                            SubHeaderVisibility = Visibility.Visible;
                            NewVersion = _availableUpdate.Version;
                            NewVersionVisibility = Visibility.Visible;
                            StatusMessage = $"Version {_availableUpdate.Version} is available";
                            CanDownloadInstall = true;
                            DownloadInstallButtonVisibility = Visibility.Visible;

                            Logger?.Information("Update found: {CurrentVersion} → {NewVersion}", CurrentVersion, _availableUpdate.Version);
                        }
                        else
                        {
                            // No update available
                            HeaderText = "✅ You're up to date!";
                            SubHeaderText = "No updates are currently available";
                            SubHeaderVisibility = Visibility.Visible;
                            StatusMessage = "You have the latest version";
                            CanCheckUpdates = true;
                            CheckButtonVisibility = Visibility.Collapsed;

                            Logger?.Information("No updates available for current version {CurrentVersion}", CurrentVersion);
                        }

                        IsChecking = false;
                    });

                    Logger?.Information("Automatic update check completed. Update available: {HasUpdate}", _availableUpdate != null);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "Error during automatic update check");

                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        HeaderText = "❌ Error Checking Updates";
                        StatusMessage = $"Failed to check for updates: {ex.Message}";
                        IsChecking = false;
                        CanCheckUpdates = true;
                        CheckButtonVisibility = Visibility.Visible;
                    });
                }
            }
        }

        [RelayCommand]
        private async Task CheckUpdatesAsync()
        {
            Logger?.Information("Manual update check requested");

            IsChecking = true;
            CanCheckUpdates = false;
            StatusMessage = "Checking for updates...";
            CheckButtonVisibility = Visibility.Collapsed;
            NewVersionVisibility = Visibility.Collapsed;
            DownloadInstallButtonVisibility = Visibility.Collapsed;

            try
            {
                _availableUpdate = await _updateService.CheckForUpdatesAsync();

                if (_availableUpdate != null)
                {
                    HeaderText = "🎉 Update Available!";
                    SubHeaderText = $"A new version of Bucket is ready to install";
                    SubHeaderVisibility = Visibility.Visible;
                    NewVersion = _availableUpdate.Version;
                    NewVersionVisibility = Visibility.Visible;
                    StatusMessage = $"Version {_availableUpdate.Version} is available";
                    CanDownloadInstall = true;
                    DownloadInstallButtonVisibility = Visibility.Visible;
                }
                else
                {
                    HeaderText = "✅ You're up to date!";
                    SubHeaderText = "No updates are currently available";
                    SubHeaderVisibility = Visibility.Visible;
                    StatusMessage = "You have the latest version";
                    CanCheckUpdates = true;
                    CheckButtonVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                HeaderText = "❌ Error Checking Updates";
                StatusMessage = $"Failed to check for updates: {ex.Message}";
                CanCheckUpdates = true;
                CheckButtonVisibility = Visibility.Visible;
                Logger?.Error(ex, "Error during manual update check");
            }
            finally
            {
                IsChecking = false;
            }
        }

        [RelayCommand]
        private void DownloadInstall()
        {
            if (_availableUpdate != null)
            {
                Logger?.LogUserAction("InitiateDownloadInstall", new 
                { 
                    CurrentVersion = CurrentVersion,
                    NewVersion = _availableUpdate.Version,
                    Channel = UpdateChannel,
                    FileSize = _availableUpdate.FileSize
                });
                Logger?.Information("Starting download and install process for version {Version}", _availableUpdate.Version);
                
                var mainWindow = App.MainWindow as MainWindow;
                var frame = mainWindow?.ContentFrame;
                if (frame != null)
                {
                    frame.Navigate(typeof(DownloadPage), _availableUpdate);
                    Logger?.Debug("Navigated to DownloadPage for version {Version}", _availableUpdate.Version);
                }
                else
                {
                    Logger?.Error("Failed to get ContentFrame for navigation");
                }
            }
            else
            {
                Logger?.Error("DownloadInstall command called with null available update");
            }
        }

        [RelayCommand]
        private void Close()
        {
            Logger?.LogUserAction("CloseUpdater", new 
            { 
                CurrentVersion = CurrentVersion,
                HasAvailableUpdate = _availableUpdate != null,
                UpdateVersion = _availableUpdate?.Version
            });
            Logger?.Information("Closing updater from update check page");
            App.MainWindow.Close();
        }
    }
}