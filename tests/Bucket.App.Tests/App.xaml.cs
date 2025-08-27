namespace Bucket.App.Tests
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public static Window MainWindow = Window.Current;
        public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);

        public App()
        {
            this.InitializeComponent();

            // Enables Multicore JIT with the specified profile
            System.Runtime.ProfileOptimization.SetProfileRoot(Constants.RootDirectoryPath);
            System.Runtime.ProfileOptimization.StartProfile("Startup.Profile");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.CreateDefaultUI();

            MainWindow = new MainWindow();

            MainWindow.Title = MainWindow.AppWindow.Title = "Bucket.App.Tests";
            MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");
            MainWindow.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            MainWindow.Activate();

            UITestMethodAttribute.DispatcherQueue = MainWindow.DispatcherQueue;
            Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(Environment.CommandLine);
        }
    }

}
