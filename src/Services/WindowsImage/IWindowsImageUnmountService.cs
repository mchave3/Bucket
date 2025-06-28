using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image unmounting operations.
/// Handles dismounting of Windows images with various options and cleanup operations.
/// </summary>
public interface IWindowsImageUnmountService
{
    /// <summary>
    /// Unmounts a Windows image from the specified mount point.
    /// </summary>
    /// <param name="mountedImage">The mounted image information.</param>
    /// <param name="saveChanges">Whether to save changes made to the mounted image.</param>
    /// <param name="progress">The progress reporter with status messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the unmount operation.</returns>
    Task UnmountImageAsync(MountedImageInfo mountedImage, bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unmounts all currently mounted Windows images.
    /// </summary>
    /// <param name="saveChanges">Whether to save changes made to the mounted images.</param>
    /// <param name="progress">The progress reporter with status messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the unmount all operation.</returns>
    Task UnmountAllImagesAsync(bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces the unmount of a Windows image, discarding any changes.
    /// Use this when normal unmount fails.
    /// </summary>
    /// <param name="mountedImage">The mounted image information.</param>
    /// <param name="progress">The progress reporter with status messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the force unmount operation.</returns>
    Task ForceUnmountImageAsync(MountedImageInfo mountedImage, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up orphaned mount directories that are no longer in use.
    /// </summary>
    /// <param name="progress">The progress reporter with status messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the cleanup operation.</returns>
    Task CleanupOrphanedMountsAsync(IProgress<string> progress = null, CancellationToken cancellationToken = default);
} 