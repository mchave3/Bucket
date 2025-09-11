using Serilog;
using System.Diagnostics;

namespace Bucket.Updater.Common
{
    /// <summary>
    /// Helper class for measuring and logging performance metrics
    /// </summary>
    public static class PerformanceLogger
    {
        /// <summary>
        /// Measures execution time of an action and logs performance metrics
        /// </summary>
        public static T MeasureAndLog<T>(string operationName, Func<T> operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                logger?.Debug("Starting performance measurement for {OperationName}", operationName);
                var result = operation();
                stopwatch.Stop();
                
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger?.Error(ex, "Operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Measures execution time of an async action and logs performance metrics
        /// </summary>
        public static async Task<T> MeasureAndLogAsync<T>(string operationName, Func<Task<T>> operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                logger?.Debug("Starting async performance measurement for {OperationName}", operationName);
                var result = await operation();
                stopwatch.Stop();
                
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger?.Error(ex, "Async operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Measures execution time of an action without return value and logs performance metrics
        /// </summary>
        public static void MeasureAndLog(string operationName, Action operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                logger?.Debug("Starting performance measurement for {OperationName}", operationName);
                operation();
                stopwatch.Stop();
                
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger?.Error(ex, "Operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Measures execution time of an async action without return value and logs performance metrics
        /// </summary>
        public static async Task MeasureAndLogAsync(string operationName, Func<Task> operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                logger?.Debug("Starting async performance measurement for {OperationName}", operationName);
                await operation();
                stopwatch.Stop();
                
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger?.Error(ex, "Async operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Creates a disposable performance measurement scope
        /// </summary>
        public static IDisposable BeginMeasurement(string operationName, ILogger? logger = null)
        {
            return new PerformanceMeasurementScope(operationName, logger ?? LoggerSetup.Logger);
        }
    }

    internal class PerformanceMeasurementScope : IDisposable
    {
        private readonly string _operationName;
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        public PerformanceMeasurementScope(string operationName, ILogger logger)
        {
            _operationName = operationName;
            _logger = logger;
            _stopwatch = Stopwatch.StartNew();
            
            _logger?.Debug("Performance measurement started for {OperationName}", operationName);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                _logger?.LogPerformance(_operationName, _stopwatch.Elapsed);
                _disposed = true;
            }
        }
    }
}