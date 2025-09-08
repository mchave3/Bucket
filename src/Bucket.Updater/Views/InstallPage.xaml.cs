namespace Bucket.Updater.Views
{
    public sealed partial class InstallPage : Page
    {
        public InstallPageViewModel ViewModel { get; }

        public InstallPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<InstallPageViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is object param)
            {
                // Handle anonymous type parameter
                var updateInfoProperty = param.GetType().GetProperty("UpdateInfo");
                var downloadPathProperty = param.GetType().GetProperty("DownloadPath");
                
                if (updateInfoProperty?.GetValue(param) is Bucket.Updater.Models.UpdateInfo updateInfo && 
                    downloadPathProperty?.GetValue(param) is string downloadPath)
                {
                    ViewModel.SetInstallationInfo(updateInfo, downloadPath);
                }
            }
        }
    }
}