namespace Bucket.Updater.Views
{
    public sealed partial class DownloadInstallPage : Page
    {
        public DownloadInstallPageViewModel ViewModel { get; }

        public DownloadInstallPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<DownloadInstallPageViewModel>();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is Bucket.Updater.Models.UpdateInfo updateInfo)
            {
                ViewModel.StartDownloadAndInstall(updateInfo);
            }
        }
    }
}