# WindowsVersionsConfigService Class Documentation

## Overview
The `WindowsVersionsConfigService` class provides functionality to load and manage Windows versions configuration data from a JSON file. It handles filtering and validation of valid combinations of operating systems, versions, architectures, and update types for the Microsoft Update Catalog functionality.

## Location
- **File**: `src/Services/WindowsVersionsConfigService.cs`
- **Namespace**: `Bucket.Services`

## Class Definition
```csharp
public class WindowsVersionsConfigService : IWindowsVersionsConfigService
```

## Interface
```csharp
public interface IWindowsVersionsConfigService
```

## Methods

### LoadConfigAsync
```csharp
public async Task<WindowsVersionsConfig> LoadConfigAsync()
```
Loads the Windows versions configuration from the JSON file. Uses singleton pattern with thread-safe lazy loading.

**Returns**: `Task<WindowsVersionsConfig>` - The loaded configuration object

### GetOperatingSystemsAsync
```csharp
public async Task<List<string>> GetOperatingSystemsAsync()
```
Gets all available operating systems from the configuration.

**Returns**: `Task<List<string>>` - List of operating system names

### GetVersionsForOperatingSystemAsync
```csharp
public async Task<List<string>> GetVersionsForOperatingSystemAsync(string operatingSystem)
```
Gets available versions for a specific operating system.

**Parameters**:
- `operatingSystem` - The operating system name

**Returns**: `Task<List<string>>` - List of version identifiers

### GetArchitecturesForVersionAsync
```csharp
public async Task<List<string>> GetArchitecturesForVersionAsync(string operatingSystem, string version)
```
Gets available architectures for a specific operating system and version combination.

**Parameters**:
- `operatingSystem` - The operating system name
- `version` - The version identifier

**Returns**: `Task<List<string>>` - List of supported architectures (includes "All" option if multiple architectures available)

### GetUpdateTypesForVersionAsync
```csharp
public async Task<List<string>> GetUpdateTypesForVersionAsync(string operatingSystem, string version)
```
Gets available update types for a specific operating system and version combination.

**Parameters**:
- `operatingSystem` - The operating system name
- `version` - The version identifier

**Returns**: `Task<List<string>>` - List of supported update types (includes "All" option)

### IsValidCombinationAsync
```csharp
public async Task<bool> IsValidCombinationAsync(string operatingSystem, string version, string architecture, string updateType)
```
Validates if a combination of OS, version, architecture, and update type is valid.

**Parameters**:
- `operatingSystem` - The operating system name
- `version` - The version identifier
- `architecture` - The architecture ("All" is always valid)
- `updateType` - The update type ("All" is always valid)

**Returns**: `Task<bool>` - True if the combination is valid

### GetVersionDisplayNameAsync
```csharp
public async Task<string> GetVersionDisplayNameAsync(string operatingSystem, string version)
```
Gets the user-friendly display name for a version.

**Parameters**:
- `operatingSystem` - The operating system name
- `version` - The version identifier

**Returns**: `Task<string>` - The display name or the version identifier if not found

### GetVersionBuildNumberAsync
```csharp
public async Task<string> GetVersionBuildNumberAsync(string operatingSystem, string version)
```
Gets the build number for a specific version.

**Parameters**:
- `operatingSystem` - The operating system name
- `version` - The version identifier

**Returns**: `Task<string>` - The build number or empty string if not found

## Usage Examples

### Basic Service Usage
```csharp
// Inject the service
private readonly IWindowsVersionsConfigService _configService;

public MyClass(IWindowsVersionsConfigService configService)
{
    _configService = configService;
}

// Get operating systems
var operatingSystems = await _configService.GetOperatingSystemsAsync();

// Get versions for Windows 11
var versions = await _configService.GetVersionsForOperatingSystemAsync("Windows 11");

// Get architectures for Windows 11 24H2
var architectures = await _configService.GetArchitecturesForVersionAsync("Windows 11", "24H2");

// Validate a combination
var isValid = await _configService.IsValidCombinationAsync("Windows 11", "24H2", "x64", "Cumulative Update");
```

### Cascading Dropdown Implementation
```csharp
// When OS changes, update versions
private async Task OnOperatingSystemChanged(string selectedOS)
{
    var versions = await _configService.GetVersionsForOperatingSystemAsync(selectedOS);
    VersionsCollection.Clear();
    foreach (var version in versions)
    {
        VersionsCollection.Add(version);
    }
}

// When version changes, update architectures and update types
private async Task OnVersionChanged(string selectedOS, string selectedVersion)
{
    var architectures = await _configService.GetArchitecturesForVersionAsync(selectedOS, selectedVersion);
    var updateTypes = await _configService.GetUpdateTypesForVersionAsync(selectedOS, selectedVersion);
    
    // Update UI collections
    ArchitecturesCollection.Clear();
    UpdateTypesCollection.Clear();
    
    foreach (var arch in architectures)
        ArchitecturesCollection.Add(arch);
    
    foreach (var updateType in updateTypes)
        UpdateTypesCollection.Add(updateType);
}
```

## Features

- **Thread-Safe Loading**: Uses singleton pattern with thread-safe lazy loading
- **Caching**: Configuration is loaded once and cached for subsequent calls
- **Validation**: Provides validation of valid OS/Version/Architecture combinations
- **Error Handling**: Comprehensive error handling with logging
- **Filtering**: Automatically filters options based on parent selections
- **"All" Options**: Automatically adds "All" options where appropriate

## Configuration File Location

The service loads configuration from:
```
{AppContext.BaseDirectory}/Assets/WindowsVersionsConfig.json
```

## Dependencies

- **System.Text.Json**: For JSON deserialization
- **Bucket.Models**: For configuration model classes
- **Logger**: For logging operations and errors

## Related Files

- [`WindowsVersionsConfig.md`](../Models/WindowsVersionsConfig.md) - Configuration model
- [`MicrosoftUpdateCatalogViewModel.md`](../ViewModels/MicrosoftUpdateCatalogViewModel.md) - Consumer of this service
- [`IWindowsVersionsConfigService.cs`](../../src/Services/IWindowsVersionsConfigService.cs) - Service interface

## Best Practices

1. **Dependency Injection**: Register as singleton in DI container
2. **Error Handling**: Always handle exceptions when calling service methods
3. **Validation**: Use `IsValidCombinationAsync` before making catalog requests
4. **Caching**: Service automatically caches configuration, no need for external caching
5. **Logging**: Service logs all operations for debugging and monitoring

## Error Handling

The service handles several error scenarios:
- **Missing Configuration File**: Throws `FileNotFoundException` with detailed message
- **Invalid JSON**: Throws `JsonException` during deserialization
- **Missing OS/Version**: Returns empty collections and logs warnings
- **Invalid Combinations**: Returns false for validation methods

## Performance Considerations

- **Single Load**: Configuration is loaded once and cached
- **Thread Safety**: Uses lock for thread-safe initialization
- **Memory Efficient**: Filters data on-demand rather than pre-computing all combinations
- **Async Operations**: All methods are async to avoid blocking UI thread

## Service Registration

Register in `App.xaml.cs`:
```csharp
services.AddSingleton<IWindowsVersionsConfigService, WindowsVersionsConfigService>();
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 