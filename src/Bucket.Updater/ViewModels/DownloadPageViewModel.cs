using System.Diagnostics;

namespace Bucket.Updater.ViewModels
{
    public partial class DownloadPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private CancellationTokenSource? _cancellationTokenSource;
        private Bucket.Updater.Models.UpdateInfo? _updateInfo;
        private string? _downloadPath;
        private readonly Stopwatch _stopwatch = new();
        private long _lastBytesReceived = 0;
        private DateTime _lastProgressUpdate = DateTime.Now;

        [ObservableProperty]
        private string updateVersion = string.Empty;

        [ObservableProperty]
        private bool isDownloading = false;

        [ObservableProperty]
        private bool isCompleted = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private bool isPaused = false;

        [ObservableProperty]
        private double progressPercentage = 0;

        [ObservableProperty]
        private string statusMessage = "Preparing download...";

        [ObservableProperty]
        private string downloadedSize = "0 MB";

        [ObservableProperty]
        private string totalSize = "0 MB";

        [ObservableProperty]
        private string downloadSpeed = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool showProgressDetails = false;

        [ObservableProperty]
        private bool canGoBack = false;

        [ObservableProperty]
        private bool showCancelButton = false;

        [ObservableProperty]
        private bool showRetryButton = false;

        [ObservableProperty]
        private bool showInstallButton = false;

        public DownloadPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.Information("DownloadPageViewModel initialized");
        }

        public async void SetUpdateInfo(Bucket.Updater.Models.UpdateInfo updateInfo)
        {
            _updateInfo = updateInfo;
            UpdateVersion = updateInfo.Version;
            TotalSize = FormatFileSize(updateInfo.FileSize);
            
            Logger?.Information("Starting download for version {Version}, size: {Size}", updateInfo.Version, TotalSize);
            await StartDownloadAsync();
        }

        private async Task StartDownloadAsync()
        {
            if (_updateInfo == null) return;

            try
            {
                IsDownloading = true;
                ShowCancelButton = true;
                ShowProgressDetails = true;
                StatusMessage = "Starting download...";
                _cancellationTokenSource = new CancellationTokenSource();
                _stopwatch.Start();

                var progress = new Progress<(long downloaded, long total)>(OnProgressChanged);
                _downloadPath = await _updateService.DownloadUpdateAsync(_updateInfo, progress, _cancellationTokenSource.Token);

                IsDownloading = false;
                IsCompleted = true;
                ShowCancelButton = false;
                ShowInstallButton = true;
                CanGoBack = true;
                StatusMessage = "Download completed successfully!";
                ProgressPercentage = 100;
                DownloadSpeed = string.Empty;
                Logger?.Information("Download completed successfully for version {Version}", _updateInfo.Version);
            }
            catch (OperationCanceledException)
            {
                Logger?.Information("Download cancelled by user for version {Version}", _updateInfo.Version);
                HandleDownloadCancelled();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Download failed for version {Version}", _updateInfo.Version);
                HandleDownloadError(ex.Message);
            }
            finally
            {
                _stopwatch.Stop();
            }
        }

        private void OnProgressChanged((long downloaded, long total) progress)
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

        private void HandleDownloadCancelled()
        {
            IsDownloading = false;
            ShowCancelButton = false;
            ShowRetryButton = true;
            CanGoBack = true;
            StatusMessage = "Download cancelled";
            DownloadSpeed = string.Empty;
        }

        private void HandleDownloadError(string errorMessage)
        {
            IsDownloading = false;
            HasError = true;
            ShowCancelButton = false;
            ShowRetryButton = true;
            CanGoBack = true;
            StatusMessage = "Download failed";
            ErrorMessage = errorMessage;
            DownloadSpeed = string.Empty;
        }

        [RelayCommand]
        private void Back()
        {
            var mainWindow = App.MainWindow as MainWindow;
            var frame = mainWindow?.ContentFrame;
            frame?.GoBack();
        }

        [RelayCommand]
        private void Cancel()
        {
            Logger?.Information("User cancelled download");
            _cancellationTokenSource?.Cancel();
        }

        [RelayCommand]
        private async Task RetryAsync()
        {
            Logger?.Information("Retrying download for version {Version}", _updateInfo?.Version);
            HasError = false;
            ShowRetryButton = false;
            CanGoBack = false;
            ErrorMessage = string.Empty;
            ProgressPercentage = 0;
            _lastBytesReceived = 0;
            _stopwatch.Reset();
            
            await StartDownloadAsync();
        }

        [RelayCommand]
        private void Install()
        {
            if (_updateInfo != null && !string.IsNullOrEmpty(_downloadPath))
            {
                Logger?.Information("Navigating to install page for version {Version}", _updateInfo.Version);
                var mainWindow = App.MainWindow as MainWindow;
                var frame = mainWindow?.ContentFrame;
                frame?.Navigate(typeof(InstallPage), new { UpdateInfo = _updateInfo, DownloadPath = _downloadPath });
            }
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