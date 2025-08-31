using Bucket.Core.Models;

namespace Bucket.Core.Helpers
{
    /// <summary>
    /// Helper class for localization-related operations
    /// </summary>
    public static class LocalizationHelper
    {
        /// <summary>
        /// Validates a language code and returns a valid one
        /// </summary>
        /// <param name="languageCode">Language code to validate</param>
        /// <returns>Valid language code (may fallback to default)</returns>
        public static string ValidateLanguageCode(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return SupportedLanguages.DefaultLanguage;

            var supportedLanguage = SupportedLanguages.GetByCode(languageCode);
            return supportedLanguage?.Code ?? SupportedLanguages.DefaultLanguage;
        }

        /// <summary>
        /// Gets the appropriate language item for a given code
        /// </summary>
        /// <param name="languageCode">Language code</param>
        /// <returns>LanguageItem (falls back to default if not found)</returns>
        public static LanguageItem GetLanguageItem(string? languageCode)
        {
            var validCode = ValidateLanguageCode(languageCode);
            return SupportedLanguages.GetByCode(validCode) ?? SupportedLanguages.GetDefault();
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
}
