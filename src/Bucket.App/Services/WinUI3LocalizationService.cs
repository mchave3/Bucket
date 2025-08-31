using WinUI3Localizer;
using Windows.Storage;
using Bucket.Core.Models;
using Bucket.Core.Services;

namespace Bucket.App.Services
{
    /// <summary>
    /// WinUI3 implementation of the localization service with automatic OS language detection
    /// </summary>
    public class WinUI3LocalizationService : ILocalizationService
    {
        private readonly ISystemLanguageDetectionService _systemLanguageDetection;
        private readonly Action<string> _saveLanguageToConfig;
        private ILocalizer _localizer;
        private string _currentLanguage = Core.Models.SupportedLanguages.DefaultLanguage;

        public string CurrentLanguage => _currentLanguage;
        public IReadOnlyList<LanguageItem> SupportedLanguages => Core.Models.SupportedLanguages.All;

        public event EventHandler<Core.Services.LanguageChangedEventArgs> LanguageChanged;

        public WinUI3LocalizationService(
            ISystemLanguageDetectionService systemLanguageDetection,
            Action<string> saveLanguageToConfig)
        {
            _systemLanguageDetection = systemLanguageDetection ?? throw new ArgumentNullException(nameof(systemLanguageDetection));
            _saveLanguageToConfig = saveLanguageToConfig ?? throw new ArgumentNullException(nameof(saveLanguageToConfig));
        }

        public async Task InitializeAsync(string savedLanguageCode = null)
        {
            await InitializeInternalAsync(savedLanguageCode, false);
        }

        public async Task InitializeWithAutoDetectionAsync(string savedLanguageCode = null, bool isFirstStartup = false)
        {
            await InitializeInternalAsync(savedLanguageCode, isFirstStartup);
        }

        private async Task InitializeInternalAsync(string savedLanguageCode, bool isFirstStartup)
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
                        options.DefaultLanguage = Core.Models.SupportedLanguages.DefaultLanguage;
                    })
                    .Build();

                // Determine which language to use
                string languageToSet;

                if (isFirstStartup)
                {
                    // First startup - detect OS language
                    languageToSet = _systemLanguageDetection.GetBestMatchingLanguage();

                    // Save the detected language to config
                    _saveLanguageToConfig(languageToSet);
                }
                else
                {
                    // Use saved language or default
                    languageToSet = string.IsNullOrWhiteSpace(savedLanguageCode) ? Core.Models.SupportedLanguages.DefaultLanguage : savedLanguageCode;
                }

                await SetLanguageInternalAsync(languageToSet);
            }
            catch (Exception ex)
            {
                // Log error if logger is available
                System.Diagnostics.Debug.WriteLine($"Failed to initialize localization: {ex.Message}");
                _currentLanguage = Core.Models.SupportedLanguages.DefaultLanguage;
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

            var oldLanguage = _currentLanguage;
            bool success = await SetLanguageInternalAsync(languageCode);

            if (success)
            {
                // Save the new language to config
                _saveLanguageToConfig(languageCode);

                // Notify language change
                LanguageChanged?.Invoke(this, new Core.Services.LanguageChangedEventArgs(oldLanguage, languageCode));
            }

            return success;
        }

        public string GetString(string key)
        {
            if (_localizer == null)
                return key;

            try
            {
                return _localizer.GetLocalizedString(key);
            }
            catch (Exception)
            {
                return key; // Return key if localization fails
            }
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