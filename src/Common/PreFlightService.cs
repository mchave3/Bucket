using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using Bucket.Models;

namespace Bucket.Common;

/// <summary>
/// Service responsible for performing pre-flight system checks at application startup.
/// Ensures all necessary prerequisites are met for Windows image management operations.
/// </summary>
public class PreFlightService
{
    private readonly List<string> _errorMessages = new();
    private readonly List<string> _warningMessages = new();

    /// <summary>
    /// Runs all pre-flight checks and returns the results.
    /// </summary>
    /// <returns>A PreFlightResult containing the status of all checks.</returns>
    public async Task<PreFlightResult> RunPreFlightChecksAsync()
    {
        Logger.Information("========== BUCKET PRE-FLIGHT CHECKS ==========");
        Logger.Information("Starting pre-flight system verification");

        var result = new PreFlightResult();

        try
        {
            // Critical checks - these must pass for the app to start
            Logger.Information("---------- Critical System Checks ----------");
            result.AdminPrivileges = CheckAdminPrivileges();
            result.DirectoryStructure = await CreateDirectoryStructureAsync();
            result.WritePermissions = await TestWritePermissionsAsync();
            result.DiskSpace = CheckAvailableDiskSpace();
            result.SystemTools = CheckSystemTools();

            // Recommended checks - these generate warnings only
            Logger.Information("---------- Recommended System Checks ----------");
            result.WindowsVersion = CheckWindowsVersion();
            result.WindowsServices = CheckWindowsServices();
            result.NetworkConnectivity = await TestNetworkConnectivityAsync();

            // Configuration checks - these auto-repair
            Logger.Information("---------- Configuration Setup ----------");
            result.ConfigFiles = await CreateConfigurationFilesAsync();

        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Unexpected error during pre-flight checks");
            _errorMessages.Add($"Unexpected error: {ex.Message}");
        }

        // Populate result with messages
        result.ErrorMessages = _errorMessages.ToList();
        result.WarningMessages = _warningMessages.ToList();

        // Log summary
        if (result.AllCriticalChecksPassed)
        {
            Logger.Information("========== PRE-FLIGHT COMPLETE - SUCCESS ==========");
            Logger.Information("All critical checks passed. Bucket is ready for use.");
        }
        else
        {
            Logger.Error("========== PRE-FLIGHT COMPLETE - FAILURES ==========");
            Logger.Error("Critical checks failed: {FailedChecks}", string.Join(", ", result.FailedCriticalChecks));
        }

        // Update configuration
        AppHelper.Settings.PreFlightCompleted = result.AllCriticalChecksPassed;
        AppHelper.Settings.LastPreFlightCheck = DateTime.Now;

        return result;
    }

    #region Critical Checks

    /// <summary>
    /// Verifies that the application is running with administrator privileges.
    /// </summary>
    private bool CheckAdminPrivileges()
    {
        try
        {
            if (AppHelper.Settings.SkipAdminCheck)
            {
                Logger.Warning("Administrator check skipped by configuration");
                _warningMessages.Add("Administrator privilege check was skipped");
                return true;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (isAdmin)
            {
                Logger.Information("Administrator privileges verified");
                return true;
            }
            else
            {
                var errorMsg = "Bucket requires administrator privileges for Windows image management operations.";
                Logger.Error(errorMsg);
                _errorMessages.Add(errorMsg);
                _errorMessages.Add("Please restart the application as Administrator.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error checking administrator privileges");
            _errorMessages.Add($"Failed to verify administrator privileges: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Creates the required directory structure in ProgramData.
    /// </summary>
    private Task<bool> CreateDirectoryStructureAsync()
    {
        try
        {
            var requiredDirectories = new[]
            {
                Constants.RootDirectoryPath,
                Constants.UpdatesDirectoryPath,
                Constants.StagingDirectoryPath,
                Constants.MountDirectoryPath,
                Constants.CompletedWIMsDirectoryPath,
                Constants.ImportedWIMsDirectoryPath,
                Constants.ConfigsDirectoryPath,
                Constants.LogDirectoryPath
            };

            var directoryNames = new[]
            {
                "Bucket Data Directory",
                "Updates",
                "Staging",
                "Mount",
                "Completed WIMs",
                "Imported WIMs",
                "Configs",
                "Logs"
            };

            for (var i = 0; i < requiredDirectories.Length; i++)
            {
                var dirPath = requiredDirectories[i];
                var dirName = directoryNames[i];

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    Logger.Information("Created: {DirectoryName} directory at {Path}", dirName, dirPath);
                }
                else
                {
                    Logger.Debug("Verified: {DirectoryName} directory exists at {Path}", dirName, dirPath);
                }
            }

            Logger.Information("Directory structure verification completed");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to create directory structure");
            _errorMessages.Add($"Failed to create required directories: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Tests write permissions in all required directories.
    /// </summary>
    private async Task<bool> TestWritePermissionsAsync()
    {
        try
        {
            var testDirectories = new[]
            {
                Constants.RootDirectoryPath,
                Constants.UpdatesDirectoryPath,
                Constants.StagingDirectoryPath,
                Constants.MountDirectoryPath,
                Constants.CompletedWIMsDirectoryPath,
                Constants.ImportedWIMsDirectoryPath,
                Constants.ConfigsDirectoryPath,
                Constants.LogDirectoryPath
            };

            var allPermissionsOk = true;

            foreach (var directory in testDirectories)
            {
                try
                {
                    var testFile = Path.Combine(directory, $"test_permission_{Guid.NewGuid()}.tmp");
                    await File.WriteAllTextAsync(testFile, "test");
                    File.Delete(testFile);
                    Logger.Debug("Write permission verified: {Directory}", Path.GetFileName(directory));
                }
                catch (Exception ex)
                {
                    Logger.Warning("Write permission issue in {Directory}: {Error}", directory, ex.Message);
                    _warningMessages.Add($"Write permission warning in {Path.GetFileName(directory)}: {ex.Message}");
                    // Don't fail critical check for permission warnings, but log them
                }
            }

            return allPermissionsOk;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error testing write permissions");
            _errorMessages.Add($"Failed to test write permissions: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks available disk space on the system drive.
    /// </summary>
    private bool CheckAvailableDiskSpace()
    {
        try
        {
            if (AppHelper.Settings.SkipDiskSpaceCheck)
            {
                Logger.Warning("Disk space check skipped by configuration");
                _warningMessages.Add("Disk space check was skipped");
                return true;
            }

            var driveInfo = new DriveInfo("C:");
            var availableGB = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024);
            var totalGB = driveInfo.TotalSize / (1024 * 1024 * 1024);

            Logger.Information("Disk space: {Available} GB available of {Total} GB total", availableGB, totalGB);

            if (availableGB >= Constants.MinimumDiskSpaceGB)
            {
                Logger.Information("Disk space requirement met ({Available} GB >= {Required} GB)", availableGB, Constants.MinimumDiskSpaceGB);
                return true;
            }
            else
            {
                var errorMsg = $"Insufficient disk space: {availableGB} GB available, {Constants.MinimumDiskSpaceGB} GB required.";
                Logger.Error(errorMsg);
                _errorMessages.Add(errorMsg);
                _errorMessages.Add("Windows image files and updates require significant disk space.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error checking disk space");
            _errorMessages.Add($"Failed to check disk space: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifies that required system tools are available.
    /// </summary>
    private bool CheckSystemTools()
    {
        try
        {
            var allToolsFound = true;

            // Check for PowerShell
            if (IsToolAvailable("powershell.exe"))
            {
                Logger.Information("System tool verified: PowerShell.exe");
            }
            else
            {
                Logger.Error("Required system tool not found: PowerShell.exe");
                _errorMessages.Add("PowerShell.exe is required for Windows image management but was not found.");
                allToolsFound = false;
            }

            // Verify Get-WindowsImage cmdlet availability
            if (IsPowerShellCmdletAvailable("Get-WindowsImage"))
            {
                Logger.Information("PowerShell cmdlet verified: Get-WindowsImage");
            }
            else
            {
                Logger.Warning("Get-WindowsImage cmdlet not available");
                _warningMessages.Add("Get-WindowsImage cmdlet not found - Windows imaging features may be limited.");
            }

            return allToolsFound;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error checking system tools");
            _errorMessages.Add($"Failed to check system tools: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a tool is available in the system PATH.
    /// </summary>
    private bool IsToolAvailable(string toolName)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "where",
                Arguments = toolName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a PowerShell cmdlet is available.
    /// </summary>
    private bool IsPowerShellCmdletAvailable(string cmdletName)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"Get-Command {cmdletName} -ErrorAction SilentlyContinue | Select-Object -First 1\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process == null) return false;

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();

            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) && output.Contains(cmdletName);
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Recommended Checks

    /// <summary>
    /// Checks Windows version compatibility.
    /// </summary>
    private bool CheckWindowsVersion()
    {
        try
        {
            var version = Environment.OSVersion.Version;
            var buildNumber = GetWindowsBuildNumber();

            Logger.Information("Windows version: {Version}, Build: {Build}", version, buildNumber);

            if (buildNumber >= Constants.MinimumWindowsBuild)
            {
                Logger.Information("Windows version meets requirements (Build {Build} >= {MinBuild})", buildNumber, Constants.MinimumWindowsBuild);
                return true;
            }
            else
            {
                var warningMsg = $"Windows build {buildNumber} is older than recommended minimum {Constants.MinimumWindowsBuild}. Some features may not work properly.";
                Logger.Warning(warningMsg);
                _warningMessages.Add(warningMsg);
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Could not determine Windows version");
            _warningMessages.Add($"Could not verify Windows version: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the Windows build number.
    /// </summary>
    private int GetWindowsBuildNumber()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (key?.GetValue("CurrentBuild") is string buildString && int.TryParse(buildString, out var build))
            {
                return build;
            }
        }
        catch
        {
            // Fallback to environment version
        }

        return Environment.OSVersion.Version.Build;
    }

    /// <summary>
    /// Checks the status of required Windows services.
    /// </summary>
    private bool CheckWindowsServices()
    {
        try
        {
            var requiredServices = new[]
            {
                ("wuauserv", "Windows Update"),
                ("BITS", "Background Intelligent Transfer Service")
            };

            var allServicesOk = true;

            foreach (var (serviceName, displayName) in requiredServices)
            {
                try
                {
                    // Use WMI query instead of ServiceController to avoid package dependency
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"query {serviceName}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });

                    process?.WaitForExit();
                    var output = process?.StandardOutput.ReadToEnd() ?? "";

                    if (output.Contains("RUNNING"))
                    {
                        Logger.Information("Service verified: {DisplayName} is running", displayName);
                    }
                    else if (output.Contains("STOPPED"))
                    {
                        Logger.Warning("Service warning: {DisplayName} is stopped", displayName);
                        _warningMessages.Add($"{displayName} service is stopped. This may affect update download functionality.");
                        allServicesOk = false;
                    }
                    else
                    {
                        Logger.Warning("Service status unknown: {DisplayName}", displayName);
                        _warningMessages.Add($"Could not determine {displayName} service status.");
                        allServicesOk = false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Could not check service {ServiceName}: {Error}", serviceName, ex.Message);
                    _warningMessages.Add($"Could not verify {displayName} service status.");
                    allServicesOk = false;
                }
            }

            return allServicesOk;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error checking Windows services");
            _warningMessages.Add($"Could not verify Windows services: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tests network connectivity to Microsoft servers.
    /// </summary>
    private async Task<bool> TestNetworkConnectivityAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            try
            {
                var response = await client.GetAsync("https://catalog.update.microsoft.com");
                if (response.IsSuccessStatusCode)
                {
                    Logger.Information("Network connectivity verified: Microsoft Update servers reachable");
                    return true;
                }
                else
                {
                    Logger.Warning("Network connectivity issue: Microsoft Update servers returned {StatusCode}", response.StatusCode);
                    _warningMessages.Add($"Microsoft Update servers returned status {response.StatusCode}. Update downloads may be affected.");
                    return false;
                }
            }
            catch (TaskCanceledException)
            {
                Logger.Warning("Network connectivity timeout: Microsoft Update servers not reachable within 10 seconds");
                _warningMessages.Add("Network connectivity timeout. Update downloads may be affected.");
                return false;
            }
            catch (HttpRequestException ex)
            {
                Logger.Warning("Network connectivity error: {Error}", ex.Message);
                _warningMessages.Add($"Network connectivity issue: {ex.Message}. Update downloads may be affected.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error testing network connectivity");
            _warningMessages.Add($"Could not test network connectivity: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Configuration Setup

    /// <summary>
    /// Creates or verifies configuration files.
    /// </summary>
    private async Task<bool> CreateConfigurationFilesAsync()
    {
        try
        {
            // Create WIMs.xml if it doesn't exist
            if (!File.Exists(Constants.WIMsConfigPath))
            {
                var defaultWimsXml = """
                    <?xml version="1.0" encoding="utf-8"?>
                    <WIMs>
                    </WIMs>
                    """;

                await File.WriteAllTextAsync(Constants.WIMsConfigPath, defaultWimsXml, Encoding.UTF8);
                Logger.Information("Created: WIMs.xml configuration file");
            }
            else
            {
                // Verify existing file is valid XML
                try
                {
                    var xmlContent = await File.ReadAllTextAsync(Constants.WIMsConfigPath);
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(xmlContent);

                    if (xmlDoc.DocumentElement?.Name != "WIMs")
                    {
                        throw new Exception("Invalid root element");
                    }

                    Logger.Debug("Verified: WIMs.xml configuration file is valid");
                }
                catch (Exception ex)
                {
                    Logger.Warning("WIMs.xml file is invalid: {Error}. Recreating with default content.", ex.Message);

                    // Backup corrupted file
                    var backupPath = Constants.WIMsConfigPath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Move(Constants.WIMsConfigPath, backupPath);

                    // Create new default file
                    var defaultWimsXml = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <WIMs>
                        </WIMs>
                        """;
                    await File.WriteAllTextAsync(Constants.WIMsConfigPath, defaultWimsXml, Encoding.UTF8);
                    Logger.Information("Recreated: WIMs.xml configuration file (backup saved)");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to create configuration files");
            _errorMessages.Add($"Failed to create configuration files: {ex.Message}");
            return false;
        }
    }

    #endregion
}
