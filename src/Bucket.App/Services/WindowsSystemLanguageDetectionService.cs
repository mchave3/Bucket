using System.Diagnostics;
using Bucket.Core.Models;
using Bucket.Core.Services;
using Windows.System.UserProfile;

namespace Bucket.App.Services
{
    /// <summary>
    /// Windows-specific implementation for detecting system language
    /// </summary>
    public class WindowsSystemLanguageDetectionService : ISystemLanguageDetectionService
    {
        /// <summary>
        /// Gets the current system language code using Windows APIs
        /// </summary>
        /// <returns>System language code (e.g., "en-US", "fr-FR")</returns>
        public string GetSystemLanguageCode()
        {
            try
            {
                // Get the primary language from Windows user profile
                var languages = GlobalizationPreferences.Languages;
                Debug.WriteLine($"[SystemLanguageDetection] Available system languages: {string.Join(", ", languages)}");
                Debug.WriteLine($"[SystemLanguageDetection] Total languages count: {languages?.Count ?? 0}");

                if (languages?.Count > 0)
                {
                    var primaryLanguage = languages[0];
                    Debug.WriteLine($"[SystemLanguageDetection] Primary system language: {primaryLanguage}");
                    return primaryLanguage;
                }

                Debug.WriteLine("[SystemLanguageDetection] No languages found in GlobalizationPreferences");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SystemLanguageDetection] Exception accessing GlobalizationPreferences: {ex.Message}");
                // Fallback to current culture if Windows API fails
                var fallbackLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
                Debug.WriteLine($"[SystemLanguageDetection] Fallback to CurrentUICulture: {fallbackLanguage}");
                return fallbackLanguage;
            }

            // Final fallback
            Debug.WriteLine($"[SystemLanguageDetection] Using final fallback: {SupportedLanguages.DefaultLanguage}");
            return SupportedLanguages.DefaultLanguage;
        }

        /// <summary>
        /// Gets the best matching supported language based on system preferences
        /// </summary>
        /// <returns>Supported language code that best matches system preferences</returns>
        public string GetBestMatchingLanguage()
        {
            var systemLanguage = GetSystemLanguageCode();
            Debug.WriteLine($"[SystemLanguageDetection] System language code: {systemLanguage}");

            var mappedLanguage = SupportedLanguages.MapOSLanguageToSupported(systemLanguage);
            Debug.WriteLine($"[SystemLanguageDetection] Mapped to supported language: {mappedLanguage}");
            Debug.WriteLine($"[SystemLanguageDetection] Mapping successful: {systemLanguage} -> {mappedLanguage}");

            return mappedLanguage;
        }
    }
}
