namespace Bucket.Updater.ViewModels
{
    /// <summary>
    /// ViewModel for the installation page that manages the update installation workflow
    /// using a state machine pattern. Handles installation progress, user feedback, error
    /// recovery, and file cleanup to ensure a smooth installation experience.
    /// </summary>
    public partial class InstallPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private InstallInfo? _installInfo;

        // State management
        /// <summary>
        /// Represents the current state of the installation process
        /// </summary>
        private enum InstallState
        {
            /// <summary>Installation is currently in progress</summary>
            Installing,
            /// <summary>Installation completed successfully</summary>
            Completed,
            /// <summary>Installation failed with an error</summary>
            Error
        }
        private InstallState _currentState = InstallState.Installing;

        /// <summary>
        /// Header text displayed at the top of the installation page
        /// Changes based on installation state (Installing/Complete/Failed)
        /// </summary>
        [ObservableProperty]
        private string headerText = "🔄 Installing Update";

        /// <summary>
        /// Version number of the update being installed
        /// Displayed to the user for confirmation and tracking
        /// </summary>
        [ObservableProperty]
        private string updateVersion = string.Empty;

        /// <summary>
        /// Controls whether the progress ring is actively spinning
        /// Set to false when installation completes or fails
        /// </summary>
        [ObservableProperty]
        private bool isProgressActive = true;

        /// <summary>
        /// Controls visibility of the progress ring indicator
        /// Hidden when installation is complete or has failed
        /// </summary>
        [ObservableProperty]
        private Visibility progressRingVisibility = Visibility.Visible;

        /// <summary>
        /// Controls visibility of the success checkmark icon
        /// Shown only when installation completes successfully
        /// </summary>
        [ObservableProperty]
        private Visibility successIconVisibility = Visibility.Collapsed;

        /// <summary>
        /// Controls visibility of the error icon
        /// Shown only when installation fails
        /// </summary>
        [ObservableProperty]
        private Visibility errorIconVisibility = Visibility.Collapsed;

        /// <summary>
        /// Current status message displayed to the user
        /// Updated throughout the installation process with progress information
        /// </summary>
        [ObservableProperty]
        private string statusMessage = "Installing update...";

        /// <summary>
        /// Timestamped log of installation progress and messages
        /// Displayed in a scrollable text area for user visibility
        /// </summary>
        [ObservableProperty]
        private string installationLog = string.Empty;

        /// <summary>
        /// Controls visibility of the installation logs section
        /// Can be hidden to simplify the UI if needed
        /// </summary>
        [ObservableProperty]
        private Visibility logsVisibility = Visibility.Visible;

        /// <summary>
        /// Indicates whether the installation has completed successfully
        /// Used for UI state management and button visibility
        /// </summary>
        [ObservableProperty]
        private bool isCompleted = false;

        /// <summary>
        /// Indicates whether an error occurred during installation
        /// Used to show error-specific UI elements and messages
        /// </summary>
        [ObservableProperty]
        private bool hasError = false;

        /// <summary>
        /// Detailed error message displayed when installation fails
        /// Provides specific information about what went wrong
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        /// <summary>
        /// Controls visibility of the cancel button
        /// Shown during installation to allow user cancellation
        /// </summary>
        [ObservableProperty]
        private Visibility cancelButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Controls visibility of the retry button
        /// Shown only when installation fails to allow retry attempts
        /// </summary>
        [ObservableProperty]
        private Visibility retryButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Controls visibility of the finish button
        /// Shown when installation completes (success or failure) to close the updater
        /// </summary>
        [ObservableProperty]
        private Visibility finishButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Initializes a new instance of the InstallPageViewModel
        /// </summary>
        /// <param name="updateService">Service for handling update installation operations</param>
        public InstallPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.LogMethodEntry(nameof(InstallPageViewModel));
            Logger?.Information("InstallPageViewModel initialized with UpdateService");
        }

        /// <summary>
        /// Initiates the installation process with the provided installation information
        /// Sets up the UI state and begins the asynchronous installation workflow
        /// </summary>
        /// <param name="installInfo">Installation details including update version and file path</param>
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

        /// <summary>
        /// Performs the actual installation process asynchronously
        /// Manages state transitions, progress reporting, and error handling
        /// </summary>
        /// <returns>Task representing the installation operation</returns>
        private async Task StartInstallationAsync()
        {
            using var performanceScope = PerformanceLogger.BeginMeasurement("Installation");

            // Validate installation prerequisites
            if (_installInfo?.DownloadPath == null)
            {
                Logger?.Error("Installation started with null download path");
                HandleError("Installation file not found");
                return;
            }

            // Log installation context for debugging
            var installationContext = new
            {
                Version = _installInfo.UpdateInfo?.Version,
                DownloadPath = _installInfo.DownloadPath,
                FileSize = File.Exists(_installInfo.DownloadPath) ? new FileInfo(_installInfo.DownloadPath).Length : -1
            };

            Logger?.Information("Starting installation process {@Context}", installationContext);

            try
            {
                // Transition to Installing state and update UI
                Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Installing, "User initiated installation");
                _currentState = InstallState.Installing;
                HeaderText = "🔄 Installing Update";
                StatusMessage = "Installing update...";
                IsProgressActive = true;
                ProgressRingVisibility = Visibility.Visible;
                SuccessIconVisibility = Visibility.Collapsed;
                ErrorIconVisibility = Visibility.Collapsed;
                LogsVisibility = Visibility.Visible;

                // Execute installation with progress reporting
                var progress = new Progress<string>(OnInstallationProgress);
                var success = await PerformanceLogger.MeasureAndLogAsync(
                    "UpdateService.InstallUpdate",
                    () => _updateService.InstallUpdateAsync(_installInfo.DownloadPath, progress));

                // Handle installation result
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

        /// <summary>
        /// Handles progress updates from the installation service
        /// Updates the status message and appends progress to the installation log
        /// </summary>
        /// <param name="message">Progress message from the installation service</param>
        private void OnInstallationProgress(string message)
        {
            StatusMessage = message;
            AppendToLog(message);
        }

        /// <summary>
        /// Appends a timestamped message to the installation log
        /// Automatically scrolls to show the latest entry and notifies the UI
        /// </summary>
        /// <param name="message">Message to add to the installation log</param>
        private void AppendToLog(string message)
        {
            // Format message with timestamp for better readability
            var timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var logEntry = $"[{timestamp}] {message}";

            // Build log string efficiently
            if (string.IsNullOrEmpty(InstallationLog))
            {
                InstallationLog = logEntry;
            }
            else
            {
                InstallationLog += Environment.NewLine + logEntry;
            }

            // Notify UI to scroll to the latest entry
            OnLogUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fired when the installation log is updated
        /// Used by the UI to automatically scroll to the latest log entry
        /// </summary>
        public event EventHandler OnLogUpdated;

        /// <summary>
        /// Safely removes downloaded installation files to free disk space
        /// Logs the cleanup reason and handles any cleanup errors gracefully
        /// </summary>
        /// <param name="reason">Reason for the cleanup operation for logging purposes</param>
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
                    // Log cleanup failures but don't fail the installation
                    Logger?.Warning(ex, "Failed to cleanup downloaded files");
                }
            }
            else
            {
                Logger?.Debug("No download path to cleanup for reason: {Reason}", reason);
            }
        }

        /// <summary>
        /// Handles successful completion of the installation process
        /// Transitions to the Completed state and updates the UI to show success
        /// </summary>
        private void HandleInstallationComplete()
        {
            // Transition to completed state
            Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Completed, "Installation successful");
            _currentState = InstallState.Completed;
            
            // Update UI to show success state
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

        /// <summary>
        /// Handles installation errors by transitioning to error state and updating UI
        /// Shows error message, enables retry functionality, and logs the failure
        /// </summary>
        /// <param name="message">User-friendly error message to display</param>
        private void HandleError(string message)
        {
            // Transition to error state
            Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Error, $"Error occurred: {message}");
            _currentState = InstallState.Error;
            
            // Update UI to show error state
            HeaderText = "❌ Installation Failed";
            HasError = true;
            ErrorMessage = message;
            StatusMessage = "Installation failed";
            IsProgressActive = false;
            ProgressRingVisibility = Visibility.Collapsed;
            ErrorIconVisibility = Visibility.Visible;
            RetryButtonVisibility = Visibility.Visible;
            FinishButtonVisibility = Visibility.Visible;

            // Log the error and add to user-visible log
            Logger?.Error("Installation error: {ErrorMessage} for version {Version}",
                message, _installInfo?.UpdateInfo?.Version);
            AppendToLog($"Error: {message}");
        }

        /// <summary>
        /// Command to cancel the installation process
        /// Cleans up downloaded files and either navigates back or closes the application
        /// </summary>
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

            // Clean up any downloaded files to free disk space
            CleanupDownloadedFiles("User cancelled installation");

            // Navigate back or close application as appropriate
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

        /// <summary>
        /// Command to retry a failed installation
        /// Validates the installation file still exists and resets the UI state before retrying
        /// </summary>
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

            // Validate installation file still exists before retrying
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

            // Reset UI state for retry attempt
            Logger?.LogStateTransition("InstallationState", _currentState, InstallState.Installing, "User initiated retry");
            HasError = false;
            ErrorMessage = string.Empty;
            InstallationLog = string.Empty;
            RetryButtonVisibility = Visibility.Collapsed;
            FinishButtonVisibility = Visibility.Collapsed;
            ErrorIconVisibility = Visibility.Collapsed;

            await StartInstallationAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Command to finish the installation process and close the updater
        /// Cleans up any remaining files if installation failed before closing
        /// </summary>
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

            // Clean up files if installation failed (successful installations already cleaned up)
            if (_currentState == InstallState.Error)
            {
                CleanupDownloadedFiles("Finishing after installation error");
            }

            App.MainWindow.Close();
        }

        /// <summary>
        /// Performs cleanup when the ViewModel is being disposed
        /// Only cleans up files if installation was not completed successfully
        /// to prevent memory leaks and free disk space
        /// </summary>
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