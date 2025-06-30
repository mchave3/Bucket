using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image mounting operations.
/// Provides functionality to mount, unmount, and manage Windows images with support for multi-mount scenarios.
/// </summary>
public interface IWindowsImageMountService
{
    /// <summary>
    /// Mounts a Windows image index to a unique directory.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="index">The index number to mount.</param>
    /// <param name="imageName">The friendly name of the image.</param>
    /// <param name="editionName">The name of the Windows edition.</param>
    /// <param name="progress">The progress reporter with status messages.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mounted image information.</returns>
    Task<MountedImageInfo> MountImageAsync(string imagePath, int index, string imageName, string editionName, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all currently mounted images using Get-WindowsImage -Mounted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of mounted image information.</returns>
    Task<List<MountedImageInfo>> GetMountedImagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the mount directory for a mounted image in Windows Explorer.
    /// </summary>
    /// <param name="mountedImage">The mounted image whose directory to open.</param>
    Task OpenMountDirectoryAsync(MountedImageInfo mountedImage);

    /// <summary>
    /// Checks if a specific image index is currently mounted.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="index">The index number to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the image is mounted, false otherwise.</returns>
    Task<bool> IsImageMountedAsync(string imagePath, int index, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the mount directory path for a specific image and index.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="index">The index number.</param>
    /// <returns>The mount directory path.</returns>
    string GetMountDirectoryPath(string imagePath, int index);

    /// <summary>
    /// Cleans up orphaned mount directories that are not currently active.
    /// </summary>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the cleanup operation.</returns>
    Task CleanupOrphanedMountDirectoriesAsync(IProgress<string> progress = null, CancellationToken cancellationToken = default);
} 