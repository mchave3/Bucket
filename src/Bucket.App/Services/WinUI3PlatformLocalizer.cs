using WinUI3Localizer;
using Windows.Storage;
using Bucket.Core.Services;

namespace Bucket.App.Services
{
    /// <summary>
    /// Minimal WinUI3 platform-specific localizer implementation
    /// </summary>
    public class WinUI3PlatformLocalizer : IPlatformLocalizer
    {
        private ILocalizer _localizer;

        /// <summary>
        /// Initializes the WinUI3Localizer with the specified language
        /// </summary>
        /// <param name="languageCode">Language code to initialize with</param>
        public async Task InitializeAsync(string languageCode)
        {
            try
            {
                // Initialize the "Strings" folder in the executables folder
                string stringsFolderPath = Path.Combine(AppContext.BaseDirectory, "Strings");
                StorageFolder stringsFolder = await StorageFolder.GetFolderFromPathAsync(stringsFolderPath);

                _localizer = await new LocalizerBuilder()
                    .AddStringResourcesFolderForLanguageDictionaries(stringsFolderPath)
                    .SetOptions(options =>
                    {
                        options.DefaultLanguage = SupportedLanguages.DefaultLanguage;
                    })
                    .Build();

                // Set the initial language
                if (_localizer != null)
                {
                    await _localizer.SetLanguage(languageCode);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize WinUI3 localizer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sets the language in WinUI3Localizer
        /// </summary>
        /// <param name="languageCode">Language code to set</param>
        public async Task<bool> SetLanguageAsync(string languageCode)
        {

            try
            {
                await _localizer.SetLanguage(languageCode);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set WinUI3 language to {languageCode}: {ex.Message}");
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

            try
            {
                return _localizer.GetLocalizedString(key);
            }
            catch (Exception)
            {
                return key; // Return key if localization fails
            }
        }
    }
}