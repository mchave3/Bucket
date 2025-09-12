namespace Bucket.Updater.Views
{
    /// <summary>
    /// Page for checking and discovering available updates
    /// </summary>
    public sealed partial class UpdateCheckPage : Page
    {
        /// <summary>
        /// ViewModel for managing update check functionality
        /// </summary>
        public UpdateCheckPageViewModel ViewModel { get; }

        /// <summary>
        /// Initializes the update check page with dependency injection
        /// </summary>
        public UpdateCheckPage()
        {
            this.InitializeComponent();
            
            // Get ViewModel through dependency injection
            ViewModel = App.GetService<UpdateCheckPageViewModel>();
            DataContext = ViewModel;
        }
    }
}