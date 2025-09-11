using Serilog.Events;
using Serilog.Formatting;
using System.Globalization;
using System.Text;

namespace Bucket.Updater.Common
{
    /// <summary>
    /// Formatter for user-friendly log messages that filters technical details
    /// </summary>
    public class UserFriendlyFormatter : ITextFormatter
    {
        private static readonly Dictionary<string, string> UserFriendlyActions = new()
        {
            { "InitiateDownloadInstall", "UPDATE" },
            { "StartDownload", "DOWNLOAD" },
            { "StartInstallation", "INSTALL" },
            { "CheckForUpdates", "CHECK" },
            { "UpdateCheck", "CHECK" },
            { "Download", "DOWNLOAD" },
            { "Installation", "INSTALL" },
            { "MSIInstallation", "INSTALL" },
            { "ValidateMsiFile", "VALIDATE" },
            { "EnsureBucketProcessStopped", "PREPARE" }
        };

        public void Format(LogEvent logEvent, TextWriter output)
        {
            // Only process Information and Warning levels for user-friendly logs
            if (logEvent.Level != LogEventLevel.Information && logEvent.Level != LogEventLevel.Warning)
                return;

            var timestamp = logEvent.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var message = logEvent.RenderMessage();
            var action = ExtractUserFriendlyAction(logEvent, message);

            // Skip highly technical messages
            if (ShouldSkipMessage(message, logEvent))
                return;

            var userFriendlyMessage = MakeMessageUserFriendly(message, logEvent);
            
            // Format: [HH:mm:ss] ACTION: Message
            output.WriteLine($"[{timestamp}] {action}: {userFriendlyMessage}");
        }

        private string ExtractUserFriendlyAction(LogEvent logEvent, string message)
        {
            // Check for specific action types in properties
            if (logEvent.Properties.TryGetValue("ActionType", out var actionType))
            {
                var actionStr = actionType.ToString().Trim('"');
                if (UserFriendlyActions.TryGetValue(actionStr, out var friendlyAction))
                    return friendlyAction;
            }

            // Check for operation name
            if (logEvent.Properties.TryGetValue("OperationName", out var operationName))
            {
                var opStr = operationName.ToString().Trim('"');
                if (UserFriendlyActions.TryGetValue(opStr, out var friendlyAction))
                    return friendlyAction;
            }

            // Categorize by message content
            if (message.Contains("check", StringComparison.OrdinalIgnoreCase))
                return "CHECK";
            
            if (message.Contains("download", StringComparison.OrdinalIgnoreCase))
                return "DOWNLOAD";
            
            if (message.Contains("install", StringComparison.OrdinalIgnoreCase))
                return "INSTALL";

            if (message.Contains("update", StringComparison.OrdinalIgnoreCase))
                return "UPDATE";

            return "INFO";
        }

        private bool ShouldSkipMessage(string message, LogEvent logEvent)
        {
            // Skip highly technical messages
            var technicalKeywords = new[]
            {
                "Operation ", "started with ID", "completed in", "ms (ID:", 
                "Entering ", "Exiting ", "Performance:", "Debug",
                "with parameters:", "Method", "Process.GetProcessesByName",
                "LogContext", "Stopwatch", "BeginOperationScope",
                "SessionId", "OperationId", "ProcessId"
            };

            if (technicalKeywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Skip debug level messages
            if (logEvent.Level == LogEventLevel.Debug)
                return true;

            return false;
        }

        private string MakeMessageUserFriendly(string message, LogEvent logEvent)
        {
            // Replace technical terms with user-friendly equivalents
            var friendlyMessage = message;

            // Version updates
            if (message.Contains("Update available:") && message.Contains("→"))
            {
                if (logEvent.Properties.TryGetValue("NewVersion", out var newVersion))
                {
                    var version = newVersion.ToString().Trim('"');
                    return $"New version available: {version}";
                }
                return "New version available";
            }

            if (message.Contains("Update check completed, update available:"))
            {
                return "Update check completed - Update available";
            }

            if (message.Contains("No updates available"))
            {
                return "No update available";
            }

            // Download progress
            if (message.Contains("Download completed successfully"))
            {
                if (logEvent.Properties.TryGetValue("FileSize", out var fileSize))
                {
                    if (long.TryParse(fileSize.ToString(), out var size))
                    {
                        var sizeStr = FormatFileSize(size);
                        return $"Download completed ({sizeStr})";
                    }
                }
                return "Download completed";
            }

            if (message.Contains("Starting download"))
            {
                if (logEvent.Properties.TryGetValue("Version", out var version))
                {
                    var versionStr = version.ToString().Trim('"');
                    return $"Starting download version {versionStr}";
                }
                return "Starting download";
            }

            // Installation
            if (message.Contains("MSI installation completed successfully"))
            {
                return "Installation completed successfully";
            }

            if (message.Contains("Starting MSI installation"))
            {
                return "Starting installation";
            }

            if (message.Contains("Installation failed"))
            {
                return "Installation failed";
            }

            if (message.Contains("Installation completed"))
            {
                return "Installation completed";
            }

            // Process management
            if (message.Contains("Bucket process") && message.Contains("stopped"))
            {
                return "Application closed for update";
            }

            if (message.Contains("Found") && message.Contains("Bucket process"))
            {
                return "Closing application";
            }

            // Validation
            if (message.Contains("MSI file validation successful"))
            {
                return "Installation file verified";
            }

            // User actions
            if (message.StartsWith("User action:"))
            {
                if (message.Contains("InitiateDownloadInstall"))
                    return "Installation started by user";
                if (message.Contains("StartDownload"))
                    return "Download started by user";
                if (message.Contains("StartInstallation"))
                    return "Installation started by user";
            }

            // Generic cleanup
            friendlyMessage = friendlyMessage.Replace("UpdateService", "Update Service");
            friendlyMessage = friendlyMessage.Replace("InstallationService", "Installation Service");
            
            // Remove technical details like file paths, IDs, etc.
            friendlyMessage = System.Text.RegularExpressions.Regex.Replace(
                friendlyMessage, @"\b[A-Z]:\\[^\s]+", "[file_path]");
            
            friendlyMessage = System.Text.RegularExpressions.Regex.Replace(
                friendlyMessage, @"\b[a-f0-9]{8}\b", "");
            
            friendlyMessage = friendlyMessage.Trim();

            return string.IsNullOrEmpty(friendlyMessage) ? message : friendlyMessage;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}