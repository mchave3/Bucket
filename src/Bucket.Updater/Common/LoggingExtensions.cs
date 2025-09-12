using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Diagnostics;

namespace Bucket.Updater.Common
{
    /// <summary>
    /// Extension methods for enhanced logging with Serilog
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Creates a logging scope with operation ID and timing
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="additionalProperties">Optional additional properties</param>
        /// <returns>Disposable scope that logs completion when disposed</returns>
        public static IDisposable BeginOperationScope(this ILogger logger, string operationName, object? additionalProperties = null)
        {
            // Generate short operation ID for correlation
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var stopwatch = Stopwatch.StartNew();

            // Build list of properties for log context
            var properties = new List<(string, object)>
            {
                ("OperationId", operationId),
                ("OperationName", operationName),
                ("OperationStartTime", DateTimeOffset.Now)
            };

            // Add additional properties if provided
            if (additionalProperties != null)
            {
                var props = additionalProperties.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var value = prop.GetValue(additionalProperties);
                    if (value != null)
                    {
                        properties.Add((prop.Name, value));
                    }
                }
            }

            // Log operation start
            logger?.Information("Operation {OperationName} started with ID {OperationId}", operationName, operationId);

            // Create log context with all properties
            var contexts = properties.Select(p => LogContext.PushProperty(p.Item1, p.Item2)).ToArray();

            return new OperationScope(logger, operationName, operationId, stopwatch, contexts);
        }

        /// <summary>
        /// Logs method entry with optional parameters (Debug level)
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="methodName">Name of the method</param>
        /// <param name="parameters">Optional method parameters</param>
        public static void LogMethodEntry(this ILogger logger, string methodName, object? parameters = null)
        {
            if (parameters != null)
            {
                logger?.Debug("Entering {MethodName} with parameters: {@Parameters}", methodName, parameters);
            }
            else
            {
                logger?.Debug("Entering {MethodName}", methodName);
            }
        }

        /// <summary>
        /// Logs method exit with execution time and optional result (Debug level)
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="methodName">Name of the method</param>
        /// <param name="duration">Method execution duration</param>
        /// <param name="result">Optional method result</param>
        public static void LogMethodExit(this ILogger logger, string methodName, TimeSpan duration, object? result = null)
        {
            if (result != null)
            {
                logger?.Debug("Exiting {MethodName} after {Duration}ms with result: {@Result}",
                    methodName, duration.TotalMilliseconds, result);
            }
            else
            {
                logger?.Debug("Exiting {MethodName} after {Duration}ms", methodName, duration.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Logs user actions for auditing and tracking
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="action">The user action performed</param>
        /// <param name="context">Optional action context</param>
        public static void LogUserAction(this ILogger logger, string action, object? context = null)
        {
            using (LogContext.PushProperty("Category", "UserAction"))
            using (LogContext.PushProperty("ActionType", action))
            {
                if (context != null)
                {
                    logger?.Information("User action: {Action} {@Context}", action, context);
                }
                else
                {
                    logger?.Information("User action: {Action}", action);
                }
            }
        }

        /// <summary>
        /// Logs performance metrics with duration and optional throughput
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Operation duration</param>
        /// <param name="bytesProcessed">Optional bytes processed for throughput calculation</param>
        /// <param name="additionalMetrics">Optional additional metrics</param>
        public static void LogPerformance(this ILogger logger, string operationName, TimeSpan duration, long? bytesProcessed = null, object? additionalMetrics = null)
        {
            using (LogContext.PushProperty("Category", "Performance"))
            using (LogContext.PushProperty("OperationName", operationName))
            using (LogContext.PushProperty("DurationMs", duration.TotalMilliseconds))
            {
                var message = "Performance: {OperationName} completed in {Duration}ms";
                var args = new List<object> { operationName, duration.TotalMilliseconds };

                if (bytesProcessed.HasValue)
                {
                    message += ", processed {BytesProcessed} bytes";
                    args.Add(bytesProcessed.Value);

                    // Calculate throughput in MB/s
                    var throughputMBps = (bytesProcessed.Value / 1024.0 / 1024.0) / duration.TotalSeconds;
                    using (LogContext.PushProperty("ThroughputMBps", throughputMBps))
                    {
                        message += " (throughput: {ThroughputMBps:F2} MB/s)";
                        args.Add(throughputMBps);
                    }
                }

                if (additionalMetrics != null)
                {
                    message += " {@AdditionalMetrics}";
                    args.Add(additionalMetrics);
                }

                logger?.Information(message, args.ToArray());
            }
        }

        /// <summary>
        /// Logs state changes for entities with optional reason
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="entity">The entity changing state</param>
        /// <param name="fromState">Previous state</param>
        /// <param name="toState">New state</param>
        /// <param name="reason">Optional reason for the change</param>
        public static void LogStateTransition(this ILogger logger, string entity, object fromState, object toState, string? reason = null)
        {
            using (LogContext.PushProperty("Category", "StateTransition"))
            using (LogContext.PushProperty("Entity", entity))
            {
                if (!string.IsNullOrEmpty(reason))
                {
                    logger?.Information("State transition for {Entity}: {FromState} → {ToState} (reason: {Reason})",
                        entity, fromState, toState, reason);
                }
                else
                {
                    logger?.Information("State transition for {Entity}: {FromState} → {ToState}",
                        entity, fromState, toState);
                }
            }
        }

        /// <summary>
        /// Logs HTTP requests with response details
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Request URL</param>
        /// <param name="statusCode">Optional HTTP status code</param>
        /// <param name="duration">Optional request duration</param>
        /// <param name="responseSize">Optional response size in bytes</param>
        public static void LogNetworkRequest(this ILogger logger, string method, string url, int? statusCode = null, TimeSpan? duration = null, long? responseSize = null)
        {
            using (LogContext.PushProperty("Category", "Network"))
            using (LogContext.PushProperty("HttpMethod", method))
            using (LogContext.PushProperty("Url", url))
            {
                if (statusCode.HasValue && duration.HasValue)
                {
                    var message = "Network request: {Method} {Url} → {StatusCode} in {Duration}ms";
                    var args = new object[] { method, url, statusCode.Value, duration.Value.TotalMilliseconds };

                    // Include response size if available
                    if (responseSize.HasValue)
                    {
                        using (LogContext.PushProperty("ResponseSizeBytes", responseSize.Value))
                        {
                            logger?.Information(message + " ({ResponseSize} bytes)", args.Concat(new object[] { responseSize.Value }).ToArray());
                        }
                    }
                    else
                    {
                        logger?.Information(message, args);
                    }
                }
                else
                {
                    logger?.Information("Network request: {Method} {Url}", method, url);
                }
            }
        }

        /// <summary>
        /// Logs user-friendly messages for display in user interfaces
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="action">The action being performed</param>
        /// <param name="message">User-friendly message</param>
        /// <param name="context">Optional message context</param>
        public static void LogUserFriendlyMessage(this ILogger logger, string action, string message, object? context = null)
        {
            using (LogContext.PushProperty("Category", "UserFriendly"))
            using (LogContext.PushProperty("ActionType", action))
            {
                if (context != null)
                {
                    logger?.Information("{Action}: {Message} {@Context}", action, message, context);
                }
                else
                {
                    logger?.Information("{Action}: {Message}", action, message);
                }
            }
        }

        /// <summary>
        /// Logs progress updates with percentage and status
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation in progress</param>
        /// <param name="status">Current status</param>
        /// <param name="percentage">Optional completion percentage</param>
        /// <param name="additionalInfo">Optional additional information</param>
        public static void LogProgressUpdate(this ILogger logger, string operation, string status, int? percentage = null, object? additionalInfo = null)
        {
            using (LogContext.PushProperty("Category", "Progress"))
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("Status", status))
            {
                if (percentage.HasValue)
                {
                    using (LogContext.PushProperty("PercentageComplete", percentage.Value))
                    {
                        if (additionalInfo != null)
                        {
                            logger?.Information("Progress: {Operation} - {Status} ({Percentage}%) {@AdditionalInfo}",
                                operation, status, percentage.Value, additionalInfo);
                        }
                        else
                        {
                            logger?.Information("Progress: {Operation} - {Status} ({Percentage}%)",
                                operation, status, percentage.Value);
                        }
                    }
                }
                else
                {
                    if (additionalInfo != null)
                    {
                        logger?.Information("Progress: {Operation} - {Status} {@AdditionalInfo}",
                            operation, status, additionalInfo);
                    }
                    else
                    {
                        logger?.Information("Progress: {Operation} - {Status}", operation, status);
                    }
                }
            }
        }

        /// <summary>
        /// Logs both technical and user-friendly messages
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">Log level for technical message</param>
        /// <param name="technicalMessage">Technical details for developers</param>
        /// <param name="userFriendlyAction">User-friendly action name</param>
        /// <param name="userMessage">User-friendly message</param>
        /// <param name="context">Optional context information</param>
        public static void LogWithUserMessage(this ILogger logger, LogEventLevel level, string technicalMessage, string userFriendlyAction, string userMessage, object? context = null)
        {
            // Log technical message first for developers
            if (context != null)
            {
                logger?.Write(level, technicalMessage + " {@Context}", context);
            }
            else
            {
                logger?.Write(level, technicalMessage);
            }

            // Then log user-friendly message for UI display
            logger?.LogUserFriendlyMessage(userFriendlyAction, userMessage, context);
        }
    }

    /// <summary>
    /// Internal class that manages operation scope lifecycle and logging
    /// </summary>
    internal class OperationScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly string _operationId;
        private readonly Stopwatch _stopwatch;
        private readonly IDisposable[] _contexts;
        private bool _disposed;

        public OperationScope(ILogger logger, string operationName, string operationId, Stopwatch stopwatch, IDisposable[] contexts)
        {
            _logger = logger;
            _operationName = operationName;
            _operationId = operationId;
            _stopwatch = stopwatch;
            _contexts = contexts;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                
                // Log operation completion with duration
                _logger?.Information("Operation {OperationName} completed in {Duration}ms (ID: {OperationId})",
                    _operationName, _stopwatch.ElapsedMilliseconds, _operationId);

                // Clean up log context properties
                foreach (var context in _contexts)
                {
                    context?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}