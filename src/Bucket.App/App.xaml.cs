using Windows.Storage;
using WinUI3Localizer;
using Bucket.App.Services;
using Bucket.Core.Services;

namespace Bucket.App
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public static Window MainWindow = Window.Current;
        public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
        public IServiceProvider Services { get; }
        public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
        public IThemeService ThemeService => GetService<IThemeService>();

        public static T GetService<T>() where T : class
        {
            if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();

            // Enables Multicore JIT with the specified profile
            System.Runtime.ProfileOptimization.SetProfileRoot(Constants.RootDirectoryPath);
            System.Runtime.ProfileOptimization.StartProfile("Startup.Profile");
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IJsonNavigationService, JsonNavigationService>();

            // Register language detection service
            services.AddSingleton<ISystemLanguageDetectionService, WindowsSystemLanguageDetectionService>();

            // Register localization service with factory pattern
            services.AddSingleton<ILocalizationService>(provider =>
            {
                var systemLanguageDetection = provider.GetRequiredService<ISystemLanguageDetectionService>();
                return new WinUI3LocalizationService(
                    systemLanguageDetection,
                    language => Settings.SelectedLanguage = language
                );
            });

            // Keep backward compatibility
            services.AddSingleton<WinUI3LocalizationService>(provider =>
                (WinUI3LocalizationService)provider.GetRequiredService<ILocalizationService>());

            services.AddTransient<MainViewModel>();
            services.AddSingleton<ContextMenuService>();
            services.AddTransient<GeneralSettingViewModel>();
            services.AddTransient<AppUpdateSettingViewModel>();
            services.AddTransient<AboutUsSettingViewModel>();

            return services.BuildServiceProvider();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // IMPORTANT: Initialize localization BEFORE creating MainWindow
            // to ensure NavService gets the correct language from the start
            await InitializeLocalizationServiceAsync();

            // After language is set, initialize the main window
            InitializeMainWindow();
        }

        private void InitializeMainWindow()
        {
            MainWindow = new MainWindow();

            MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
            MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

            ThemeService.AutoInitialize(MainWindow);

            MainWindow.Activate();

            InitializeApp();
        }

        private async void InitializeApp()
        {
            var menuService = GetService<ContextMenuService>();
            if (menuService != null && RuntimeHelper.IsPackaged())
            {
                ContextMenuItem menu = new ContextMenuItem
                {
                    Title = "Open Bucket.App Here",
                    Param = @"""{path}""",
                    AcceptFileFlag = (int)FileMatchFlagEnum.All,
                    AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background | DirectoryMatchFlagEnum.Desktop),
                    AcceptMultipleFilesFlag = (int)FilesMatchFlagEnum.Each,
                    Index = 0,
                    Enabled = true,
                    Icon = ProcessInfoHelper.GetFileVersionInfo().FileName,
                    Exe = "Bucket.App.exe"
                };

                await menuService.SaveAsync(menu);
            }

            if (Settings.UseDeveloperMode)
            {
                ConfigureLogger();
            }

            UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
        }

        private async Task InitializeLocalizationServiceAsync()
        {
            var localizationService = GetService<ILocalizationService>();

            // Check if this is the first startup
            bool isFirstStartup = !Settings.HasBeenStartedBefore;
            string savedLanguage = Settings.SelectedLanguage;

            // For non-first startup, set the language override immediately
            if (!isFirstStartup && !string.IsNullOrEmpty(savedLanguage))
            {
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = savedLanguage;
            }

            // Initialize with auto-detection for first startup
            await localizationService.InitializeWithAutoDetectionAsync(savedLanguage, isFirstStartup);

            // CRITICAL: After initialization, ensure DevWinUI uses the same language
            // Get the current language from the localization service (could be auto-detected or saved)
            string currentLanguage = localizationService.CurrentLanguage;
            if (!string.IsNullOrEmpty(currentLanguage))
            {
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = currentLanguage;
                System.Diagnostics.Debug.WriteLine($"Language synchronization: Set ApplicationLanguages.PrimaryLanguageOverride to '{currentLanguage}' (FirstStartup: {isFirstStartup})");
            }

            // Mark as started if it was the first time
            if (isFirstStartup)
            {
                Settings.HasBeenStartedBefore = true;
            }
        }
    }

}
