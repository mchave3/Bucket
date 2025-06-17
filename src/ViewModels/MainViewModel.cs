namespace Bucket.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Logger.Debug("MainViewModel initialized");
        }

        /// <summary>
        /// Demonstrates the logging system with various examples
        /// </summary>
        [RelayCommand]
        private void TestLogging()
        {
            Logger.Information("=== Logging System Test Started ===");

            // Test different log levels
            Logger.Debug("Debug message: This will only show in developer mode");
            Logger.Information("Application action: User triggered logging test");
            Logger.Warning("Warning example: This is a test warning message");

            // Test structured logging
            Logger.Information("User interaction: Button clicked at {Timestamp} by {User}",
                DateTime.Now, Environment.UserName);

            // Test Windows imaging domain specific logging
            Logger.Information("WIM Analysis: Found {ImageCount} images in {FilePath}",
                3, @"C:\Windows\install.wim");

            Logger.Information("Update Progress: {ProgressPercent}% complete - {Downloaded}MB/{Total}MB",
                75, 234, 512);

            // Test performance logging
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Thread.Sleep(50); // Simulate some work
            stopwatch.Stop();

            Logger.Information("Performance: {Operation} completed in {ElapsedMs}ms",
                "Test Operation", stopwatch.ElapsedMilliseconds);

            // Test error handling
            try
            {
                throw new InvalidOperationException("This is a test exception");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Test error occurred during {Operation}", "Logging Demonstration");
            }

            Logger.Information("=== Logging System Test Completed ===");
        }
    }
}
