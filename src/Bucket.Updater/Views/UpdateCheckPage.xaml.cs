namespace Bucket.Updater.Views
{
    public sealed partial class UpdateCheckPage : Page
    {
        public UpdateCheckPageViewModel ViewModel { get; }

        public UpdateCheckPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<UpdateCheckPageViewModel>();
            DataContext = ViewModel;
        }
    }
}