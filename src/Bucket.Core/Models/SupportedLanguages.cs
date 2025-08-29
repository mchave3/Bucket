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
    }
}
