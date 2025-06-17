# Constants Class Documentation

## Overview

The `Constants` class centralizes all application-wide constant values, primarily focusing on file and directory paths. This ensures consistent path management throughout the Bucket application and provides a single point of maintenance for system paths.

## Location

- **File**: `src/Common/Constants.cs`
- **Namespace**: `Bucket.Common`

## Class Definition

```csharp
public static partial class Constants
```

## Constants

### RootDirectoryPath

```csharp
public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductNameAndVersion);
```

- **Type**: `string`
- **Access**: Public, Static, ReadOnly
- **Purpose**: Main application data directory in user's AppData folder
- **Structure**: `%APPDATA%\[ProductName][Version]\`
- **Dependencies**:
  - `PathHelper.GetAppDataFolderPath()` - Gets AppData path
  - `ProcessInfoHelper.ProductNameAndVersion` - Gets app name and version

### LogDirectoryPath

```csharp
public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
```

- **Type**: `string`
- **Access**: Public, Static, ReadOnly
- **Purpose**: Directory for storing application log files
- **Structure**: `%APPDATA%\[ProductName][Version]\Log\`
- **Parent**: Subdirectory of `RootDirectoryPath`

### LogFilePath

```csharp
public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
```

- **Type**: `string`
- **Access**: Public, Static, ReadOnly
- **Purpose**: Full path to the main application log file
- **Structure**: `%APPDATA%\[ProductName][Version]\Log\Log.txt`
- **Usage**: Primary log file for application events and errors

### AppConfigPath

```csharp
public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");
```

- **Type**: `string`
- **Access**: Public, Static, ReadOnly
- **Purpose**: Full path to the application configuration JSON file
- **Structure**: `%APPDATA%\[ProductName][Version]\AppConfig.json`
- **Usage**: Stores user preferences and application settings

## Directory Structure

The constants define the following directory hierarchy:

```
%APPDATA%\[ProductName][Version]\
├── AppConfig.json          (AppConfigPath)
└── Log\                    (LogDirectoryPath)
    └── Log.txt            (LogFilePath)
```

## Usage Examples

### Direct Access

```csharp
// Create log directory if it doesn't exist
if (!Directory.Exists(Constants.LogDirectoryPath))
{
    Directory.CreateDirectory(Constants.LogDirectoryPath);
}

// Read configuration file
string configContent = File.ReadAllText(Constants.AppConfigPath);
```

### With Global Using

Thanks to `global using Bucket.Common;` in `GlobalUsings.cs`:

```csharp
// Direct access without namespace prefix
string logPath = Constants.LogFilePath;
string configPath = Constants.AppConfigPath;
```

### In Other Classes

```csharp
// LoggerSetup using constants
Logger = new LoggerConfiguration()
    .WriteTo.File(Constants.LogFilePath, ...)
    .CreateLogger();

// AppConfig using constants
private string fileName { get; set; } = Constants.AppConfigPath;
```

## Design Patterns

### Centralized Configuration

- **Single Source of Truth**: All paths defined in one location
- **Easy Maintenance**: Change paths in one place affects entire application
- **Consistency**: Prevents path duplication and typos

### Dependency Injection Ready

Constants can be easily injected into services:

```csharp
public class FileService
{
    private readonly string _logPath;

    public FileService()
    {
        _logPath = Constants.LogFilePath;
    }
}
```

## Best Practices

### Path Construction

1. **Use Path.Combine()**: Ensures cross-platform compatibility
2. **ReadOnly Fields**: Prevents accidental modification
3. **Static Access**: No instantiation required

### Naming Conventions

1. **Descriptive Names**: Clear indication of purpose
2. **Consistent Suffixes**:
   - `Path` for full file paths
   - `DirectoryPath` for directory paths
3. **Logical Grouping**: Related constants grouped together

### Platform Considerations

- **Cross-Platform**: Uses `Path.Combine()` for OS-specific separators
- **User Data**: Stores in appropriate user directory per platform
- **Permissions**: User AppData directory ensures write access

## Extension Points

Since the class is `partial`, you can extend it:

```csharp
// In another partial file
public static partial class Constants
{
    public static readonly string BackupDirectoryPath = Path.Combine(RootDirectoryPath, "Backup");
    public static readonly string TempDirectoryPath = Path.Combine(RootDirectoryPath, "Temp");
    public static readonly string CacheDirectoryPath = Path.Combine(RootDirectoryPath, "Cache");
}
```

## Error Handling

### Path Validation

```csharp
public static bool ValidatePaths()
{
    try
    {
        // Ensure root directory exists
        if (!Directory.Exists(RootDirectoryPath))
        {
            Directory.CreateDirectory(RootDirectoryPath);
        }

        // Validate write permissions
        string testFile = Path.Combine(RootDirectoryPath, "test.tmp");
        File.WriteAllText(testFile, "test");
        File.Delete(testFile);

        return true;
    }
    catch
    {
        return false;
    }
}
```

## Dependencies

- **System.IO.Path**: For path manipulation methods
- **PathHelper**: Custom helper for AppData path resolution
- **ProcessInfoHelper**: For application name and version information

## Related Files

- [`AppHelper.cs`](./AppHelper.md) - Uses `AppConfigPath` for settings
- [`AppConfig.cs`](./AppConfig.md) - References `AppConfigPath` for file location
- [`LoggerSetup.cs`](./LoggerSetup.md) - Uses log paths for file output

## Security Considerations

- **User Directory**: Stored in user-specific AppData (not system-wide)
- **Access Control**: Inherits user directory permissions
- **Path Traversal**: Using `Path.Combine()` prevents directory traversal attacks

## Performance Notes

- **Static ReadOnly**: Computed once at application startup
- **No Runtime Computation**: Paths calculated during static initialization
- **Memory Efficient**: Single string instances shared across application
