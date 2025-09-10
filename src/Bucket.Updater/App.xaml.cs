namespace Bucket.Updater
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public static Window MainWindow { get; private set; }
        public static IntPtr Hwnd => MainWindow != null ? WinRT.Interop.WindowNative.GetWindowHandle(MainWindow) : IntPtr.Zero;
        public IServiceProvider Services { get; }
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

            // DevWinUI Services
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ContextMenuService>();

            // Configuration Services
            services.AddSingleton<IAppConfigReader, AppConfigReader>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            // Application Services
            services.AddSingleton<IGitHubService, GitHubService>();
            services.AddSingleton<IInstallationService, InstallationService>();
            services.AddSingleton<IUpdateService, UpdateService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<UpdateCheckPageViewModel>();
            services.AddTransient<DownloadInstallPageViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();

            MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
            MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

            ThemeService.AutoInitialize(MainWindow);

            MainWindow.Activate();

            InitializeApp();

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        private async void InitializeApp()
        {
            // Configure logging
            ConfigureLogger();
            Logger?.Information("Bucket Updater started");

            // Setup unhandled exception logging
            UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");

            var menuService = GetService<ContextMenuService>();
            if (menuService != null && RuntimeHelper.IsPackaged())
            {
                ContextMenuItem menu = new ContextMenuItem
                {
                    Title = "Open Bucket.Updater Here",
                    Param = @"""{path}""",
                    AcceptFileFlag = (int)FileMatchFlagEnum.All,
                    AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background | DirectoryMatchFlagEnum.Desktop),
                    AcceptMultipleFilesFlag = (int)FilesMatchFlagEnum.Each,
                    Index = 0,
                    Enabled = true,
                    Icon = ProcessInfoHelper.GetFileVersionInfo().FileName,
                    Exe = "Bucket.Updater.exe"
                };

                await menuService.SaveAsync(menu);
            }
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            Logger?.Information("Bucket Updater shutting down");
        }
    }
}
