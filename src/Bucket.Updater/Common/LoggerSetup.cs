using Serilog;

namespace Bucket.Updater.Common
{
    /// <summary>
    /// Provides static methods to configure and manage logging for the Bucket Updater application.
    /// </summary>
    public static partial class LoggerSetup
    {
        /// <summary>
        /// The main Serilog logger instance used throughout the application.
        /// </summary>
        public static ILogger Logger { get; private set; }

        /// <summary>
        /// Initializes and configures the Serilog logger with simple file logging.
        /// </summary>
        public static void ConfigureLogger()
        {
            if (!Directory.Exists(Constants.LogDirectoryPath))
            {
                Directory.CreateDirectory(Constants.LogDirectoryPath);
            }

            Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
                .MinimumLevel.Information()
                .WriteTo.File(Constants.LogFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// Logs shutdown information and disposes the logger if necessary.
        /// </summary>
        public static void Shutdown()
        {
            Logger?.Information("=== Bucket Updater Shutting Down ===");
            if (Logger is IDisposable disposableLogger)
            {
                disposableLogger.Dispose();
            }
        }
    }
}