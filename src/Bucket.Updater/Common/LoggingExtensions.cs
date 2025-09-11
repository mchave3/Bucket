using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Diagnostics;

namespace Bucket.Updater.Common
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Creates a disposable scope for correlated logging
        /// </summary>
        public static IDisposable BeginOperationScope(this ILogger logger, string operationName, object? additionalProperties = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var stopwatch = Stopwatch.StartNew();
            
            var properties = new List<(string, object)>
            {
                ("OperationId", operationId),
                ("OperationName", operationName),
                ("OperationStartTime", DateTimeOffset.Now)
            };

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

            logger?.Information("Operation {OperationName} started with ID {OperationId}", operationName, operationId);

            var contexts = properties.Select(p => LogContext.PushProperty(p.Item1, p.Item2)).ToArray();
            
            return new OperationScope(logger, operationName, operationId, stopwatch, contexts);
        }

        /// <summary>
        /// Logs method entry with parameters
        /// </summary>
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
        /// Logs method exit with duration and result
        /// </summary>
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
        /// Logs user action for audit purposes
        /// </summary>
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
        /// Logs performance metrics
        /// </summary>
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
        /// Logs state transition
        /// </summary>
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
        /// Logs network request/response
        /// </summary>
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
        /// Logs user-friendly message that will be formatted nicely in user logs
        /// </summary>
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
        /// Logs progress update for user-friendly display
        /// </summary>
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
        /// Logs both technical details and user-friendly message
        /// </summary>
        public static void LogWithUserMessage(this ILogger logger, LogEventLevel level, string technicalMessage, string userFriendlyAction, string userMessage, object? context = null)
        {
            // Log technical message first
            if (context != null)
            {
                logger?.Write(level, technicalMessage + " {@Context}", context);
            }
            else
            {
                logger?.Write(level, technicalMessage);
            }

            // Log user-friendly message
            logger?.LogUserFriendlyMessage(userFriendlyAction, userMessage, context);
        }
    }

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
                _logger?.Information("Operation {OperationName} completed in {Duration}ms (ID: {OperationId})", 
                    _operationName, _stopwatch.ElapsedMilliseconds, _operationId);

                foreach (var context in _contexts)
                {
                    context?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}