using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Bucket.Models;

namespace Bucket.Services;

/// <summary>
/// Service for managing Windows image files and their metadata.
/// </summary>
public class WindowsImageService
{
    private readonly string _imagesDataPath;
    private readonly string _imagesDirectory;

    /// <summary>
    /// Initializes a new instance of the WindowsImageService class.
    /// </summary>
    public WindowsImageService()
    {
        _imagesDirectory = Constants.ImportedWIMsDirectoryPath;
        _imagesDataPath = Path.Combine(_imagesDirectory, "images.json");

        // Ensure the images directory exists
        Directory.CreateDirectory(_imagesDirectory);

        Logger.Debug("WindowsImageService initialized with directory: {Directory}", _imagesDirectory);
    }

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
            var images = JsonSerializer.Deserialize<List<WindowsImageInfo>>(jsonContent) ?? new List<WindowsImageInfo>();

            // Validate that image files still exist
            var validImages = images.Where(img => File.Exists(img.FilePath)).ToList();

            if (validImages.Count != images.Count)
            {
                Logger.Warning("Found {Removed} missing image files, updating metadata", images.Count - validImages.Count);
                await SaveImagesAsync(validImages, cancellationToken);
            }

            Logger.Information("Loaded {Count} Windows images", validImages.Count);
            return new ObservableCollection<WindowsImageInfo>(validImages);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load Windows images");
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
            var jsonContent = JsonSerializer.Serialize(images, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_imagesDataPath, jsonContent, cancellationToken);

            Logger.Information("Successfully saved metadata for {Count} images", images.Count());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save Windows images metadata");
            throw;
        }
    }

    /// <summary>
    /// Analyzes a WIM/ESD file and extracts its indices asynchronously.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image indices.</returns>
    public async Task<List<WindowsImageIndex>> AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Analyzing Windows image: {ImagePath}", imagePath);
        progress?.Report("Initializing image analysis...");

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        try
        {
            var indices = new List<WindowsImageIndex>();
            progress?.Report("Extracting image information...");

            // Use DISM to get image information
            var processInfo = new ProcessStartInfo
            {
                FileName = "dism.exe",
                Arguments = $"/Get-WimInfo /WimFile:\"{imagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new List<string>();
            var errors = new List<string>();

            process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) output.Add(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) errors.Add(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var errorMessage = string.Join("\n", errors);
                throw new InvalidOperationException($"DISM failed with exit code {process.ExitCode}: {errorMessage}");
            }

            progress?.Report("Parsing image information...");

            // Parse DISM output
            indices = ParseDismOutput(output);

            Logger.Information("Successfully analyzed image with {Count} indices", indices.Count);
            return indices;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to analyze Windows image: {ImagePath}", imagePath);
            throw;
        }
    }

    /// <summary>
    /// Imports a new Windows image and adds it to the collection.
    /// </summary>
    /// <param name="imagePath">The path to the image file to import.</param>
    /// <param name="name">The display name for the image.</param>
    /// <param name="sourceIsoPath">The source ISO path if applicable.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The imported Windows image information.</returns>
    public async Task<WindowsImageInfo> ImportImageAsync(string imagePath, string name, string sourceIsoPath = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Importing Windows image: {ImagePath}", imagePath);
        progress?.Report("Starting image import...");

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        try
        {
            // Analyze the image to get its indices
            var indices = await AnalyzeImageAsync(imagePath, progress, cancellationToken);

            // Get file information
            var fileInfo = new FileInfo(imagePath);

            // Create the image info object
            var imageInfo = new WindowsImageInfo
            {
                Name = name,
                FilePath = imagePath,
                ImageType = Path.GetExtension(imagePath).ToUpperInvariant().TrimStart('.'),
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                FileSizeBytes = fileInfo.Length,
                SourceIsoPath = sourceIsoPath,
                Indices = new ObservableCollection<WindowsImageIndex>(indices)
            };

            // Load existing images and add the new one
            var existingImages = await GetImagesAsync(cancellationToken);
            existingImages.Add(imageInfo);

            // Save the updated collection
            await SaveImagesAsync(existingImages, cancellationToken);

            progress?.Report("Image import completed");
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
            // Load existing images
            var existingImages = await GetImagesAsync(cancellationToken);

            // Remove the image from the collection
            var imageToRemove = existingImages.FirstOrDefault(img => img.Id == imageInfo.Id);
            if (imageToRemove != null)
            {
                existingImages.Remove(imageToRemove);

                // Save the updated collection
                await SaveImagesAsync(existingImages, cancellationToken);
            }

            // Delete from disk if requested
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
    /// Parses DISM output to extract Windows image indices.
    /// </summary>
    /// <param name="dismOutput">The DISM command output lines.</param>
    /// <returns>A list of Windows image indices.</returns>
    private static List<WindowsImageIndex> ParseDismOutput(List<string> dismOutput)
    {
        var indices = new List<WindowsImageIndex>();
        WindowsImageIndex currentIndex = null;

        foreach (var line in dismOutput)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("Index : "))
            {
                // Save previous index if exists
                if (currentIndex != null)
                {
                    indices.Add(currentIndex);
                }

                // Start new index
                if (int.TryParse(trimmedLine.Substring(8), out var indexNumber))
                {
                    currentIndex = new WindowsImageIndex { Index = indexNumber };
                }
            }
            else if (currentIndex != null)
            {
                if (trimmedLine.StartsWith("Name : "))
                {
                    currentIndex.Name = trimmedLine.Substring(7);
                }
                else if (trimmedLine.StartsWith("Description : "))
                {
                    currentIndex.Description = trimmedLine.Substring(14);
                }
                else if (trimmedLine.StartsWith("Architecture : "))
                {
                    currentIndex.Architecture = trimmedLine.Substring(15);
                }
                else if (trimmedLine.StartsWith("Size : "))
                {
                    var sizeText = trimmedLine.Substring(7);
                    // Parse size (format: "X,XXX,XXX bytes")
                    if (sizeText.Contains("bytes"))
                    {
                        var bytesText = sizeText.Replace("bytes", "").Replace(",", "").Trim();
                        if (long.TryParse(bytesText, out var sizeBytes))
                        {
                            currentIndex.SizeMB = Math.Round(sizeBytes / (1024.0 * 1024.0), 1);
                        }
                    }
                }
            }
        }

        // Add the last index
        if (currentIndex != null)
        {
            indices.Add(currentIndex);
        }

        return indices;
    }
}
