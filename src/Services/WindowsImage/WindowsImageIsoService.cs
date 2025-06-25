using System.Diagnostics;
using Bucket.Models;
using Windows.Storage;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for Windows image ISO operations including mounting, dismounting, and import operations.
/// Handles all ISO-related functionality for Windows image management including PowerShell-based
/// mounting/dismounting operations and finding Windows images within mounted ISOs.
/// </summary>
public class WindowsImageIsoService : IWindowsImageIsoService
{
    #region Private Fields

    private readonly IWindowsImageFileService _fileService;
    private readonly IWindowsImagePowerShellService _powerShellService;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowsImageIsoService class.
    /// </summary>
    /// <param name="fileService">The file service for handling file operations.</param>
    /// <param name="powerShellService">The PowerShell service for image analysis.</param>
    public WindowsImageIsoService(IWindowsImageFileService fileService, IWindowsImagePowerShellService powerShellService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _powerShellService = powerShellService ?? throw new ArgumentNullException(nameof(powerShellService));

        Logger.Debug("WindowsImageIsoService initialized");
    }

    #endregion

    #region Public Methods - ISO Import Operations

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
                // Step 2: Find Windows images in the mounted ISO
                progress?.Report("Searching for Windows images...");
                var wimFiles = await FindWindowsImagesAsync(mountPath, cancellationToken);

                if (!wimFiles.Any())
                {
                    throw new InvalidOperationException("No Windows image files found in the ISO.");
                }

                // Step 3: Use the first WIM file found (typically install.wim)
                var wimPath = wimFiles.First();
                Logger.Information("Found Windows image file: {WimPath}", wimPath);

                // Step 4: Get a friendly name for the image
                var imageName = string.IsNullOrWhiteSpace(customName)
                    ? Path.GetFileNameWithoutExtension(isoFile.Name)
                    : customName;

                // Step 5: Copy the WIM to the managed directory
                progress?.Report("Copying Windows image file...");
                var copiedWimPath = await _fileService.CopyImageToManagedDirectoryAsync(
                    wimPath,
                    imageName,
                    progress,
                    cancellationToken);

                // Step 6: Analyze the copied image to get indices
                progress?.Report("Analyzing Windows image...");
                var indices = await _powerShellService.AnalyzeImageAsync(copiedWimPath, progress, cancellationToken);

                // Step 7: Create the WindowsImageInfo object
                var imageInfo = new WindowsImageInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = imageName,
                    FilePath = copiedWimPath,
                    SourceIsoPath = isoFile.Path,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Indices = new System.Collections.ObjectModel.ObservableCollection<WindowsImageIndex>(indices)
                };

                progress?.Report("Import completed successfully");
                Logger.Information("Successfully imported Windows image from ISO: {Name}", imageName);

                return imageInfo;
            }
            finally
            {
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

            // Copy the WIM to the managed directory
            progress?.Report("Copying Windows image file...");
            var copiedWimPath = await _fileService.CopyImageToManagedDirectoryAsync(
                wimFile.Path,
                imageName,
                progress,
                cancellationToken);

            // Analyze the copied image to get indices
            progress?.Report("Analyzing Windows image...");
            var indices = await _powerShellService.AnalyzeImageAsync(copiedWimPath, progress, cancellationToken);

            // Create the WindowsImageInfo object
            var imageInfo = new WindowsImageInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = imageName,
                FilePath = copiedWimPath,
                SourceIsoPath = "",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Indices = new System.Collections.ObjectModel.ObservableCollection<WindowsImageIndex>(indices)
            };

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

    #region Public Methods - ISO Mount/Dismount Operations

    /// <summary>
    /// Mounts an ISO file and returns the mount path.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The mount path of the ISO.</returns>
    public async Task<string> MountIsoAsync(string isoPath, CancellationToken cancellationToken)
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
    public async Task DismountIsoAsync(string isoPath, CancellationToken cancellationToken)
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
    /// Checks if an ISO file is currently mounted.
    /// </summary>
    /// <param name="isoPath">The path to the ISO file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the ISO is mounted, false otherwise.</returns>
    public async Task<bool> IsIsoMountedAsync(string isoPath, CancellationToken cancellationToken)
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

    #endregion

    #region Private Methods

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
}
