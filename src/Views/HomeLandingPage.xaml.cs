using Bucket.ViewModels;

namespace Bucket.Views
{
    public sealed partial class HomeLandingPage : Page
    {
        public MainViewModel ViewModel { get; }

        public HomeLandingPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<MainViewModel>();
            DataContext = ViewModel;
        }
    }

}
