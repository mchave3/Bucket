using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bucket.Updater.Common
{
    public static partial class LoggerSetup
    {
        public static ILogger Logger { get; private set; }
        public static string SessionId { get; private set; } = Guid.NewGuid().ToString("N")[..8];

        public static void ConfigureLogger()
        {
            EnsureLogDirectoryExists();

            var logLevel = GetLogLevel();

            Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)

                // Enrichers for contextual information
                .Enrich.WithProperty("Application", "Bucket.Updater")
                .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
                .Enrich.WithProperty("SessionId", SessionId)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("UserName", Environment.UserName)
                .Enrich.WithProperty("ProcessId", Environment.ProcessId)
                .Enrich.WithProperty("Architecture", RuntimeInformation.OSArchitecture.ToString())
                .Enrich.FromLogContext()

                // File sink with structured logging (JSON)
                .WriteTo.File(
                    path: Path.Combine(Constants.LogDirectoryPath, "updater-.json"),
                    formatter: new JsonFormatter(renderMessage: true),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30, // Keep 30 days
                    fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB max per file
                    rollOnFileSizeLimit: true,
                    shared: true)

                // Human readable file sink
                .WriteTo.File(
                    path: Path.Combine(Constants.LogDirectoryPath, "updater-.log"),
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SessionId}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 15, // Keep 15 days for readable logs
                    fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB max per file
                    rollOnFileSizeLimit: true,
                    shared: true)

                // User-friendly file sink
                .WriteTo.File(
                    path: Path.Combine(Constants.LogDirectoryPath, "updater-user-.log"),
                    formatter: new UserFriendlyFormatter(),
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7, // Keep 7 days for user logs
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB max per file
                    rollOnFileSizeLimit: true,
                    shared: true)

                // Debug sink for development
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{SessionId}] {Message:lj} {Properties:j}{NewLine}{Exception}")

                // Console sink when debugger attached
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(_ => Debugger.IsAttached)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"))

                .CreateLogger();

            // Log startup information
            Logger.Information("=== Bucket Updater Starting ===");
            Logger.Information("Session: {SessionId}, Version: {Version}, Machine: {MachineName}",
                SessionId, ProcessInfoHelper.Version, Environment.MachineName);
            Logger.Information("Process: {ProcessId}, User: {UserName}, Architecture: {Architecture}",
                Environment.ProcessId, Environment.UserName, RuntimeInformation.OSArchitecture);

            // Schedule log cleanup
            _ = Task.Run(CleanupOldLogsAsync);
        }

        private static LogEventLevel GetLogLevel()
        {
            // Check environment variable first
            var envLogLevel = Environment.GetEnvironmentVariable("BUCKET_UPDATER_LOG_LEVEL");
            if (!string.IsNullOrEmpty(envLogLevel) && Enum.TryParse<LogEventLevel>(envLogLevel, true, out var parsedLevel))
            {
                return parsedLevel;
            }

            // Check if debugger is attached
            if (Debugger.IsAttached)
            {
                return LogEventLevel.Debug;
            }

#if DEBUG
            return LogEventLevel.Debug;
#else
            return LogEventLevel.Information;
#endif
        }

        private static void EnsureLogDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(Constants.LogDirectoryPath))
                {
                    Directory.CreateDirectory(Constants.LogDirectoryPath);
                }
            }
            catch (Exception ex)
            {
                // Fallback to temp directory if cannot create in CommonApplicationData
                var tempLogPath = Path.Combine(Path.GetTempPath(), "Bucket", "Log");
                Directory.CreateDirectory(tempLogPath);

                // Update constants to use temp path
                typeof(Constants).GetField(nameof(Constants.LogDirectoryPath))?.SetValue(null, tempLogPath);
                typeof(Constants).GetField(nameof(Constants.LogFilePath))?.SetValue(null, Path.Combine(tempLogPath, "Updater-Log.log"));

                Console.WriteLine($"Failed to create log directory, using temp: {tempLogPath}. Error: {ex.Message}");
            }
        }

        private static async Task CleanupOldLogsAsync()
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30)); // Wait 30s after startup

                var logDirectory = new DirectoryInfo(Constants.LogDirectoryPath);
                if (!logDirectory.Exists) return;

                var retentionDays = GetLogRetentionDays();
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);

                var oldFiles = logDirectory.GetFiles("updater-*.*")
                    .Where(f => f.CreationTime < cutoffDate)
                    .ToList();

                foreach (var file in oldFiles)
                {
                    try
                    {
                        file.Delete();
                        Logger?.Debug("Deleted old log file: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        Logger?.Warning(ex, "Failed to delete old log file: {FileName}", file.Name);
                    }
                }

                if (oldFiles.Count > 0)
                {
                    Logger?.Information("Cleaned up {Count} old log files older than {Days} days", oldFiles.Count, retentionDays);
                }
            }
            catch (Exception ex)
            {
                Logger?.Warning(ex, "Error during log cleanup");
            }
        }

        private static int GetLogRetentionDays()
        {
            var envRetention = Environment.GetEnvironmentVariable("BUCKET_UPDATER_LOG_RETENTION_DAYS");
            if (!string.IsNullOrEmpty(envRetention) && int.TryParse(envRetention, out var days) && days > 0)
            {
                return days;
            }

            return 60; // Default 60 days retention
        }

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