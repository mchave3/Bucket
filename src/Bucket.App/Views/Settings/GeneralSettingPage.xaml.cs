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

            // Sélectionner la langue actuelle
            string currentLanguage = Localizer.Get().GetCurrentLanguage();
            var currentItem = availableLanguages.FirstOrDefault(x => x.Code == currentLanguage);
            if (currentItem != null)
            {
                LanguageComboBox.SelectedItem = currentItem;
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is LanguageItem selectedLanguage)
            {
                try
                {
                    await Localizer.Get().SetLanguage(selectedLanguage.Code);
                }
                catch (Exception ex)
                {
                    // Log l'erreur si nécessaire
                    System.Diagnostics.Debug.WriteLine($"Erreur lors du changement de langue: {ex.Message}");
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
