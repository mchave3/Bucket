using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Bucket.Models;
using Windows.Storage;

namespace Bucket.Services;

/// <summary>
/// Service for managing Windows image files and their metadata using PowerShell Get-WindowsImage cmdlet.
/// Provides functionality to analyze, import, manage, and extract detailed information from WIM/ESD files.
/// </summary>
public class WindowsImageService
{
    #region Private Fields

    private readonly string _imagesDataPath;
    private readonly string _imagesDirectory;

    #region Constructor

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

    #endregion

    #region Public Methods - Image Collection Management

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



    #endregion

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
                    throw new InvalidOperationException("The Get-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
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

    #endregion

    #region Public Methods - Image Analysis and Details

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
        Logger.Information("Getting detailed information for image index {Index} in {ImagePath}", index, imagePath);

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        try
        {
            progress?.Report($"Loading detailed information for index {index}...");

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // Use PowerShell Get-WindowsImage with specific index to get detailed info
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Get-WindowsImage -ImagePath '{imagePath}' -Index {index} | ConvertTo-Json -Depth 10\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), cancellationToken);

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString().Trim();
                Logger.Error("PowerShell Get-WindowsImage failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);

                if (errorMessage.Contains("Get-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The Get-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }

                throw new InvalidOperationException($"PowerShell Get-WindowsImage failed with exit code {process.ExitCode}: {errorMessage}");
            }

            progress?.Report("Parsing detailed image information...");

            var jsonOutput = outputBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(jsonOutput))
            {
                Logger.Warning("No output received from Get-WindowsImage for index {Index} in {ImagePath}", index, imagePath);
                return null;
            }

            // Parse the JSON output for detailed information
            var detailedIndices = ParsePowerShellOutput(jsonOutput);
            var detailedIndex = detailedIndices.FirstOrDefault();

            if (detailedIndex != null)
            {
                Logger.Information("Successfully loaded detailed information for index {Index}: {Name}", index, detailedIndex.Name);
            }
            else
            {
                Logger.Warning("No detailed information found for index {Index} in {ImagePath}", index, imagePath);
            }

            return detailedIndex;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to get detailed information for index {Index} in {ImagePath}", index, imagePath);
            throw;
        }
    }

    #endregion

    #region ISO Import Operations

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
        Logger.Information("Starting ISO import process for: {IsoPath}", isoFile.Path);
        progress?.Report("Starting ISO import...");

        try
        {
            // Step 1: Mount the ISO
            progress?.Report("Mounting ISO file...");
            var mountPath = await MountIsoAsync(isoFile.Path, cancellationToken);

            try
            {
                // Step 2: Find Windows image files
                progress?.Report("Scanning for Windows images...");
                var wimFiles = await FindWindowsImagesAsync(mountPath, cancellationToken);

                if (!wimFiles.Any())
                {
                    throw new InvalidOperationException("No Windows image files (install.wim or install.esd) found in the ISO.");
                }

                // Step 3: Select the main installation image (usually install.wim or install.esd)
                var mainImageFile = wimFiles.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f).Equals("install", StringComparison.OrdinalIgnoreCase))
                    ?? wimFiles.First();

                Logger.Information("Found Windows image: {ImagePath}", mainImageFile);

                // Step 4: Generate destination path using unified naming system
                var imageName = string.IsNullOrWhiteSpace(customName)
                    ? ExtractBaseName(isoFile.Name)
                    : customName;

                var destinationPath = GenerateUniqueFilePath(imageName, Path.GetExtension(mainImageFile));

                // Step 5: Copy the image file
                progress?.Report("Copying Windows image file...");
                await CopyFileWithProgressAsync(mainImageFile, destinationPath, progress, cancellationToken);

                // Step 6: Import the copied image
                progress?.Report("Analyzing Windows image...");
                var imageInfo = await ImportImageAsync(
                    destinationPath,
                    imageName,
                    isoFile.Path,
                    false, // copyToManagedDirectory = false (already copied)
                    progress,
                    cancellationToken);

                progress?.Report("Import completed successfully");
                Logger.Information("Successfully imported Windows image from ISO: {Name}", imageName);

                return imageInfo;
            }
            finally
            {
                // Step 7: Dismount the ISO
                progress?.Report("Cleaning up...");
                await DismountIsoAsync(isoFile.Path, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to import Windows image from ISO: {IsoPath}", isoFile.Path);
            throw;
        }
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
        Logger.Information("Starting WIM import process for: {WimPath}", wimFile.Path);
        progress?.Report("Starting WIM import...");

        try
        {
            // Get a friendly name for the image
            var imageName = string.IsNullOrWhiteSpace(customName)
                ? Path.GetFileNameWithoutExtension(wimFile.Name)
                : customName;

            // Import the image using the existing ImportImageAsync method
            var imageInfo = await ImportImageAsync(
                wimFile.Path,
                imageName,
                sourceIsoPath: "",
                copyToManagedDirectory: true,
                progress: progress,
                cancellationToken: cancellationToken);

            progress?.Report("Import completed successfully");
            Logger.Information("Successfully imported Windows image from WIM: {Name}", imageName);

            return imageInfo;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to import Windows image from WIM: {WimPath}", wimFile.Path);
            throw;
        }
    }

    #endregion

    #region ISO Mount/Dismount Operations

    /// <summary>
    /// Mounts an ISO file and returns the mount path.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The mount path of the ISO.</returns>
    private async Task<string> MountIsoAsync(string isoPath, CancellationToken cancellationToken)
    {
        Logger.Information("Mounting ISO: {IsoPath}", isoPath);

        try
        {
            // First check if ISO is already mounted
            if (await IsIsoMountedAsync(isoPath, cancellationToken))
            {
                Logger.Information("ISO is already mounted, getting drive letter");

                // Get the drive letter of already mounted ISO
                var getDriveProcessInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"Get-DiskImage -ImagePath '{isoPath.Replace("'", "''")}' | Get-Volume | Select-Object -ExpandProperty DriveLetter\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var getDriveProcess = new Process { StartInfo = getDriveProcessInfo };
                getDriveProcess.Start();

                var driveOutput = await getDriveProcess.StandardOutput.ReadToEndAsync();
                await getDriveProcess.WaitForExitAsync(cancellationToken);

                var existingDriveLetter = driveOutput.Trim();
                if (!string.IsNullOrEmpty(existingDriveLetter))
                {
                    var existingMountPath = $"{existingDriveLetter}:\\";
                    Logger.Information("Found existing mount at: {MountPath}", existingMountPath);
                    return existingMountPath;
                }
            }

            // Mount the ISO
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"Mount-DiskImage -ImagePath '{isoPath.Replace("'", "''")}' -PassThru | Get-Volume | Select-Object -ExpandProperty DriveLetter\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to mount ISO: {error}");
            }

            var driveLetter = output.Trim();
            if (string.IsNullOrEmpty(driveLetter))
            {
                throw new InvalidOperationException("Could not determine mount drive letter");
            }

            var mountPath = $"{driveLetter}:\\";
            Logger.Information("ISO mounted at: {MountPath}", mountPath);

            return mountPath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to mount ISO: {IsoPath}", isoPath);
            throw;
        }
    }

    /// <summary>
    /// Dismounts an ISO file.
    /// </summary>
    /// <param name="isoPath">The original ISO file path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task DismountIsoAsync(string isoPath, CancellationToken cancellationToken)
    {
        Logger.Information("Dismounting ISO: {IsoPath}", isoPath);

        try
        {
            // Use a more reliable dismount command with timeout
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"try {{ Dismount-DiskImage -ImagePath '{isoPath.Replace("'", "''")}' -ErrorAction Stop; Write-Output 'SUCCESS' }} catch {{ Write-Error $_.Exception.Message }}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            // Use a timeout for dismount operation to prevent hanging
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                if (process.ExitCode == 0 && output.Contains("SUCCESS"))
                {
                    Logger.Information("ISO dismounted successfully");
                }
                else
                {
                    Logger.Warning("ISO dismount returned exit code {ExitCode}. Output: {Output}. Error: {Error}",
                        process.ExitCode, output, error);

                    // Try alternative dismount method
                    await TryAlternativeDismountAsync(isoPath, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                Logger.Warning("ISO dismount timed out after 30 seconds, attempting to kill process");
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                    }
                }
                catch (Exception killEx)
                {
                    Logger.Warning(killEx, "Failed to kill dismount process");
                }

                // Try alternative dismount method
                await TryAlternativeDismountAsync(isoPath, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to dismount ISO cleanly: {IsoPath}", isoPath);

            // Try alternative dismount method as last resort
            await TryAlternativeDismountAsync(isoPath, cancellationToken);
        }
    }

    /// <summary>
    /// Attempts to dismount ISO using alternative method.
    /// </summary>
    /// <param name="isoPath">The original ISO file path.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task TryAlternativeDismountAsync(string isoPath, CancellationToken cancellationToken)
    {
        Logger.Information("Attempting alternative dismount method for: {IsoPath}", isoPath);

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"Get-DiskImage -ImagePath '{isoPath.Replace("'", "''")}' | Dismount-DiskImage -Force\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);

            if (process.ExitCode == 0)
            {
                Logger.Information("Alternative dismount method succeeded");
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                Logger.Warning("Alternative dismount method failed with exit code {ExitCode}: {Error}",
                    process.ExitCode, error);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Alternative dismount method also failed, ISO may remain mounted");
        }
    }

    /// <summary>
    /// Checks if an ISO file is currently mounted.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the ISO is mounted, false otherwise.</returns>
    private async Task<bool> IsIsoMountedAsync(string isoPath, CancellationToken cancellationToken)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"Get-DiskImage -ImagePath '{isoPath.Replace("'", "''")}' | Select-Object -ExpandProperty Attached\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);

            if (process.ExitCode == 0)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                return output.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to check if ISO is mounted: {IsoPath}", isoPath);
        }

        return false;
    }

    /// <summary>
    /// Finds Windows image files in the mounted ISO.
    /// </summary>
    /// <param name="mountPath">The mount path to search.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A list of found Windows image files.</returns>
    private Task<List<string>> FindWindowsImagesAsync(string mountPath, CancellationToken cancellationToken)
    {
        Logger.Information("Searching for Windows images in: {MountPath}", mountPath);

        var wimFiles = new List<string>();
        var searchPaths = new[]
        {
            Path.Combine(mountPath, "sources"),
            mountPath
        };

        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                // Look for install.wim, install.esd, boot.wim
                var patterns = new[] { "install.wim", "install.esd", "boot.wim" };

                foreach (var pattern in patterns)
                {
                    var files = Directory.GetFiles(searchPath, pattern, SearchOption.TopDirectoryOnly);
                    wimFiles.AddRange(files);
                }
            }
        }

        Logger.Information("Found {Count} Windows image files", wimFiles.Count);
        return Task.FromResult(wimFiles.Distinct().ToList());
    }

    #endregion

    #region Private Methods

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

            // Extract detailed properties (available when querying specific index)
            if (imageElement.TryGetProperty("WIMBoot", out var wimBootProperty))
            {
                index.WIMBoot = wimBootProperty.GetBoolean();
                index.HasDetailedInfo = true;
            }

            if (imageElement.TryGetProperty("Version", out var versionProperty))
            {
                index.Version = versionProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("SPBuild", out var spBuildProperty))
            {
                index.SPBuild = spBuildProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("SPLevel", out var spLevelProperty))
            {
                index.SPLevel = spLevelProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("EditionId", out var editionIdProperty))
            {
                index.EditionId = editionIdProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("InstallationType", out var installationTypeProperty))
            {
                index.InstallationType = installationTypeProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("ProductType", out var productTypeProperty))
            {
                index.ProductType = productTypeProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("ProductSuite", out var productSuiteProperty))
            {
                index.ProductSuite = GetStringOrArray(productSuiteProperty);
            }

            if (imageElement.TryGetProperty("SystemRoot", out var systemRootProperty))
            {
                index.SystemRoot = systemRootProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("Hal", out var halProperty))
            {
                index.Hal = halProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("DirectoryCount", out var dirCountProperty))
            {
                index.DirectoryCount = dirCountProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("FileCount", out var fileCountProperty))
            {
                index.FileCount = fileCountProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("CreatedTime", out var createdTimeProperty))
            {
                var createdTimeString = createdTimeProperty.GetString();
                if (!string.IsNullOrEmpty(createdTimeString))
                {
                    // Handle both ISO 8601 format and Microsoft JSON.NET legacy format
                    DateTime createdTime;
                    if (TryParseMicrosoftJsonDate(createdTimeString, out createdTime) ||
                        DateTime.TryParse(createdTimeString, null, DateTimeStyles.RoundtripKind, out createdTime))
                    {
                        index.CreatedTime = createdTime;
                    }
                    else
                    {
                        Logger.Debug("Failed to parse CreatedTime: {CreatedTimeString}", createdTimeString);
                    }
                }
            }

            if (imageElement.TryGetProperty("ModifiedTime", out var modifiedTimeProperty))
            {
                var modifiedTimeString = modifiedTimeProperty.GetString();
                if (!string.IsNullOrEmpty(modifiedTimeString))
                {
                    // Handle both ISO 8601 format and Microsoft JSON.NET legacy format
                    DateTime modifiedTime;
                    if (TryParseMicrosoftJsonDate(modifiedTimeString, out modifiedTime) ||
                        DateTime.TryParse(modifiedTimeString, null, DateTimeStyles.RoundtripKind, out modifiedTime))
                    {
                        index.ModifiedTime = modifiedTime;
                    }
                    else
                    {
                        Logger.Debug("Failed to parse ModifiedTime: {ModifiedTimeString}", modifiedTimeString);
                    }
                }
            }

            if (imageElement.TryGetProperty("Languages", out var languagesProperty))
            {
                index.Languages = GetStringOrArray(languagesProperty);
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

        // Use unified naming system to generate unique file path
        var targetPath = GenerateUniqueFilePath(targetName, extension);

        // Ensure target directory exists
        Directory.CreateDirectory(_imagesDirectory);

        // Check available disk space
        var driveInfo = new DriveInfo(Path.GetPathRoot(_imagesDirectory));
        if (driveInfo.AvailableFreeSpace < sourceInfo.Length)
        {
            throw new IOException($"Insufficient disk space. Need {sourceInfo.Length} bytes, but only {driveInfo.AvailableFreeSpace} bytes available.");
        }

        try
        {
            // Copy the file with progress reporting
            await CopyFileWithProgressAsync(sourcePath, targetPath, progress, cancellationToken);

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
    /// Copies a file with progress reporting.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="destinationPath">The destination file path.</param>
    /// <param name="progress">Progress reporter for the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task CopyFileWithProgressAsync(string sourcePath, string destinationPath, IProgress<string> progress, CancellationToken cancellationToken)
    {
        Logger.Information("Copying file from {Source} to {Destination}", sourcePath, destinationPath);

        try
        {
            // Ensure destination directory exists
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var sourceInfo = new FileInfo(sourcePath);
            var totalBytes = sourceInfo.Length;
            var copiedBytes = 0L;

            const int bufferSize = 1024 * 1024; // 1MB buffer
            var buffer = new byte[bufferSize];

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    copiedBytes += bytesRead;

                    var percentComplete = (int)((double)copiedBytes / totalBytes * 100);
                    progress?.Report($"Copying file... {percentComplete}% ({FormatBytes(copiedBytes)} / {FormatBytes(totalBytes)})");

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            Logger.Information("File copied successfully: {Destination}", destinationPath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy file from {Source} to {Destination}", sourcePath, destinationPath);
            throw;
        }
    }

    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A formatted string representation.</returns>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:F1} {suffixes[suffixIndex]}";
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

    /// <summary>
    /// Helper method to get a string value from a JsonElement that could be either a string or an array
    /// </summary>
    /// <param name="element">The JsonElement to process</param>
    /// <returns>A string representation of the value</returns>
    private static string GetStringOrArray(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;

            case JsonValueKind.Array:
                var arrayValues = new List<string>();
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var value = item.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            arrayValues.Add(value);
                        }
                    }
                }
                return string.Join(", ", arrayValues);

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return string.Empty;

            default:
                return element.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Tries to parse Microsoft JSON.NET legacy date format like "/Date(1725596151358)/"
    /// </summary>
    /// <param name="dateString">The date string to parse</param>
    /// <param name="result">The parsed DateTime if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    private static bool TryParseMicrosoftJsonDate(string dateString, out DateTime result)
    {
        result = default;

        if (string.IsNullOrEmpty(dateString))
            return false;

        // Check if it matches the Microsoft JSON.NET format: /Date(timestamp)/
        if (dateString.StartsWith("/Date(") && dateString.EndsWith(")/"))
        {
            try
            {
                // Extract the timestamp part
                var timestampString = dateString.Substring(6, dateString.Length - 8);

                // Handle timezone offset if present (e.g., "/Date(1725596151358+0200)/")
                var plusIndex = timestampString.IndexOf('+');
                var minusIndex = timestampString.IndexOf('-');

                if (plusIndex > 0 || minusIndex > 0)
                {
                    // Remove timezone part for now, just use the timestamp
                    var timezoneIndex = Math.Max(plusIndex, minusIndex);
                    timestampString = timestampString.Substring(0, timezoneIndex);
                }

                // Parse the timestamp (milliseconds since Unix epoch)
                if (long.TryParse(timestampString, out var timestamp))
                {
                    // Convert from Unix timestamp (milliseconds) to DateTime
                    result = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                    return true;
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        return false;
    }

    #endregion

    #region Private Methods - File Naming

    /// <summary>
    /// Generates a unique file name based on the base name and extension, resolving conflicts with a counter.
    /// </summary>
    /// <param name="baseName">The base name for the file (without extension).</param>
    /// <param name="extension">The file extension (e.g., ".wim", ".esd").</param>
    /// <returns>A unique file name that doesn't conflict with existing files.</returns>
    private string GenerateUniqueFileName(string baseName, string extension)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            throw new ArgumentException("Base name cannot be null or empty.", nameof(baseName));
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));
        }

        // Ensure extension starts with a dot
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        // Sanitize the base name to remove invalid file name characters
        var sanitizedBaseName = SanitizeFileName(baseName);

        // Generate the initial target path
        var targetPath = Path.Combine(_imagesDirectory, $"{sanitizedBaseName}{extension}");

        // If no conflict, return the original name
        if (!File.Exists(targetPath))
        {
            Logger.Debug("Generated unique file name: {FileName}", $"{sanitizedBaseName}{extension}");
            return $"{sanitizedBaseName}{extension}";
        }

        // Find the next available counter
        var counter = 1;
        do
        {
            var numberedName = $"{sanitizedBaseName}_{counter:D3}";
            targetPath = Path.Combine(_imagesDirectory, $"{numberedName}{extension}");

            if (!File.Exists(targetPath))
            {
                Logger.Debug("Generated unique file name with counter: {FileName}", $"{numberedName}{extension}");
                return $"{numberedName}{extension}";
            }

            counter++;
        }
        while (counter <= 999); // Limit to avoid infinite loop

        // If we reach here, we have too many conflicts
        throw new InvalidOperationException($"Unable to generate unique file name for base name '{sanitizedBaseName}'. Too many existing files.");
    }

    /// <summary>
    /// Generates the full path for a unique file name.
    /// </summary>
    /// <param name="baseName">The base name for the file (without extension).</param>
    /// <param name="extension">The file extension (e.g., ".wim", ".esd").</param>
    /// <returns>The full path to the unique file.</returns>
    private string GenerateUniqueFilePath(string baseName, string extension)
    {
        var fileName = GenerateUniqueFileName(baseName, extension);
        return Path.Combine(_imagesDirectory, fileName);
    }

    /// <summary>
    /// Extracts the base name from an original file name, removing the extension.
    /// </summary>
    /// <param name="originalFileName">The original file name.</param>
    /// <returns>The base name without extension.</returns>
    private string ExtractBaseName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("Original file name cannot be null or empty.", nameof(originalFileName));
        }

        return Path.GetFileNameWithoutExtension(originalFileName);
    }

    /// <summary>
    /// Validates if a file name is valid for the current file system.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>True if the file name is valid, false otherwise.</returns>
    private bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
        {
            return false;
        }

        // Check for reserved names on Windows
        string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();

        if (reservedNames.Contains(nameWithoutExtension))
        {
            return false;
        }

        // Check length (Windows limit is 255 characters for file name)
        if (fileName.Length > 255)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sanitizes a file name by removing or replacing invalid characters.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>A sanitized file name safe for the file system.</returns>
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "UnnamedImage";
        }

        // Replace invalid characters with underscores
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;

        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        // Remove multiple consecutive underscores
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, "_+", "_");

        // Remove leading/trailing underscores and spaces
        sanitized = sanitized.Trim('_', ' ');

        // Ensure we have something left
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "UnnamedImage";
        }

        // Truncate if too long (leave room for counter and extension)
        if (sanitized.Length > 200)
        {
            sanitized = sanitized.Substring(0, 200).TrimEnd('_', ' ');
        }

        return sanitized;
    }

    /// <summary>
    /// Checks if a file name would conflict with existing files in the images directory.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file name would conflict, false otherwise.</returns>
    private bool WouldConflict(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var fullPath = Path.Combine(_imagesDirectory, fileName);
        return File.Exists(fullPath);
    }

    #endregion

    #region Rename Image

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

        if (!IsValidFileName(newName))
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
                newFilePath = GenerateUniqueFilePath(newName, extension);

                // Copy file to new location
                File.Move(oldFilePath, newFilePath);
                Logger.Information("Renamed physical file: {OldPath} -> {NewPath}", oldFilePath, newFilePath);
            }

            // Update image metadata
            imageInfo.Name = newName;
            imageInfo.FilePath = newFilePath;
            imageInfo.ModifiedDate = DateTime.Now;

            // Load existing images and update the collection
            var existingImages = await GetImagesAsync(cancellationToken);
            var imageToUpdate = existingImages.FirstOrDefault(img => img.Id == imageInfo.Id);

            if (imageToUpdate != null)
            {
                imageToUpdate.Name = newName;
                imageToUpdate.FilePath = newFilePath;
                imageToUpdate.ModifiedDate = imageInfo.ModifiedDate;

                // Save the updated collection
                await SaveImagesAsync(existingImages, cancellationToken);
            }

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
}
