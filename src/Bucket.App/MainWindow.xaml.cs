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

            // Add window closed event handler for application shutdown
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

        /// <summary>
        /// Handles the main window closing event.
        /// Performs application shutdown by cleaning up the Serilog logger and safely terminating the process.
        /// </summary>
        private void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            // Shutdown Serilog logger and safely exit application
            _ = Task.Run(() =>
            {
                CleanupSerilogLogger();
                SafeShutdownService.InitiateSafeShutdown();
            });
        }

        /// <summary>
        /// Cleanly shuts down the Serilog logger system.
        /// Ensures all log buffers are flushed and resources are properly released.
        /// </summary>
        private static void CleanupSerilogLogger()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Shutting down Serilog logger...");
                LoggerSetup.Shutdown();
                System.Diagnostics.Debug.WriteLine("Serilog shutdown complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Serilog cleanup error: {ex.Message}");
            }
        }

    }

}
