using Serilog;
using System.Diagnostics;

namespace Bucket.Updater.Common
{
    /// <summary>
    /// Utility class for measuring execution time and logging performance metrics
    /// </summary>
    public static class PerformanceLogger
    {
        /// <summary>
        /// Measures execution time of a function and logs performance
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operation">Function to execute and measure</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>Result of the executed function</returns>
        public static T MeasureAndLog<T>(string operationName, Func<T> operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger?.Debug("Starting performance measurement for {OperationName}", operationName);
                var result = operation();
                stopwatch.Stop();

                // Log performance metrics using extension method
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Log failure with elapsed time for debugging
                logger?.Error(ex, "Operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Measures execution time of an async function and logs performance
        /// </summary>
        /// <typeparam name="T">Return type of the async function</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operation">Async function to execute and measure</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>Task with result of the executed function</returns>
        public static async Task<T> MeasureAndLogAsync<T>(string operationName, Func<Task<T>> operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger?.Debug("Starting async performance measurement for {OperationName}", operationName);
                var result = await operation();
                stopwatch.Stop();

                // Log performance metrics for async operation
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Log async operation failure with timing
                logger?.Error(ex, "Async operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Measures execution time of an action and logs performance
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operation">Action to execute and measure</param>
        /// <param name="logger">Optional logger instance</param>
        public static void MeasureAndLog(string operationName, Action operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger?.Debug("Starting performance measurement for {OperationName}", operationName);
                operation();
                stopwatch.Stop();

                // Log performance for void operation
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Ensure timing is logged even on failure
                logger?.Error(ex, "Operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Measures execution time of an async action and logs performance
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operation">Async action to execute and measure</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>Task representing the async operation</returns>
        public static async Task MeasureAndLogAsync(string operationName, Func<Task> operation, ILogger? logger = null)
        {
            logger ??= LoggerSetup.Logger;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger?.Debug("Starting async performance measurement for {OperationName}", operationName);
                await operation();
                stopwatch.Stop();

                // Log performance for async void operation
                logger?.LogPerformance(operationName, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Log async void operation failure with timing
                logger?.Error(ex, "Async operation {OperationName} failed after {Duration}ms", operationName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Creates a disposable scope that measures performance until disposed
        /// </summary>
        /// <param name="operationName">Name of the operation to measure</param>
        /// <param name="logger">Optional logger instance</param>
        /// <returns>Disposable scope that logs performance when disposed</returns>
        public static IDisposable BeginMeasurement(string operationName, ILogger? logger = null)
        {
            return new PerformanceMeasurementScope(operationName, logger ?? LoggerSetup.Logger);
        }
    }

    /// <summary>
    /// Internal scope for measuring operation performance with automatic cleanup
    /// </summary>
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

            // Log start of measurement
            _logger?.Debug("Performance measurement started for {OperationName}", operationName);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                
                // Log final performance metrics
                _logger?.LogPerformance(_operationName, _stopwatch.Elapsed);
                _disposed = true;
            }
        }
    }
}