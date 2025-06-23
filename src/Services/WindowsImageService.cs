using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
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
    /// Analyzes a WIM/ESD file and extracts its indices asynchronously using PowerShell Get-WindowsImage.
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

        // Validate file format
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();
        if (!IsValidImageFormat(extension))
        {
            throw new NotSupportedException($"Unsupported image format: {extension}");
        }

        // Check available disk space
        var fileInfo = new FileInfo(imagePath);
        var driveInfo = new DriveInfo(Path.GetPathRoot(imagePath));
        if (driveInfo.AvailableFreeSpace < fileInfo.Length * 2) // Need at least 2x file size for operations
        {
            Logger.Warning("Low disk space detected: {AvailableSpace} bytes available, {FileSize} bytes needed",
                driveInfo.AvailableFreeSpace, fileInfo.Length * 2);
        }

        try
        {
            var indices = new List<WindowsImageIndex>();
            progress?.Report("Extracting image information...");

            // Use PowerShell Get-WindowsImage cmdlet for better reliability and structured output
            var escapedPath = imagePath.Replace("'", "''"); // Escape single quotes for PowerShell
            var powerShellCommand = $"Get-WindowsImage -ImagePath '{escapedPath}' | ConvertTo-Json -Depth 3";

            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"{powerShellCommand}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) errorBuilder.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString();

                // Check for specific error conditions
                if (errorMessage.Contains("Access is denied") || errorMessage.Contains("UnauthorizedAccess"))
                {
                    throw new UnauthorizedAccessException("Administrator privileges are required to analyze Windows images.");
                }
                else if (errorMessage.Contains("cannot access") || errorMessage.Contains("FileNotFound"))
                {
                    throw new IOException($"The image file is in use or corrupted: {imagePath}");
                }
                else if (errorMessage.Contains("Get-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The DISM PowerShell module is not available. Please ensure Windows ADK or DISM module is installed.");
                }

                throw new InvalidOperationException($"PowerShell Get-WindowsImage failed with exit code {process.ExitCode}: {errorMessage}");
            }

            progress?.Report("Parsing image information...");

            var jsonOutput = outputBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(jsonOutput))
            {
                Logger.Warning("No output received from Get-WindowsImage for {ImagePath}", imagePath);
                return indices;
            }

            // Parse PowerShell JSON output
            indices = ParsePowerShellOutput(jsonOutput);

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
    /// Validates if the file format is supported for Windows imaging.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>True if the format is supported, false otherwise.</returns>
    private static bool IsValidImageFormat(string extension)
    {
        return extension switch
        {
            ".wim" => true,
            ".esd" => true,
            ".swm" => true,
            _ => false
        };
    }

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

        if (!IsValidFilePath(imagePath))
        {
            throw new ArgumentException($"Invalid file path: {imagePath}");
        }

        try
        {
            var finalImagePath = imagePath;

            // Copy to managed directory if requested
            if (copyToManagedDirectory)
            {
                progress?.Report("Copying image to managed directory...");
                finalImagePath = await CopyImageToManagedDirectoryAsync(imagePath, name, progress, cancellationToken);
            }

            // Analyze the image to get its indices
            var indices = await AnalyzeImageAsync(finalImagePath, progress, cancellationToken);

            // Get file information
            var fileInfo = new FileInfo(finalImagePath);

            // Create the image info object
            var imageInfo = new WindowsImageInfo
            {
                Name = name,
                FilePath = finalImagePath,
                ImageType = Path.GetExtension(finalImagePath).ToUpperInvariant().TrimStart('.'),
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
    /// Parses PowerShell Get-WindowsImage JSON output to extract Windows image indices.
    /// </summary>
    /// <param name="jsonOutput">The JSON output from PowerShell Get-WindowsImage cmdlet.</param>
    /// <returns>A list of Windows image indices.</returns>
    private static List<WindowsImageIndex> ParsePowerShellOutput(string jsonOutput)
    {
        var indices = new List<WindowsImageIndex>();

        try
        {
            // Handle both single object and array responses
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(jsonOutput);
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "Failed to parse PowerShell JSON output: {Output}", jsonOutput);
                throw new InvalidOperationException("Invalid JSON output from PowerShell Get-WindowsImage command", ex);
            }

            using (document)
            {
                JsonElement root = document.RootElement;

                // Handle array of images
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var imageElement in root.EnumerateArray())
                    {
                        var index = ParseImageElement(imageElement);
                        if (index != null)
                        {
                            indices.Add(index);
                        }
                    }
                }
                // Handle single image object
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    var index = ParseImageElement(root);
                    if (index != null)
                    {
                        indices.Add(index);
                    }
                }
            }

            Logger.Debug("Parsed {Count} image indices from PowerShell output", indices.Count);
            return indices;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to parse PowerShell output");
            throw;
        }
    }

    /// <summary>
    /// Parses a single image element from PowerShell JSON output.
    /// </summary>
    /// <param name="imageElement">The JSON element representing a single image.</param>
    /// <returns>A WindowsImageIndex object or null if parsing fails.</returns>
    private static WindowsImageIndex ParseImageElement(JsonElement imageElement)
    {
        try
        {
            var index = new WindowsImageIndex();

            // Extract index number
            if (imageElement.TryGetProperty("ImageIndex", out var indexProperty))
            {
                index.Index = indexProperty.GetInt32();
            }

            // Extract image name
            if (imageElement.TryGetProperty("ImageName", out var nameProperty))
            {
                index.Name = nameProperty.GetString() ?? string.Empty;
            }

            // Extract description
            if (imageElement.TryGetProperty("ImageDescription", out var descProperty))
            {
                index.Description = descProperty.GetString() ?? string.Empty;
            }

            // Extract architecture
            if (imageElement.TryGetProperty("Architecture", out var archProperty))
            {
                var archValue = archProperty.GetInt32();
                index.Architecture = archValue switch
                {
                    0 => "x86",
                    5 => "ARM",
                    6 => "IA64",
                    9 => "x64",
                    12 => "ARM64",
                    _ => archValue.ToString()
                };
            }

            // Extract size
            if (imageElement.TryGetProperty("ImageSize", out var sizeProperty))
            {
                var sizeBytes = sizeProperty.GetInt64();
                index.SizeMB = Math.Round(sizeBytes / (1024.0 * 1024.0), 1);
            }

            // Only return valid indices with required properties
            if (index.Index > 0 && !string.IsNullOrEmpty(index.Name))
            {
                return index;
            }

            Logger.Warning("Skipping invalid image index: Index={Index}, Name={Name}", index.Index, index.Name);
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to parse image element from JSON");
            return null;
        }
    }

    /// <summary>
    /// Copies an image file to the managed images directory.
    /// </summary>
    /// <param name="sourcePath">The source image file path.</param>
    /// <param name="targetName">The target file name (without extension).</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path to the copied image file.</returns>
    public async Task<string> CopyImageToManagedDirectoryAsync(string sourcePath, string targetName, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Copying image to managed directory: {SourcePath} -> {TargetName}", sourcePath, targetName);
        progress?.Report("Preparing to copy image file...");

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Source image file not found: {sourcePath}");
        }

        var sourceInfo = new FileInfo(sourcePath);
        var extension = sourceInfo.Extension;
        var targetPath = Path.Combine(_imagesDirectory, $"{targetName}{extension}");

        // Ensure target directory exists
        Directory.CreateDirectory(_imagesDirectory);

        // Check if target already exists
        if (File.Exists(targetPath))
        {
            var counter = 1;
            var baseName = targetName;
            do
            {
                targetName = $"{baseName}_{counter}";
                targetPath = Path.Combine(_imagesDirectory, $"{targetName}{extension}");
                counter++;
            }
            while (File.Exists(targetPath));
        }

        // Check available disk space
        var driveInfo = new DriveInfo(Path.GetPathRoot(_imagesDirectory));
        if (driveInfo.AvailableFreeSpace < sourceInfo.Length)
        {
            throw new IOException($"Insufficient disk space. Need {sourceInfo.Length} bytes, but only {driveInfo.AvailableFreeSpace} bytes available.");
        }

        try
        {
            progress?.Report($"Copying {sourceInfo.Name}...");

            // Copy the file
            await Task.Run(() => File.Copy(sourcePath, targetPath), cancellationToken);

            progress?.Report("Copy completed");
            Logger.Information("Successfully copied image to: {TargetPath}", targetPath);

            return targetPath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy image file: {SourcePath} -> {TargetPath}", sourcePath, targetPath);

            // Clean up partial copy if it exists
            if (File.Exists(targetPath))
            {
                try
                {
                    File.Delete(targetPath);
                }
                catch (Exception deleteEx)
                {
                    Logger.Warning(deleteEx, "Failed to clean up partial copy: {TargetPath}", targetPath);
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Validates file path to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>True if the path is safe, false otherwise.</returns>
    public static bool IsValidFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // Check for directory traversal attempts
        if (filePath.Contains("..") || filePath.Contains("~"))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (filePath.Any(c => invalidChars.Contains(c)))
            return false;

        try
        {
            // Try to get full path - this will throw if invalid
            Path.GetFullPath(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
