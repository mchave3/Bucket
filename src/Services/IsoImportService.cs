using System.Diagnostics;
using System.IO.Compression;
using Bucket.Models;
using Windows.Storage;

namespace Bucket.Services;

/// <summary>
/// Service for handling ISO file operations and extracting Windows images.
/// </summary>
public class IsoImportService
{
    private readonly WindowsImageService _imageService;

    /// <summary>
    /// Initializes a new instance of the IsoImportService class.
    /// </summary>
    public IsoImportService()
    {
        _imageService = new WindowsImageService();
        Logger.Debug("IsoImportService initialized");
    }

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

                // Step 4: Generate destination path
                var imageName = string.IsNullOrWhiteSpace(customName)
                    ? Path.GetFileNameWithoutExtension(isoFile.Name)
                    : customName;

                var destinationPath = Path.Combine(Constants.ImportedWIMsDirectoryPath,
                    $"{imageName}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(mainImageFile)}");

                // Step 5: Copy the image file
                progress?.Report("Copying Windows image file...");
                await CopyFileWithProgressAsync(mainImageFile, destinationPath, progress, cancellationToken);

                // Step 6: Import the copied image
                progress?.Report("Analyzing Windows image...");
                var imageInfo = await _imageService.ImportImageAsync(
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
                await DismountIsoAsync(mountPath, cancellationToken);
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
            // Generate destination path
            var imageName = string.IsNullOrWhiteSpace(customName)
                ? Path.GetFileNameWithoutExtension(wimFile.Name)
                : customName;

            var destinationPath = Path.Combine(Constants.ImportedWIMsDirectoryPath,
                $"{imageName}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(wimFile.Path)}");

            // Copy the image file
            progress?.Report("Copying Windows image file...");
            await CopyFileWithProgressAsync(wimFile.Path, destinationPath, progress, cancellationToken);

            // Import the copied image
            progress?.Report("Analyzing Windows image...");
            var imageInfo = await _imageService.ImportImageAsync(
                destinationPath,
                imageName,
                "", // No source ISO for direct WIM import
                false, // copyToManagedDirectory = false (already copied)
                progress,
                cancellationToken);

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
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"Mount-DiskImage -ImagePath '{isoPath}' -PassThru | Get-Volume | Select-Object -ExpandProperty DriveLetter\"",
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
    /// <param name="mountPath">The mount path to dismount.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task DismountIsoAsync(string mountPath, CancellationToken cancellationToken)
    {
        Logger.Information("Dismounting ISO from: {MountPath}", mountPath);

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"Get-DiskImage | Where-Object {{$_.DevicePath -like '*{mountPath.Substring(0, 1)}*'}} | Dismount-DiskImage\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            await process.WaitForExitAsync(cancellationToken);

            Logger.Information("ISO dismounted successfully");
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to dismount ISO cleanly, but continuing");
        }
    }    /// <summary>
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
    }    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A formatted string representation.</returns>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
