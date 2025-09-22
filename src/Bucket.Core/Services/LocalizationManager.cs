using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using DevWinUI;
using WinUI3Localizer;
using Windows.Storage;
using Windows.System.UserProfile;

namespace Bucket.Core.Services
{
    public static class LocalizationConstants
    {
        public const string StringsFolderName = "Strings";
        public static readonly TimeSpan DebugTimeout = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan ProductionTimeout = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan InitializationDelay = TimeSpan.FromMilliseconds(50);
    }

    public record LanguageItem(string Code, string DisplayName);

    public class LanguageChangedEventArgs : EventArgs
    {
        public string OldLanguage { get; }
        public string NewLanguage { get; }

        public LanguageChangedEventArgs(string oldLanguage, string newLanguage)
        {
            OldLanguage = oldLanguage;
            NewLanguage = newLanguage;
        }
    }

    public static class SupportedLanguages
    {
        public const string DefaultLanguage = "en-US";

        public static readonly IReadOnlyList<LanguageItem> All = new List<LanguageItem>
        {
            new("en-US", "English"),
            new("fr-FR", "Français")
        }.AsReadOnly();

        private static readonly IReadOnlyDictionary<string, LanguageItem> _languageCache =
            All.ToDictionary(lang => lang.Code, StringComparer.OrdinalIgnoreCase);


        public static LanguageItem? GetByCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            _languageCache.TryGetValue(code, out var language);
            return language;
        }

        public static bool IsSupported(string code) =>
            !string.IsNullOrEmpty(code) && _languageCache.ContainsKey(code);

        public static LanguageItem GetDefault() => GetByCode(DefaultLanguage) ?? All[0];

        public static string MapOSLanguageToSupported(string osLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(osLanguageCode))
                return DefaultLanguage;

            // Direct match first - return the normalized code from our supported languages
            var directMatch = GetByCode(osLanguageCode);
            if (directMatch != null)
                return directMatch.Code;

            // Try to match by language family (e.g., "fr-CA" -> "fr-FR", "en-GB" -> "en-US")
            var languageFamily = osLanguageCode.Split('-')[0];

            var matchingLanguage = All.FirstOrDefault(lang =>
                lang.Code.Split('-')[0].Equals(languageFamily, StringComparison.OrdinalIgnoreCase));

            return matchingLanguage?.Code ?? DefaultLanguage;
        }

        public static string ValidateLanguageCode(string? languageCode) =>
            string.IsNullOrWhiteSpace(languageCode) ? DefaultLanguage : (GetByCode(languageCode)?.Code ?? DefaultLanguage);

        public static LanguageItem GetLanguageItem(string? languageCode) =>
            GetByCode(ValidateLanguageCode(languageCode)) ?? GetDefault();
    }

    public interface IPlatformLocalizer
    {
        Task InitializeAsync(string languageCode, CancellationToken cancellationToken = default);
        Task<bool> SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default);
        string GetString(string key);
    }

    public interface IPlatformLanguageDetector
    {
        string GetBestMatchingLanguage();
    }

    public interface IPlatformUIRefresher
    {
        Task RefreshUIAsync(string languageCode, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Central localization manager that orchestrates all localization functionality
    /// </summary>
    public class LocalizationManager
    {
        private readonly IPlatformLocalizer _platformLocalizer;
        private readonly IPlatformLanguageDetector _platformLanguageDetector;
        private readonly IPlatformUIRefresher _platformUIRefresher;
        private readonly Action<string> _saveLanguageToConfig;

        private string _currentLanguage = SupportedLanguages.DefaultLanguage;

        /// <summary>
        /// Gets the current language code
        /// </summary>
        public string CurrentLanguage => _currentLanguage;

        /// <summary>
        /// Gets all supported languages
        /// </summary>
        public IReadOnlyList<LanguageItem> AllSupportedLanguages => SupportedLanguages.All;

        /// <summary>
        /// Event raised when the language changes
        /// </summary>
        public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

        public LocalizationManager(
            IPlatformLocalizer platformLocalizer,
            IPlatformLanguageDetector platformLanguageDetector,
            IPlatformUIRefresher platformUIRefresher,
            Action<string> saveLanguageToConfig)
        {
            _platformLocalizer = platformLocalizer ?? throw new ArgumentNullException(nameof(platformLocalizer));
            _platformLanguageDetector = platformLanguageDetector ?? throw new ArgumentNullException(nameof(platformLanguageDetector));
            _platformUIRefresher = platformUIRefresher ?? throw new ArgumentNullException(nameof(platformUIRefresher));
            _saveLanguageToConfig = saveLanguageToConfig ?? throw new ArgumentNullException(nameof(saveLanguageToConfig));
        }

        /// <summary>
        /// Initializes the localization manager
        /// </summary>
        /// <param name="savedLanguageCode">Previously saved language code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task InitializeAsync(string? savedLanguageCode = null, CancellationToken cancellationToken = default)
        {
            await InitializeInternalAsync(savedLanguageCode, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes with automatic language detection for first startup
        /// </summary>
        /// <param name="savedLanguageCode">Previously saved language code</param>
        /// <param name="isFirstStartup">Whether this is the first startup</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task InitializeWithAutoDetectionAsync(string? savedLanguageCode = null, bool isFirstStartup = false, CancellationToken cancellationToken = default)
        {
            await InitializeInternalAsync(savedLanguageCode, isFirstStartup, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current language with full UI refresh
        /// </summary>
        /// <param name="languageCode">Language code to set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task<bool> SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default)
        {
            var validLanguageCode = SupportedLanguages.ValidateLanguageCode(languageCode);

            // No change needed if languages are the same
            if (_currentLanguage.Equals(validLanguageCode, StringComparison.OrdinalIgnoreCase))
                return true;

            var oldLanguage = _currentLanguage;

            try
            {
                // 1. Set platform language
                var success = await _platformLocalizer.SetLanguageAsync(validLanguageCode, cancellationToken).ConfigureAwait(false);
                if (!success)
                {
                    return false;
                }

                // 2. Update current language
                _currentLanguage = validLanguageCode;

                // 3. Save to config
                _saveLanguageToConfig(validLanguageCode);

                // 4. Refresh platform UI (navigation menu, etc.)
                await _platformUIRefresher.RefreshUIAsync(validLanguageCode, cancellationToken).ConfigureAwait(false);

                // 5. Notify language change
                LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldLanguage, validLanguageCode));

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set language to {validLanguageCode}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a localized string by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string or key if not found</returns>
        public string GetString(string key)
        {
            return _platformLocalizer.GetString(key);
        }

        /// <summary>
        /// Gets the language item for a given code
        /// </summary>
        /// <param name="languageCode">Language code</param>
        /// <returns>LanguageItem</returns>
        public LanguageItem GetLanguageItem(string? languageCode)
        {
            return SupportedLanguages.GetLanguageItem(languageCode);
        }

        private async Task InitializeInternalAsync(string? savedLanguageCode, bool isFirstStartup, CancellationToken cancellationToken)
        {
            try
            {
                // Determine which language to use
                string languageToSet;

                if (isFirstStartup)
                {
                    // First startup - detect OS language
                    languageToSet = _platformLanguageDetector.GetBestMatchingLanguage();
                    
                    // Save the detected language to config
                    _saveLanguageToConfig(languageToSet);
                }
                else
                {
                    // Use saved language or default
                    languageToSet = SupportedLanguages.ValidateLanguageCode(savedLanguageCode);
                }

                // Initialize platform localizer
                await _platformLocalizer.InitializeAsync(languageToSet, cancellationToken).ConfigureAwait(false);

                // Update current language
                _currentLanguage = languageToSet;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize localization: {ex.Message}");
                _currentLanguage = SupportedLanguages.DefaultLanguage;

                // Fallback initialization
                try
                {
                    await _platformLocalizer.InitializeAsync(_currentLanguage, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Silent fallback
                }
            }
        }
    }

    // ================================================================================================
    // PLATFORM-SPECIFIC IMPLEMENTATIONS - Consolidated from separate files
    // ================================================================================================

    /// <summary>
    /// Windows-specific implementation for detecting system language
    /// </summary>
    public class WindowsPlatformLanguageDetector : IPlatformLanguageDetector
    {
        /// <summary>
        /// Gets the best matching supported language based on system preferences
        /// </summary>
        /// <returns>Supported language code that best matches system preferences</returns>
        public string GetBestMatchingLanguage()
        {
            var systemLanguage = GetSystemLanguageCode();
            var mappedLanguage = SupportedLanguages.MapOSLanguageToSupported(systemLanguage);
            return mappedLanguage;
        }

        /// <summary>
        /// Gets the current system language code using Windows APIs
        /// </summary>
        /// <returns>System language code (e.g., "en-US", "fr-FR")</returns>
        private string GetSystemLanguageCode()
        {
            // Try Windows API first
            try
            {
                var languages = GlobalizationPreferences.Languages;
                if (languages?.Count > 0 && !string.IsNullOrEmpty(languages[0]))
                    return languages[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WinRT API call failed: {ex.Message}");
            }

            // Fallback to current culture
            try
            {
                var fallbackLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
                if (!string.IsNullOrEmpty(fallbackLanguage))
                    return fallbackLanguage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fallback culture detection failed: {ex.Message}");
            }

            return SupportedLanguages.DefaultLanguage;
        }
    }

    /// <summary>
    /// WinUI3 platform-specific localizer implementation
    /// </summary>
    public class WinUI3PlatformLocalizer : IPlatformLocalizer, IAsyncDisposable, IDisposable
    {
        private ILocalizer _localizer = null!;
        private bool _disposed = false;

        /// <summary>
        /// Initializes the WinUI3Localizer with the specified language
        /// </summary>
        /// <param name="languageCode">Language code to initialize with</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task InitializeAsync(string languageCode, CancellationToken cancellationToken = default)
        {
            try
            {
                // Initialize the "Strings" folder in the executables folder
                string stringsFolderPath = Path.Combine(AppContext.BaseDirectory, LocalizationConstants.StringsFolderName);
                StorageFolder stringsFolder = await StorageFolder.GetFolderFromPathAsync(stringsFolderPath);

                _localizer = await new LocalizerBuilder()
                    .AddStringResourcesFolderForLanguageDictionaries(stringsFolderPath)
                    .SetOptions(options =>
                    {
                        options.DefaultLanguage = SupportedLanguages.DefaultLanguage;
                    })
                    .Build().ConfigureAwait(false);

                // Set the initial language
                if (_localizer != null)
                {
                    await _localizer.SetLanguage(languageCode).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize WinUI3 localizer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sets the language in WinUI3Localizer
        /// </summary>
        /// <param name="languageCode">Language code to set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task<bool> SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default)
        {
            try
            {
                await _localizer.SetLanguage(languageCode).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set WinUI3 language to {languageCode}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a localized string from WinUI3Localizer
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string or key if not found</returns>
        public string GetString(string key)
        {
            if (_disposed) return key;

            try
            {
                return _localizer.GetLocalizedString(key);
            }
            catch (Exception)
            {
                return key; // Return key if localization fails
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            try
            {
                if (_localizer is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (_localizer is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing WinUI3 localizer: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (_localizer is IDisposable disposableLocalizer)
                {
                    disposableLocalizer.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing WinUI3 localizer: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// WinUI platform-specific UI refresher for navigation menu and UI elements
    /// </summary>
    public class WinUIPlatformUIRefresher : IPlatformUIRefresher
    {
        private readonly Func<object> _getNavService;

        public WinUIPlatformUIRefresher(Func<object> getNavService)
        {
            _getNavService = getNavService;
        }

        /// <summary>
        /// Refreshes WinUI platform-specific UI elements after language change
        /// </summary>
        /// <param name="languageCode">New language code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task RefreshUIAsync(string languageCode, CancellationToken cancellationToken = default)
        {
            try
            {
                // Step 1: Set the system language override for WinUI and DevWinUI
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                // Step 2: Use the new DevWinUI ReInitialize() method to refresh navigation
                var navService = _getNavService() as JsonNavigationService;
                if (navService != null)
                {
                    navService.ReInitialize();
                }

                // Small delay to ensure reinitialization completes
                await Task.Delay(LocalizationConstants.InitializationDelay, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during WinUI UI refresh: {ex.Message}");
                throw;
            }
        }
    }
}