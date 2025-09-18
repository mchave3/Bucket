namespace Bucket.Updater.Views
{
    /// <summary>
    /// Page for displaying installation progress with auto-scrolling log view
    /// </summary>
    public sealed partial class InstallPage : Page
    {
        /// <summary>
        /// ViewModel for managing installation process and state
        /// </summary>
        public InstallPageViewModel ViewModel { get; }
        
        /// <summary>
        /// Timer for debouncing auto-scroll operations
        /// </summary>
        private DispatcherTimer? _scrollTimer;

        /// <summary>
        /// Initializes the install page with dependency injection and auto-scroll setup
        /// </summary>
        public InstallPage()
        {
            this.InitializeComponent();
            
            // Get ViewModel through dependency injection
            ViewModel = App.GetService<InstallPageViewModel>();
            DataContext = ViewModel;

            // Initialize scroll timer for debouncing scroll requests
            _scrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 100ms debounce delay
            };
            _scrollTimer.Tick += ScrollTimer_Tick;

            // Subscribe to log changes to trigger auto-scroll
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        /// <summary>
        /// Handles ViewModel property changes and triggers auto-scroll when log updates
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.InstallationLog))
            {
                // Debounce scroll requests to avoid excessive scrolling during rapid updates
                _scrollTimer?.Stop();
                _scrollTimer?.Start();
            }
        }

        /// <summary>
        /// Timer tick handler that executes the actual scroll operation
        /// </summary>
        private void ScrollTimer_Tick(object? sender, object e)
        {
            _scrollTimer?.Stop();
            ScrollToBottom();
        }

        /// <summary>
        /// Scrolls the log viewer to the bottom to show latest messages
        /// </summary>
        private void ScrollToBottom()
        {
            try
            {
                // Queue scroll operation on UI thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    // Only scroll if there's content to scroll
                    if (LogScrollViewer?.ScrollableHeight > 0)
                    {
                        LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, false);
                    }
                });
            }
            catch
            {
                // Ignore any exceptions during scrolling to prevent crashes
            }
        }

        /// <summary>
        /// Handles navigation to this page and starts installation if InstallInfo is provided
        /// </summary>
        /// <param name="e">Navigation event arguments containing optional InstallInfo parameter</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Start installation if InstallInfo parameter was passed during navigation
            if (e.Parameter is InstallInfo installInfo)
            {
                ViewModel.StartInstallation(installInfo);
            }
        }

        /// <summary>
        /// Handles navigation away from this page and performs comprehensive cleanup
        /// </summary>
        /// <param name="e">Navigation event arguments</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Unsubscribe from events to prevent memory leaks
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            // Clean up scroll timer resources
            _scrollTimer?.Stop();
            if (_scrollTimer != null)
            {
                _scrollTimer.Tick -= ScrollTimer_Tick;
                _scrollTimer = null;
            }

            // Cleanup ViewModel resources
            ViewModel.Cleanup();
        }
    }
}