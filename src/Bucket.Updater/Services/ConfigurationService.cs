namespace Bucket.Updater.Services
{
    public interface IConfigurationService
    {
        UpdaterConfiguration GetConfiguration();
        Task<UpdaterConfiguration> LoadConfigurationAsync();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IAppConfigReader _appConfigReader;
        private UpdaterConfiguration? _cachedConfiguration;

        public ConfigurationService(IAppConfigReader appConfigReader)
        {
            _appConfigReader = appConfigReader;
            Logger?.Information("ConfigurationService initialized (read-only mode)");
        }

        public UpdaterConfiguration GetConfiguration()
        {
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            return LoadConfigurationSync();
        }


        private UpdaterConfiguration LoadConfigurationSync()
        {
            try
            {
                Logger?.Information("Loading configuration from AppConfig.json");
                var configuration = _appConfigReader.ReadConfiguration();

                if (configuration != null)
                {
                    _cachedConfiguration = configuration;
                    Logger?.Information("Configuration loaded successfully from AppConfig.json");
                    return configuration;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error reading AppConfig.json");
            }

            // Fallback to default configuration
            Logger?.Information("Using default configuration");
            var defaultConfig = new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            return defaultConfig;
        }


        public async Task<UpdaterConfiguration> LoadConfigurationAsync()
        {
            if (_cachedConfiguration != null)
            {
                Logger?.Debug("Returning cached configuration");
                return _cachedConfiguration;
            }

            try
            {
                Logger?.Information("Loading configuration from AppConfig.json (async)");
                var configuration = await _appConfigReader.ReadConfigurationAsync();

                if (configuration != null)
                {
                    _cachedConfiguration = configuration;
                    Logger?.Information("Configuration loaded successfully from AppConfig.json");
                    return configuration;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error reading AppConfig.json");
            }

            // Fallback to default configuration
            Logger?.Information("Using default configuration");
            var defaultConfig = new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            return defaultConfig;
        }



    }
}