using Windows.Storage;
using WinUI3Localizer;
using Bucket.App.Services;
using Bucket.Core.Services;

namespace Bucket.App
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public static Window MainWindow { get; private set; }
        public static IntPtr Hwnd => MainWindow != null ? WinRT.Interop.WindowNative.GetWindowHandle(MainWindow) : IntPtr.Zero;
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

            // Register new unified localization system
            services.AddSingleton<IPlatformLocalizer, WinUI3PlatformLocalizer>();
            services.AddSingleton<IPlatformLanguageDetector, WindowsPlatformLanguageDetector>();
            services.AddSingleton<IPlatformUIRefresher, WinUIPlatformUIRefresher>();

            // Register centralized LocalizationManager
            services.AddSingleton<LocalizationManager>(provider =>
            {
                return new LocalizationManager(
                    provider.GetRequiredService<IPlatformLocalizer>(),
                    provider.GetRequiredService<IPlatformLanguageDetector>(),
                    provider.GetRequiredService<IPlatformUIRefresher>(),
                    language => Settings.SelectedLanguage = language
                );
            });

            services.AddTransient<MainViewModel>();
            services.AddSingleton<ContextMenuService>();
            services.AddSingleton<IUpdateService, UpdateService>();
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

            ThemeService.Initialize(MainWindow);

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

            // Check for updates on startup
            _ = Task.Run(CheckForUpdatesOnStartupAsync);
        }

        private async Task InitializeLocalizationServiceAsync()
        {
            var localizationManager = GetService<LocalizationManager>();

            // Check if this is the first startup
            bool isFirstStartup = !Settings.HasBeenStartedBefore;
            string savedLanguage = Settings.SelectedLanguage;

            // Initialize with auto-detection for first startup
            await localizationManager.InitializeWithAutoDetectionAsync(savedLanguage, isFirstStartup);

            // CRITICAL: After initialization, ensure DevWinUI uses the same language
            // Get the current language from the localization manager (could be auto-detected or saved)
            string currentLanguage = localizationManager.CurrentLanguage;
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

        /// <summary>
        /// Checks for application updates on startup and prompts user if update is available
        /// </summary>
        private async Task CheckForUpdatesOnStartupAsync()
        {
            try
            {
                // Add a small delay to avoid blocking startup
                await Task.Delay(3000);

                var updateService = GetService<IUpdateService>();
                if (updateService == null)
                {
                    Logger?.Warning("UpdateService not available");
                    return;
                }

                // Only check if updater is available
                if (!updateService.IsUpdaterAvailable())
                {
                    Logger?.Information("Updater not available, skipping startup update check");
                    return;
                }

                Logger?.Information("Checking for updates on startup");
                var updateInfo = await updateService.GetUpdateInfoAsync();
                if (updateInfo != null)
                {
                    Logger?.Information("Update available: {Version}", updateInfo.Version);

                    // Show update dialog on UI thread
                    await MainWindow.DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await ShowUpdateAvailableDialogAsync(updateInfo, updateService);
                    });
                }
                else
                {
                    Logger?.Information("No updates available on startup");
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error during startup update check");
            }
        }

        /// <summary>
        /// Shows a dialog informing the user that an update is available
        /// </summary>
        /// <param name="updateInfo">Information about the available update</param>
        /// <param name="updateService">Update service for launching the updater</param>
        private async Task ShowUpdateAvailableDialogAsync(UpdateInfo updateInfo, IUpdateService updateService)
        {
            try
            {
                var updateType = updateInfo.IsPreRelease ? "Pre-release" : "Stable release";
                var dialogContent = $"A new {updateType.ToLower()} version is available!\n\n" +
                                  $"Current version: {ProcessInfoHelper.VersionWithPrefix}\n" +
                                  $"Available version: {updateInfo.Version}\n" +
                                  $"Published: {updateInfo.PublishedAt:yyyy-MM-dd}\n\n" +
                                  "Would you like to update now?";

                var dialog = new ContentDialog
                {
                    Title = $"Update Available - {updateType}",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = dialogContent,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(10)
                        },
                        MaxHeight = 300
                    },
                    PrimaryButtonText = "Update Now",
                    SecondaryButtonText = "Show Release Notes",
                    CloseButtonText = "Later",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = MainWindow.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                switch (result)
                {
                    case ContentDialogResult.Primary:
                        Logger?.Information("User chose to update now");
                        var launched = await updateService.LaunchUpdaterAsync();
                        if (!launched)
                        {
                            await ShowUpdateErrorDialogAsync();
                        }
                        break;

                    case ContentDialogResult.Secondary:
                        Logger?.Information("User chose to view release notes");
                        await ShowReleaseNotesDialogAsync(updateInfo);
                        // After showing release notes, ask again
                        await ShowUpdateAvailableDialogAsync(updateInfo, updateService);
                        break;

                    case ContentDialogResult.None:
                        Logger?.Information("User chose to update later");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error showing update dialog");
            }
        }

        /// <summary>
        /// Shows the release notes for the available update
        /// </summary>
        /// <param name="updateInfo">Update information containing changelog</param>
        private async Task ShowReleaseNotesDialogAsync(UpdateInfo updateInfo)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = $"Release Notes - {updateInfo.Version}",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = string.IsNullOrEmpty(updateInfo.Changelog)
                                ? "No release notes available."
                                : updateInfo.Changelog,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(10)
                        },
                        MaxHeight = 400
                    },
                    CloseButtonText = "Close",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = MainWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error showing release notes dialog");
            }
        }

        /// <summary>
        /// Shows an error dialog when the updater fails to launch
        /// </summary>
        private async Task ShowUpdateErrorDialogAsync()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Update Error",
                    Content = "Failed to launch the updater. Please try updating manually from the GitHub releases page.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = MainWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Error showing update error dialog");
            }
        }
    }

}
