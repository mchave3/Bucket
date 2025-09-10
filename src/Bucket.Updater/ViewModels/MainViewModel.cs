namespace Bucket.Updater.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;

        [ObservableProperty]
        private string currentVersion = "1.0.0.0";

        [ObservableProperty]
        private UpdaterConfiguration configuration;

        public MainViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
            configuration = _updateService.GetConfiguration();
            CurrentVersion = configuration.CurrentVersion;
            
            Logger?.Information("MainViewModel initialized with version {Version}", CurrentVersion);
        }
    }
}
