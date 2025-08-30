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
                // Use Task.Run with timeout to prevent indefinite blocking of WinRT APIs
                var task = Task.Run(() =>
                {
                    var languages = GlobalizationPreferences.Languages;
                    if (languages?.Count > 0)
                    {
                        return languages[0];
                    }
                    return null;
                });

                // Wait with timeout to avoid infinite blocking
                if (task.Wait(TimeSpan.FromSeconds(5)))
                {
                    var result = task.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                // Fallback to current culture if Windows API fails
                var fallbackLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
                if (!string.IsNullOrEmpty(fallbackLanguage))
                {
                    return fallbackLanguage;
                }
            }

            // Final fallback
            return SupportedLanguages.DefaultLanguage;
        }

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
    }
}
