using Serilog;
using Serilog.Events;

namespace Bucket.Common
{
    /// <summary>
    /// Provides global logging configuration and access using Serilog
    /// </summary>
    public static partial class LoggerSetup
    {
        /// <summary>
        /// Global logger instance accessible throughout the application
        /// </summary>
        public static ILogger Logger { get; private set; } = Serilog.Log.Logger;

        /// <summary>
        /// Configures the global logger with appropriate sinks and settings
        /// </summary>
        public static void ConfigureLogger()
        {
            try
            {
                // Ensure log directory exists
                if (!Directory.Exists(Constants.LogDirectoryPath))
                {
                    Directory.CreateDirectory(Constants.LogDirectoryPath);
                }

                // Determine log level based on developer mode
                var logLevel = AppHelper.Settings.UseDeveloperMode
                    ? LogEventLevel.Debug
                    : LogEventLevel.Information;

                // Configure logger
                Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(logLevel)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.WithProperty("Application", ProcessInfoHelper.ProductName)
                    .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
                    .Enrich.WithProperty("MachineName", Environment.MachineName)
                    .Enrich.WithProperty("UserName", Environment.UserName)
                    .WriteTo.File(
                        Constants.LogFilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    .WriteTo.Debug(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                // Set as global logger
                Serilog.Log.Logger = Logger;

                Logger.Information("Logger configured successfully with level {LogLevel}", logLevel);
            }
            catch (Exception ex)
            {
                // Fallback to console if logger configuration fails
                Console.WriteLine($"Failed to configure logger: {ex.Message}");
                Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
            }
        }

        /// <summary>
        /// Closes and flushes the logger
        /// </summary>
        public static void CloseLogger()
        {
            Logger?.Information("Shutting down logger");
            Serilog.Log.CloseAndFlush();
        }
    }
}
