namespace Bucket.Updater.Services
{
    public interface IConfigurationService
    {
        UpdaterConfiguration GetConfiguration();
        void SaveConfiguration(UpdaterConfiguration configuration);
        Task<UpdaterConfiguration> LoadConfigurationAsync();
        Task SaveConfigurationAsync(UpdaterConfiguration configuration);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configPath;
        private UpdaterConfiguration? _cachedConfiguration;

        public ConfigurationService()
        {
            _configPath = Path.Combine(Constants.RootDirectoryPath, "updater-config.json");
            EnsureConfigDirectoryExists();
            Logger?.Information("ConfigurationService initialized with path: {ConfigPath}", _configPath);
        }

        public UpdaterConfiguration GetConfiguration()
        {
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            return LoadConfigurationSync();
        }

        public void SaveConfiguration(UpdaterConfiguration configuration)
        {
            SaveConfigurationSync(configuration);
        }

        private UpdaterConfiguration LoadConfigurationSync()
        {
            try
            {
                Logger?.Information("Loading configuration synchronously from {ConfigPath}", _configPath);

                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var configuration = JsonSerializer.Deserialize<UpdaterConfiguration>(json);
                        if (configuration != null)
                        {
                            configuration.InitializeRuntimeProperties();
                            _cachedConfiguration = configuration;
                            Logger?.Information("Configuration loaded successfully");
                            return configuration;
                        }
                    }
                }
                else
                {
                    Logger?.Information("Configuration file not found, creating default configuration");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error reading configuration file");
            }

            // Try to read from Bucket.App AppConfig if available
            var defaultConfig = LoadFromAppConfig() ?? new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            SaveConfigurationSync(defaultConfig);
            Logger?.Information("Default configuration created and saved");
            return defaultConfig;
        }

        private void SaveConfigurationSync(UpdaterConfiguration configuration)
        {
            try
            {
                Logger?.Information("Saving configuration synchronously to {ConfigPath}", _configPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(configuration, options);
                File.WriteAllText(_configPath, json);
                _cachedConfiguration = configuration;
                Logger?.Information("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to save configuration");
            }
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
                if (File.Exists(_configPath))
                {
                    Logger?.Information("Loading configuration from {ConfigPath}", _configPath);
                    var json = await File.ReadAllTextAsync(_configPath);
                    var configuration = JsonSerializer.Deserialize<UpdaterConfiguration>(json);
                    if (configuration != null)
                    {
                        configuration.InitializeRuntimeProperties();
                        _cachedConfiguration = configuration;
                        Logger?.Information("Configuration loaded successfully");
                        return configuration;
                    }
                }
                else
                {
                    Logger?.Information("Configuration file not found, creating default configuration");
                }
            }
            catch (Exception ex)
            {
                Logger?.Warning(ex, "Failed to load configuration file, creating default configuration");
            }

            // Try to read from Bucket.App AppConfig if available
            var defaultConfig = LoadFromAppConfig() ?? new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            await SaveConfigurationAsync(defaultConfig);
            Logger?.Information("Default configuration created and saved");
            return defaultConfig;
        }

        public async Task SaveConfigurationAsync(UpdaterConfiguration configuration)
        {
            try
            {
                Logger?.Information("Saving configuration to {ConfigPath}", _configPath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(configuration, options);
                await File.WriteAllTextAsync(_configPath, json);
                _cachedConfiguration = configuration;
                Logger?.Information("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to save configuration");
            }
        }

        private void EnsureConfigDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_configPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Logger?.Information("Created configuration directory: {Directory}", directory);
            }
        }

        private UpdaterConfiguration? LoadFromAppConfig()
        {
            try
            {
                // Try to find Bucket.App's AppConfig.json in the same directory or parent directory
                var currentDir = AppContext.BaseDirectory;
                var parentDir = Directory.GetParent(currentDir)?.FullName;
                
                string[] possiblePaths = {
                    Path.Combine(currentDir, "AppConfig.json"),
                    Path.Combine(parentDir ?? currentDir, "AppConfig.json"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bucket", "AppConfig.json")
                };

                foreach (var configPath in possiblePaths)
                {
                    if (File.Exists(configPath))
                    {
                        Logger?.Information("Found Bucket.App AppConfig at {Path}", configPath);
                        var json = File.ReadAllText(configPath);
                        
                        // Parse the AppConfig JSON to extract relevant information
                        using var document = JsonDocument.Parse(json);
                        if (document.RootElement.TryGetProperty("Version", out var versionElement) &&
                            document.RootElement.TryGetProperty("updateChannel", out var channelElement) &&
                            document.RootElement.TryGetProperty("architecture", out var archElement))
                        {
                            var config = new UpdaterConfiguration();
                            
                            // Set version
                            if (versionElement.GetString() is string version)
                            {
                                config.CurrentVersion = version;
                            }
                            
                            // Set update channel
                            if (channelElement.GetString() is string channel)
                            {
                                config.UpdateChannel = channel.Equals("Nightly", StringComparison.OrdinalIgnoreCase) 
                                    ? UpdateChannel.Nightly 
                                    : UpdateChannel.Release;
                            }
                            
                            // Architecture will be set by InitializeRuntimeProperties()
                            
                            Logger?.Information("Loaded configuration from AppConfig: Version={Version}, Channel={Channel}", 
                                config.CurrentVersion, config.UpdateChannel);
                            return config;
                        }
                    }
                }
                
                Logger?.Information("No Bucket.App AppConfig found, using default configuration");
            }
            catch (Exception ex)
            {
                Logger?.Warning(ex, "Failed to load from Bucket.App AppConfig, using default configuration");
            }
            
            return null;
        }
    }
}