namespace Bucket.Core.Models
{
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
            return GetByCode(DefaultLanguage) ?? All.First();
        }

        /// <summary>
        /// Maps an OS language code to a supported language code
        /// </summary>
        /// <param name="osLanguageCode">OS language code (e.g., "en-US", "fr-FR", "fr-CA", "en-GB")</param>
        /// <returns>Supported language code or default if not supported</returns>
        public static string MapOSLanguageToSupported(string osLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(osLanguageCode))
                return DefaultLanguage;

            // Direct match first
            if (IsSupported(osLanguageCode))
                return osLanguageCode;

            // Try to match by language family (e.g., "fr-CA" -> "fr-FR", "en-GB" -> "en-US")
            var languageFamily = osLanguageCode.Split('-')[0].ToLowerInvariant();

            var matchingLanguage = All.FirstOrDefault(lang =>
                lang.Code.Split('-')[0].Equals(languageFamily, StringComparison.OrdinalIgnoreCase));

            return matchingLanguage?.Code ?? DefaultLanguage;
        }
    }
}
