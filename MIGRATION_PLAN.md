# MISSION : Migration de la configuration Bucket.Updater vers AppConfig.json

## 📋 CONTEXTE
Je travaille sur un projet .NET avec deux applications :
- **Bucket.App** : Application principale qui utilise `AppConfig.json` (avec Nucs.JsonSettings)
- **Bucket.Updater** : Application de mise à jour qui utilise actuellement `updater-config.json`

## 🎯 OBJECTIF
Éliminer complètement `updater-config.json` et faire en sorte que Bucket.Updater utilise exclusivement `AppConfig.json` comme source de configuration. L'updater doit devenir en lecture seule et ne plus créer/modifier de fichier de configuration.

## 📊 ÉTAT ACTUEL

### AppConfig.json (Bucket.App) - Structure :
```csharp
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    public Version Version { get; set; } = new Version(1, 0, 0, 0);
    private string updateChannel { get; set; } = "Release";
    private string architecture { get; set; } = "x64";
    public string GitHubOwner { get; set; } = "mchave3";
    public string GitHubRepository { get; set; } = "Bucket";
    private string lastUpdateCheck { get; set; }
    // ... autres propriétés
}
```

### UpdaterConfiguration (Bucket.Updater) - Structure :
```csharp
public class UpdaterConfiguration
{
    public UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Release;
    public SystemArchitecture Architecture { get; set; } = SystemArchitecture.X64;
    public string GitHubOwner { get; set; } = "mchave3";
    public string GitHubRepository { get; set; } = "Bucket";
    public string CurrentVersion { get; set; } = "1.0.0.0";
    public DateTime LastUpdateCheck { get; set; } = DateTime.MinValue;
}
```

## 🚀 PLAN D'ACTIONS

### ÉTAPE 1 : Créer AppConfigReader
Créer `src/Bucket.Updater/Services/AppConfigReader.cs` :

```csharp
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

                // Version
                if (root.TryGetProperty("Version", out var versionElement) && versionElement.GetString() is string version)
                {
                    config.CurrentVersion = version;
                }

                // UpdateChannel (propriété privée dans AppConfig)
                if (root.TryGetProperty("updateChannel", out var channelElement) && channelElement.GetString() is string channel)
                {
                    config.UpdateChannel = channel.Equals("Nightly", StringComparison.OrdinalIgnoreCase)
                        ? UpdateChannel.Nightly
                        : UpdateChannel.Release;
                }

                // GitHubOwner
                if (root.TryGetProperty("GitHubOwner", out var ownerElement) && ownerElement.GetString() is string owner)
                {
                    config.GitHubOwner = owner;
                }

                // GitHubRepository
                if (root.TryGetProperty("GitHubRepository", out var repoElement) && repoElement.GetString() is string repo)
                {
                    config.GitHubRepository = repo;
                }

                // LastUpdateCheck (propriété privée dans AppConfig)
                if (root.TryGetProperty("lastUpdateCheck", out var lastCheckElement) && lastCheckElement.GetString() is string lastCheck)
                {
                    if (DateTime.TryParse(lastCheck, out var parsedDate))
                    {
                        config.LastUpdateCheck = parsedDate;
                    }
                }

                // Initialiser les propriétés runtime
                config.InitializeRuntimeProperties();

                Logger?.Information("Successfully parsed AppConfig: Version={Version}, Channel={Channel}, Owner={Owner}, Repo={Repo}",
                    config.CurrentVersion, config.UpdateChannel, config.GitHubOwner, config.GitHubRepository);

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
```

### ÉTAPE 2 : Créer service de nettoyage
Créer `src/Bucket.Updater/Services/ConfigurationCleanupService.cs` :

```csharp
namespace Bucket.Updater.Services
{
    public interface IConfigurationCleanupService
    {
        void CleanupLegacyConfigFiles();
    }

    public class ConfigurationCleanupService : IConfigurationCleanupService
    {
        private readonly string _legacyConfigPath;

        public ConfigurationCleanupService()
        {
            _legacyConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket", "updater-config.json");
        }

        public void CleanupLegacyConfigFiles()
        {
            try
            {
                if (File.Exists(_legacyConfigPath))
                {
                    Logger?.Information("Found legacy updater-config.json, removing it: {Path}", _legacyConfigPath);
                    File.Delete(_legacyConfigPath);
                    Logger?.Information("Successfully removed legacy configuration file");
                }
            }
            catch (Exception ex)
            {
                Logger?.Warning(ex, "Failed to remove legacy configuration file: {Path}", _legacyConfigPath);
            }
        }
    }
}
```

### ÉTAPE 3 : Refactorer IConfigurationService
Modifier `src/Bucket.Updater/Services/ConfigurationService.cs` :

```csharp
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
        private readonly IConfigurationCleanupService _cleanupService;
        private UpdaterConfiguration? _cachedConfiguration;
        private static bool _cleanupPerformed = false;

        public ConfigurationService(IAppConfigReader appConfigReader, IConfigurationCleanupService cleanupService)
        {
            _appConfigReader = appConfigReader;
            _cleanupService = cleanupService;

            // Effectuer le nettoyage une seule fois
            if (!_cleanupPerformed)
            {
                _cleanupService.CleanupLegacyConfigFiles();
                _cleanupPerformed = true;
            }

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

            // Fallback vers configuration par défaut
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

            // Fallback vers configuration par défaut
            Logger?.Information("Using default configuration");
            var defaultConfig = new UpdaterConfiguration();
            defaultConfig.InitializeRuntimeProperties();
            _cachedConfiguration = defaultConfig;
            return defaultConfig;
        }
    }
}
```

### ÉTAPE 4 : Modifier IUpdateService
Modifier `src/Bucket.Updater/Services/UpdateService.cs` :

```csharp
namespace Bucket.Updater.Services
{
    public interface IUpdateService
    {
        Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync();
        Task<string> DownloadUpdateAsync(Bucket.Updater.Models.UpdateInfo updateInfo, IProgress<(long downloaded, long total)>? progress = null, CancellationToken cancellationToken = default);
        Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
        void CleanupFiles(string downloadPath);
        UpdaterConfiguration GetConfiguration();
        // SUPPRIMÉ : Task SaveConfigurationAsync(UpdaterConfiguration configuration);
    }

    public class UpdateService : IUpdateService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IGitHubService _gitHubService;
        private readonly IInstallationService _installationService;

        public UpdateService(
            IConfigurationService configurationService,
            IGitHubService gitHubService,
            IInstallationService installationService)
        {
            _configurationService = configurationService;
            _gitHubService = gitHubService;
            _installationService = installationService;
            Logger?.Information("UpdateService initialized (read-only configuration)");
        }

        public async Task<Bucket.Updater.Models.UpdateInfo?> CheckForUpdatesAsync()
        {
            Logger?.Information("UpdateService checking for updates");
            try
            {
                var configuration = await _configurationService.LoadConfigurationAsync();
                var updateInfo = await _gitHubService.CheckForUpdatesAsync(configuration);

                if (updateInfo != null)
                {
                    Logger?.Information("Update check completed, update available: {Version}", updateInfo.Version);

                    // NOTE: LastUpdateCheck n'est plus mis à jour ici car Bucket.Updater est en lecture seule
                    // Cette responsabilité pourrait être déléguée à Bucket.App si nécessaire
                }
                else
                {
                    Logger?.Information("Update check completed, no updates available");
                }

                return updateInfo;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "UpdateService failed to check for updates");
                return null;
            }
        }

        // ... autres méthodes inchangées (DownloadUpdateAsync, InstallUpdateAsync, CleanupFiles)

        public UpdaterConfiguration GetConfiguration()
        {
            return _configurationService.GetConfiguration();
        }

        // SUPPRIMÉ : SaveConfigurationAsync
    }
}
```

### ÉTAPE 5 : Modifier l'injection de dépendances
Modifier `src/Bucket.Updater/App.xaml.cs` dans `ConfigureServices` :

```csharp
private static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // DevWinUI Services
    services.AddSingleton<IThemeService, ThemeService>();
    services.AddSingleton<ContextMenuService>();

    // Configuration Services
    services.AddSingleton<IAppConfigReader, AppConfigReader>();
    services.AddSingleton<IConfigurationCleanupService, ConfigurationCleanupService>();
    services.AddSingleton<IConfigurationService, ConfigurationService>();

    // Application Services
    services.AddSingleton<IGitHubService, GitHubService>();
    services.AddSingleton<IInstallationService, InstallationService>();
    services.AddSingleton<IUpdateService, UpdateService>();

    // ViewModels
    services.AddTransient<MainViewModel>();
    services.AddTransient<UpdateCheckPageViewModel>();
    services.AddTransient<DownloadInstallPageViewModel>();

    return services.BuildServiceProvider();
}
```

### ÉTAPE 6 : Adapter les ViewModels
Modifier `src/Bucket.Updater/ViewModels/UpdateCheckPageViewModel.cs` - Supprimer les appels de sauvegarde :

Dans `CheckForUpdatesAutomaticallyAsync` et `CheckUpdatesAsync`, supprimer ou commenter ces lignes :
```csharp
// SUPPRIMÉ : configuration.LastUpdateCheck = DateTime.Now;
// SUPPRIMÉ : await _configurationService.SaveConfigurationAsync(configuration);
```

### ÉTAPE 7 : Tests de validation
Créer ces scénarios de test :

1. **Installation fraîche** : Pas d'AppConfig.json → Configuration par défaut
2. **AppConfig.json existant** : Lecture correcte des propriétés
3. **Migration depuis updater-config.json** : Suppression automatique du fichier legacy
4. **AppConfig.json corrompu** : Fallback vers configuration par défaut

## ⚠️ POINTS D'ATTENTION

1. **Propriétés privées** : `updateChannel`, `architecture`, `lastUpdateCheck` sont privées dans AppConfig → utiliser JsonDocument
2. **Permissions** : S'assurer que Bucket.Updater peut lire AppConfig.json
3. **Nettoyage** : Supprimer automatiquement `updater-config.json`
4. **Logging** : Tracer toutes les opérations pour faciliter le debug
5. **Cache** : Le cache est temporaire (session uniquement)

## 🧹 NETTOYAGE FINAL

- Supprimer toutes les références à la sauvegarde de configuration dans Bucket.Updater
- Supprimer les méthodes de sauvegarde de IConfigurationService
- Tester que `updater-config.json` est automatiquement supprimé
- Vérifier que l'updater fonctionne en mode lecture seule

## ✅ VALIDATION

Après implémentation, vérifier que :
- [ ] Bucket.Updater lit AppConfig.json correctement
- [ ] updater-config.json est supprimé automatiquement
- [ ] Les ViewModels affichent les bonnes informations
- [ ] Aucune tentative de sauvegarde de configuration
- [ ] Fallback gracieux si AppConfig.json manque
- [ ] Logging approprié pour toutes les opérations

Procède étape par étape en commençant par ÉTAPE 1, puis valide chaque étape avant de passer à la suivante.
