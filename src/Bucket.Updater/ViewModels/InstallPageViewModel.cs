namespace Bucket.Updater.ViewModels
{
    public partial class InstallPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private InstallInfo? _installInfo;

        // State management
        private enum InstallState
        {
            Installing,
            Completed,
            Error
        }
        private InstallState _currentState = InstallState.Installing;

        [ObservableProperty]
        private string headerText = "🔄 Installing Update";

        [ObservableProperty]
        private string updateVersion = string.Empty;

        [ObservableProperty]
        private bool isProgressActive = true;

        [ObservableProperty]
        private Visibility progressRingVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility successIconVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility errorIconVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string statusMessage = "Installing update...";

        [ObservableProperty]
        private string installationLog = string.Empty;

        [ObservableProperty]
        private Visibility logsVisibility = Visibility.Visible;

        [ObservableProperty]
        private bool isCompleted = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private Visibility cancelButtonVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility retryButtonVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility finishButtonVisibility = Visibility.Collapsed;

        public InstallPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.Information("InstallPageViewModel initialized");
        }

        public async void StartInstallation(InstallInfo installInfo)
        {
            _installInfo = installInfo;
            if (_installInfo?.UpdateInfo != null)
            {
                UpdateVersion = _installInfo.UpdateInfo.Version;
                Logger?.Information("Starting installation for version {Version}", _installInfo.UpdateInfo.Version);
            }

            await StartInstallationAsync();
        }

        private async Task StartInstallationAsync()
        {
            if (_installInfo?.DownloadPath == null)
            {
                HandleError("Installation file not found");
                return;
            }

            try
            {
                _currentState = InstallState.Installing;
                HeaderText = "🔄 Installing Update";
                StatusMessage = "Installing update...";
                IsProgressActive = true;
                ProgressRingVisibility = Visibility.Visible;
                SuccessIconVisibility = Visibility.Collapsed;
                ErrorIconVisibility = Visibility.Collapsed;
                LogsVisibility = Visibility.Visible;

                var progress = new Progress<string>(OnInstallationProgress);
                var success = await _updateService.InstallUpdateAsync(_installInfo.DownloadPath, progress);

                if (success)
                {
                    HandleInstallationComplete();

                    // Cleanup downloaded file only after successful installation
                    CleanupDownloadedFiles("Installation completed successfully");
                }
                else
                {
                    HandleError("Installation failed");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Installation exception for version {Version}", _installInfo?.UpdateInfo?.Version);
                HandleError($"Installation failed: {ex.Message}");
            }
        }

        private void OnInstallationProgress(string message)
        {
            StatusMessage = message;
            AppendToLog(message);
        }

        private void AppendToLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var logEntry = $"[{timestamp}] {message}";

            if (string.IsNullOrEmpty(InstallationLog))
            {
                InstallationLog = logEntry;
            }
            else
            {
                InstallationLog += Environment.NewLine + logEntry;
            }

            // Trigger scroll to bottom if needed
            OnLogUpdated?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnLogUpdated;

        private void CleanupDownloadedFiles(string reason)
        {
            if (!string.IsNullOrEmpty(_installInfo?.DownloadPath))
            {
                Logger?.Information("Cleaning up downloaded files: {Reason}", reason);
                try
                {
                    _updateService?.CleanupFiles(_installInfo.DownloadPath);
                }
                catch (Exception ex)
                {
                    Logger?.Warning(ex, "Failed to cleanup downloaded files");
                }
            }
            else
            {
                Logger?.Debug("No download path to cleanup for reason: {Reason}", reason);
            }
        }

        private void HandleInstallationComplete()
        {
            _currentState = InstallState.Completed;
            HeaderText = "✅ Installation Complete";
            StatusMessage = "Update has been installed successfully!";
            IsProgressActive = false;
            ProgressRingVisibility = Visibility.Collapsed;
            SuccessIconVisibility = Visibility.Visible;
            IsCompleted = true;
            FinishButtonVisibility = Visibility.Visible;
            AppendToLog("Installation completed successfully");

            Logger?.Information("Installation completed successfully for version {Version}", _installInfo?.UpdateInfo?.Version);
        }

        private void HandleError(string message)
        {
            _currentState = InstallState.Error;
            HeaderText = "❌ Installation Failed";
            HasError = true;
            ErrorMessage = message;
            StatusMessage = "Installation failed";
            IsProgressActive = false;
            ProgressRingVisibility = Visibility.Collapsed;
            ErrorIconVisibility = Visibility.Visible;
            RetryButtonVisibility = Visibility.Visible;
            FinishButtonVisibility = Visibility.Visible;

            AppendToLog($"Error: {message}");
        }

        [RelayCommand]
        private void Cancel()
        {
            Logger?.Information("User cancelled installation process");

            // Cleanup downloaded files when cancelling, especially after an error
            CleanupDownloadedFiles("User cancelled installation");

            // Navigate back or close
            var frame = App.MainWindow.Content as Frame;
            if (frame?.CanGoBack == true)
            {
                frame.GoBack();
            }
            else
            {
                App.MainWindow.Close();
            }
        }

        [RelayCommand]
        private async Task RetryAsync()
        {
            if (_installInfo?.UpdateInfo == null) return;

            Logger?.Information("Retrying installation for version {Version}", _installInfo.UpdateInfo.Version);

            // Check if installation file still exists
            if (string.IsNullOrEmpty(_installInfo.DownloadPath) || !File.Exists(_installInfo.DownloadPath))
            {
                Logger?.Warning("Installation file not found, redirecting to download page");

                // Navigate on UI thread using DispatcherQueue
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    var frame = App.MainWindow.Content as Frame;
                    frame?.Navigate(typeof(DownloadPage), _installInfo.UpdateInfo);
                });
                return;
            }

            // Reset error state
            HasError = false;
            ErrorMessage = string.Empty;
            InstallationLog = string.Empty;
            RetryButtonVisibility = Visibility.Collapsed;
            FinishButtonVisibility = Visibility.Collapsed;
            ErrorIconVisibility = Visibility.Collapsed;

            // Restart the installation process directly
            await StartInstallationAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private void Finish()
        {
            Logger?.Information("Installation process completed, closing updater");

            // Cleanup files if installation failed and we're finishing
            if (_currentState == InstallState.Error)
            {
                CleanupDownloadedFiles("Finishing after installation error");
            }

            App.MainWindow.Close();
        }

        public void Cleanup()
        {
            // Only cleanup downloaded files if installation failed or was cancelled
            // If installation succeeded, files were already cleaned up in HandleInstallationComplete
            if (_currentState == InstallState.Error || _currentState == InstallState.Installing)
            {
                CleanupDownloadedFiles("ViewModel cleanup - installation not completed successfully");
                Logger?.Information("Cleaned up files for incomplete installation (state: {State})", _currentState);
            }
            else if (_currentState == InstallState.Completed)
            {
                Logger?.Information("Skipping cleanup for completed installation - files already cleaned");
            }
        }
    }
}