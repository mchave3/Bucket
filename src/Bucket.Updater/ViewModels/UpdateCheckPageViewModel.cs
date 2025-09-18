namespace Bucket.Updater.ViewModels
{
    /// <summary>
    /// ViewModel for the update check page that manages the update discovery workflow.
    /// Handles automatic and manual update checking, UI state management, and navigation to the download page.
    /// </summary>
    public partial class UpdateCheckPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private Bucket.Updater.Models.UpdateInfo? _availableUpdate;

        /// <summary>
        /// Main header text displayed on the update check page
        /// </summary>
        [ObservableProperty]
        private string headerText = "🔍 Checking for Updates...";

        /// <summary>
        /// Secondary header text that provides additional context about the update status
        /// </summary>
        [ObservableProperty]
        private string subHeaderText = string.Empty;

        /// <summary>
        /// Controls whether the sub-header text is visible
        /// </summary>
        [ObservableProperty]
        private Visibility subHeaderVisibility = Visibility.Collapsed;

        /// <summary>
        /// Currently installed version of the application
        /// </summary>
        [ObservableProperty]
        private string currentVersion = string.Empty;

        /// <summary>
        /// Available new version when an update is found
        /// </summary>
        [ObservableProperty]
        private string newVersion = string.Empty;

        /// <summary>
        /// Controls whether the new version information is displayed
        /// </summary>
        [ObservableProperty]
        private Visibility newVersionVisibility = Visibility.Collapsed;

        /// <summary>
        /// Update channel being used (Release or Nightly)
        /// </summary>
        [ObservableProperty]
        private string updateChannel = string.Empty;

        /// <summary>
        /// System architecture (x86, x64, ARM64)
        /// </summary>
        [ObservableProperty]
        private string architecture = string.Empty;

        /// <summary>
        /// Indicates whether an update check is currently in progress
        /// </summary>
        [ObservableProperty]
        private bool isChecking = true;

        /// <summary>
        /// Status message displayed to the user about the current operation
        /// </summary>
        [ObservableProperty]
        private string statusMessage = "Checking for updates...";

        /// <summary>
        /// Controls whether the manual check updates button is enabled
        /// </summary>
        [ObservableProperty]
        private bool canCheckUpdates = false;

        /// <summary>
        /// Controls whether the download and install button is enabled
        /// </summary>
        [ObservableProperty]
        private bool canDownloadInstall = false;


        /// <summary>
        /// Controls whether the manual check updates button is visible
        /// </summary>
        [ObservableProperty]
        private Visibility checkButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Controls whether the download and install button is visible
        /// </summary>
        [ObservableProperty]
        private Visibility downloadInstallButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Initializes a new instance of UpdateCheckPageViewModel
        /// </summary>
        /// <param name="updateService">Service for checking and managing updates</param>
        public UpdateCheckPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.LogMethodEntry(nameof(UpdateCheckPageViewModel));
            LoadSystemInfo();
            Logger?.Information("UpdateCheckPageViewModel initialized with system info: {CurrentVersion}, {Channel}, {Architecture}",
                CurrentVersion, UpdateChannel, Architecture);

            // Start update check automatically on a background thread to avoid blocking UI initialization
            _ = Task.Run(CheckForUpdatesAutomaticallyAsync);
        }

        /// <summary>
        /// Loads current system information including version, update channel, and architecture
        /// </summary>
        private void LoadSystemInfo()
        {
            var config = _updateService.GetConfiguration();
            CurrentVersion = config.CurrentVersion;
            // Convert enum to user-friendly display string
            UpdateChannel = config.UpdateChannel == Models.UpdateChannel.Release ? "Release" : "Nightly";
            Architecture = config.GetArchitectureString();
        }

        /// <summary>
        /// Performs automatic update check when the page is loaded.
        /// Updates UI state based on whether an update is available or not.
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task CheckForUpdatesAutomaticallyAsync()
        {
            using (Logger?.BeginOperationScope("AutomaticUpdateCheck"))
            {
                // Small delay to allow UI to render before starting intensive operation
                await Task.Delay(500);

                try
                {
                    Logger?.Information("Starting automatic update check for {CurrentVersion} ({Channel})", CurrentVersion, UpdateChannel);

                    // Perform update check with performance logging
                    _availableUpdate = await PerformanceLogger.MeasureAndLogAsync(
                        "UpdateService.CheckForUpdates",
                        () => _updateService.CheckForUpdatesAsync());

                    // Update UI on main thread with results
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (_availableUpdate != null)
                        {
                            // Update available - configure UI for download workflow
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
                            // No update available - show up-to-date status
                            HeaderText = "✅ You're up to date!";
                            SubHeaderText = "No updates are currently available";
                            SubHeaderVisibility = Visibility.Visible;
                            StatusMessage = "You have the latest version";
                            CanCheckUpdates = true;
                            CheckButtonVisibility = Visibility.Collapsed;

                            Logger?.Information("No updates available for current version {CurrentVersion}", CurrentVersion);
                        }

                        // Update check completed - reset checking state
                        IsChecking = false;
                    });

                    Logger?.Information("Automatic update check completed. Update available: {HasUpdate}", _availableUpdate != null);
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex, "Error during automatic update check");

                    // Handle error state - show error message and enable manual retry
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

        /// <summary>
        /// Command to manually check for updates.
        /// Resets UI state and performs a new update check.
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        [RelayCommand]
        private async Task CheckUpdatesAsync()
        {
            Logger?.Information("Manual update check requested");

            // Reset UI to checking state
            IsChecking = true;
            CanCheckUpdates = false;
            StatusMessage = "Checking for updates...";
            CheckButtonVisibility = Visibility.Collapsed;
            NewVersionVisibility = Visibility.Collapsed;
            DownloadInstallButtonVisibility = Visibility.Collapsed;

            try
            {
                _availableUpdate = await _updateService.CheckForUpdatesAsync();

                // Configure UI based on update check results
                if (_availableUpdate != null)
                {
                    // Update found - prepare for download workflow
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
                    // No update - show current status
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
                // Handle error and enable retry option
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

        /// <summary>
        /// Command to start the download and installation process.
        /// Navigates to the download page with the available update information.
        /// </summary>
        [RelayCommand]
        private void DownloadInstall()
        {
            if (_availableUpdate != null)
            {
                // Log user action for analytics and debugging
                Logger?.LogUserAction("InitiateDownloadInstall", new
                {
                    CurrentVersion = CurrentVersion,
                    NewVersion = _availableUpdate.Version,
                    Channel = UpdateChannel,
                    FileSize = _availableUpdate.FileSize
                });
                Logger?.Information("Starting download and install process for version {Version}", _availableUpdate.Version);

                // Navigate to download page with update information
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
                // Guard against invalid state
                Logger?.Error("DownloadInstall command called with null available update");
            }
        }

        /// <summary>
        /// Command to close the updater application.
        /// Logs user action and closes the main window.
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            // Log user action with context for analytics
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