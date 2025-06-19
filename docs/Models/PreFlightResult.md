# PreFlightResult Class Documentation

## Overview

The `PreFlightResult` class represents the comprehensive results of pre-flight system checks performed at application startup. It contains the status of all verification checks and provides summary information for error handling and user notification.

## Location

- **File**: `src/Models/PreFlightResult.cs`
- **Namespace**: `Bucket.Models`

## Class Definition

```csharp
public class PreFlightResult
```

## Properties

### Critical Check Results

#### AdminPrivileges
```csharp
public bool AdminPrivileges { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether the application is running with administrator privileges
- **Critical**: Yes - Required for Windows image management operations

#### DirectoryStructure
```csharp
public bool DirectoryStructure { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether the required directory structure was created successfully
- **Critical**: Yes - Required for file operations

#### WritePermissions
```csharp
public bool WritePermissions { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether write permissions are available in all required directories
- **Critical**: Yes - Required for file operations

#### DiskSpace
```csharp
public bool DiskSpace { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether sufficient disk space is available for operations
- **Critical**: Yes - Windows images require significant space

#### SystemTools
```csharp
public bool SystemTools { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether all required system tools (DISM, PowerShell) are available
- **Critical**: Yes - Required for image manipulation

### Recommended Check Results

#### WindowsVersion
```csharp
public bool WindowsVersion { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether the Windows version is compatible with the application
- **Critical**: No - Generates warning only

#### WindowsServices
```csharp
public bool WindowsServices { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether required Windows services are running
- **Critical**: No - May affect update functionality

#### NetworkConnectivity
```csharp
public bool NetworkConnectivity { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether network connectivity to Microsoft servers is available
- **Critical**: No - May affect update downloads

### Configuration Check Results

#### ConfigFiles
```csharp
public bool ConfigFiles { get; set; }
```
- **Type**: `bool`
- **Purpose**: Indicates whether configuration files were created or repaired successfully
- **Critical**: Auto-repair - Issues are resolved automatically

### Summary Properties
```
- **Type**: `bool`
- **Purpose**: Indicates whether the logging system was initialized successfully
- **Critical**: Auto-repair - Non-critical for core functionality

### Summary Properties

#### AllCriticalChecksPassed
```csharp
public bool AllCriticalChecksPassed =>
    AdminPrivileges && DirectoryStructure && WritePermissions && DiskSpace && SystemTools;
```
- **Type**: `bool` (computed property)
- **Purpose**: Gets a value indicating whether all critical checks passed
- **Usage**: Determines if application can start safely

#### FailedCriticalChecks
```csharp
public List<string> FailedCriticalChecks { get; }
```
- **Type**: `List<string>` (computed property)
- **Purpose**: Gets a list of failed critical checks
- **Usage**: Error reporting and user notification

#### FailedRecommendedChecks
```csharp
public List<string> FailedRecommendedChecks { get; }
```
- **Type**: `List<string>` (computed property)
- **Purpose**: Gets a list of failed recommended checks
- **Usage**: Warning generation and user notification

### Message Collections

#### ErrorMessages
```csharp
public List<string> ErrorMessages { get; set; } = new List<string>();
```
- **Type**: `List<string>`
- **Purpose**: Collection of detailed error messages from the pre-flight checks
- **Usage**: Displaying specific error information to users

#### WarningMessages
```csharp
public List<string> WarningMessages { get; set; } = new List<string>();
```
- **Type**: `List<string>`
- **Purpose**: Collection of warning messages from the pre-flight checks
- **Usage**: Displaying non-critical issues to users

## Usage Examples

### Basic Result Evaluation

```csharp
var preFlightService = GetService<PreFlightService>();
var result = await preFlightService.RunPreFlightChecksAsync();

if (result.AllCriticalChecksPassed)
{
    Logger.Information("Pre-flight checks passed - application ready");
    // Continue with application startup
}
else
{
    Logger.Error("Critical pre-flight checks failed: {FailedChecks}",
                 string.Join(", ", result.FailedCriticalChecks));
    // Show error dialog and exit
}
```

### Error Message Display

```csharp
if (!result.AllCriticalChecksPassed)
{
    var errorMessage = string.Join("\n", new[]
    {
        "The following critical issues prevent Bucket from starting:",
        "",
        string.Join("\n", result.ErrorMessages),
        "",
        "Please resolve these issues and restart the application."
    });

    var dialog = new ContentDialog
    {
        Title = "Bucket - Startup Failed",
        Content = errorMessage,
        CloseButtonText = "Exit"
    };

    await dialog.ShowAsync();
}
```

### Warning Handling

```csharp
if (result.WarningMessages.Any())
{
    Logger.Warning("Pre-flight warnings detected: {Warnings}",
                   string.Join(", ", result.WarningMessages));

    var warningMessage = string.Join("\n", new[]
    {
        "System warnings detected:",
        "",
        string.Join("\n", result.WarningMessages),
        "",
        "The application will continue but some features may be limited."
    });

    // Show non-blocking warning dialog
    ShowWarningDialog(warningMessage);
}
```

### Individual Check Analysis

```csharp
if (!result.AdminPrivileges)
{
    Logger.Error("Administrator privileges required");
    ShowElevationPrompt();
}

if (!result.DiskSpace)
{
    Logger.Error("Insufficient disk space");
    ShowDiskSpaceDialog();
}

if (!result.SystemTools)
{
    Logger.Error("Required system tools missing");
    ShowToolsInstallationDialog();
}
```

## Check Categories

### Critical Checks (Application Exit if Failed)

1. **AdminPrivileges** - Application requires elevation
2. **DirectoryStructure** - Working directories must be created
3. **WritePermissions** - File system access required
4. **DiskSpace** - Minimum 15GB free space needed
5. **SystemTools** - DISM.exe and other tools required

### Recommended Checks (Warnings Only)

1. **WindowsVersion** - Windows 10 1809+ recommended
2. **WindowsServices** - Update services should be running
3. **NetworkConnectivity** - Internet access for updates

### Configuration Checks (Auto-Repair)

1. **ConfigFiles** - WIMs.xml and other config files
2. **LoggingSystem** - Working directory logging

## Integration with Application Startup

### Pre-Flight Flow

```csharp
// 1. Service execution
var result = await preFlightService.RunPreFlightChecksAsync();

// 2. Critical check evaluation
if (!result.AllCriticalChecksPassed)
{
    ShowErrorDialog(result.ErrorMessages);
    Application.Current.Exit();
    return;
}

// 3. Warning handling (non-blocking)
if (result.WarningMessages.Any())
{
    ShowWarningDialog(result.WarningMessages);
}

// 4. Continue application initialization
```

### Configuration Updates

The service automatically updates `AppConfig` with result information:

```csharp
AppHelper.Settings.PreFlightCompleted = result.AllCriticalChecksPassed;
AppHelper.Settings.LastPreFlightCheck = DateTime.Now;
```

## Best Practices

### Error Handling

1. **Graceful Degradation**: Continue with warnings where possible
2. **Clear Messages**: Provide specific, actionable error descriptions
3. **User Guidance**: Include resolution steps in error messages
4. **Logging Integration**: Log all results for troubleshooting

### Result Processing

1. **Check Order**: Process critical checks first
2. **Message Aggregation**: Combine related errors/warnings
3. **User Experience**: Show progress during long-running checks
4. **Recovery Options**: Offer automatic fixes where possible

## Related Files

- [`PreFlightService.cs`](../Common/PreFlightService.md) - Service implementation
- [`AppConfig.cs`](../Common/AppConfig.md) - Configuration persistence
- [`Constants.cs`](../Common/Constants.md) - System requirements
- [`App.xaml.cs`](../App.md) - Application startup integration

## Security Considerations

- **Error Information**: Avoid exposing sensitive system details
- **Privilege Escalation**: Handle UAC prompts gracefully
- **File Access**: Validate all file system operations
- **Network Requests**: Use secure protocols for connectivity tests
