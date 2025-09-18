namespace Bucket.Updater.Services
{
    /// <summary>
    /// Interface for reading application configuration from AppConfig.json file
    /// </summary>
    public interface IAppConfigReader
    {
        /// <summary>
        /// Reads the updater configuration synchronously from the AppConfig.json file
        /// </summary>
        /// <returns>The updater configuration if found and valid, otherwise null</returns>
        UpdaterConfiguration? ReadConfiguration();

        /// <summary>
        /// Reads the updater configuration asynchronously from the AppConfig.json file
        /// </summary>
        /// <returns>The updater configuration if found and valid, otherwise null</returns>
        Task<UpdaterConfiguration?> ReadConfigurationAsync();
    }

    /// <summary>
    /// Service for reading and parsing application configuration from the AppConfig.json file.
    /// This service reads configuration data that was originally written by the main Bucket application.
    /// </summary>
    public class AppConfigReader : IAppConfigReader
    {
        private readonly string _appConfigPath;

        /// <summary>
        /// Initializes a new instance of AppConfigReader with the default configuration path
        /// </summary>
        public AppConfigReader()
        {
            // Path to AppConfig.json in common application data folder
            _appConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket", "AppConfig.json");
        }

        /// <summary>
        /// Reads the updater configuration synchronously from the AppConfig.json file
        /// </summary>
        /// <returns>The updater configuration if found and valid, otherwise null</returns>
        public UpdaterConfiguration? ReadConfiguration()
        {
            try
            {
                // Check if configuration file exists at expected location
                if (!File.Exists(_appConfigPath))
                {
                    Logger?.Warning("AppConfig.json not found at {Path}", _appConfigPath);
                    return null;
                }

                // Read configuration file content
                var json = File.ReadAllText(_appConfigPath);
                return ParseAppConfig(json);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to read AppConfig.json");
                return null;
            }
        }

        /// <summary>
        /// Reads the updater configuration asynchronously from the AppConfig.json file
        /// </summary>
        /// <returns>The updater configuration if found and valid, otherwise null</returns>
        public async Task<UpdaterConfiguration?> ReadConfigurationAsync()
        {
            try
            {
                // Check if configuration file exists at expected location
                if (!File.Exists(_appConfigPath))
                {
                    Logger?.Warning("AppConfig.json not found at {Path}", _appConfigPath);
                    return null;
                }

                // Read configuration file content asynchronously
                var json = await File.ReadAllTextAsync(_appConfigPath);
                return ParseAppConfig(json);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to read AppConfig.json");
                return null;
            }
        }

        /// <summary>
        /// Parses JSON configuration content into UpdaterConfiguration object
        /// </summary>
        /// <param name="json">The JSON content to parse</param>
        /// <returns>The parsed configuration if successful, otherwise null</returns>
        private UpdaterConfiguration? ParseAppConfig(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var config = new UpdaterConfiguration();

                // Parse application version from configuration
                if (root.TryGetProperty("Version", out var versionElement) && versionElement.GetString() is string version)
                {
                    config.CurrentVersion = version;
                }

                // Parse update channel (private property in AppConfig)
                if (root.TryGetProperty("updateChannel", out var channelElement) && channelElement.GetString() is string channel)
                {
                    config.UpdateChannel = channel.Equals("Nightly", StringComparison.OrdinalIgnoreCase)
                        ? UpdateChannel.Nightly
                        : UpdateChannel.Release;
                }

                // Parse system architecture (private property in AppConfig)
                if (root.TryGetProperty("architecture", out var archElement) && archElement.GetString() is string arch)
                {
                    config.Architecture = arch.ToLowerInvariant() switch
                    {
                        "x86" => SystemArchitecture.X86,
                        "x64" => SystemArchitecture.X64,
                        "arm64" => SystemArchitecture.ARM64,
                        _ => SystemArchitecture.X64 // Default fallback to x64
                    };
                }

                // Parse GitHub repository owner (private property in AppConfig)
                if (root.TryGetProperty("gitHubOwner", out var ownerElement) && ownerElement.GetString() is string owner)
                {
                    config.GitHubOwner = owner;
                }

                // Parse GitHub repository name (private property in AppConfig)
                if (root.TryGetProperty("gitHubRepository", out var repoElement) && repoElement.GetString() is string repo)
                {
                    config.GitHubRepository = repo;
                }

                // Initialize additional runtime properties based on parsed configuration
                config.InitializeRuntimeProperties();

                Logger?.Information("Successfully parsed AppConfig: Version={Version}, Channel={Channel}, Architecture={Architecture}, Owner={Owner}, Repo={Repo}",
                    config.CurrentVersion, config.UpdateChannel, config.Architecture, config.GitHubOwner, config.GitHubRepository);

                return config;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to parse AppConfig.json");
                return null;
            }
        }
    }
}