# WindowsVersionsConfig Class Documentation

## Overview
The `WindowsVersionsConfig` class represents the configuration structure for Windows versions and their valid combinations. It provides a data model for loading and managing Windows operating system versions, supported architectures, update types, and their relationships from a JSON configuration file.

## Location
- **File**: `src/Models/WindowsVersionsConfig.cs`
- **Namespace**: `Bucket.Models`

## Class Definition
```csharp
public class WindowsVersionsConfig
```

## Properties

### OperatingSystems
```csharp
[JsonPropertyName("operatingSystems")]
public Dictionary<string, OperatingSystemConfig> OperatingSystems { get; set; } = new();
```
Dictionary containing all supported operating systems and their configurations.

### Architectures
```csharp
[JsonPropertyName("architectures")]
public Dictionary<string, ArchitectureConfig> Architectures { get; set; } = new();
```
Dictionary containing all supported architectures (x64, x86, ARM64) and their descriptions.

### UpdateTypes
```csharp
[JsonPropertyName("updateTypes")]
public Dictionary<string, UpdateTypeConfig> UpdateTypes { get; set; } = new();
```
Dictionary containing all supported update types and their descriptions.

### Metadata
```csharp
[JsonPropertyName("metadata")]
public ConfigMetadata Metadata { get; set; } = new();
```
Configuration metadata including version and last updated information.

## Related Classes

### OperatingSystemConfig
Represents configuration for a specific operating system:
```csharp
public class OperatingSystemConfig
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("versions")]
    public List<VersionConfig> Versions { get; set; } = new();
}
```

### VersionConfig
Represents configuration for a specific version of an operating system:
```csharp
public class VersionConfig
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("buildNumber")]
    public string BuildNumber { get; set; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("supportedArchitectures")]
    public List<string> SupportedArchitectures { get; set; } = new();

    [JsonPropertyName("supportedUpdateTypes")]
    public List<string> SupportedUpdateTypes { get; set; } = new();
}
```

### ArchitectureConfig
Represents configuration for a specific architecture:
```csharp
public class ArchitectureConfig
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
```

### UpdateTypeConfig
Represents configuration for a specific update type:
```csharp
public class UpdateTypeConfig
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
```

### ConfigMetadata
Represents metadata about the configuration:
```csharp
public class ConfigMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
```

## Usage Examples

### Loading Configuration
```csharp
var jsonContent = File.ReadAllText("WindowsVersionsConfig.json");
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
};

var config = JsonSerializer.Deserialize<WindowsVersionsConfig>(jsonContent, options);
```

### Accessing Operating Systems
```csharp
foreach (var os in config.OperatingSystems)
{
    Console.WriteLine($"OS: {os.Key} - {os.Value.DisplayName}");
    foreach (var version in os.Value.Versions)
    {
        Console.WriteLine($"  Version: {version.Version} - {version.DisplayName}");
    }
}
```

### Checking Supported Architectures
```csharp
var windows11 = config.OperatingSystems["Windows 11"];
var version24H2 = windows11.Versions.FirstOrDefault(v => v.Version == "24H2");
if (version24H2 != null)
{
    Console.WriteLine($"Supported architectures: {string.Join(", ", version24H2.SupportedArchitectures)}");
}
```

## Features

- **JSON Serialization**: Fully compatible with System.Text.Json
- **Hierarchical Structure**: Organized by OS → Version → Architectures/UpdateTypes
- **Validation Support**: Enables validation of valid OS/Version/Architecture combinations
- **Metadata Tracking**: Includes version and update tracking information
- **Display Names**: Provides user-friendly display names for all components

## Dependencies

- **System.Text.Json**: For JSON serialization attributes
- **System.Collections.Generic**: For collection types

## Related Files

- [`WindowsVersionsConfigService.md`](../Services/WindowsVersionsConfigService.md) - Service that uses this model
- [`MicrosoftUpdateCatalogViewModel.md`](../ViewModels/MicrosoftUpdateCatalogViewModel.md) - ViewModel that consumes this configuration

## Best Practices

1. **Immutable After Load**: Treat the configuration as read-only after loading
2. **Validation**: Always validate combinations using the service layer
3. **Error Handling**: Handle missing or invalid configuration gracefully
4. **Caching**: Cache the loaded configuration to avoid repeated file reads

## Configuration File Structure

The JSON configuration file should follow this structure:
```json
{
  "operatingSystems": {
    "Windows 11": {
      "displayName": "Windows 11",
      "versions": [
        {
          "version": "24H2",
          "displayName": "24H2 (2024 Update)",
          "buildNumber": "26100",
          "releaseDate": "2024-10-01",
          "supportedArchitectures": ["x64", "ARM64"],
          "supportedUpdateTypes": ["Cumulative Update", "Dynamic Update", ...]
        }
      ]
    }
  },
  "architectures": { ... },
  "updateTypes": { ... },
  "metadata": { ... }
}
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 