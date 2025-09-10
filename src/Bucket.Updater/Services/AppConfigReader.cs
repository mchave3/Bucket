using System.Text.Json;

namespace Bucket.Updater.Services
{
    public interface IAppConfigReader
    {
        UpdaterConfiguration? ReadConfiguration();
        Task<UpdaterConfiguration?> ReadConfigurationAsync();
    }

    public class AppConfigReader : IAppConfigReader
    {
        private readonly string _appConfigPath;

        public AppConfigReader()
        {
            _appConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket", "AppConfig.json");
        }

        public UpdaterConfiguration? ReadConfiguration()
        {
            try
            {
                if (!File.Exists(_appConfigPath))
                {
                    Logger?.Warning("AppConfig.json not found at {Path}", _appConfigPath);
                    return null;
                }

                var json = File.ReadAllText(_appConfigPath);
                return ParseAppConfig(json);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to read AppConfig.json");
                return null;
            }
        }

        public async Task<UpdaterConfiguration?> ReadConfigurationAsync()
        {
            try
            {
                if (!File.Exists(_appConfigPath))
                {
                    Logger?.Warning("AppConfig.json not found at {Path}", _appConfigPath);
                    return null;
                }

                var json = await File.ReadAllTextAsync(_appConfigPath);
                return ParseAppConfig(json);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to read AppConfig.json");
                return null;
            }
        }

        private UpdaterConfiguration? ParseAppConfig(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var config = new UpdaterConfiguration();

                // Parse Version
                if (root.TryGetProperty("Version", out var versionElement) && versionElement.GetString() is string version)
                {
                    config.CurrentVersion = version;
                }

                // Parse updateChannel (private property in AppConfig)
                if (root.TryGetProperty("updateChannel", out var channelElement) && channelElement.GetString() is string channel)
                {
                    config.UpdateChannel = channel.Equals("Nightly", StringComparison.OrdinalIgnoreCase)
                        ? UpdateChannel.Nightly
                        : UpdateChannel.Release;
                }

                // Parse architecture (private property in AppConfig)
                if (root.TryGetProperty("architecture", out var archElement) && archElement.GetString() is string arch)
                {
                    config.Architecture = arch.ToLowerInvariant() switch
                    {
                        "x86" => SystemArchitecture.X86,
                        "x64" => SystemArchitecture.X64,
                        "arm64" => SystemArchitecture.ARM64,
                        _ => SystemArchitecture.X64 // Default fallback
                    };
                }

                // Parse GitHubOwner
                if (root.TryGetProperty("GitHubOwner", out var ownerElement) && ownerElement.GetString() is string owner)
                {
                    config.GitHubOwner = owner;
                }

                // Parse GitHubRepository
                if (root.TryGetProperty("GitHubRepository", out var repoElement) && repoElement.GetString() is string repo)
                {
                    config.GitHubRepository = repo;
                }

                // Initialize runtime properties
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