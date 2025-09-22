using System.Runtime.InteropServices;

namespace Bucket.App.Services
{
    /// <summary>
    /// Service to handle safe application shutdown avoiding WinRT crash issues
    /// </summary>
    internal static class SafeShutdownService
    {
        private static bool _isShuttingDown = false;

        [DllImport("kernel32.dll")]
        private static extern void ExitProcess(uint exitCode);

        public static void InitiateSafeShutdown()
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;

            // Delay to allow cleanup to complete
            try
            {
                Task.Delay(250).Wait();
            }
            catch { }

            // Immediate exit after cleanup
            ExitProcess(0);
        }

        public static bool IsShuttingDown => _isShuttingDown;
    }
}
