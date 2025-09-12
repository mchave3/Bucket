using Serilog.Events;
using Serilog.Formatting;

namespace Bucket.Updater.Common
{
    /// <summary>
    /// Log formatter that converts technical messages into user-friendly display text
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

        /// <summary>
        /// Formats log events for user-friendly display
        /// </summary>
        /// <param name="logEvent">The log event to format</param>
        /// <param name="output">Text writer for the formatted output</param>
        public void Format(LogEvent logEvent, TextWriter output)
        {
            // Only process Information and Warning levels for user-friendly logs
            if (logEvent.Level != LogEventLevel.Information && logEvent.Level != LogEventLevel.Warning)
                return;

            var timestamp = logEvent.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var message = logEvent.RenderMessage(CultureInfo.InvariantCulture);
            var action = ExtractUserFriendlyAction(logEvent, message);

            // Skip highly technical messages
            if (ShouldSkipMessage(message, logEvent))
                return;

            var userFriendlyMessage = MakeMessageUserFriendly(message, logEvent);

            // Format: [HH:mm:ss] ACTION: Message
            output.WriteLine($"[{timestamp}] {action}: {userFriendlyMessage}");
        }

        /// <summary>
        /// Extracts a user-friendly action category from log event
        /// </summary>
        private string ExtractUserFriendlyAction(LogEvent logEvent, string message)
        {
            // Check for specific action types in log properties
            if (logEvent.Properties.TryGetValue("ActionType", out var actionType))
            {
                var actionStr = actionType.ToString().Trim('"');
                if (UserFriendlyActions.TryGetValue(actionStr, out var friendlyAction))
                    return friendlyAction;
            }

            // Check for operation name in log properties
            if (logEvent.Properties.TryGetValue("OperationName", out var operationName))
            {
                var opStr = operationName.ToString().Trim('"');
                if (UserFriendlyActions.TryGetValue(opStr, out var friendlyAction))
                    return friendlyAction;
            }

            // Fallback to message content analysis
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

        /// <summary>
        /// Determines if a message should be filtered out from user display
        /// </summary>
        private bool ShouldSkipMessage(string message, LogEvent logEvent)
        {
            // Skip debug level messages - too technical
            if (logEvent.Level == LogEventLevel.Debug)
                return true;

            // Filter out highly technical developer messages
            var technicalKeywords = new[]
            {
                "Operation ", "started with ID", "completed in", "ms (ID:",
                "Entering ", "Exiting ", "Performance:", "Debug",
                "with parameters:", "Method", "Process.GetProcessesByName",
                "LogContext", "Stopwatch", "BeginOperationScope",
                "SessionId", "OperationId", "ProcessId", "async performance measurement",
                "Deleted existing file", "Download configuration:", "Created temp directory",
                "Starting download from", "Download started, size:", "File.Create",
                "GitHubService", "ConfigurationService", "InstallationService",
                "UpdateService cleaning up", "Could not get Update Service"
            };

            if (technicalKeywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Skip messages that are already user-friendly formatted (avoid double processing)
            if (message.Contains(": \"", StringComparison.Ordinal) && (message.Contains("CHECK", StringComparison.OrdinalIgnoreCase) || message.Contains("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("DOWNLOAD", StringComparison.OrdinalIgnoreCase) || message.Contains("INSTALL", StringComparison.OrdinalIgnoreCase)))
                return true;

            // Skip technical JSON-like messages with object properties
            if (message.Contains('{', StringComparison.Ordinal) && message.Contains('}', StringComparison.Ordinal) &&
                (message.Contains("CurrentVersion", StringComparison.OrdinalIgnoreCase) || message.Contains("NewVersion", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("FileSize", StringComparison.OrdinalIgnoreCase) || message.Contains("DownloadUrl", StringComparison.OrdinalIgnoreCase)))
                return true;

            // Skip already formatted download completion messages to avoid duplicates
            if (message.Contains("Download completed", StringComparison.OrdinalIgnoreCase) && message.Contains('(', StringComparison.Ordinal) && message.Contains("MB)", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Converts technical messages into user-friendly text
        /// </summary>
        private string MakeMessageUserFriendly(string message, LogEvent logEvent)
        {
            // Handle specific message patterns with user-friendly alternatives

            // Version updates - extract version info from message text
            if (message.Contains("Update available:", StringComparison.OrdinalIgnoreCase) && message.Contains('→', StringComparison.Ordinal))
            {
                // Extract version from message like "Update available: 1.0.0.0 → 25.9.9"
                var parts = message.Split('→');
                if (parts.Length == 2)
                {
                    var newVersion = parts[1].Trim().Split(' ')[0]; // Get version before any additional info
                    return $"New version available: {newVersion}";
                }
                return "New version available";
            }

            if (message.Contains("Update check completed, update available:", StringComparison.OrdinalIgnoreCase))
            {
                return "Update check completed - Update available";
            }

            if (message.Contains("No updates available", StringComparison.OrdinalIgnoreCase))
            {
                return "No update available";
            }

            // Download progress - extract size from message when available
            if (message.Contains("Download completed successfully", StringComparison.OrdinalIgnoreCase))
            {
                // Try to extract size from message like "...to path (60132300 bytes)"
                var bytesMatch = System.Text.RegularExpressions.Regex.Match(message, @"\((\d+) bytes\)");
                if (bytesMatch.Success && long.TryParse(bytesMatch.Groups[1].Value, out var size))
                {
                    var sizeStr = FormatFileSize(size);
                    return $"Download completed ({sizeStr})";
                }
                return "Download completed";
            }

            // Extract version from download messages
            if (message.Contains("Starting download process for version", StringComparison.OrdinalIgnoreCase))
            {
                // Extract from "Starting download process for version 25.9.9, size: ..."
                var versionMatch = System.Text.RegularExpressions.Regex.Match(message, @"version ([^\s,]+)");
                if (versionMatch.Success)
                {
                    return $"Starting download version {versionMatch.Groups[1].Value}";
                }
                return "Starting download";
            }

            // Installation messages - handle various installation patterns
            if (message.Contains("MSI installation completed successfully", StringComparison.OrdinalIgnoreCase))
            {
                return "Installation completed successfully";
            }

            if (message.Contains("Starting installation for version", StringComparison.OrdinalIgnoreCase))
            {
                var versionMatch = System.Text.RegularExpressions.Regex.Match(message, @"version ([^\s,\)]+)");
                if (versionMatch.Success)
                {
                    return $"Starting installation version {versionMatch.Groups[1].Value}";
                }
                return "Starting installation";
            }

            if (message.Contains("Starting MSI installation", StringComparison.OrdinalIgnoreCase))
            {
                return "Starting installation";
            }

            if (message.Contains("Installation failed", StringComparison.OrdinalIgnoreCase))
            {
                return "Installation failed";
            }

            if (message.Contains("Installation completed", StringComparison.OrdinalIgnoreCase))
            {
                return "Installation completed";
            }

            // Process management
            if (message.Contains("Bucket process", StringComparison.OrdinalIgnoreCase) && message.Contains("stopped", StringComparison.OrdinalIgnoreCase))
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

            // If no specific pattern matched, do basic cleanup but be more selective
            var friendlyMessage = message;

            // Only do generic cleanup if the message seems worth showing to users
            if (message.Contains("Update") || message.Contains("Download") || message.Contains("Install"))
            {
                // Basic cleanup
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

            // For other messages, return as-is (they'll likely be filtered out anyway)
            return message;
        }

        /// <summary>
        /// Formats byte count into human-readable file size
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            // Convert to appropriate unit (KB, MB, etc.)
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
}