namespace Bucket.Updater.Views
{
    public sealed partial class ChangelogPage : Page
    {
        public ChangelogPageViewModel ViewModel { get; }

        public ChangelogPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<ChangelogPageViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Bucket.Updater.Models.UpdateInfo updateInfo)
            {
                ViewModel.SetUpdateInfo(updateInfo);
            }
        }
    }
}