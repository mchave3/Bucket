using WinUI3Localizer;

namespace Bucket.App.Views
{
    public record LanguageItem(string Code, string DisplayName);

    public sealed partial class GeneralSettingPage : Page
    {
        public GeneralSettingViewModel ViewModel { get; }

        public GeneralSettingPage()
        {
            ViewModel = App.GetService<GeneralSettingViewModel>();
            this.InitializeComponent();
            this.Loaded += GeneralSettingPage_Loaded;
        }

        private void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvailableLanguages();
        }

        private void LoadAvailableLanguages()
        {
            var availableLanguages = new List<LanguageItem>
            {
                new("en-US", "English"),
                new("fr-FR", "Français")
            };

            LanguageComboBox.ItemsSource = availableLanguages;

            // Select the current language from configuration
            string currentLanguage = Settings.SelectedLanguage;
            var currentItem = availableLanguages.FirstOrDefault(x => x.Code == currentLanguage);
            if (currentItem != null)
            {
                LanguageComboBox.SelectedItem = currentItem;
            }
            else
            {
                // Fallback to default language if not found
                var defaultItem = availableLanguages.FirstOrDefault(x => x.Code == "en-US");
                LanguageComboBox.SelectedItem = defaultItem;
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is LanguageItem selectedLanguage)
            {
                try
                {
                    await Localizer.Get().SetLanguage(selectedLanguage.Code);

                    // Save the selected language in configuration
                    Settings.SelectedLanguage = selectedLanguage.Code;
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
