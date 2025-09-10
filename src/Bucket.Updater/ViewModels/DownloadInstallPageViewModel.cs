using System.Diagnostics;

namespace Bucket.Updater.ViewModels
{
    public partial class DownloadInstallPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private CancellationTokenSource? _cancellationTokenSource;
        private Bucket.Updater.Models.UpdateInfo? _updateInfo;
        private string? _downloadPath;
        private readonly Stopwatch _stopwatch = new();
        private long _lastBytesReceived = 0;
        private DateTime _lastProgressUpdate = DateTime.Now;

        // State management
        private enum UpdateState
        {
            Downloading,
            Installing,
            Completed,
            Error
        }
        private UpdateState _currentState = UpdateState.Downloading;

        [ObservableProperty]
        private string headerText = "Downloading Update";

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
        private double progressPercentage = 0;

        [ObservableProperty]
        private Visibility progressBarVisibility = Visibility.Visible;

        [ObservableProperty]
        private string statusMessage = "Starting download...";

        [ObservableProperty]
        private string downloadedSize = "0 MB";

        [ObservableProperty]
        private string totalSize = "0 MB";

        [ObservableProperty]
        private string downloadSpeed = string.Empty;

        [ObservableProperty]
        private Visibility downloadInfoVisibility = Visibility.Visible;

        [ObservableProperty]
        private string installationLog = string.Empty;

        [ObservableProperty]
        private Visibility installLogVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool isCompleted = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private Visibility cancelButtonVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility retryButtonVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility finishButtonVisibility = Visibility.Collapsed;

        public DownloadInstallPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.Information("DownloadInstallPageViewModel initialized");
        }

        public async void StartDownloadAndInstall(Bucket.Updater.Models.UpdateInfo updateInfo)
        {
            _updateInfo = updateInfo;
            UpdateVersion = updateInfo.Version;
            TotalSize = FormatFileSize(updateInfo.FileSize);
            
            Logger?.Information("Starting download and install process for version {Version}, size: {Size}", updateInfo.Version, TotalSize);
            await StartDownloadAsync();
        }

        private async Task StartDownloadAsync()
        {
            if (_updateInfo == null) return;

            try
            {
                _currentState = UpdateState.Downloading;
                HeaderText = "Downloading Update";
                IsProgressActive = true;
                ProgressRingVisibility = Visibility.Visible;
                SuccessIconVisibility = Visibility.Collapsed;
                ErrorIconVisibility = Visibility.Collapsed;
                ProgressBarVisibility = Visibility.Visible;
                DownloadInfoVisibility = Visibility.Visible;
                InstallLogVisibility = Visibility.Collapsed;
                StatusMessage = "Starting download...";
                CancelButtonVisibility = Visibility.Visible;
                RetryButtonVisibility = Visibility.Collapsed;
                FinishButtonVisibility = Visibility.Collapsed;

                _cancellationTokenSource = new CancellationTokenSource();
                _stopwatch.Start();

                var progress = new Progress<(long downloaded, long total)>(OnDownloadProgressChanged);
                _downloadPath = await _updateService.DownloadUpdateAsync(_updateInfo, progress, _cancellationTokenSource.Token);

                Logger?.Information("Download completed, starting installation for version {Version}", _updateInfo.Version);
                
                // Immediately start installation after successful download
                await StartInstallationAsync();
            }
            catch (OperationCanceledException)
            {
                Logger?.Information("Download cancelled by user for version {Version}", _updateInfo.Version);
                HandleError("Download was cancelled", true);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Download failed for version {Version}", _updateInfo.Version);
                HandleError($"Download failed: {ex.Message}", true);
            }
            finally
            {
                _stopwatch.Stop();
            }
        }

        private async Task StartInstallationAsync()
        {
            if (_updateInfo == null || string.IsNullOrEmpty(_downloadPath)) return;

            try
            {
                _currentState = UpdateState.Installing;
                HeaderText = "Installing Update";
                StatusMessage = "Installing update...";
                ProgressBarVisibility = Visibility.Collapsed;
                DownloadInfoVisibility = Visibility.Collapsed;
                InstallLogVisibility = Visibility.Visible;
                CancelButtonVisibility = Visibility.Collapsed;

                var progress = new Progress<string>(OnInstallationProgress);
                var success = await _updateService.InstallUpdateAsync(_downloadPath, progress);

                if (success)
                {
                    HandleInstallationComplete();
                }
                else
                {
                    HandleError("Installation failed", false);
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Installation exception for version {Version}", _updateInfo?.Version);
                HandleError($"Installation failed: {ex.Message}", false);
            }
            finally
            {
                // Cleanup downloaded file after installation attempt
                Logger?.Information("Cleaning up downloaded files");
                if (!string.IsNullOrEmpty(_downloadPath))
                {
                    _updateService.CleanupFiles(_downloadPath);
                }
            }
        }

        private void OnDownloadProgressChanged((long downloaded, long total) progress)
        {
            var now = DateTime.Now;
            var percentage = progress.total > 0 ? (double)progress.downloaded / progress.total * 100 : 0;
            
            ProgressPercentage = percentage;
            DownloadedSize = FormatFileSize(progress.downloaded);
            TotalSize = FormatFileSize(progress.total);

            // Calculate download speed
            if ((now - _lastProgressUpdate).TotalMilliseconds >= 1000)
            {
                var bytesDelta = progress.downloaded - _lastBytesReceived;
                var timeDelta = (now - _lastProgressUpdate).TotalSeconds;
                
                if (timeDelta > 0)
                {
                    var speed = bytesDelta / timeDelta;
                    DownloadSpeed = $"{FormatFileSize((long)speed)}/s";
                }

                _lastBytesReceived = progress.downloaded;
                _lastProgressUpdate = now;
            }

            StatusMessage = $"Downloading... {percentage:F1}%";
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

        private void HandleInstallationComplete()
        {
            _currentState = UpdateState.Completed;
            HeaderText = "Installation Complete";
            StatusMessage = "Update has been installed successfully!";
            IsProgressActive = false;
            ProgressRingVisibility = Visibility.Collapsed;
            SuccessIconVisibility = Visibility.Visible;
            IsCompleted = true;
            FinishButtonVisibility = Visibility.Visible;
            AppendToLog("Installation completed successfully");
            
            Logger?.Information("Installation completed successfully for version {Version}", _updateInfo?.Version);
        }

        private void HandleError(string message, bool allowRetry)
        {
            _currentState = UpdateState.Error;
            HasError = true;
            ErrorMessage = message;
            StatusMessage = "Update failed";
            IsProgressActive = false;
            ProgressRingVisibility = Visibility.Collapsed;
            ErrorIconVisibility = Visibility.Visible;
            CancelButtonVisibility = Visibility.Collapsed;
            
            if (allowRetry)
            {
                RetryButtonVisibility = Visibility.Visible;
            }
            else
            {
                FinishButtonVisibility = Visibility.Visible;
            }

            if (_currentState == UpdateState.Installing)
            {
                AppendToLog($"Error: {message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            Logger?.Information("User cancelled update process");
            _cancellationTokenSource?.Cancel();
        }

        [RelayCommand]
        private async Task RetryAsync()
        {
            Logger?.Information("Retrying update process for version {Version}", _updateInfo?.Version);
            
            // Reset state
            HasError = false;
            ErrorMessage = string.Empty;
            ProgressPercentage = 0;
            _lastBytesReceived = 0;
            _stopwatch.Reset();
            InstallationLog = string.Empty;
            
            await StartDownloadAsync();
        }

        [RelayCommand]
        private void Finish()
        {
            Logger?.Information("Update process completed, closing updater");
            App.MainWindow.Close();
        }

        public void Cleanup()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _stopwatch.Stop();
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
    }
}