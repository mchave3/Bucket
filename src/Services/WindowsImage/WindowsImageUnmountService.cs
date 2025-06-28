using System.Diagnostics;
using System.Text;
using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for unmounting Windows images with progress reporting.
/// </summary>
public class WindowsImageUnmountService : IWindowsImageUnmountService
{
    private readonly IWindowsImageMountService _mountService;

    /// <summary>
    /// Initializes a new instance of the WindowsImageUnmountService.
    /// </summary>
    /// <param name="mountService">The mount service for checking mounted images.</param>
    public WindowsImageUnmountService(IWindowsImageMountService mountService)
    {
        _mountService = mountService ?? throw new ArgumentNullException(nameof(mountService));
    }

    /// <inheritdoc />
    public async Task UnmountImageAsync(MountedImageInfo mountedImage, bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        if (mountedImage == null)
            throw new ArgumentNullException(nameof(mountedImage));

        Logger.Information("Unmounting Windows image: {ImagePath}, Index: {Index}, SaveChanges: {SaveChanges}", 
            mountedImage.ImagePath, mountedImage.Index, saveChanges);

        progress?.Report("Preparing to unmount image...");

        try
        {
            mountedImage.Status = MountStatus.Unmounting;

            // Execute unmount command using PowerShell
            var saveDiscardParam = saveChanges ? " -Save" : " -Discard";
            var command = $"Dismount-WindowsImage -Path '{mountedImage.MountPath}'{saveDiscardParam}";
            
            var actionText = saveChanges ? $"Unmounting index {mountedImage.Index} and saving changes..." : $"Unmounting index {mountedImage.Index} and discarding changes...";
            progress?.Report(actionText);
            
            await ExecutePowerShellCommandAsync(command, cancellationToken);

            // Clean up mount directory if it's empty
            await CleanupMountDirectoryAsync(mountedImage.MountPath);

            progress?.Report("Unmount completed successfully");
            Logger.Information("Successfully unmounted image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
        }
        catch (Exception ex)
        {
            mountedImage.Status = MountStatus.Error;
            Logger.Error(ex, "Failed to unmount image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UnmountAllImagesAsync(bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Unmounting all mounted Windows images, SaveChanges: {SaveChanges}", saveChanges);
        progress?.Report("Getting list of mounted images...");

        try
        {
            var mountedImages = await _mountService.GetMountedImagesAsync(cancellationToken);
            
            if (mountedImages.Count == 0)
            {
                progress?.Report("No mounted images found");
                Logger.Information("No mounted images to unmount");
                return;
            }

            progress?.Report($"Found {mountedImages.Count} mounted images to unmount");
            Logger.Information("Found {Count} mounted images to unmount", mountedImages.Count);

            for (int i = 0; i < mountedImages.Count; i++)
            {
                var mountedImage = mountedImages[i];
                progress?.Report($"Unmounting image {i + 1} of {mountedImages.Count}: {mountedImage.ImageName}");
                
                try
                {
                    await UnmountImageAsync(mountedImage, saveChanges, null, cancellationToken);
                    Logger.Debug("Successfully unmounted image {Index}: {ImageName}", i + 1, mountedImage.ImageName);
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to unmount image {Index}: {ImageName}", i + 1, mountedImage.ImageName);
                    // Continue with other images even if one fails
                }
            }

            progress?.Report("All unmount operations completed");
            Logger.Information("Completed unmounting all images");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to unmount all images");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task ForceUnmountImageAsync(MountedImageInfo mountedImage, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        if (mountedImage == null)
            throw new ArgumentNullException(nameof(mountedImage));

        Logger.Warning("Force unmounting Windows image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
        progress?.Report($"Force unmounting index {mountedImage.Index} (discarding all changes)...");

        try
        {
            mountedImage.Status = MountStatus.Unmounting;

            // Force unmount with discard and cleanup
            var command = $"Dismount-WindowsImage -Path '{mountedImage.MountPath}' -Discard";
            await ExecutePowerShellCommandAsync(command, cancellationToken);

            // Force cleanup of mount directory
            progress?.Report("Cleaning up mount directory...");
            if (Directory.Exists(mountedImage.MountPath))
            {
                try
                {
                    Directory.Delete(mountedImage.MountPath, true);
                    Logger.Information("Force cleaned up mount directory: {MountPath}", mountedImage.MountPath);
                }
                catch (Exception cleanupEx)
                {
                    Logger.Warning(cleanupEx, "Failed to clean up mount directory during force unmount: {MountPath}", mountedImage.MountPath);
                }
            }

            progress?.Report("Force unmount completed");
            Logger.Information("Successfully force unmounted image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
        }
        catch (Exception ex)
        {
            mountedImage.Status = MountStatus.Error;
            Logger.Error(ex, "Failed to force unmount image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CleanupOrphanedMountsAsync(IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Starting cleanup of orphaned mount directories");
        progress?.Report("Checking for orphaned mount directories...");

        try
        {
            if (!Directory.Exists(Constants.MountDirectoryPath))
            {
                Logger.Debug("Mount directory does not exist, nothing to clean up");
                return;
            }

            var mountedImages = await _mountService.GetMountedImagesAsync(cancellationToken);
            var activeMountPaths = mountedImages.Select(m => m.MountPath).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allMountDirectories = Directory.GetDirectories(Constants.MountDirectoryPath);
            var orphanedDirectories = allMountDirectories.Where(dir => !activeMountPaths.Contains(dir)).ToList();

            if (orphanedDirectories.Count == 0)
            {
                Logger.Information("No orphaned mount directories found");
                progress?.Report("No orphaned directories found");
                return;
            }

            Logger.Information("Found {Count} orphaned mount directories", orphanedDirectories.Count);

            for (int i = 0; i < orphanedDirectories.Count; i++)
            {
                var orphanedDir = orphanedDirectories[i];
                try
                {
                    progress?.Report($"Cleaning up: {Path.GetFileName(orphanedDir)}");
                    
                    if (Directory.GetFileSystemEntries(orphanedDir).Length == 0)
                    {
                        Directory.Delete(orphanedDir);
                        Logger.Debug("Deleted empty orphaned directory: {Directory}", orphanedDir);
                    }
                    else
                    {
                        Logger.Warning("Orphaned directory is not empty, skipping: {Directory}", orphanedDir);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to clean up orphaned directory: {Directory}", orphanedDir);
                }
            }

            progress?.Report("Cleanup completed");
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
    /// Executes a PowerShell command.
    /// </summary>
    private async Task ExecutePowerShellCommandAsync(string command, CancellationToken cancellationToken)
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
                Logger.Error("PowerShell unmount command failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);

                if (errorMessage.Contains("Access is denied") || errorMessage.Contains("UnauthorizedAccess"))
                {
                    throw new UnauthorizedAccessException($"Access denied when executing unmount operation. Please run as administrator.");
                }
                else if (errorMessage.Contains("Dismount-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The Dismount-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }
                else if (errorMessage.Contains("not mounted") || errorMessage.Contains("No images are mounted"))
                {
                    throw new InvalidOperationException("The image is not currently mounted.");
                }
                else if (errorMessage.Contains("in use") || errorMessage.Contains("being used"))
                {
                    throw new InvalidOperationException("The mounted image is currently in use. Please close all applications accessing the mount point and try again.");
                }

                throw new InvalidOperationException($"Unmount operation failed: {errorMessage}");
            }

            Logger.Debug("PowerShell unmount command executed successfully: {Command}", command);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "PowerShell unmount command failed: {Command}", command);
            throw;
        }
    }

    /// <summary>
    /// Cleans up a mount directory if it's empty.
    /// </summary>
    private async Task CleanupMountDirectoryAsync(string mountPath)
    {
        await Task.Run(() =>
        {
            if (Directory.Exists(mountPath))
            {
                try
                {
                    if (!Directory.GetFileSystemEntries(mountPath).Any())
                    {
                        Directory.Delete(mountPath);
                        Logger.Debug("Cleaned up empty mount directory: {MountPath}", mountPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to clean up mount directory: {MountPath}", mountPath);
                }
            }
        });
    }

    #endregion
} 