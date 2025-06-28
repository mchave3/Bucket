namespace Bucket
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
            Logger.Verbose("Starting dependency injection container configuration");
            var services = new ServiceCollection();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<ImageManagementViewModel>();
            services.AddTransient<ImageDetailsViewModel>();
            services.AddSingleton<ContextMenuService>();
            services.AddTransient<GeneralSettingViewModel>();
            services.AddTransient<AppUpdateSettingViewModel>();
            services.AddTransient<AboutUsSettingViewModel>();

            // Pre-flight service
            services.AddSingleton<PreFlightService>();

            // Windows Image Services
            services.AddSingleton<IWindowsImageMetadataService, WindowsImageMetadataService>(provider =>
                new WindowsImageMetadataService(Constants.ImportedWIMsDirectoryPath));
            services.AddSingleton<IWindowsImageFileService, WindowsImageFileService>(provider =>
                new WindowsImageFileService(Constants.ImportedWIMsDirectoryPath));
            services.AddSingleton<IWindowsImagePowerShellService, WindowsImagePowerShellService>();
            services.AddSingleton<IWindowsImageIsoService, WindowsImageIsoService>();
            services.AddSingleton<IWindowsImageIndexEditingService, WindowsImageIndexEditingService>();
            services.AddSingleton<WindowsImageService>();

            var serviceProvider = services.BuildServiceProvider();
            Logger.Verbose("Dependency injection container configured with {ServiceCount} services", services.Count);
            return serviceProvider;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
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
            // Configure logger first thing
            ConfigureLogger();
            Logger.Information("Application starting - {ProductName} {Version}", ProcessInfoHelper.ProductName, ProcessInfoHelper.Version);

            // Run pre-flight checks
            Logger.Information("Running pre-flight system checks...");
            var preFlightService = GetService<PreFlightService>();
            var preFlightResult = await preFlightService.RunPreFlightChecksAsync();

            // Handle pre-flight results
            if (!preFlightResult.AllCriticalChecksPassed)
            {
                var errorMessage = $"Pre-flight checks failed:\n\n{string.Join("\n", preFlightResult.ErrorMessages)}";
                Logger.Fatal("Pre-flight checks failed, cannot start application");

                // Show error dialog and exit
                var dialog = new ContentDialog
                {
                    Title = "Bucket - Startup Failed",
                    Content = errorMessage,
                    CloseButtonText = "Exit",
                    XamlRoot = MainWindow?.Content?.XamlRoot
                };

                await dialog.ShowAsync();
                Application.Current.Exit();
                return;
            }

            // Log warnings if any
            if (preFlightResult.WarningMessages.Any())
            {
                Logger.Warning("Pre-flight warnings: {Warnings}", string.Join(", ", preFlightResult.WarningMessages));

                // Optionally show warning dialog (non-blocking)
                var warningDialog = new ContentDialog
                {
                    Title = "Bucket - System Warnings",
                    Content = $"Some system checks generated warnings:\n\n{string.Join("\n", preFlightResult.WarningMessages)}\n\nThe application will continue to run, but some features may be limited.",
                    PrimaryButtonText = "Continue",
                    XamlRoot = MainWindow?.Content?.XamlRoot
                };

                _ = warningDialog.ShowAsync(); // Fire and forget
            }

            var menuService = GetService<ContextMenuService>();
            if (menuService != null && RuntimeHelper.IsPackaged())
            {
                Logger.Debug("Configuring context menu service");
                ContextMenuItem menu = new ContextMenuItem
                {
                    Title = "Open Bucket Here",
                    Param = @"""{path}""",
                    AcceptFileFlag = (int)FileMatchFlagEnum.All,
                    AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background | DirectoryMatchFlagEnum.Desktop),
                    AcceptMultipleFilesFlag = (int)FilesMatchFlagEnum.Each,
                    Index = 0,
                    Enabled = true,
                    Icon = ProcessInfoHelper.GetFileVersionInfo().FileName,
                    Exe = "Bucket.exe"
                };

                await menuService.SaveAsync(menu);
                Logger.Information("Context menu configured successfully");
            }
            if (AppHelper.Settings.UseDeveloperMode)
            {
                Logger.Information("Developer mode enabled");
            }
            UnhandledException += (s, e) =>
            {
                Logger.Fatal(e.Exception, "Unhandled exception occurred");
                Logger.Information("Application terminating due to unhandled exception");
            };

            Logger.Information("Application initialization completed");
        }
    }

}
