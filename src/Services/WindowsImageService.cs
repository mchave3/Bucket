using System.Collections.ObjectModel;
using Bucket.Models;
using Bucket.Services.WindowsImage;
using Windows.Storage;

namespace Bucket.Services;

/// <summary>
/// Main service for managing Windows image files and their metadata.
/// Coordinates between specialized services to provide a unified API for Windows image operations
/// including analysis, import, management, and metadata handling.
/// </summary>
public class WindowsImageService
{
    #region Private Fields

    private readonly IWindowsImageMetadataService _metadataService;
    private readonly IWindowsImageFileService _fileService;
    private readonly IWindowsImagePowerShellService _powerShellService;
    private readonly IWindowsImageIsoService _isoService;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowsImageService class.
    /// </summary>
    /// <param name="metadataService">The metadata service for handling image metadata operations.</param>
    /// <param name="fileService">The file service for handling file operations.</param>
    /// <param name="powerShellService">The PowerShell service for image analysis.</param>
    /// <param name="isoService">The ISO service for ISO-related operations.</param>
    public WindowsImageService(
        IWindowsImageMetadataService metadataService,
        IWindowsImageFileService fileService,
        IWindowsImagePowerShellService powerShellService,
        IWindowsImageIsoService isoService)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _powerShellService = powerShellService ?? throw new ArgumentNullException(nameof(powerShellService));
        _isoService = isoService ?? throw new ArgumentNullException(nameof(isoService));

        Logger.Debug("WindowsImageService initialized with injected services");
    }

    #endregion

    #region Public Methods - Image Collection Management

    /// <summary>
    /// Gets all available Windows images asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image information.</returns>
    public async Task<ObservableCollection<WindowsImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        return await _metadataService.GetImagesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves the Windows images metadata asynchronously.
    /// </summary>
    /// <param name="images">The collection of images to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)
    {
        await _metadataService.SaveImagesAsync(images, cancellationToken);
    }

    /// <summary>
    /// Saves the order of Windows images asynchronously to maintain user preferences.
    /// </summary>
    /// <param name="images">The collection of images in the desired order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SaveImagesOrderAsync(IList<WindowsImageInfo> images, CancellationToken cancellationToken = default)
    {
        Logger.Information("Saving image order with {Count} images", images.Count);
        await _metadataService.SaveImagesAsync(images, cancellationToken);
    }

    #endregion

    #region Public Methods - Image Analysis

    /// <summary>
    /// Analyzes a WIM/ESD file and extracts its indices asynchronously using PowerShell Get-WindowsImage.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image indices.</returns>
    public async Task<List<WindowsImageIndex>> AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        return await _powerShellService.AnalyzeImageAsync(imagePath, progress, cancellationToken);
    }

    /// <summary>
    /// Gets detailed information for a specific Windows image index.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <param name="index">The index number to get detailed information for.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A WindowsImageIndex object with detailed information, or null if not found.</returns>
    public async Task<WindowsImageIndex> GetImageIndexDetailsAsync(string imagePath, int index, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        return await _powerShellService.GetImageIndexDetailsAsync(imagePath, index, progress, cancellationToken);
    }

    #endregion

    #region Public Methods - Image Management

    /// <summary>
    /// Imports a new Windows image and adds it to the collection.
    /// </summary>
    /// <param name="imagePath">The path to the image file to import.</param>
    /// <param name="name">The display name for the image.</param>
    /// <param name="sourceIsoPath">The source ISO path if applicable.</param>
    /// <param name="copyToManagedDirectory">Whether to copy the image to the managed directory.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The imported Windows image information.</returns>
    public async Task<WindowsImageInfo> ImportImageAsync(string imagePath, string name, string sourceIsoPath = "", bool copyToManagedDirectory = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Importing Windows image: {ImagePath}", imagePath);
        progress?.Report("Starting image import...");

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        if (!_fileService.IsValidFilePath(imagePath))
        {
            throw new ArgumentException($"Invalid file path: {imagePath}");
        }

        try
        {
            var finalImagePath = imagePath;

            if (copyToManagedDirectory)
            {
                progress?.Report("Copying image to managed directory...");
                finalImagePath = await _fileService.CopyImageToManagedDirectoryAsync(imagePath, name, progress, cancellationToken);
            }

            // Analyze the image to get indices
            progress?.Report("Analyzing image structure...");
            var indices = await _powerShellService.AnalyzeImageAsync(finalImagePath, progress, cancellationToken);

            // Get file information
            var fileInfo = new FileInfo(finalImagePath);

            // Create the WindowsImageInfo object
            var imageInfo = new WindowsImageInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                FilePath = finalImagePath,
                SourceIsoPath = sourceIsoPath,
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                FileSizeBytes = fileInfo.Length,
                ImageType = Path.GetExtension(finalImagePath).TrimStart('.').ToUpperInvariant(),
                Indices = new System.Collections.ObjectModel.ObservableCollection<WindowsImageIndex>(indices)
            };

            // Add to metadata collection
            var existingImages = await _metadataService.GetImagesAsync(cancellationToken);
            existingImages.Add(imageInfo);
            await _metadataService.SaveImagesAsync(existingImages, cancellationToken);

            progress?.Report("Import completed successfully");
            Logger.Information("Successfully imported Windows image: {Name}", name);

            return imageInfo;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to import Windows image: {ImagePath}", imagePath);
            throw;
        }
    }

    /// <summary>
    /// Deletes a Windows image from the collection and optionally from disk.
    /// </summary>
    /// <param name="imageInfo">The image to delete.</param>
    /// <param name="deleteFromDisk">Whether to delete the file from disk.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task DeleteImageAsync(WindowsImageInfo imageInfo, bool deleteFromDisk = false, CancellationToken cancellationToken = default)
    {
        Logger.Information("Deleting Windows image: {Name} (DeleteFromDisk: {DeleteFromDisk})", imageInfo.Name, deleteFromDisk);

        try
        {
            // Remove from metadata
            await _metadataService.RemoveImageAsync(imageInfo, cancellationToken);

            // Delete physical file if requested
            if (deleteFromDisk && File.Exists(imageInfo.FilePath))
            {
                File.Delete(imageInfo.FilePath);
                Logger.Information("Deleted image file from disk: {FilePath}", imageInfo.FilePath);
            }

            Logger.Information("Successfully deleted Windows image: {Name}", imageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete Windows image: {Name}", imageInfo.Name);
            throw;
        }
    }

    /// <summary>
    /// Renames an existing Windows image file and updates its metadata.
    /// </summary>
    /// <param name="imageInfo">The image to rename.</param>
    /// <param name="newName">The new display name for the image.</param>
    /// <param name="renamePhysicalFile">Whether to rename the physical file as well.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated image information.</returns>
    public async Task<WindowsImageInfo> RenameImageAsync(WindowsImageInfo imageInfo, string newName, bool renamePhysicalFile = false, CancellationToken cancellationToken = default)
    {
        Logger.Information("Renaming Windows image: {OldName} -> {NewName} (RenameFile: {RenameFile})", imageInfo.Name, newName, renamePhysicalFile);

        if (imageInfo == null)
        {
            throw new ArgumentNullException(nameof(imageInfo));
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New name cannot be null or empty.", nameof(newName));
        }

        if (!_fileService.IsValidFileName(newName))
        {
            throw new ArgumentException($"Invalid file name: {newName}", nameof(newName));
        }

        try
        {
            var oldFilePath = imageInfo.FilePath;
            var newFilePath = oldFilePath;

            // Rename physical file if requested
            if (renamePhysicalFile && File.Exists(oldFilePath))
            {
                var extension = Path.GetExtension(oldFilePath);
                newFilePath = _fileService.GenerateUniqueFilePath(newName, extension);

                // Move file to new location
                File.Move(oldFilePath, newFilePath);
                Logger.Information("Renamed physical file: {OldPath} -> {NewPath}", oldFilePath, newFilePath);
            }

            // Update image metadata
            imageInfo.Name = newName;
            imageInfo.FilePath = newFilePath;
            imageInfo.ModifiedDate = DateTime.Now;

            // Update in metadata collection
            await _metadataService.UpdateImageAsync(imageInfo, cancellationToken);

            Logger.Information("Successfully renamed Windows image: {Name}", newName);
            return imageInfo;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to rename Windows image: {OldName} -> {NewName}", imageInfo.Name, newName);
            throw;
        }
    }

    #endregion

    #region Public Methods - ISO Operations

    /// <summary>
    /// Imports a Windows image from an ISO file.
    /// </summary>
    /// <param name="isoFile">The ISO file to import from.</param>
    /// <param name="customName">Optional custom name for the imported image.</param>
    /// <param name="progress">Progress reporter for the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The imported Windows image information.</returns>
    public async Task<WindowsImageInfo> ImportFromIsoAsync(StorageFile isoFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        var imageInfo = await _isoService.ImportFromIsoAsync(isoFile, customName, progress, cancellationToken);

        // Add to metadata collection
        var existingImages = await _metadataService.GetImagesAsync(cancellationToken);
        existingImages.Add(imageInfo);
        await _metadataService.SaveImagesAsync(existingImages, cancellationToken);

        return imageInfo;
    }

    /// <summary>
    /// Imports a Windows image directly from a WIM/ESD file.
    /// </summary>
    /// <param name="wimFile">The WIM/ESD file to import.</param>
    /// <param name="customName">Optional custom name for the imported image.</param>
    /// <param name="progress">Progress reporter for the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The imported Windows image information.</returns>
    public async Task<WindowsImageInfo> ImportFromWimAsync(StorageFile wimFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        var imageInfo = await _isoService.ImportFromWimAsync(wimFile, customName, progress, cancellationToken);

        // Add to metadata collection
        var existingImages = await _metadataService.GetImagesAsync(cancellationToken);
        existingImages.Add(imageInfo);
        await _metadataService.SaveImagesAsync(existingImages, cancellationToken);

        return imageInfo;
    }

    #endregion

    #region Public Methods - File Information Refresh

    /// <summary>
    /// Refreshes file information (size, dates, type) for all images in the collection.
    /// This is useful for updating metadata after file system changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated collection of images.</returns>
    public async Task<ObservableCollection<WindowsImageInfo>> RefreshFileInfoAsync(CancellationToken cancellationToken = default)
    {
        Logger.Information("Refreshing file information for all images");

        var images = await _metadataService.GetImagesAsync(cancellationToken);
        var hasChanges = false;

        foreach (var image in images)
        {
            if (File.Exists(image.FilePath))
            {
                var fileInfo = new FileInfo(image.FilePath);

                // Update file information if it has changed
                if (image.FileSizeBytes != fileInfo.Length ||
                    image.ModifiedDate != fileInfo.LastWriteTime ||
                    string.IsNullOrEmpty(image.ImageType))
                {
                    image.FileSizeBytes = fileInfo.Length;
                    image.CreatedDate = fileInfo.CreationTime;
                    image.ModifiedDate = fileInfo.LastWriteTime;
                    image.ImageType = Path.GetExtension(image.FilePath).TrimStart('.').ToUpperInvariant();
                    hasChanges = true;

                    Logger.Debug("Updated file info for image: {Name} - Size: {Size} bytes",
                        image.Name, image.FileSizeBytes);
                }
            }
            else
            {
                Logger.Warning("Image file not found: {FilePath}", image.FilePath);
            }
        }

        // Save changes if any were made
        if (hasChanges)
        {
            await _metadataService.SaveImagesAsync(images, cancellationToken);
            Logger.Information("File information refreshed and saved for {Count} images", images.Count);
        }
        else
        {
            Logger.Information("No file information updates needed");
        }

        return images;
    }

    #endregion
}
