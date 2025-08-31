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
                    try
                    {
                        var languages = GlobalizationPreferences.Languages;
                        if (languages?.Count > 0)
                        {
                            return languages[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception for debugging but don't throw
                        System.Diagnostics.Debug.WriteLine($"WinRT API call failed: {ex.Message}");
                    }
                    return null;
                });

                // Wait with timeout to avoid infinite blocking - reduced timeout for CI
                var timeout = System.Diagnostics.Debugger.IsAttached ?
                    TimeSpan.FromSeconds(10) :  // Longer timeout when debugging
                    TimeSpan.FromSeconds(2);    // Shorter timeout for CI/production

                if (task.Wait(timeout))
                {
                    var result = task.Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
                else
                {
                    // Task timed out - likely in CI environment or Windows API unavailable
                    System.Diagnostics.Debug.WriteLine("GlobalizationPreferences.Languages call timed out - using fallback");
                }
            }
            catch (Exception ex)
            {
                // Log exception and continue to fallback
                System.Diagnostics.Debug.WriteLine($"Failed to get system language: {ex.Message}");
            }

            // Fallback to current culture if Windows API fails or times out
            try
            {
                var fallbackLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
                if (!string.IsNullOrEmpty(fallbackLanguage))
                {
                    return fallbackLanguage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback culture detection failed: {ex.Message}");
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
