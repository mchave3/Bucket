namespace Bucket.Updater.Views
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePageViewModel ViewModel { get; }

        public WelcomePage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<WelcomePageViewModel>();
        }
    }
}