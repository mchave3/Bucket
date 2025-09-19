using Serilog;

namespace Bucket.App.Common
{
    public static partial class LoggerSetup
    {
        public static ILogger Logger { get; private set; }

        public static void ConfigureLogger(bool isDeveloperMode = false)
        {
            if (!Directory.Exists(Constants.LogDirectoryPath))
            {
                Directory.CreateDirectory(Constants.LogDirectoryPath);
            }

            var loggerConfig = new LoggerConfiguration()
                .Enrich.WithProperty("Version", ProcessInfoHelper.Version);

            if (isDeveloperMode)
            {
                // Developer mode: Detailed logging with Debug output
                loggerConfig
                    .MinimumLevel.Debug()
                    .WriteTo.File(Constants.LogFilePath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.Debug()
                    .WriteTo.Console();
            }
            else
            {
                // Production mode: Essential logging only
                loggerConfig
                    .MinimumLevel.Information()
                    .WriteTo.File(Constants.LogFilePath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            Logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// Logs shutdown information and disposes the logger if necessary.
        /// </summary>
        public static void Shutdown()
        {
            Logger?.Information("=== Bucket App Shutting Down ===");
            if (Logger is IDisposable disposableLogger)
            {
                disposableLogger.Dispose();
            }
        }
    }

}
