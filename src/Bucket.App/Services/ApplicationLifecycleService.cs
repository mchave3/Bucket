using Microsoft.Extensions.DependencyInjection;

namespace Bucket.App.Services
{
    /// <summary>
    /// Service to handle application lifecycle events and cleanup
    /// </summary>
    public interface IApplicationLifecycleService
    {
        void RegisterForCleanup(IDisposable resource);
        void CleanupAll();
    }

    public class ApplicationLifecycleService : IApplicationLifecycleService, IDisposable
    {
        private readonly List<IDisposable> _disposableResources = new();
        private readonly object _lock = new object();
        private bool _disposed = false;

        public void RegisterForCleanup(IDisposable resource)
        {
            if (resource == null) return;

            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposableResources.Add(resource);
                }
            }
        }

        public void CleanupAll()
        {
            lock (_lock)
            {
                if (_disposed) return;

                foreach (var resource in _disposableResources)
                {
                    try
                    {
                        resource?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing resource: {ex.Message}");
                    }
                }

                _disposableResources.Clear();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            CleanupAll();
        }
    }
}
