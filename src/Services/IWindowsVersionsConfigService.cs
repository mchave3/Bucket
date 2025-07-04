using System.Collections.Generic;
using System.Threading.Tasks;
using Bucket.Models;

namespace Bucket.Services;

public interface IWindowsVersionsConfigService
{
    /// <summary>
    /// Loads the Windows versions configuration from the JSON file
    /// </summary>
    Task<WindowsVersionsConfig> LoadConfigAsync();

    /// <summary>
    /// Gets all available operating systems
    /// </summary>
    Task<List<string>> GetOperatingSystemsAsync();

    /// <summary>
    /// Gets available versions for a specific operating system
    /// </summary>
    Task<List<string>> GetVersionsForOperatingSystemAsync(string operatingSystem);

    /// <summary>
    /// Gets available architectures for a specific operating system and version
    /// </summary>
    Task<List<string>> GetArchitecturesForVersionAsync(string operatingSystem, string version);

    /// <summary>
    /// Gets available update types for a specific operating system and version
    /// </summary>
    Task<List<string>> GetUpdateTypesForVersionAsync(string operatingSystem, string version);

    /// <summary>
    /// Checks if a combination of OS, version, architecture, and update type is valid
    /// </summary>
    Task<bool> IsValidCombinationAsync(string operatingSystem, string version, string architecture, string updateType);

    /// <summary>
    /// Gets the display name for a version
    /// </summary>
    Task<string> GetVersionDisplayNameAsync(string operatingSystem, string version);

    /// <summary>
    /// Gets the build number for a version
    /// </summary>
    Task<string> GetVersionBuildNumberAsync(string operatingSystem, string version);
} 