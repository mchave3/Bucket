using Microsoft.UI.Windowing;
using Bucket.Core.Helpers;

namespace Bucket.App.Views
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public string AppVersion => VersionHelper.GetAppVersion();

        public MainWindow()
        {
            ViewModel = App.GetService<MainViewModel>();
            this.InitializeComponent();
            Instance = this;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

            var navService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
            if (navService != null)
            {
                navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                    .ConfigureDefaultPage(typeof(HomeLandingPage))
                    .ConfigureSettingsPage(typeof(SettingsPage))
                    .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
                    .ConfigureTitleBar(AppTitleBar)
                    .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.ChangeThemeWithoutSave(App.MainWindow);
        }

        private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
        }

        private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
        }

        public void ReInitialize()
        {
            var navService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
            if (navService != null)
            {
                navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                    .ConfigureDefaultPage(typeof(HomeLandingPage))
                    .ConfigureSettingsPage(typeof(SettingsPage))
                    .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
                    .ConfigureTitleBar(AppTitleBar)
                    .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
            }
        }

        internal static MainWindow Instance { get; private set; }
    }

}
