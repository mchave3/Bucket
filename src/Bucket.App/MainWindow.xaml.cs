using Microsoft.UI.Windowing;
using Bucket.Core.Helpers;
using Bucket.Core.Services;
using Bucket.App.Services;
using Bucket.App.Common;

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

            // Add window closed event handler for cleanup
            this.Closed += MainWindow_Closed;
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.ThemeService.SetElementThemeWithoutSaveAsync();
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

        private void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            // Cleanup resources before shutdown
            _ = Task.Run(async () =>
            {
                await CleanupResourcesAsync();
                // Use safe shutdown service to avoid WinRT crash
                SafeShutdownService.InitiateSafeShutdown();
            });
        }

        private async Task CleanupResourcesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting async cleanup...");

                // 1. First dispose localization resources asynchronously
                try
                {
                    var platformLocalizer = App.GetService<IPlatformLocalizer>();
                    if (platformLocalizer is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                        System.Diagnostics.Debug.WriteLine("Localization disposed asynchronously");
                    }
                    else if (platformLocalizer is IDisposable disposable)
                    {
                        disposable.Dispose();
                        System.Diagnostics.Debug.WriteLine("Localization disposed synchronously");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Localization cleanup error: {ex.Message}");
                }

                // 2. Then cleanup logger (this might log, so do it last)
                try
                {
                    LoggerSetup.Shutdown();
                    System.Diagnostics.Debug.WriteLine("Logger shutdown complete");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Logger cleanup error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General cleanup error: {ex.Message}");
            }
        }

    }

}
