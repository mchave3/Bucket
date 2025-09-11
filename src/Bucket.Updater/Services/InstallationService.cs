using System.Diagnostics;

namespace Bucket.Updater.Services
{
    public interface IInstallationService
    {
        Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
        Task<bool> ValidateMsiFileAsync(string msiFilePath);
        Task<bool> EnsureBucketProcessStoppedAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
        void CleanupDownloadedFiles(string downloadPath);
        void CleanupAllTemporaryFiles();
    }

    public class InstallationService : IInstallationService
    {
        public async Task<bool> InstallUpdateAsync(string msiFilePath, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            Logger?.Information("Starting MSI installation: {FilePath}", msiFilePath);

            try
            {
                if (!File.Exists(msiFilePath))
                {
                    Logger?.Error("MSI file not found: {FilePath}", msiFilePath);
                    progress?.Report("MSI file not found");
                    return false;
                }

                progress?.Report("Validating MSI file...");
                if (!await ValidateMsiFileAsync(msiFilePath))
                {
                    Logger?.Error("MSI file validation failed: {FilePath}", msiFilePath);
                    progress?.Report("MSI file validation failed");
                    return false;
                }

                progress?.Report("Stopping Bucket application...");
                if (!await EnsureBucketProcessStoppedAsync(progress, cancellationToken))
                {
                    Logger?.Error("Failed to stop Bucket process before installation");
                    progress?.Report("Failed to stop Bucket application");
                    return false;
                }

                progress?.Report("Starting installation...");
                Logger?.Information("Executing MSI installation with msiexec");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{msiFilePath}\" /passive /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Logger?.Error("Failed to start msiexec process");
                    progress?.Report("Failed to start installation process");
                    return false;
                }

                progress?.Report("Installation in progress...");
                Logger?.Information("Waiting for msiexec process to complete");

                await process.WaitForExitAsync(cancellationToken);

                switch (process.ExitCode)
                {
                    case 0:
                        Logger?.Information("MSI installation completed successfully");
                        progress?.Report("Installation completed successfully");
                        return true;
                    case 1602:
                        Logger?.Warning("MSI installation was cancelled by user");
                        progress?.Report("Installation was cancelled by user");
                        return false;
                    case 1603:
                        Logger?.Error("MSI installation failed with error 1603 (fatal error during installation)");
                        progress?.Report("Installation failed with error 1603");
                        return false;
                    case 3010:
                        Logger?.Information("MSI installation completed successfully, restart required");
                        progress?.Report("Installation completed successfully (restart required)");
                        return true;
                    default:
                        Logger?.Error("MSI installation failed with exit code: {ExitCode}", process.ExitCode);
                        progress?.Report($"Installation failed with exit code: {process.ExitCode}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "MSI installation threw exception");
                progress?.Report($"Installation error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateMsiFileAsync(string msiFilePath)
        {
            try
            {
                if (!File.Exists(msiFilePath))
                    return false;

                var fileInfo = new FileInfo(msiFilePath);
                if (fileInfo.Length < 1024)
                    return false;

                var buffer = new byte[8];
                using var fileStream = File.OpenRead(msiFilePath);
                await fileStream.ReadAsync(buffer, 0, 8);

                // MSI files use OLE/Structured Storage format with signature: D0 CF 11 E0 A1 B1 1A E1
                var expectedSignature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

                for (int i = 0; i < expectedSignature.Length; i++)
                {
                    if (buffer[i] != expectedSignature[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnsureBucketProcessStoppedAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                Logger?.Information("Checking for running Bucket processes");
                var bucketProcesses = Process.GetProcessesByName("Bucket").ToList();

                if (!bucketProcesses.Any())
                {
                    Logger?.Information("No Bucket processes found running");
                    return true;
                }

                Logger?.Information("Found {Count} Bucket process(es) running", bucketProcesses.Count);
                progress?.Report($"Found {bucketProcesses.Count} Bucket process(es) running...");

                foreach (var process in bucketProcesses)
                {
                    try
                    {
                        if (process.HasExited)
                            continue;

                        Logger?.Information("Attempting to gracefully close Bucket process (PID: {ProcessId})", process.Id);
                        progress?.Report("Attempting graceful shutdown...");

                        // Try graceful shutdown first
                        if (process.CloseMainWindow())
                        {
                            // Wait up to 10 seconds for graceful shutdown
                            bool exited = await Task.Run(() => process.WaitForExit(10000), cancellationToken);

                            if (exited)
                            {
                                Logger?.Information("Bucket process (PID: {ProcessId}) closed gracefully", process.Id);
                                continue;
                            }
                        }

                        // If graceful shutdown failed, force kill
                        if (!process.HasExited)
                        {
                            Logger?.Warning("Graceful shutdown failed, force killing Bucket process (PID: {ProcessId})", process.Id);
                            progress?.Report("Force closing Bucket application...");

                            process.Kill();
                            await Task.Run(() => process.WaitForExit(5000), cancellationToken);

                            Logger?.Information("Bucket process (PID: {ProcessId}) force killed", process.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.Error(ex, "Error stopping Bucket process (PID: {ProcessId})", process.Id);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                // Final check to ensure all processes are stopped
                await Task.Delay(1000, cancellationToken); // Small delay to ensure processes are fully terminated

                var remainingProcesses = Process.GetProcessesByName("Bucket");
                if (remainingProcesses.Any())
                {
                    Logger?.Error("Failed to stop all Bucket processes. {Count} processes still running", remainingProcesses.Length);
                    progress?.Report($"Failed to stop all Bucket processes. {remainingProcesses.Length} still running");

                    foreach (var process in remainingProcesses)
                        process.Dispose();

                    return false;
                }

                Logger?.Information("All Bucket processes successfully stopped");
                progress?.Report("Bucket application stopped successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Exception occurred while stopping Bucket processes");
                progress?.Report($"Error stopping Bucket application: {ex.Message}");
                return false;
            }
        }

        public void CleanupDownloadedFiles(string downloadPath)
        {
            try
            {
                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                }

                var directory = Path.GetDirectoryName(downloadPath);
                if (directory != null && Directory.Exists(directory))
                {
                    var tempFiles = Directory.GetFiles(directory, "*.tmp")
                                            .Concat(Directory.GetFiles(directory, "*.partial"))
                                            .ToArray();

                    foreach (var tempFile in tempFiles)
                    {
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch
                        {
                            // Ignore individual file deletion errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        public void CleanupAllTemporaryFiles()
        {
            try
            {
                var tempPath = Path.GetTempPath();
                Logger?.Information("Cleaning up all temporary update files in {TempPath}", tempPath);

                // Look for MSI files that might be from Bucket updates
                var patterns = new[] { "Bucket*.msi", "bucket*.msi", "*.tmp", "*.partial" };

                foreach (var pattern in patterns)
                {
                    try
                    {
                        var files = Directory.GetFiles(tempPath, pattern, SearchOption.TopDirectoryOnly);
                        foreach (var file in files)
                        {
                            try
                            {
                                // Check if file is likely from our update process
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.CreationTime > DateTime.Now.AddDays(-1)) // Only files from last 24 hours
                                {
                                    File.Delete(file);
                                    Logger?.Debug("Deleted temporary file: {File}", file);
                                }
                            }
                            catch
                            {
                                // Ignore individual file deletion errors
                            }
                        }
                    }
                    catch
                    {
                        // Ignore pattern search errors
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}