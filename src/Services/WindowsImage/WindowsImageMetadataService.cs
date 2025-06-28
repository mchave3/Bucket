using System.Collections.ObjectModel;
using System.Text.Json;
using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for Windows image metadata management.
/// Handles loading, saving, and managing Windows image metadata collections using JSON persistence.
/// </summary>
public class WindowsImageMetadataService : IWindowsImageMetadataService
{
    #region Private Fields

    private readonly string _imagesDataPath;
    private readonly string _imagesDirectory;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowsImageMetadataService class.
    /// </summary>
    /// <param name="imagesDirectory">The directory path where images are stored.</param>
    public WindowsImageMetadataService(string imagesDirectory)
    {
        _imagesDirectory = imagesDirectory ?? throw new ArgumentNullException(nameof(imagesDirectory));
        _imagesDataPath = Path.Combine(_imagesDirectory, "images.json");

        // Ensure the images directory exists
        Directory.CreateDirectory(_imagesDirectory);

        Logger.Debug("WindowsImageMetadataService initialized with directory: {Directory}", _imagesDirectory);
    }

    #endregion

    #region Public Methods - Metadata Management

    /// <summary>
    /// Gets all available Windows images asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image information.</returns>
    public async Task<ObservableCollection<WindowsImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Loading Windows images from {Path}", _imagesDataPath);

        try
        {
            if (!File.Exists(_imagesDataPath))
            {
                Logger.Information("Images data file not found, returning empty collection");
                return new ObservableCollection<WindowsImageInfo>();
            }

            var jsonContent = await File.ReadAllTextAsync(_imagesDataPath, cancellationToken);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                Logger.Information("Images data file is empty, returning empty collection");
                return new ObservableCollection<WindowsImageInfo>();
            }

            var images = JsonSerializer.Deserialize<List<WindowsImageInfo>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<WindowsImageInfo>();

            // Validate that image files still exist
            var validImages = new List<WindowsImageInfo>();
            foreach (var image in images)
            {
                if (File.Exists(image.FilePath))
                {
                    validImages.Add(image);
                }
                else
                {
                    Logger.Warning("Image file no longer exists, removing from collection: {FilePath}", image.FilePath);
                }
            }

            // If some images were removed, save the updated collection
            if (validImages.Count != images.Count)
            {
                await SaveImagesAsync(validImages, cancellationToken);
                Logger.Information("Removed {Count} missing images from metadata", images.Count - validImages.Count);
            }

            Logger.Information("Loaded {Count} Windows images", validImages.Count);
            return new ObservableCollection<WindowsImageInfo>(validImages);
        }
        catch (JsonException ex)
        {
            Logger.Error(ex, "Failed to deserialize images data from {Path}", _imagesDataPath);
            throw new InvalidOperationException($"Invalid images data format in {_imagesDataPath}", ex);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load Windows images from {Path}", _imagesDataPath);
            throw;
        }
    }

    /// <summary>
    /// Saves the Windows images metadata asynchronously.
    /// </summary>
    /// <param name="images">The collection of images to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)
    {
        Logger.Debug("Saving Windows images metadata to {Path}", _imagesDataPath);

        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(_imagesDirectory);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var jsonContent = JsonSerializer.Serialize(images.ToList(), jsonOptions);
            await File.WriteAllTextAsync(_imagesDataPath, jsonContent, cancellationToken);

            Logger.Debug("Successfully saved {Count} Windows images metadata", images.Count());
            Logger.Verbose("Metadata file written to: {FilePath} with {FileSize} bytes", _imagesDataPath, new FileInfo(_imagesDataPath).Length);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save Windows images metadata to {Path}", _imagesDataPath);
            throw;
        }
    }

    /// <summary>
    /// Removes an image from the metadata collection.
    /// </summary>
    /// <param name="imageInfo">The image to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RemoveImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default)
    {
        if (imageInfo == null)
        {
            throw new ArgumentNullException(nameof(imageInfo));
        }

        Logger.Information("Removing image from metadata: {Name} (ID: {Id})", imageInfo.Name, imageInfo.Id);

        try
        {
            var existingImages = await GetImagesAsync(cancellationToken);
            var imageToRemove = existingImages.FirstOrDefault(img => img.Id == imageInfo.Id);

            if (imageToRemove != null)
            {
                existingImages.Remove(imageToRemove);
                await SaveImagesAsync(existingImages, cancellationToken);
                Logger.Information("Successfully removed image from metadata: {Name}", imageInfo.Name);
            }
            else
            {
                Logger.Warning("Image not found in metadata collection: {Name} (ID: {Id})", imageInfo.Name, imageInfo.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to remove image from metadata: {Name} (ID: {Id})", imageInfo.Name, imageInfo.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing image in the metadata collection.
    /// </summary>
    /// <param name="imageInfo">The image to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task UpdateImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default)
    {
        if (imageInfo == null)
        {
            throw new ArgumentNullException(nameof(imageInfo));
        }

        Logger.Information("Updating image metadata: {Name} (ID: {Id})", imageInfo.Name, imageInfo.Id);

        try
        {
            var existingImages = await GetImagesAsync(cancellationToken);
            var imageToUpdate = existingImages.FirstOrDefault(img => img.Id == imageInfo.Id);

            if (imageToUpdate != null)
            {
                // Update properties
                imageToUpdate.Name = imageInfo.Name;
                imageToUpdate.FilePath = imageInfo.FilePath;
                imageToUpdate.ModifiedDate = imageInfo.ModifiedDate;
                imageToUpdate.SourceIsoPath = imageInfo.SourceIsoPath;

                // CRITICAL: Update the Indices collection to persist index-level changes
                if (imageInfo.Indices != null && imageInfo.Indices.Any())
                {
                    // Update each index in the existing image with the corresponding data from the updated image
                    foreach (var updatedIndex in imageInfo.Indices)
                    {
                        var existingIndex = imageToUpdate.Indices?.FirstOrDefault(i => i.Index == updatedIndex.Index);
                        if (existingIndex != null)
                        {
                            // Update index properties
                            existingIndex.Name = updatedIndex.Name;
                            existingIndex.Description = updatedIndex.Description;
                            existingIndex.IsIncluded = updatedIndex.IsIncluded;
                            // Note: DisplayText is a computed property that will update automatically

                            Logger.Debug("Updated index {Index}: Name='{Name}', Description='{Description}'",
                                existingIndex.Index, existingIndex.Name, existingIndex.Description);
                        }
                        else
                        {
                            Logger.Warning("Index {Index} not found in existing image during update", updatedIndex.Index);
                        }
                    }

                    Logger.Information("Updated {Count} indices in image metadata", imageInfo.Indices.Count());
                }

                await SaveImagesAsync(existingImages, cancellationToken);
                Logger.Information("Successfully updated image metadata: {Name}", imageInfo.Name);
            }
            else
            {
                Logger.Warning("Image not found in metadata collection for update: {Name} (ID: {Id})", imageInfo.Name, imageInfo.Id);

                // Add as new image if not found
                existingImages.Add(imageInfo);
                await SaveImagesAsync(existingImages, cancellationToken);
                Logger.Information("Added new image to metadata collection: {Name}", imageInfo.Name);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to update image metadata: {Name} (ID: {Id})", imageInfo.Name, imageInfo.Id);
            throw;
        }
    }

    #endregion

    #region Public Methods - Configuration

    /// <summary>
    /// Gets the path to the images data file.
    /// </summary>
    /// <returns>The path to the images metadata file.</returns>
    public string GetImagesDataPath()
    {
        return _imagesDataPath;
    }

    /// <summary>
    /// Gets the path to the images directory.
    /// </summary>
    /// <returns>The path to the images directory.</returns>
    public string GetImagesDirectory()
    {
        return _imagesDirectory;
    }

    #endregion
}
