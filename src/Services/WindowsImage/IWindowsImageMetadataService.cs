using System.Collections.ObjectModel;
using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image metadata management.
/// Handles loading, saving, and managing Windows image metadata collections.
/// </summary>
public interface IWindowsImageMetadataService
{
    /// <summary>
    /// Gets all available Windows images asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image information.</returns>
    Task<ObservableCollection<WindowsImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the Windows images metadata asynchronously.
    /// </summary>
    /// <param name="images">The collection of images to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an image from the metadata collection.
    /// </summary>
    /// <param name="imageInfo">The image to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing image in the metadata collection.
    /// </summary>
    /// <param name="imageInfo">The image to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the path to the images data file.
    /// </summary>
    /// <returns>The path to the images metadata file.</returns>
    string GetImagesDataPath();

    /// <summary>
    /// Gets the path to the images directory.
    /// </summary>
    /// <returns>The path to the images directory.</returns>
    string GetImagesDirectory();
}
