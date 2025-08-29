namespace Bucket.Core.Services
{
    /// <summary>
    /// Service for detecting system language preferences
    /// </summary>
    public interface ISystemLanguageDetectionService
    {
        /// <summary>
        /// Gets the current system language code
        /// </summary>
        /// <returns>System language code (e.g., "en-US", "fr-FR")</returns>
        string GetSystemLanguageCode();

        /// <summary>
        /// Gets the best matching supported language based on system preferences
        /// </summary>
        /// <returns>Supported language code that best matches system preferences</returns>
        string GetBestMatchingLanguage();
    }
}
