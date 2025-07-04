using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bucket.Models;
using Windows.Storage;

namespace Bucket.Services;

public class WindowsVersionsConfigService : IWindowsVersionsConfigService
{
    private WindowsVersionsConfig? _config;
    private readonly object _configLock = new();

    public async Task<WindowsVersionsConfig> LoadConfigAsync()
    {
        if (_config != null)
            return _config;

        lock (_configLock)
        {
            if (_config != null)
                return _config;

            try
            {
                var configPath = Path.Combine(AppContext.BaseDirectory, "Assets", "WindowsVersionsConfig.json");
                
                if (!File.Exists(configPath))
                {
                    Logger.Error("Windows versions configuration file not found at: {Path}", configPath);
                    throw new FileNotFoundException($"Configuration file not found: {configPath}");
                }

                var jsonContent = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                _config = JsonSerializer.Deserialize<WindowsVersionsConfig>(jsonContent, options);
                
                if (_config == null)
                {
                    Logger.Error("Failed to deserialize Windows versions configuration");
                    throw new InvalidOperationException("Failed to load Windows versions configuration");
                }

                Logger.Information("Windows versions configuration loaded successfully. Version: {Version}", _config.Metadata.Version);
                return _config;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading Windows versions configuration");
                throw;
            }
        }
    }

    public async Task<List<string>> GetOperatingSystemsAsync()
    {
        var config = await LoadConfigAsync();
        return config.OperatingSystems.Keys.ToList();
    }

    public async Task<List<string>> GetVersionsForOperatingSystemAsync(string operatingSystem)
    {
        if (string.IsNullOrEmpty(operatingSystem))
            return new List<string>();

        var config = await LoadConfigAsync();
        
        if (!config.OperatingSystems.TryGetValue(operatingSystem, out var osConfig))
        {
            Logger.Warning("Operating system not found in configuration: {OS}", operatingSystem);
            return new List<string>();
        }

        return osConfig.Versions.Select(v => v.Version).ToList();
    }

    public async Task<List<string>> GetArchitecturesForVersionAsync(string operatingSystem, string version)
    {
        if (string.IsNullOrEmpty(operatingSystem) || string.IsNullOrEmpty(version))
            return new List<string>();

        var config = await LoadConfigAsync();
        
        if (!config.OperatingSystems.TryGetValue(operatingSystem, out var osConfig))
        {
            Logger.Warning("Operating system not found in configuration: {OS}", operatingSystem);
            return new List<string>();
        }

        var versionConfig = osConfig.Versions.FirstOrDefault(v => v.Version == version);
        if (versionConfig == null)
        {
            Logger.Warning("Version not found in configuration: {OS} {Version}", operatingSystem, version);
            return new List<string>();
        }

        // Add "All" option if there are multiple architectures
        var architectures = versionConfig.SupportedArchitectures.ToList();
        if (architectures.Count > 1)
        {
            architectures.Insert(0, "All");
        }

        return architectures;
    }

    public async Task<List<string>> GetUpdateTypesForVersionAsync(string operatingSystem, string version)
    {
        if (string.IsNullOrEmpty(operatingSystem) || string.IsNullOrEmpty(version))
            return new List<string>();

        var config = await LoadConfigAsync();
        
        if (!config.OperatingSystems.TryGetValue(operatingSystem, out var osConfig))
        {
            Logger.Warning("Operating system not found in configuration: {OS}", operatingSystem);
            return new List<string>();
        }

        var versionConfig = osConfig.Versions.FirstOrDefault(v => v.Version == version);
        if (versionConfig == null)
        {
            Logger.Warning("Version not found in configuration: {OS} {Version}", operatingSystem, version);
            return new List<string>();
        }

        // Add "All" option
        var updateTypes = versionConfig.SupportedUpdateTypes.ToList();
        updateTypes.Insert(0, "All");

        return updateTypes;
    }

    public async Task<bool> IsValidCombinationAsync(string operatingSystem, string version, string architecture, string updateType)
    {
        if (string.IsNullOrEmpty(operatingSystem) || string.IsNullOrEmpty(version))
            return false;

        var config = await LoadConfigAsync();
        
        if (!config.OperatingSystems.TryGetValue(operatingSystem, out var osConfig))
            return false;

        var versionConfig = osConfig.Versions.FirstOrDefault(v => v.Version == version);
        if (versionConfig == null)
            return false;

        // Check architecture
        if (!string.IsNullOrEmpty(architecture) && architecture != "All")
        {
            if (!versionConfig.SupportedArchitectures.Contains(architecture))
                return false;
        }

        // Check update type
        if (!string.IsNullOrEmpty(updateType) && updateType != "All")
        {
            if (!versionConfig.SupportedUpdateTypes.Contains(updateType))
                return false;
        }

        return true;
    }

    public async Task<string> GetVersionDisplayNameAsync(string operatingSystem, string version)
    {
        if (string.IsNullOrEmpty(operatingSystem) || string.IsNullOrEmpty(version))
            return version;

        var config = await LoadConfigAsync();
        
        if (!config.OperatingSystems.TryGetValue(operatingSystem, out var osConfig))
            return version;

        var versionConfig = osConfig.Versions.FirstOrDefault(v => v.Version == version);
        return versionConfig?.DisplayName ?? version;
    }

    public async Task<string> GetVersionBuildNumberAsync(string operatingSystem, string version)
    {
        if (string.IsNullOrEmpty(operatingSystem) || string.IsNullOrEmpty(version))
            return string.Empty;

        var config = await LoadConfigAsync();
        
        if (!config.OperatingSystems.TryGetValue(operatingSystem, out var osConfig))
            return string.Empty;

        var versionConfig = osConfig.Versions.FirstOrDefault(v => v.Version == version);
        return versionConfig?.BuildNumber ?? string.Empty;
    }
} 