using Bucket.App.Services;
using Bucket.Core.Services;

namespace Bucket.App.Views
{
    // Removed local definition - using Bucket.Core.Models.LanguageItem

    public sealed partial class GeneralSettingPage : Page
    {
        public GeneralSettingViewModel ViewModel { get; }
        private readonly LocalizationService _localizationService;
        private bool _isLoadingLanguages = false;

        public GeneralSettingPage()
        {
            ViewModel = App.GetService<GeneralSettingViewModel>();
            _localizationService = App.GetService<LocalizationService>();
            this.InitializeComponent();
            this.Loaded += GeneralSettingPage_Loaded;
        }

        private void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvailableLanguages();
        }

        private void LoadAvailableLanguages()
        {
            _isLoadingLanguages = true;

            try
            {
                // Use centralized language list from the localization service
                var availableLanguages = LocalizationService.AllSupportedLanguages.ToList();

                LanguageComboBox.ItemsSource = availableLanguages;

                // Select the current language from the localization service
                string currentLanguage = _localizationService.CurrentLanguage;
                var currentItem = availableLanguages.FirstOrDefault(x => x.Code == currentLanguage);
                if (currentItem != null)
                {
                    LanguageComboBox.SelectedItem = currentItem;
                }
                else
                {
                    // Fallback to default language if not found
                    var defaultItem = availableLanguages.FirstOrDefault();
                    LanguageComboBox.SelectedItem = defaultItem;
                }
            }
            finally
            {
                _isLoadingLanguages = false;
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignore selection changes during loading to prevent infinite loop
            if (_isLoadingLanguages)
                return;

            if (e.AddedItems.FirstOrDefault() is LanguageItem selectedLanguage)
            {
                try
                {
                    // LocalizationService handles everything: save to config, platform language change, UI refresh
                    bool success = await _localizationService.SetLanguageAsync(selectedLanguage.Code);

                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to set language to {selectedLanguage.Code}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error if necessary
                    System.Diagnostics.Debug.WriteLine($"Error while changing language: {ex.Message}");
                }
            }
        }

        private async void NavigateToLogPath_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = (sender as HyperlinkButton).Content.ToString();
            if (Directory.Exists(folderPath))
            {
                Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                await Windows.System.Launcher.LaunchFolderAsync(folder);
            }
        }
    }
}
