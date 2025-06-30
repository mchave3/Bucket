# WindowsImageUnmountService Class Documentation

## Overview
Implementation of the Windows image unmounting service. Provides concrete functionality for dismounting Windows images with various options including save/discard changes, force unmount, batch operations, and cleanup operations. This service handles PowerShell DISM command execution for unmounting operations.

## Location
- **File**: `src/Services/WindowsImage/WindowsImageUnmountService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Class Definition
```csharp
public class WindowsImageUnmountService : IWindowsImageUnmountService
```

## Constructor

### WindowsImageUnmountService
```csharp
public WindowsImageUnmountService(IWindowsImageMountService mountService)
```
Initializes a new instance of the WindowsImageUnmountService class.

**Parameters:**
- `mountService`: The mount service for checking mounted images

**Validation:**
- Throws `ArgumentNullException` if mountService is null
- Logs initialization for debugging

## Public Methods

### UnmountImageAsync
```csharp
public async Task UnmountImageAsync(MountedImageInfo mountedImage, bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Unmounts a Windows image from the specified mount point using PowerShell DISM commands.

**Parameters:**
- `mountedImage`: The mounted image information
- `saveChanges`: Whether to save changes made to the mounted image (default: true)
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Process:**
1. Validates input parameters
2. Sets mount status to "Unmounting"
3. Executes `Dismount-WindowsImage` with `-Save` or `-Discard` parameter
4. Cleans up empty mount directory
5. Reports completion

**Exceptions:**
- `ArgumentNullException`: mountedImage is null
- Various PowerShell execution exceptions

### UnmountAllImagesAsync
```csharp
public async Task UnmountAllImagesAsync(bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Unmounts all currently mounted Windows images in sequence.

**Parameters:**
- `saveChanges`: Whether to save changes made to mounted images (default: true)
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Process:**
1. Gets list of all mounted images
2. Iterates through each mounted image
3. Calls `UnmountImageAsync` for each image
4. Continues even if individual unmounts fail
5. Reports overall progress

**Features:**
- **Batch Processing**: Handles multiple images efficiently
- **Error Resilience**: Continues processing even if individual unmounts fail
- **Progress Tracking**: Reports progress for each image
- **Comprehensive Logging**: Logs success and failure for each image

### ForceUnmountImageAsync
```csharp
public async Task ForceUnmountImageAsync(MountedImageInfo mountedImage, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Forces the unmount of a Windows image, discarding any changes. Use when normal unmount fails.

**Parameters:**
- `mountedImage`: The mounted image information
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Process:**
1. Validates input parameters
2. Executes `Dismount-WindowsImage` with `-Discard` parameter
3. Force deletes mount directory and all contents
4. Reports completion

**Warning:** This method discards all changes and forcibly removes directories.

## Private Methods

### ExecutePowerShellCommandAsync
```csharp
private async Task ExecutePowerShellCommandAsync(string command, CancellationToken cancellationToken)
```
Executes a PowerShell command for unmount operations.

**Features:**
- **Process Management**: Creates and manages PowerShell process
- **Error Handling**: Captures and processes command errors
- **Timeout Support**: Handles command timeouts
- **Output Capture**: Captures both standard output and error streams

### CleanupMountDirectoryAsync
```csharp
private async Task CleanupMountDirectoryAsync(string mountPath)
```
Cleans up a mount directory if it's empty.

**Features:**
- **Safety Check**: Only removes empty directories
- **Async Operation**: Non-blocking directory operations
- **Error Tolerance**: Logs warnings but doesn't throw exceptions
- **Path Validation**: Validates directory existence before cleanup

## Usage Examples

### Basic Unmount with Changes Saved
```csharp
var unmountService = serviceProvider.GetService<WindowsImageUnmountService>();

await unmountService.UnmountImageAsync(
    mountedImage, 
    saveChanges: true, 
    new Progress<string>(msg => Console.WriteLine(msg)));
```

### Unmount Without Saving Changes
```csharp
await unmountService.UnmountImageAsync(
    mountedImage, 
    saveChanges: false, 
    progress);
```

### Force Unmount (Emergency)
```csharp
try
{
    await unmountService.UnmountImageAsync(mountedImage, true, progress);
}
catch (Exception)
{
    // If normal unmount fails, try force unmount
    await unmountService.ForceUnmountImageAsync(mountedImage, progress);
}
```

### Unmount All Images
```csharp
await unmountService.UnmountAllImagesAsync(
    saveChanges: true, 
    new Progress<string>(msg => Console.WriteLine(msg)));
```

## Features

- **Save/Discard Control**: Option to save or discard changes when unmounting
- **Progress Reporting**: User-friendly progress updates during all operations (e.g., "Unmounting index 2 and saving changes...")
- **Force Unmount**: Emergency unmount capability for stuck mounts
- **Batch Operations**: Ability to unmount all images at once
- **Error Resilience**: Continues operation even when individual operations fail
- **PowerShell Integration**: Uses native PowerShell Dismount-WindowsImage cmdlets
- **Directory Management**: Safe and intelligent mount directory cleanup

## Dependencies

- `IWindowsImageMountService`: Required for checking mount status and getting mount information
- `Bucket.Models.MountedImageInfo`: Model for mounted image information
- `Bucket.Common.Constants`: Mount directory path constants
- System.Diagnostics: For PowerShell process management

## Related Files

- [`IWindowsImageUnmountService.md`](./IWindowsImageUnmountService.md) - Interface documentation
- [`IWindowsImageMountService.md`](./IWindowsImageMountService.md) - Mount service interface
- [`MountedImageInfo.md`](../../Models/MountedImageInfo.md) - Mounted image model
- [`Constants.md`](../../Common/Constants.md) - Application constants

## Best Practices

### Service Usage
1. **Dependency Injection**: Always inject via constructor
2. **Progress Reporting**: Provide progress callbacks for all operations
3. **Error Handling**: Wrap operations in try-catch blocks
4. **Cancellation Support**: Use cancellation tokens appropriately

### Unmount Operations
1. **Save by Default**: Default to saving changes unless explicitly discarding
2. **Force Unmount Sparingly**: Only use force unmount when normal unmount fails
3. **Batch Processing**: Use UnmountAllImagesAsync for multiple images

### Error Handling
1. **Graceful Degradation**: Handle individual failures in batch operations
2. **Detailed Logging**: Log all operations for troubleshooting
3. **User Feedback**: Provide clear progress and error messages
4. **Recovery Options**: Offer force unmount as fallback option

## Error Handling

### Common Error Scenarios
- **Access Denied**: Insufficient privileges for unmount operations
- **Mount In Use**: Mount directory being accessed by other processes
- **Corrupted Mount**: Mount state inconsistencies requiring force unmount
- **Directory Cleanup**: Failures during mount directory cleanup
- **PowerShell Errors**: DISM command execution failures

### Error Recovery Strategies
- **Retry Logic**: Automatic retry for transient failures
- **Force Unmount**: Emergency unmount for stuck mounts
- **Graceful Degradation**: Continue batch operations despite individual failures
- **Detailed Logging**: Comprehensive error information for troubleshooting

## Security Considerations

- **Administrator Privileges**: Requires elevated permissions for unmount operations
- **Path Validation**: Validates mount paths to prevent directory traversal
- **Safe Cleanup**: Conservative approach to directory deletion
- **Command Injection Prevention**: Uses parameterized PowerShell commands

## Performance Considerations

- **Async Operations**: Non-blocking operations for UI responsiveness
- **Batch Efficiency**: Optimized batch processing for multiple images
- **Resource Management**: Proper disposal of PowerShell processes
- **Memory Usage**: Minimal memory footprint for unmount operations
- **Progress Granularity**: Balanced progress reporting without performance impact

## Troubleshooting

### Common Issues
1. **Unmount Hangs**: Use force unmount for stuck operations
2. **Permission Errors**: Ensure administrator privileges
3. **Directory Not Empty**: Check for open file handles in mount directory
4. **Orphaned Directories**: Run cleanup operation to remove unused directories

### Diagnostic Information
- **Logging**: Comprehensive logging at all levels
- **Progress Messages**: Detailed progress information
- **Error Context**: Exception details with operation context
- **Mount Status**: Real-time mount status updates

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 