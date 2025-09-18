using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bucket.Core.Services
{
    // ================================================================================================
    // MODELS - Consolidated from separate model files
    // ================================================================================================

    /// <summary>
    /// Represents a language option with its code and display name
    /// </summary>
    /// <param name="Code">Language code (e.g., "en-US", "fr-FR")</param>
    /// <param name="DisplayName">Human-readable name (e.g., "English", "Français")</param>
    public record LanguageItem(string Code, string DisplayName);

    /// <summary>
    /// Event arguments for language change events
    /// </summary>
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

    // ================================================================================================
    // SUPPORTED LANGUAGES - Consolidated from SupportedLanguages.cs
    // ================================================================================================

    /// <summary>
    /// Contains supported languages and related constants
    /// </summary>
    public static class SupportedLanguages
    {
        /// <summary>
        /// Default language code
        /// </summary>
        public const string DefaultLanguage = "en-US";

        /// <summary>
        /// List of all supported languages
        /// </summary>
        public static readonly IReadOnlyList<LanguageItem> All = new List<LanguageItem>
        {
            new("en-US", "English"),
            new("fr-FR", "Français")
        }.AsReadOnly();

        /// <summary>
        /// Gets a language item by its code
        /// </summary>
        /// <param name="code">Language code</param>
        /// <returns>LanguageItem if found, null otherwise</returns>
        public static LanguageItem? GetByCode(string code)
        {
            return All.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if a language code is supported
        /// </summary>
        /// <param name="code">Language code to check</param>
        /// <returns>True if supported, false otherwise</returns>
        public static bool IsSupported(string code)
        {
            return All.Any(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the default language item
        /// </summary>
        /// <returns>Default language item</returns>
        public static LanguageItem GetDefault()
        {
            return GetByCode(DefaultLanguage) ?? All[0];
        }

        /// <summary>
        /// Maps an OS language code to a supported language code
        /// </summary>
        /// <param name="osLanguageCode">OS language code (e.g., "en-US", "fr-FR", "fr-CA", "en-GB")</param>
        /// <returns>Supported language code or default if not supported</returns>
        public static string MapOSLanguageToSupported(string osLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(osLanguageCode))
            {
                return DefaultLanguage;
            }

            // Direct match first - return the normalized code from our supported languages
            var directMatch = GetByCode(osLanguageCode);
            if (directMatch != null)
            {
                return directMatch.Code;
            }

            // Try to match by language family (e.g., "fr-CA" -> "fr-FR", "en-GB" -> "en-US")
            var languageFamily = osLanguageCode.Split('-')[0].ToUpperInvariant();

            var matchingLanguage = All.FirstOrDefault(lang =>
                lang.Code.Split('-')[0].Equals(languageFamily, StringComparison.OrdinalIgnoreCase));

            if (matchingLanguage != null)
            {
                return matchingLanguage.Code;
            }

            return DefaultLanguage;
        }

        /// <summary>
        /// Validates a language code and returns a valid one
        /// </summary>
        /// <param name="languageCode">Language code to validate</param>
        /// <returns>Valid language code (may fallback to default)</returns>
        public static string ValidateLanguageCode(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return DefaultLanguage;

            var supportedLanguage = GetByCode(languageCode);
            return supportedLanguage?.Code ?? DefaultLanguage;
        }

        /// <summary>
        /// Gets the appropriate language item for a given code
        /// </summary>
        /// <param name="languageCode">Language code</param>
        /// <returns>LanguageItem (falls back to default if not found)</returns>
        public static LanguageItem GetLanguageItem(string? languageCode)
        {
            var validCode = ValidateLanguageCode(languageCode);
            return GetByCode(validCode) ?? GetDefault();
        }

        /// <summary>
        /// Determines if a language change is actually needed
        /// </summary>
        /// <param name="currentLanguage">Current language code</param>
        /// <param name="newLanguage">New language code</param>
        /// <returns>True if change is needed, false otherwise</returns>
        public static bool ShouldChangeLanguage(string? currentLanguage, string? newLanguage)
        {
            if (string.IsNullOrWhiteSpace(newLanguage))
                return false;

            if (string.IsNullOrWhiteSpace(currentLanguage))
                return true;

            return !currentLanguage.Equals(newLanguage, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ================================================================================================
    // PLATFORM INTERFACES - Minimal interfaces for dependency injection
    // ================================================================================================

    /// <summary>
    /// Platform-specific localization interface (replaces heavy WinUI3LocalizationService)
    /// </summary>
    public interface IPlatformLocalizer
    {
        /// <summary>
        /// Initializes the platform localizer
        /// </summary>
        /// <param name="languageCode">Language code to initialize with</param>
        Task InitializeAsync(string languageCode);

        /// <summary>
        /// Sets the platform language
        /// </summary>
        /// <param name="languageCode">Language code to set</param>
        Task<bool> SetLanguageAsync(string languageCode);

        /// <summary>
        /// Gets a localized string by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string or key if not found</returns>
        string GetString(string key);
    }

    /// <summary>
    /// Platform-specific language detection interface
    /// </summary>
    public interface IPlatformLanguageDetector
    {
        /// <summary>
        /// Gets the best matching supported language based on system preferences
        /// </summary>
        /// <returns>Supported language code</returns>
        string GetBestMatchingLanguage();
    }

    /// <summary>
    /// Platform-specific UI refresh interface for navigation menu, etc.
    /// </summary>
    public interface IPlatformUIRefresher
    {
        /// <summary>
        /// Refreshes platform-specific UI elements after language change
        /// </summary>
        /// <param name="languageCode">New language code</param>
        Task RefreshUIAsync(string languageCode);
    }

    // ================================================================================================
    // CENTRAL LOCALIZATION MANAGER - Orchestrates everything
    // ================================================================================================

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
        public async Task InitializeAsync(string? savedLanguageCode = null)
        {
            await InitializeInternalAsync(savedLanguageCode, false);
        }

        /// <summary>
        /// Initializes with automatic language detection for first startup
        /// </summary>
        /// <param name="savedLanguageCode">Previously saved language code</param>
        /// <param name="isFirstStartup">Whether this is the first startup</param>
        public async Task InitializeWithAutoDetectionAsync(string? savedLanguageCode = null, bool isFirstStartup = false)
        {
            await InitializeInternalAsync(savedLanguageCode, isFirstStartup);
        }

        /// <summary>
        /// Sets the current language with full UI refresh
        /// </summary>
        /// <param name="languageCode">Language code to set</param>
        public async Task<bool> SetLanguageAsync(string languageCode)
        {
            var validLanguageCode = SupportedLanguages.ValidateLanguageCode(languageCode);
            
            if (!SupportedLanguages.ShouldChangeLanguage(_currentLanguage, validLanguageCode))
            {
                return true; // No change needed
            }

            var oldLanguage = _currentLanguage;

            try
            {
                // 1. Set platform language
                var success = await _platformLocalizer.SetLanguageAsync(validLanguageCode);
                if (!success)
                {
                    return false;
                }

                // 2. Update current language
                _currentLanguage = validLanguageCode;

                // 3. Save to config
                _saveLanguageToConfig(validLanguageCode);

                // 4. Refresh platform UI (navigation menu, etc.)
                await _platformUIRefresher.RefreshUIAsync(validLanguageCode);

                // 5. Notify language change
                LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldLanguage, validLanguageCode));

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set language to {validLanguageCode}: {ex.Message}");
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

        private async Task InitializeInternalAsync(string? savedLanguageCode, bool isFirstStartup)
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
                await _platformLocalizer.InitializeAsync(languageToSet);
                
                // Update current language
                _currentLanguage = languageToSet;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize localization: {ex.Message}");
                _currentLanguage = SupportedLanguages.DefaultLanguage;
                
                // Fallback initialization
                try
                {
                    await _platformLocalizer.InitializeAsync(_currentLanguage);
                }
                catch
                {
                    // Silent fallback
                }
            }
        }
    }
}