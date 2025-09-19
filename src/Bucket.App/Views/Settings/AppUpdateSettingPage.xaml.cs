namespace Bucket.App.Views
{
    public sealed partial class AppUpdateSettingPage : Page
    {
        public AppUpdateSettingViewModel ViewModel { get; }

        public AppUpdateSettingPage()
        {
            ViewModel = App.GetService<AppUpdateSettingViewModel>();
            this.InitializeComponent();
            this.Loaded += AppUpdateSettingPage_Loaded;
        }

        private async void AppUpdateSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Automatically check for updates when the page is loaded
            await ViewModel.InitializeAsync();
        }
    }


}
