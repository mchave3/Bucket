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

        try
        {
            // Check if already mounted
            if (await IsImageMountedAsync(imagePath, index, cancellationToken))
            {
                throw new InvalidOperationException($"Image index {index} is already mounted.");
            }

            // Create unique mount directory
            var mountPath = GetMountDirectoryPath(imagePath, index);
            Directory.CreateDirectory(mountPath);

            progress?.Report($"Mounting to: {mountPath}");

            // Execute mount command using PowerShell
            var command = $"Mount-WindowsImage -ImagePath '{imagePath}' -Index {index} -Path '{mountPath}'";
            await ExecutePowerShellCommandAsync(command, progress, cancellationToken);

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
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to mount image: {ImagePath}, Index: {Index}", imagePath, index);
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



    #region Private Methods



    /// <summary>
    /// Executes a PowerShell command and reports progress.
    /// </summary>
    private async Task ExecutePowerShellCommandAsync(string command, IProgress<string> progress, CancellationToken cancellationToken)
    {
        try
        {
            progress?.Report($"Executing PowerShell: {command}");

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
        catch (Exception ex)
        {
            Logger.Error(ex, "PowerShell command failed: {Command}", command);
            throw;
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

    #endregion
} 