using WinUI3Localizer;
using Windows.Storage;
using Bucket.Core.Models;

namespace Bucket.App.Services
{
    /// <summary>
    /// WinUI3 implementation of the localization service
    /// Now using centralized SupportedLanguages constants
    /// </summary>
    public class WinUI3LocalizationService
    {
        private ILocalizer _localizer;
        private string _currentLanguage = SupportedLanguages.DefaultLanguage;

        public string CurrentLanguage => _currentLanguage;

        public async Task InitializeAsync(string savedLanguageCode = null)
        {
            try
            {
                // Initialize a "Strings" folder in the executables folder
                string stringsFolderPath = Path.Combine(AppContext.BaseDirectory, "Strings");
                StorageFolder stringsFolder = await StorageFolder.GetFolderFromPathAsync(stringsFolderPath);

                _localizer = await new LocalizerBuilder()
                    .AddStringResourcesFolderForLanguageDictionaries(stringsFolderPath)
                    .SetOptions(options =>
                    {
                        options.DefaultLanguage = SupportedLanguages.DefaultLanguage;
                    })
                    .Build();

                // Set the saved language or default
                string languageToSet = string.IsNullOrWhiteSpace(savedLanguageCode) ? SupportedLanguages.DefaultLanguage : savedLanguageCode;
                await SetLanguageInternalAsync(languageToSet);
            }
            catch (Exception ex)
            {
                // Log error if logger is available
                System.Diagnostics.Debug.WriteLine($"Failed to initialize localization: {ex.Message}");
                _currentLanguage = SupportedLanguages.DefaultLanguage;
            }
        }

        public async Task<bool> SetLanguageAsync(string languageCode)
        {
            if (_localizer == null || string.IsNullOrWhiteSpace(languageCode))
            {
                return false;
            }

            if (_currentLanguage.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return true; // No change needed
            }

            return await SetLanguageInternalAsync(languageCode);
        }

        private async Task<bool> SetLanguageInternalAsync(string languageCode)
        {
            if (_localizer == null)
                return false;

            try
            {
                await _localizer.SetLanguage(languageCode);
                _currentLanguage = languageCode;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set language to {languageCode}: {ex.Message}");
                return false;
            }
        }
    }
}