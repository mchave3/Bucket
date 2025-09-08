namespace Bucket.Updater.ViewModels
{
    public partial class ChangelogPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string updateVersion = string.Empty;

        [ObservableProperty]
        private string releaseDate = string.Empty;

        [ObservableProperty]
        private string fileSize = string.Empty;

        [ObservableProperty]
        private string changelogText = string.Empty;

        [ObservableProperty]
        private string updateChannel = string.Empty;

        [ObservableProperty]
        private string architecture = string.Empty;

        [ObservableProperty]
        private string releaseType = string.Empty;

        private Bucket.Updater.Models.UpdateInfo? _updateInfo;

        public void SetUpdateInfo(Bucket.Updater.Models.UpdateInfo updateInfo)
        {
            _updateInfo = updateInfo;
            UpdateVersion = updateInfo.Version;
            ReleaseDate = updateInfo.PublishedAt.ToString("MMMM dd, yyyy");
            FileSize = FormatFileSize(updateInfo.FileSize);
            ChangelogText = string.IsNullOrWhiteSpace(updateInfo.Body) 
                ? "No release notes available." 
                : updateInfo.Body;
            UpdateChannel = updateInfo.Channel.ToString();
            Architecture = updateInfo.Architecture.ToString();
            ReleaseType = updateInfo.IsPrerelease ? "Pre-release" : "Stable Release";
            
            Logger?.Information("ChangelogPageViewModel initialized with update info for version {Version} ({Channel}, {Architecture})", 
                updateInfo.Version, updateInfo.Channel, updateInfo.Architecture);
        }

        [RelayCommand]
        private void Back()
        {
            var mainWindow = App.MainWindow as MainWindow;
            var frame = mainWindow?.ContentFrame;
            frame?.GoBack();
        }

        [RelayCommand]
        private void Download()
        {
            if (_updateInfo != null)
            {
                Logger?.Information("Starting download for update version {Version}", _updateInfo.Version);
                var mainWindow = App.MainWindow as MainWindow;
                var frame = mainWindow?.ContentFrame;
                frame?.Navigate(typeof(DownloadPage), _updateInfo);
            }
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