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
            Logger?.LogMethodEntry(nameof(InstallPageViewModel));
            Logger?.Information("InstallPageViewModel initialized with UpdateService");
        }

        public async void StartInstallation(InstallInfo installInfo)
        {
            using (Logger?.BeginOperationScope("InstallationProcess", new { Version = installInfo?.UpdateInfo?.Version }))
            {
                _installInfo = installInfo;
                if (_installInfo?.UpdateInfo != null)
                {
                    UpdateVersion = _installInfo.UpdateInfo.Version;
                    Logger?.LogUserAction("StartInstallation", new { Version = _installInfo.UpdateInfo.Version });
                    Logger?.Information("Starting installation for version {Version}", _installInfo.UpdateInfo.Version);
                }
                else
                {
                    Logger?.Error("StartInstallation called with null UpdateInfo");
                    HandleError("Invalid installation information");
                    return;
                }

                await StartInstallationAsync();
            }
        }

        private async Task StartInstallationAsync()
        {
            using var performanceScope = PerformanceLogger.BeginMeasurement("Installation");
            
            if (_installInfo?.DownloadPath == null)
            {
                Logger?.Error("Installation started with null download path");
                HandleError("Installation file not found");
                return;
            }

            var installationContext = new
            {
                Version = _installInfo.UpdateInfo?.Version,
                DownloadPath = _installInfo.DownloadPath,
                FileSize = File.Exists(_installInfo.DownloadPath) ? new FileInfo(_installInfo.DownloadPath).Length : -1
            };

            Logger?.Information("Starting installation process {@Context}", installationContext);

            try
            {
                Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Installing, "User initiated installation");
                _currentState = InstallState.Installing;
                HeaderText = "🔄 Installing Update";
                StatusMessage = "Installing update...";
                IsProgressActive = true;
                ProgressRingVisibility = Visibility.Visible;
                SuccessIconVisibility = Visibility.Collapsed;
                ErrorIconVisibility = Visibility.Collapsed;
                LogsVisibility = Visibility.Visible;

                var progress = new Progress<string>(OnInstallationProgress);
                var success = await PerformanceLogger.MeasureAndLogAsync(
                    "UpdateService.InstallUpdate", 
                    () => _updateService.InstallUpdateAsync(_installInfo.DownloadPath, progress));

                if (success)
                {
                    HandleInstallationComplete();
                    CleanupDownloadedFiles("Installation completed successfully");
                }
                else
                {
                    Logger?.Warning("Installation returned false for version {Version}", _installInfo.UpdateInfo?.Version);
                    HandleError("Installation failed");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Installation exception for version {Version} {@Context}", 
                    _installInfo?.UpdateInfo?.Version, installationContext);
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
            Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Completed, "Installation successful");
            _currentState = InstallState.Completed;
            HeaderText = "✅ Installation Complete";
            StatusMessage = "Update has been installed successfully!";
            IsProgressActive = false;
            ProgressRingVisibility = Visibility.Collapsed;
            SuccessIconVisibility = Visibility.Visible;
            IsCompleted = true;
            FinishButtonVisibility = Visibility.Visible;

            Logger?.LogUserAction("InstallationCompleted", new { Version = _installInfo?.UpdateInfo?.Version });
            Logger?.Information("Installation completed successfully for version {Version}", _installInfo?.UpdateInfo?.Version);
        }

        private void HandleError(string message)
        {
            Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Error, $"Error occurred: {message}");
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

            Logger?.Error("Installation error: {ErrorMessage} for version {Version}", 
                message, _installInfo?.UpdateInfo?.Version);
            AppendToLog($"Error: {message}");
        }

        [RelayCommand]
        private void Cancel()
        {
            Logger?.LogUserAction("CancelInstallation", new 
            { 
                Version = _installInfo?.UpdateInfo?.Version,
                State = _currentState.ToString(),
                HasError = HasError
            });
            Logger?.Information("User cancelled installation process in state {State}", _currentState);

            CleanupDownloadedFiles("User cancelled installation");

            var frame = App.MainWindow.Content as Frame;
            if (frame?.CanGoBack == true)
            {
                frame.GoBack();
                Logger?.Debug("Navigated back from installation page");
            }
            else
            {
                Logger?.Debug("Closing application from installation page");
                App.MainWindow.Close();
            }
        }

        [RelayCommand]
        private async Task RetryAsync()
        {
            if (_installInfo?.UpdateInfo == null) 
            {
                Logger?.Error("Retry attempted with null UpdateInfo");
                return;
            }

            Logger?.LogUserAction("RetryInstallation", new { Version = _installInfo.UpdateInfo.Version });
            Logger?.Information("Retrying installation for version {Version}", _installInfo.UpdateInfo.Version);

            // Check if installation file still exists
            if (string.IsNullOrEmpty(_installInfo.DownloadPath) || !File.Exists(_installInfo.DownloadPath))
            {
                Logger?.Warning("Installation file not found at path: {DownloadPath}", _installInfo.DownloadPath ?? "null");
                HandleError("Installation file (.msi) not found. Please download the update again.");
                return;
            }

            // Log file information for diagnostics
            if (!string.IsNullOrEmpty(_installInfo.DownloadPath))
            {
                var fileInfo = new FileInfo(_installInfo.DownloadPath);
                Logger?.Debug("Retrying with file: {Path}, Size: {Size} bytes, LastWrite: {LastWrite}",
                    _installInfo.DownloadPath, fileInfo.Length, fileInfo.LastWriteTime);
            }

            // Reset error state
            Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Installing, "User initiated retry");
            HasError = false;
            ErrorMessage = string.Empty;
            InstallationLog = string.Empty;
            RetryButtonVisibility = Visibility.Collapsed;
            FinishButtonVisibility = Visibility.Collapsed;
            ErrorIconVisibility = Visibility.Collapsed;

            await StartInstallationAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private void Finish()
        {
            Logger?.LogUserAction("FinishInstallation", new 
            { 
                Version = _installInfo?.UpdateInfo?.Version,
                State = _currentState.ToString(),
                Success = _currentState == InstallState.Completed
            });
            Logger?.Information("Installation process finished in state {State}, closing updater", _currentState);

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