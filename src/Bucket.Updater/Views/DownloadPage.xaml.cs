namespace Bucket.Updater.Views
{
    /// <summary>
    /// Page for displaying download progress and managing download operations
    /// </summary>
    public sealed partial class DownloadPage : Page
    {
        /// <summary>
        /// ViewModel for managing download operations and progress tracking
        /// </summary>
        public DownloadPageViewModel ViewModel { get; }

        /// <summary>
        /// Initializes the download page with dependency injection
        /// </summary>
        public DownloadPage()
        {
            this.InitializeComponent();
            
            // Get ViewModel through dependency injection
            ViewModel = App.GetService<DownloadPageViewModel>();
            DataContext = ViewModel;
        }

        /// <summary>
        /// Handles navigation to this page and starts download if UpdateInfo is provided
        /// </summary>
        /// <param name="e">Navigation event arguments containing optional UpdateInfo parameter</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Start download if UpdateInfo parameter was passed during navigation
            if (e.Parameter is Bucket.Updater.Models.UpdateInfo updateInfo)
            {
                ViewModel.StartDownload(updateInfo);
            }
        }

        /// <summary>
        /// Handles navigation away from this page and performs cleanup
        /// </summary>
        /// <param name="e">Navigation event arguments</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Cleanup resources to prevent memory leaks
            ViewModel.Cleanup();
        }
    }
}