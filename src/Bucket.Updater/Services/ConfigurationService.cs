namespace Bucket.Updater.Services
{
    /// <summary>
    /// Interface for managing updater configuration with caching support
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the cached configuration or loads it synchronously if not available
        /// </summary>
        /// <returns>The updater configuration, never null (falls back to default)</returns>
        UpdaterConfiguration GetConfiguration();

        /// <summary>
        /// Loads the configuration asynchronously with caching support
        /// </summary>
        /// <returns>The updater configuration, never null (falls back to default)</returns>
        Task<UpdaterConfiguration> LoadConfigurationAsync();
    }

    /// <summary>
    /// Service for managing updater configuration with caching and fallback mechanisms.
    /// Provides a high-level interface over AppConfigReader with automatic fallback to default configuration.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IAppConfigReader _appConfigReader;
        private UpdaterConfiguration? _cachedConfiguration;

        /// <summary>
        /// Initializes a new instance of ConfigurationService with the specified configuration reader
        /// </summary>
        /// <param name="appConfigReader">The configuration reader for accessing AppConfig.json</param>
        public ConfigurationService(IAppConfigReader appConfigReader)
        {
            _appConfigReader = appConfigReader;
            Logger?.Information("ConfigurationService initialized (read-only mode)");
        }

        /// <summary>
        /// Gets the cached configuration or loads it synchronously if not available
        /// </summary>
        /// <returns>The updater configuration, never null (falls back to default)</returns>
        public UpdaterConfiguration GetConfiguration()
        {
            // Return cached configuration if already loaded
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            return LoadConfigurationSync();
        }

        /// <summary>
        /// Loads configuration synchronously from AppConfig.json with fallback to defaults
        /// </summary>
        /// <returns>The loaded configuration, never null</returns>
        private UpdaterConfiguration LoadConfigurationSync()
        {
            try
            {
                Logger?.Information("Loading configuration from AppConfig.json");
                var configuration = _appConfigReader.ReadConfiguration();

                if (configuration != null)
                {
                    // Cache the loaded configuration for future requests
                    _cachedConfiguration = configuration;
                    Logger?.Information("Configuration loaded successfully from AppConfig.json");
                    return configuration;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error reading AppConfig.json");
            }

            // Fallback to default configuration when file not found or parsing fails
            Logger?.Information("Using default configuration");
            var defaultConfig = new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            return defaultConfig;
        }

        /// <summary>
        /// Loads the configuration asynchronously with caching support
        /// </summary>
        /// <returns>The updater configuration, never null (falls back to default)</returns>
        public async Task<UpdaterConfiguration> LoadConfigurationAsync()
        {
            // Return cached configuration if already loaded
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
                    // Cache the loaded configuration for future requests
                    _cachedConfiguration = configuration;
                    Logger?.Information("Configuration loaded successfully from AppConfig.json");
                    return configuration;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error reading AppConfig.json");
            }

            // Fallback to default configuration when file not found or parsing fails
            Logger?.Information("Using default configuration");
            var defaultConfig = new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            return defaultConfig;
        }



    }
}