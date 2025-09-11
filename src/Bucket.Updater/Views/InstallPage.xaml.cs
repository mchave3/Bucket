namespace Bucket.Updater.Views
{
    public sealed partial class InstallPage : Page
    {
        public InstallPageViewModel ViewModel { get; }
        private DispatcherTimer? _scrollTimer;

        public InstallPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<InstallPageViewModel>();
            DataContext = ViewModel;

            // Initialize scroll timer for debouncing
            _scrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _scrollTimer.Tick += ScrollTimer_Tick;

            // Subscribe to log changes for auto-scroll
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.InstallationLog))
            {
                // Debounce scroll requests to avoid excessive scrolling
                _scrollTimer?.Stop();
                _scrollTimer?.Start();
            }
        }

        private void ScrollTimer_Tick(object? sender, object e)
        {
            _scrollTimer?.Stop();
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            try
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (LogScrollViewer?.ScrollableHeight > 0)
                    {
                        LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, false);
                    }
                });
            }
            catch
            {
                // Ignore any exceptions during scrolling
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is InstallInfo installInfo)
            {
                ViewModel.StartInstallation(installInfo);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Unsubscribe to prevent memory leaks
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            // Cleanup timer
            _scrollTimer?.Stop();
            if (_scrollTimer != null)
            {
                _scrollTimer.Tick -= ScrollTimer_Tick;
                _scrollTimer = null;
            }

            // Cleanup ViewModel
            ViewModel.Cleanup();
        }
    }
}