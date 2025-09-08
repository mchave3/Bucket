namespace Bucket.Updater.ViewModels
{
    public partial class InstallPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private Bucket.Updater.Models.UpdateInfo? _updateInfo;
        private string? _downloadPath;

        [ObservableProperty]
        private string updateVersion = string.Empty;

        [ObservableProperty]
        private bool isInstalling = false;

        [ObservableProperty]
        private bool isCompleted = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string statusMessage = "Preparing installation...";

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string installationLog = string.Empty;

        [ObservableProperty]
        private bool showDetails = false;

        [ObservableProperty]
        private bool showRestartWarning = false;

        [ObservableProperty]
        private bool canGoBack = false;

        [ObservableProperty]
        private bool showRetryButton = false;

        [ObservableProperty]
        private bool showFinishButton = false;

        public InstallPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.Information("InstallPageViewModel initialized");
        }

        public async void SetInstallationInfo(Bucket.Updater.Models.UpdateInfo updateInfo, string downloadPath)
        {
            _updateInfo = updateInfo;
            _downloadPath = downloadPath;
            UpdateVersion = updateInfo.Version;
            
            Logger?.Information("Starting installation for version {Version} from path {Path}", updateInfo.Version, downloadPath);
            await StartInstallationAsync();
        }

        private async Task StartInstallationAsync()
        {
            if (_updateInfo == null || string.IsNullOrEmpty(_downloadPath)) return;

            try
            {
                IsInstalling = true;
                ShowDetails = true;
                StatusMessage = "Installing update...";
                
                var progress = new Progress<string>(OnInstallationProgress);
                var success = await _updateService.InstallUpdateAsync(_downloadPath, progress);

                IsInstalling = false;
                
                if (success)
                {
                    IsCompleted = true;
                    ShowFinishButton = true;
                    StatusMessage = "Installation completed successfully!";
                    AppendToLog("Installation completed successfully");
                    Logger?.Information("Installation completed successfully for version {Version}", _updateInfo.Version);
                    
                    // Check if restart is needed based on exit code
                    if (InstallationLog.Contains("restart required") || InstallationLog.Contains("3010"))
                    {
                        ShowRestartWarning = true;
                        Logger?.Information("Installation requires restart");
                    }
                }
                else
                {
                    Logger?.Error("Installation failed for version {Version}", _updateInfo.Version);
                    HandleInstallationError("Installation failed");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Installation exception for version {Version}", _updateInfo?.Version);
                HandleInstallationError(ex.Message);
            }
            finally
            {
                // Cleanup downloaded file after installation attempt
                Logger?.Information("Cleaning up downloaded files");
                _updateService.CleanupFiles(_downloadPath ?? string.Empty);
            }
        }

        private void OnInstallationProgress(string message)
        {
            StatusMessage = message;
            AppendToLog(message);
        }

        private void AppendToLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {message}";
            
            if (string.IsNullOrEmpty(InstallationLog))
            {
                InstallationLog = logEntry;
            }
            else
            {
                InstallationLog += Environment.NewLine + logEntry;
            }
        }

        private void HandleInstallationError(string errorMessage)
        {
            IsInstalling = false;
            HasError = true;
            ShowRetryButton = true;
            CanGoBack = true;
            StatusMessage = "Installation failed";
            ErrorMessage = errorMessage;
            AppendToLog($"Error: {errorMessage}");
        }

        [RelayCommand]
        private void Back()
        {
            var mainWindow = App.MainWindow as MainWindow;
            var frame = mainWindow?.ContentFrame;
            frame?.GoBack();
        }

        [RelayCommand]
        private async Task RetryAsync()
        {
            Logger?.Information("Retrying installation for version {Version}", _updateInfo?.Version);
            HasError = false;
            ShowRetryButton = false;
            CanGoBack = false;
            ErrorMessage = string.Empty;
            InstallationLog = string.Empty;
            ShowRestartWarning = false;
            
            await StartInstallationAsync();
        }

        [RelayCommand]
        private void Finish()
        {
            Logger?.Information("Installation wizard completed, closing updater");
            // Close the updater application
            App.Current.Exit();
        }
    }
}