# AppHelper Class Documentation

## Overview
The `AppHelper` class is a static utility class that provides centralized access to application configuration settings. It serves as the main entry point for configuration management throughout the Bucket application.

## Location
- **File**: `src/Common/AppHelper.cs`
- **Namespace**: `Bucket.Common`

## Class Definition
```csharp
public static partial class AppHelper
```

## Properties

### Settings
```csharp
public static AppConfig Settings
```
- **Type**: `AppConfig`
- **Access**: Public, Static
- **Description**: Provides global access to application configuration settings
- **Configuration**:
  - Uses `JsonSettings` library for automatic JSON serialization/deserialization
  - Implements recovery mechanism with `RecoveryAction.RenameAndLoadDefault`
  - Supports versioning with `VersioningResultAction.RenameAndLoadDefault`
  - Loads configuration immediately on application startup

## Dependencies
- **Nucs.JsonSettings**: For JSON-based configuration management
- **AppConfig**: Configuration data model class

## Usage Examples

### Accessing Configuration Settings
```csharp
// Access developer mode setting
bool devMode = AppHelper.Settings.UseDeveloperMode;

// Get last update check date
string lastCheck = AppHelper.Settings.LastUpdateCheck;

// Update configuration (auto-saves due to JsonSettings)
AppHelper.Settings.UseDeveloperMode = true;
```

### Global Access
Thanks to the `global using static Bucket.Common.AppHelper;` declaration in `GlobalUsings.cs`, you can access settings directly:

```csharp
// Direct access without class prefix
bool devMode = Settings.UseDeveloperMode;
```

## Features
- **Automatic Persistence**: Changes to settings are automatically saved to JSON file
- **Error Recovery**: If configuration file is corrupted, it creates a backup and loads defaults
- **Version Management**: Handles configuration schema changes between application versions
- **Thread-Safe**: JsonSettings provides thread-safe access to configuration

## Error Handling
- **Recovery Action**: If the configuration file is corrupted or missing, the system:
  1. Renames the problematic file (backup)
  2. Creates a new configuration with default values
  3. Continues application execution without interruption

## Best Practices
1. Always access settings through the `Settings` property
2. Avoid caching settings values - access them directly when needed
3. Settings are automatically saved, no manual save operation required
4. Use meaningful property names in `AppConfig` for maintainability

## Related Files
- [`AppConfig.cs`](./AppConfig.md) - Configuration data model
- [`Constants.cs`](./Constants.md) - Application constants including config file path
- [`GlobalUsings.cs`](./GlobalUsings.md) - Global using declarations

## Configuration File Location
The configuration is stored as JSON in the path defined by `Constants.AppConfigPath`, typically:
```
%APPDATA%/[AppName]/AppConfig.json
```
