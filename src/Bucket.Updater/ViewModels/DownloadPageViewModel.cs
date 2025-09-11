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
        private string headerText = "Downloading Update";

        [ObservableProperty]
        private string updateVersion = string.Empty;

        [ObservableProperty]
        private bool isProgressActive = true;

        [ObservableProperty]
        private double progressPercentage = 0;

        [ObservableProperty]
        private string statusMessage = "Starting download...";

        [ObservableProperty]
        private string downloadedSize = "0 MB";

        [ObservableProperty]
        private string totalSize = "0 MB";

        [ObservableProperty]
        private string downloadSpeed = string.Empty;


        public DownloadPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.LogMethodEntry(nameof(DownloadPageViewModel));
            Logger?.Information("DownloadPageViewModel initialized with UpdateService");
        }

        public async void StartDownload(Bucket.Updater.Models.UpdateInfo updateInfo)
        {
            using (Logger?.BeginOperationScope("DownloadProcess", new { Version = updateInfo?.Version, Size = updateInfo?.FileSize }))
            {
                _updateInfo = updateInfo;
                if (updateInfo != null)
                {
                    UpdateVersion = updateInfo.Version;
                    TotalSize = FormatFileSize(updateInfo.FileSize);

                    Logger?.LogUserAction("StartDownload", new
                    {
                        Version = updateInfo.Version,
                        FileSize = updateInfo.FileSize,
                        FormattedSize = TotalSize,
                        DownloadUrl = updateInfo.DownloadUrl
                    });
                    Logger?.Information("Starting download process for version {Version}, size: {Size}", updateInfo.Version, TotalSize);
                }
                else
                {
                    Logger?.Error("StartDownload called with null UpdateInfo");
                    HandleError("Invalid download information");
                    return;
                }

                await StartDownloadAsync();
            }
        }

        private async Task StartDownloadAsync()
        {
            if (_updateInfo == null)
            {
                Logger?.Error("StartDownloadAsync called with null UpdateInfo");
                return;
            }

            using var performanceScope = PerformanceLogger.BeginMeasurement("Download");

            var downloadContext = new
            {
                Version = _updateInfo.Version,
                FileSize = _updateInfo.FileSize,
                Url = _updateInfo.DownloadUrl
            };

            Logger?.Information("Starting download process {@Context}", downloadContext);

            try
            {
                IsProgressActive = true;
                StatusMessage = "Starting download...";

                _cancellationTokenSource = new CancellationTokenSource();
                _stopwatch.Start();
                _lastProgressUpdate = DateTime.Now;

                var progress = new Progress<(long downloaded, long total)>(OnDownloadProgressChanged);
                _downloadPath = await PerformanceLogger.MeasureAndLogAsync(
                    "UpdateService.DownloadUpdate",
                    () => _updateService.DownloadUpdateAsync(_updateInfo, progress, _cancellationTokenSource.Token));

                var finalSize = File.Exists(_downloadPath) ? new FileInfo(_downloadPath).Length : -1;
                Logger?.LogPerformance("DownloadComplete", _stopwatch.Elapsed, finalSize);
                Logger?.Information("Download completed successfully for version {Version}, final size: {Size} bytes",
                    _updateInfo.Version, finalSize);

                NavigateToInstallPage();
            }
            catch (OperationCanceledException)
            {
                Logger?.LogUserAction("DownloadCancelled", new { Version = _updateInfo.Version });
                Logger?.Information("Download cancelled by user for version {Version}", _updateInfo.Version);
                HandleError("Download was cancelled");
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Download failed for version {Version} {@Context}", _updateInfo.Version, downloadContext);
                HandleError($"Download failed: {ex.Message}");
            }
            finally
            {
                _stopwatch.Stop();
                IsProgressActive = false;
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

        private void NavigateToInstallPage()
        {
            if (_updateInfo != null && !string.IsNullOrEmpty(_downloadPath))
            {
                // Create navigation parameter with download info
                var installInfo = new InstallInfo
                {
                    UpdateInfo = _updateInfo,
                    DownloadPath = _downloadPath
                };

                // Navigate to InstallPage using MainWindow's ContentFrame
                var mainWindow = App.MainWindow as MainWindow;
                var frame = mainWindow?.ContentFrame;
                frame?.Navigate(typeof(InstallPage), installInfo);
            }
        }

        private void HandleError(string message)
        {
            StatusMessage = message;

            // Could add retry logic or error navigation here
            Logger?.Error("Download error: {Message}", message);
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

    // Helper class for navigation parameter
    public class InstallInfo
    {
        public Bucket.Updater.Models.UpdateInfo? UpdateInfo { get; set; }
        public string? DownloadPath { get; set; }
    }
}