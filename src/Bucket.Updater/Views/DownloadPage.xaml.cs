namespace Bucket.Updater.Views
{
    public sealed partial class DownloadPage : Page
    {
        public DownloadPageViewModel ViewModel { get; }

        public DownloadPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<DownloadPageViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Bucket.Updater.Models.UpdateInfo updateInfo)
            {
                ViewModel.SetUpdateInfo(updateInfo);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.Cleanup();
        }
    }
}