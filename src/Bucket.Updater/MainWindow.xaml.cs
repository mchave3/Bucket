using Microsoft.UI.Windowing;

namespace Bucket.Updater.Views
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = App.GetService<MainViewModel>();
            this.InitializeComponent();
            
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            
            SetWindowSize();
            NavigateToUpdateCheckPage();
        }

        private void SetWindowSize()
        {
            const int WindowWidth = 820;
            const int WindowHeight = 650;

            var appWindow = this.AppWindow;
            appWindow.Resize(new Windows.Graphics.SizeInt32(WindowWidth, WindowHeight));
            
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }

            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var centeredPosition = new Windows.Graphics.PointInt32(
                    (displayArea.WorkArea.Width - WindowWidth) / 2,
                    (displayArea.WorkArea.Height - WindowHeight) / 2
                );
                appWindow.Move(centeredPosition);
            }
        }

        private void NavigateToUpdateCheckPage()
        {
            ContentFrame.Navigate(typeof(UpdateCheckPage));
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page {e.SourcePageType.FullName}: {e.Exception}");
        }
    }
}
