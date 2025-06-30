using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;
using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for managing Windows image mounting operations.
/// Provides functionality to mount, unmount, and manage Windows images with support for multi-mount scenarios.
/// </summary>
public class WindowsImageMountService : IWindowsImageMountService
{
    private readonly IWindowsImagePowerShellService _powerShellService;

    /// <summary>
    /// Initializes a new instance of the WindowsImageMountService class.
    /// </summary>
    /// <param name="powerShellService">The PowerShell service for executing DISM commands.</param>
    public WindowsImageMountService(IWindowsImagePowerShellService powerShellService)
    {
        _powerShellService = powerShellService ?? throw new ArgumentNullException(nameof(powerShellService));
        Logger.Debug("WindowsImageMountService initialized");
    }

    /// <inheritdoc />
    public async Task<MountedImageInfo> MountImageAsync(string imagePath, int index, string imageName, string editionName, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));

        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image file not found: {imagePath}");

        if (index <= 0)
            throw new ArgumentException("Index must be greater than 0.", nameof(index));

        Logger.Information("Mounting Windows image: {ImagePath}, Index: {Index}", imagePath, index);
        
        progress?.Report("Preparing to mount image...");

        // Create unique mount directory
        var mountPath = GetMountDirectoryPath(imagePath, index);
        bool mountDirectoryCreated = false;

        try
        {
            // Check if already mounted
            if (await IsImageMountedAsync(imagePath, index, cancellationToken))
            {
                throw new InvalidOperationException($"Image index {index} is already mounted.");
            }

            // Create mount directory
            Directory.CreateDirectory(mountPath);
            mountDirectoryCreated = true;

            progress?.Report($"Mounting to: {mountPath}");

            // Execute mount command using PowerShell
            var command = $"Mount-WindowsImage -ImagePath '{imagePath}' -Index {index} -Path '{mountPath}'";
            await ExecutePowerShellCommandAsync(command, index, progress, cancellationToken);

            // Verify mount was successful
            if (!await IsImageMountedAsync(imagePath, index, cancellationToken))
            {
                throw new InvalidOperationException("Mount operation completed but image is not showing as mounted.");
            }

            var mountedImage = new MountedImageInfo
            {
                Id = Guid.NewGuid().ToString(),
                ImagePath = imagePath,
                Index = index,
                MountPath = mountPath,
                ImageName = imageName,
                EditionName = editionName,
                MountedAt = DateTime.Now,
                Status = MountStatus.Mounted
            };

            progress?.Report("Mount completed successfully");
            Logger.Information("Successfully mounted image: {ImagePath}, Index: {Index} to {MountPath}", imagePath, index, mountPath);

            return mountedImage;
        }
        catch (OperationCanceledException)
        {
            Logger.Information("Mount operation was cancelled by user: {ImagePath}, Index: {Index}", imagePath, index);
            
            // Clean up mount directory if it was created
            await CleanupMountDirectoryOnCancellation(mountPath, mountDirectoryCreated);
            
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to mount image: {ImagePath}, Index: {Index}", imagePath, index);
            
            // Clean up mount directory if it was created
            await CleanupMountDirectoryOnCancellation(mountPath, mountDirectoryCreated);
            
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<MountedImageInfo>> GetMountedImagesAsync(CancellationToken cancellationToken = default)
    {
        Logger.Debug("Getting list of mounted images");

        try
        {
            var command = "Get-WindowsImage -Mounted | ConvertTo-Json -Depth 10";
            var jsonOutput = await ExecutePowerShellCommandWithOutputAsync(command, cancellationToken);

            var mountedImages = new List<MountedImageInfo>();

            if (string.IsNullOrEmpty(jsonOutput))
            {
                Logger.Information("No mounted images found");
                return mountedImages;
            }

            // Parse the JSON output
            var parsedMounts = ParseMountedImagesJson(jsonOutput);
            mountedImages.AddRange(parsedMounts);

            Logger.Information("Found {Count} mounted images", mountedImages.Count);
            return mountedImages;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to get mounted images list");
            throw;
        }
    }

    /// <inheritdoc />
    public Task OpenMountDirectoryAsync(MountedImageInfo mountedImage)
    {
        if (mountedImage == null)
            throw new ArgumentNullException(nameof(mountedImage));

        if (!Directory.Exists(mountedImage.MountPath))
            throw new DirectoryNotFoundException($"Mount directory not found: {mountedImage.MountPath}");

        Logger.Information("Opening mount directory: {MountPath}", mountedImage.MountPath);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{mountedImage.MountPath}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            Logger.Debug("Successfully opened mount directory in Explorer");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open mount directory: {MountPath}", mountedImage.MountPath);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsImageMountedAsync(string imagePath, int index, CancellationToken cancellationToken = default)
    {
        try
        {
            var mountedImages = await GetMountedImagesAsync(cancellationToken);
            return mountedImages.Any(m => 
                string.Equals(m.ImagePath, imagePath, StringComparison.OrdinalIgnoreCase) && 
                m.Index == index);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to check if image is mounted: {ImagePath}, Index: {Index}", imagePath, index);
            return false;
        }
    }

    /// <inheritdoc />
    public string GetMountDirectoryPath(string imagePath, int index)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));

        if (index <= 0)
            throw new ArgumentException("Index must be greater than 0.", nameof(index));

        // Create a safe directory name from the WIM file name and index
        var fileName = Path.GetFileNameWithoutExtension(imagePath);
        var safeFileName = Regex.Replace(fileName, @"[^\w\-_]", "_");
        var directoryName = $"{safeFileName}_{index}";

        return Path.Combine(Constants.MountDirectoryPath, directoryName);
    }

    /// <inheritdoc />
    public async Task CleanupOrphanedMountDirectoriesAsync(IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Starting cleanup of orphaned mount directories");
        progress?.Report("Checking for orphaned mount directories...");

        try
        {
            if (!Directory.Exists(Constants.MountDirectoryPath))
            {
                Logger.Debug("Mount directory does not exist, nothing to clean up");
                progress?.Report("No mount directory found");
                return;
            }

            // Get currently mounted images
            var mountedImages = await GetMountedImagesAsync(cancellationToken);
            var activeMountPaths = mountedImages.Select(m => m.MountPath).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Get all directories in the mount path
            var allMountDirectories = Directory.GetDirectories(Constants.MountDirectoryPath);
            var orphanedDirectories = allMountDirectories.Where(dir => !activeMountPaths.Contains(dir)).ToList();

            if (orphanedDirectories.Count == 0)
            {
                Logger.Information("No orphaned mount directories found");
                progress?.Report("No orphaned directories found");
                return;
            }

            Logger.Information("Found {Count} orphaned mount directories", orphanedDirectories.Count);
            progress?.Report($"Found {orphanedDirectories.Count} orphaned directories to clean up");

            for (int i = 0; i < orphanedDirectories.Count; i++)
            {
                var orphanedDir = orphanedDirectories[i];
                var dirName = Path.GetFileName(orphanedDir);
                
                progress?.Report($"Cleaning up directory {i + 1}/{orphanedDirectories.Count}: {dirName}");
                
                try
                {
                    // Try DISM cleanup first
                    try
                    {
                        await ExecuteDismCleanupAsync(orphanedDir);
                        Logger.Debug("DISM cleanup completed for orphaned directory: {Directory}", orphanedDir);
                    }
                    catch (Exception dismEx)
                    {
                        Logger.Debug(dismEx, "DISM cleanup failed for orphaned directory, trying manual cleanup: {Directory}", orphanedDir);
                        await AttemptManualCleanup(orphanedDir);
                    }

                    // Remove directory if empty
                    if (Directory.Exists(orphanedDir))
                    {
                        var remainingFiles = Directory.GetFileSystemEntries(orphanedDir);
                        if (remainingFiles.Length == 0)
                        {
                            Directory.Delete(orphanedDir);
                            Logger.Information("Removed empty orphaned directory: {Directory}", orphanedDir);
                        }
                        else
                        {
                            Logger.Warning("Orphaned directory still contains {FileCount} files after cleanup: {Directory}", remainingFiles.Length, orphanedDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to clean up orphaned directory: {Directory}", orphanedDir);
                }
            }

            progress?.Report("Orphaned directory cleanup completed");
            Logger.Information("Completed cleanup of orphaned mount directories");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to cleanup orphaned mount directories");
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Executes a PowerShell command and reports progress.
    /// </summary>
    private async Task ExecutePowerShellCommandAsync(string command, int index, IProgress<string> progress, CancellationToken cancellationToken)
    {
        Process process = null;
        try
        {
            progress?.Report($"Mounting index {index}, please wait...");

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
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

            // Register cancellation callback to kill the process
            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        Logger.Information("Cancelling PowerShell process due to user cancellation");
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to kill PowerShell process during cancellation");
                }
            });

            await Task.Run(() => process.WaitForExit(), cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("PowerShell command was cancelled by user", cancellationToken);
            }

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString().Trim();
                Logger.Error("PowerShell command failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);

                if (errorMessage.Contains("Access is denied") || errorMessage.Contains("UnauthorizedAccess"))
                {
                    throw new UnauthorizedAccessException($"Access denied when executing PowerShell command. Please run as administrator.");
                }
                else if (errorMessage.Contains("Mount-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The Mount-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }
                else if (errorMessage.Contains("Dismount-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The Dismount-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }

                throw new InvalidOperationException($"PowerShell command failed with exit code {process.ExitCode}: {errorMessage}");
            }

            Logger.Debug("PowerShell command executed successfully: {Command}", command);
        }
        catch (OperationCanceledException)
        {
            Logger.Information("PowerShell command was cancelled: {Command}", command);
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "PowerShell command failed: {Command}", command);
            throw;
        }
        finally
        {
            process?.Dispose();
        }
    }

    /// <summary>
    /// Executes a PowerShell command and returns the output.
    /// </summary>
    private async Task<string> ExecutePowerShellCommandWithOutputAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
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
                Logger.Error("PowerShell command failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);

                if (errorMessage.Contains("Get-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The Get-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }

                throw new InvalidOperationException($"PowerShell command failed with exit code {process.ExitCode}: {errorMessage}");
            }

            var jsonOutput = outputBuilder.ToString().Trim();
            Logger.Debug("PowerShell command executed successfully with output: {Command}", command);
            return jsonOutput;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "PowerShell command failed: {Command}", command);
            throw;
        }
    }

    /// <summary>
    /// Parses the JSON output from Get-WindowsImage -Mounted command.
    /// </summary>
    private static List<MountedImageInfo> ParseMountedImagesJson(string jsonOutput)
    {
        var mountedImages = new List<MountedImageInfo>();

        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;

            // Handle both single object and array responses
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var imageElement in root.EnumerateArray())
                {
                    var mountedImage = ParseMountedImageElement(imageElement);
                    if (mountedImage != null)
                    {
                        mountedImages.Add(mountedImage);
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                var mountedImage = ParseMountedImageElement(root);
                if (mountedImage != null)
                {
                    mountedImages.Add(mountedImage);
                }
            }
        }
        catch (JsonException ex)
        {
            Logger.Error(ex, "Failed to parse JSON output from Get-WindowsImage -Mounted");
            throw new InvalidOperationException("Invalid JSON output from PowerShell Get-WindowsImage -Mounted command", ex);
        }

        return mountedImages;
    }

    /// <summary>
    /// Parses a single mounted image element from JSON.
    /// </summary>
    private static MountedImageInfo ParseMountedImageElement(JsonElement imageElement)
    {
        try
        {
            var mountedImage = new MountedImageInfo
            {
                Id = Guid.NewGuid().ToString(),
                Status = MountStatus.Mounted
            };

            // Parse ImagePath
            if (imageElement.TryGetProperty("ImagePath", out var imagePathElement))
            {
                mountedImage.ImagePath = imagePathElement.GetString() ?? "";
                mountedImage.ImageName = Path.GetFileNameWithoutExtension(mountedImage.ImagePath);
            }

            // Parse ImageIndex
            if (imageElement.TryGetProperty("ImageIndex", out var indexElement))
            {
                if (indexElement.ValueKind == JsonValueKind.Number)
                {
                    mountedImage.Index = indexElement.GetInt32();
                }
                else if (indexElement.ValueKind == JsonValueKind.String)
                {
                    int.TryParse(indexElement.GetString(), out var index);
                    mountedImage.Index = index;
                }
            }

            // Parse Path (mount directory)
            if (imageElement.TryGetProperty("Path", out var pathElement))
            {
                mountedImage.MountPath = pathElement.GetString() ?? "";
                mountedImage.MountedAt = Directory.Exists(mountedImage.MountPath) ? 
                    Directory.GetCreationTime(mountedImage.MountPath) : DateTime.Now;
            }

            // Parse ImageName (edition name)
            if (imageElement.TryGetProperty("ImageName", out var nameElement))
            {
                mountedImage.EditionName = nameElement.GetString() ?? "";
            }

            // Validate that we have the essential information
            if (string.IsNullOrEmpty(mountedImage.ImagePath) || 
                string.IsNullOrEmpty(mountedImage.MountPath) || 
                mountedImage.Index <= 0)
            {
                Logger.Warning("Skipping mounted image with incomplete information");
                return null;
            }

            return mountedImage;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to parse mounted image element");
            return null;
        }
    }

    /// <summary>
    /// Cleans up mount directory after cancellation or failure.
    /// </summary>
    private async Task CleanupMountDirectoryOnCancellation(string mountPath, bool mountDirectoryCreated)
    {
        if (!mountDirectoryCreated || !Directory.Exists(mountPath))
            return;

        try
        {
            await Task.Delay(1000); // Wait a moment for any processes to release handles
            
            // Check if the directory has any files (indicating partial mount)
            var files = Directory.GetFileSystemEntries(mountPath);
            if (files.Length > 0)
            {
                Logger.Warning("Mount directory contains files after cancellation, attempting DISM cleanup: {MountPath}", mountPath);
                
                // Try to use DISM to clean up the partial mount first
                try
                {
                    await ExecuteDismCleanupAsync(mountPath);
                    Logger.Information("DISM cleanup completed for mount path: {MountPath}", mountPath);
                }
                catch (Exception dismEx)
                {
                    Logger.Warning(dismEx, "DISM cleanup failed, attempting manual cleanup: {MountPath}", mountPath);
                    
                    // Fallback to manual cleanup
                    await AttemptManualCleanup(mountPath);
                }
            }
            
            // Remove the mount directory itself if it still exists and is empty
            if (Directory.Exists(mountPath))
            {
                var remainingFiles = Directory.GetFileSystemEntries(mountPath);
                if (remainingFiles.Length == 0)
                {
                    Directory.Delete(mountPath);
                    Logger.Information("Empty mount directory removed after cancellation: {MountPath}", mountPath);
                }
                else
                {
                    Logger.Warning("Mount directory still contains {FileCount} files after cleanup attempts: {MountPath}", remainingFiles.Length, mountPath);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to clean up mount directory after cancellation: {MountPath}", mountPath);
        }
    }

    /// <summary>
    /// Uses DISM to clean up a partial mount.
    /// </summary>
    private async Task ExecuteDismCleanupAsync(string mountPath)
    {
        var command = $"Dismount-WindowsImage -Path '{mountPath}' -Discard";
        
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        await Task.Run(() => process.WaitForExit());
        
        // Don't throw exception if DISM cleanup fails - we'll try manual cleanup
        if (process.ExitCode != 0)
        {
            Logger.Debug("DISM cleanup returned exit code {ExitCode} for path: {MountPath}", process.ExitCode, mountPath);
        }
    }

    /// <summary>
    /// Attempts manual cleanup of mount directory files.
    /// </summary>
    private async Task AttemptManualCleanup(string mountPath)
    {
        await Task.Run(() =>
        {
            try
            {
                // Try to remove read-only attributes and delete
                var files = Directory.GetFileSystemEntries(mountPath, "*", SearchOption.AllDirectories);
                
                // Remove read-only attributes from files
                foreach (var file in files.Where(f => File.Exists(f)))
                {
                    try
                    {
                        var attributes = File.GetAttributes(file);
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
                        }
                    }
                    catch (Exception attrEx)
                    {
                        Logger.Debug(attrEx, "Failed to remove read-only attribute from file: {File}", file);
                    }
                }
                
                // Try to delete directories recursively
                var directories = Directory.GetDirectories(mountPath);
                foreach (var dir in directories)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception dirEx)
                    {
                        Logger.Debug(dirEx, "Failed to delete directory during manual cleanup: {Directory}", dir);
                    }
                }
                
                // Try to delete remaining files
                var remainingFiles = Directory.GetFiles(mountPath);
                foreach (var file in remainingFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception fileEx)
                    {
                        Logger.Debug(fileEx, "Failed to delete file during manual cleanup: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Manual cleanup encountered errors for path: {MountPath}", mountPath);
            }
        });
    }

    #endregion
} 