# AppConfig Class Documentation

## Overview

The `AppConfig` class serves as the data model for application configuration settings. It provides a strongly-typed interface for managing application preferences and settings with automatic JSON serialization and persistence.

## Location

- **File**: `src/Common/AppConfig.cs`
- **Namespace**: `Bucket.Common`

## Class Definition

```csharp
[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
```

## Inheritance Hierarchy

- **Base Class**: `NotifiyingJsonSettings` (from Nucs.JsonSettings)
- **Interfaces**: `IVersionable`

## Attributes

### GenerateAutoSaveOnChange

Automatically generates code to save configuration changes to the JSON file when any property is modified.

## Properties

### Version

```csharp
[EnforcedVersion("1.0.0.0")]
public Version Version { get; set; } = new Version(1, 0, 0, 0);
```

- **Type**: `Version`
- **Access**: Public
- **Default**: Version 1.0.0.0
- **Purpose**: Tracks configuration schema version for migration purposes
- **Attribute**: `[EnforcedVersion]` ensures compatibility

### fileName

```csharp
private string fileName { get; set; } = Constants.AppConfigPath;
```

- **Type**: `string`
- **Access**: Private
- **Purpose**: Specifies the file path where configuration is stored
- **Default**: Value from `Constants.AppConfigPath`

### useDeveloperMode

```csharp
private bool useDeveloperMode { get; set; }
```

- **Type**: `bool`
- **Access**: Private
- **Purpose**: Controls whether developer features are enabled
- **Default**: `false`

### lastUpdateCheck

```csharp
private string lastUpdateCheck { get; set; }
```

- **Type**: `string`
- **Access**: Private
- **Purpose**: Stores the timestamp of the last update check
- **Format**: Typically ISO date string

### preFlightCompleted

```csharp
private bool preFlightCompleted { get; set; } = false;
```

- **Type**: `bool`
- **Access**: Private
- **Purpose**: Indicates if pre-flight system checks have been completed successfully
- **Default**: `false`
- **Usage**: Prevents repeated system validation on startup

### lastPreFlightCheck

```csharp
private DateTime lastPreFlightCheck { get; set; } = DateTime.MinValue;
```

- **Type**: `DateTime`
- **Access**: Private
- **Purpose**: Timestamp of last successful pre-flight check
- **Default**: `DateTime.MinValue`
- **Usage**: Determines when to re-run system validation

### skipAdminCheck

```csharp
private bool skipAdminCheck { get; set; } = false;
```

- **Type**: `bool`
- **Access**: Private
- **Purpose**: Bypasses administrator privilege verification during pre-flight
- **Default**: `false`
- **Usage**: Development and testing scenarios
- **Warning**: Should only be used in controlled environments

### skipDiskSpaceCheck

```csharp
private bool skipDiskSpaceCheck { get; set; } = false;
```

- **Type**: `bool`
- **Access**: Private
- **Purpose**: Bypasses disk space validation during pre-flight
- **Default**: `false`
- **Usage**: Development environments with limited disk space
- **Warning**: May cause operations to fail if insufficient disk space

### customWorkingDirectory

```csharp
private string customWorkingDirectory { get; set; } = "";
```

- **Type**: `string`
- **Access**: Private
- **Purpose**: Override default ProgramData working directory
- **Default**: Empty string (uses default from Constants)
- **Usage**: Custom deployment scenarios or alternative storage locations

## Features

### Automatic Persistence

- Changes to any property automatically trigger a save operation
- No manual save calls required
- JSON serialization handled transparently

### Version Management

- Supports configuration schema versioning
- Automatic migration when version changes
- Backwards compatibility handling

### Type Safety

- Strongly-typed properties prevent configuration errors
- Compile-time validation of setting access

## Usage Examples

### Accessing Configuration

```csharp
// Through AppHelper (recommended)
bool devMode = AppHelper.Settings.UseDeveloperMode;
string lastCheck = AppHelper.Settings.LastUpdateCheck;
```

### Modifying Settings

```csharp
// Auto-saves when modified
AppHelper.Settings.UseDeveloperMode = true;
AppHelper.Settings.LastUpdateCheck = DateTime.Now.ToString("O");
```

### Version Checking

```csharp
Version currentVersion = AppHelper.Settings.Version;
if (currentVersion < new Version(2, 0, 0, 0))
{
    // Handle migration
}
```

## JSON Structure

The configuration is serialized to JSON with the following structure:

```json
{
  "Version": "1.0.0.0",
  "fileName": "C:\\ProgramData\\Bucket\\AppConfig.json",
  "useDeveloperMode": false,
  "lastUpdateCheck": "2025-06-17T10:30:00.000Z",
  "preFlightCompleted": false,
  "lastPreFlightCheck": "0001-01-01T00:00:00",
  "skipAdminCheck": false,
  "skipDiskSpaceCheck": false,
  "customWorkingDirectory": ""
}
```

## Best Practices

1. **Property Naming**: Use descriptive names that clearly indicate purpose
2. **Default Values**: Always provide sensible defaults for new properties
3. **Version Management**: Increment version when adding/removing properties
4. **Access Modifiers**: Use appropriate access levels (private for internal settings)
5. **Type Selection**: Choose appropriate data types for each setting

## Dependencies

- **Nucs.JsonSettings.Examples**: For base functionality
- **Nucs.JsonSettings.Modulation**: For versioning and attributes
- **Constants**: For file path definitions

## Error Handling

- **File Corruption**: JsonSettings handles corrupted files with recovery actions
- **Missing Properties**: New properties get default values automatically
- **Version Conflicts**: Automatic migration or backup creation

## Extension Points

Since the class is `partial`, you can extend it in separate files:

```csharp
// In another partial file
public partial class AppConfig
{
    public string CustomSetting { get; set; } = "default";

    // Custom validation logic
    public bool IsValidConfiguration()
    {
        return !string.IsNullOrEmpty(lastUpdateCheck);
    }
}
```

## Related Files

- [`AppHelper.cs`](./AppHelper.md) - Configuration access point
- [`Constants.cs`](./Constants.md) - File path constants
- [`LoggerSetup.cs`](./LoggerSetup.md) - Logging configuration

## Security Considerations

- Sensitive settings should be encrypted before storage
- File permissions should restrict access to the configuration file
- Consider separate configuration files for sensitive vs. non-sensitive data

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
