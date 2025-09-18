using System.Diagnostics;

namespace Bucket.Updater.ViewModels
{
    /// <summary>
    /// ViewModel for managing download operations and progress tracking.
    /// Orchestrates the download process, calculates progress metrics, and handles navigation to the installation page.
    /// </summary>
    public partial class DownloadPageViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private CancellationTokenSource? _cancellationTokenSource;
        private Bucket.Updater.Models.UpdateInfo? _updateInfo;
        private string? _downloadPath;
        private readonly Stopwatch _stopwatch = new();
        private long _lastBytesReceived = 0;
        private DateTime _lastProgressUpdate = DateTime.Now;

        /// <summary>
        /// Header text displayed at the top of the download page
        /// </summary>
        [ObservableProperty]
        private string headerText = "Downloading Update";

        /// <summary>
        /// Version string of the update being downloaded
        /// </summary>
        [ObservableProperty]
        private string updateVersion = string.Empty;

        /// <summary>
        /// Indicates whether the progress bar should be active/animated
        /// </summary>
        [ObservableProperty]
        private bool isProgressActive = true;

        /// <summary>
        /// Current download progress as a percentage (0-100)
        /// </summary>
        [ObservableProperty]
        private double progressPercentage = 0;

        /// <summary>
        /// Current status message displayed to the user
        /// </summary>
        [ObservableProperty]
        private string statusMessage = "Starting download...";

        /// <summary>
        /// Human-readable string showing the amount of data downloaded so far
        /// </summary>
        [ObservableProperty]
        private string downloadedSize = "0 MB";

        /// <summary>
        /// Human-readable string showing the total size of the file being downloaded
        /// </summary>
        [ObservableProperty]
        private string totalSize = "0 MB";

        /// <summary>
        /// Current download speed displayed as a human-readable string (e.g., "2.5 MB/s")
        /// </summary>
        [ObservableProperty]
        private string downloadSpeed = string.Empty;


        /// <summary>
        /// Initializes a new instance of the DownloadPageViewModel
        /// </summary>
        /// <param name="updateService">Service for handling update downloads</param>
        public DownloadPageViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            Logger?.LogMethodEntry(nameof(DownloadPageViewModel));
            Logger?.Information("DownloadPageViewModel initialized with UpdateService");
        }

        /// <summary>
        /// Initiates the download process for the specified update
        /// </summary>
        /// <param name="updateInfo">Information about the update to download</param>
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

        /// <summary>
        /// Performs the actual download operation asynchronously with progress tracking and error handling
        /// </summary>
        /// <returns>Task representing the download operation</returns>
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
                // Initialize progress tracking and UI state
                IsProgressActive = true;
                StatusMessage = "Starting download...";

                // Set up cancellation support and performance monitoring
                _cancellationTokenSource = new CancellationTokenSource();
                _stopwatch.Start();
                _lastProgressUpdate = DateTime.Now;

                // Create progress reporter for real-time UI updates
                var progress = new Progress<(long downloaded, long total)>(OnDownloadProgressChanged);
                
                // Execute download with performance monitoring
                _downloadPath = await PerformanceLogger.MeasureAndLogAsync(
                    "UpdateService.DownloadUpdate",
                    () => _updateService.DownloadUpdateAsync(_updateInfo, progress, _cancellationTokenSource.Token));

                // Verify download completion and log performance metrics
                var finalSize = File.Exists(_downloadPath) ? new FileInfo(_downloadPath).Length : -1;
                Logger?.LogPerformance("DownloadComplete", _stopwatch.Elapsed, finalSize);
                Logger?.Information("Download completed successfully for version {Version}, final size: {Size} bytes",
                    _updateInfo.Version, finalSize);

                // Navigate to installation page with download results
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
                // Ensure resources are cleaned up regardless of success or failure
                _stopwatch.Stop();
                IsProgressActive = false;
            }
        }

        /// <summary>
        /// Handles download progress updates and calculates download speed metrics
        /// Updates UI elements with current progress, file sizes, and transfer rate
        /// </summary>
        /// <param name="progress">Tuple containing downloaded bytes and total file size</param>
        private void OnDownloadProgressChanged((long downloaded, long total) progress)
        {
            var now = DateTime.Now;
            
            // Calculate completion percentage, avoiding division by zero
            var percentage = progress.total > 0 ? (double)progress.downloaded / progress.total * 100 : 0;

            // Update UI with current progress and formatted file sizes
            ProgressPercentage = percentage;
            DownloadedSize = FormatFileSize(progress.downloaded);
            TotalSize = FormatFileSize(progress.total);

            // Calculate download speed (update every second to avoid UI flicker)
            if ((now - _lastProgressUpdate).TotalMilliseconds >= 1000)
            {
                // Calculate bytes transferred since last update
                var bytesDelta = progress.downloaded - _lastBytesReceived;
                var timeDelta = (now - _lastProgressUpdate).TotalSeconds;

                // Calculate and display transfer rate
                if (timeDelta > 0)
                {
                    var speed = bytesDelta / timeDelta;
                    DownloadSpeed = $"{FormatFileSize((long)speed)}/s";
                }

                // Update tracking variables for next calculation
                _lastBytesReceived = progress.downloaded;
                _lastProgressUpdate = now;
            }

            StatusMessage = $"Downloading... {percentage:F1}%";
        }

        /// <summary>
        /// Navigates to the installation page with the downloaded update information
        /// Creates an InstallInfo object containing update metadata and file path
        /// </summary>
        private void NavigateToInstallPage()
        {
            if (_updateInfo != null && !string.IsNullOrEmpty(_downloadPath))
            {
                // Create navigation parameter with download results
                var installInfo = new InstallInfo
                {
                    UpdateInfo = _updateInfo,
                    DownloadPath = _downloadPath
                };

                // Navigate to InstallPage using MainWindow's ContentFrame
                // This passes both update metadata and the local file path to the installer
                var mainWindow = App.MainWindow as MainWindow;
                var frame = mainWindow?.ContentFrame;
                frame?.Navigate(typeof(InstallPage), installInfo);
            }
        }

        /// <summary>
        /// Handles download errors by updating the UI and logging the error
        /// </summary>
        /// <param name="message">Error message to display to the user</param>
        private void HandleError(string message)
        {
            StatusMessage = message;

            // Log error for debugging - could add retry logic or error navigation here
            Logger?.Error("Download error: {Message}", message);
        }


        /// <summary>
        /// Cleans up resources and cancels any ongoing download operation
        /// Should be called when the view model is being disposed to prevent memory leaks
        /// </summary>
        public void Cleanup()
        {
            // Cancel ongoing download operation if running
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            
            // Stop performance monitoring
            _stopwatch.Stop();
        }

        /// <summary>
        /// Formats a byte count into a human-readable file size string
        /// </summary>
        /// <param name="bytes">Number of bytes to format</param>
        /// <returns>Formatted string with appropriate unit (B, KB, MB, GB, TB)</returns>
        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";

            // Available size units from smallest to largest
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            // Convert to the largest appropriate unit
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            // Format with up to 2 decimal places and appropriate unit
            return $"{size:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Navigation parameter class that carries download results to the installation page
    /// Contains both update metadata and the local path to the downloaded file
    /// </summary>
    public class InstallInfo
    {
        /// <summary>
        /// Information about the update being installed
        /// </summary>
        public Bucket.Updater.Models.UpdateInfo? UpdateInfo { get; set; }
        
        /// <summary>
        /// Local file system path to the downloaded update file
        /// </summary>
        public string? DownloadPath { get; set; }
    }
}