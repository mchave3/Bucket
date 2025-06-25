using Bucket.Models;
using Windows.Storage;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image ISO operations including mounting, dismounting, and import operations.
/// Handles all ISO-related functionality for Windows image management.
/// </summary>
public interface IWindowsImageIsoService
{
    /// <summary>
    /// Imports a Windows image from an ISO file.
    /// </summary>
    /// <param name="isoFile">The ISO file to import from.</param>
    /// <param name="customName">Optional custom name for the imported image.</param>
    /// <param name="progress">Progress reporter for the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The imported Windows image information.</returns>
    Task<WindowsImageInfo> ImportFromIsoAsync(StorageFile isoFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports a Windows image directly from a WIM/ESD file.
    /// </summary>
    /// <param name="wimFile">The WIM/ESD file to import.</param>
    /// <param name="customName">Optional custom name for the imported image.</param>
    /// <param name="progress">Progress reporter for the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The imported Windows image information.</returns>
    Task<WindowsImageInfo> ImportFromWimAsync(StorageFile wimFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mounts an ISO file and returns the mount path.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The mount path of the ISO.</returns>
    Task<string> MountIsoAsync(string isoPath, CancellationToken cancellationToken);

    /// <summary>
    /// Dismounts an ISO file.
    /// </summary>
    /// <param name="isoPath">The original ISO file path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task DismountIsoAsync(string isoPath, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an ISO file is currently mounted.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the ISO is mounted, false otherwise.</returns>
    Task<bool> IsIsoMountedAsync(string isoPath, CancellationToken cancellationToken);
}
