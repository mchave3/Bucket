namespace Bucket.Updater.ViewModels
{
    /// <summary>
    /// Main coordinator ViewModel for the updater application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;

        /// <summary>
        /// Currently installed version of the application
        /// </summary>
        [ObservableProperty]
        private string currentVersion = "1.0.0.0";

        /// <summary>
        /// Updater configuration settings
        /// </summary>
        [ObservableProperty]
        private UpdaterConfiguration configuration;

        /// <summary>
        /// Initializes the main ViewModel with update service dependency
        /// </summary>
        /// <param name="updateService">Service for handling update operations</param>
        public MainViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            
            // Load configuration and set current version
            configuration = _updateService.GetConfiguration();
            CurrentVersion = configuration.CurrentVersion;

            Logger?.Information("MainViewModel initialized with version {Version}", CurrentVersion);
        }
    }
}
