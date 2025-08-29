using Bucket.Core.Models;

namespace Bucket.Core.Services
{
    /// <summary>
    /// Interface for localization services
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the current language code
        /// </summary>
        string CurrentLanguage { get; }

        /// <summary>
        /// Gets all supported languages
        /// </summary>
        IReadOnlyList<LanguageItem> SupportedLanguages { get; }

        /// <summary>
        /// Event raised when the language changes
        /// </summary>
        event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

        /// <summary>
        /// Sets the current language
        /// </summary>
        /// <param name="languageCode">Language code to set</param>
        /// <returns>Task representing the operation</returns>
        Task<bool> SetLanguageAsync(string languageCode);

        /// <summary>
        /// Initializes the localization service with the saved language
        /// </summary>
        /// <param name="savedLanguageCode">Previously saved language code</param>
        /// <returns>Task representing the operation</returns>
        Task InitializeAsync(string? savedLanguageCode = null);

        /// <summary>
        /// Gets a localized string by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string or key if not found</returns>
        string GetString(string key);
    }

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
}
