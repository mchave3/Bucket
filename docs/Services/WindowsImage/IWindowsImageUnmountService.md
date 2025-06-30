# IWindowsImageUnmountService Interface Documentation

## Overview
Interface for Windows image unmounting operations. Handles dismounting of Windows images with various options and cleanup operations. This service follows the Single Responsibility Principle by focusing exclusively on unmounting operations, separated from mounting functionality.

## Location
- **File**: `src/Services/WindowsImage/IWindowsImageUnmountService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition
```csharp
public interface IWindowsImageUnmountService
```

## Methods

### UnmountImageAsync
```csharp
Task UnmountImageAsync(MountedImageInfo mountedImage, bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Unmounts a Windows image from the specified mount point.

**Parameters:**
- `mountedImage`: The mounted image information
- `saveChanges`: Whether to save changes made to the mounted image (default: true)
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Returns:** Task representing the unmount operation

### UnmountAllImagesAsync
```csharp
Task UnmountAllImagesAsync(bool saveChanges = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Unmounts all currently mounted Windows images.

**Parameters:**
- `saveChanges`: Whether to save changes made to mounted images (default: true)
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Returns:** Task representing the unmount all operation

### ForceUnmountImageAsync
```csharp
Task ForceUnmountImageAsync(MountedImageInfo mountedImage, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Forces the unmount of a Windows image, discarding any changes. Use this when normal unmount fails.

**Parameters:**
- `mountedImage`: The mounted image information
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Returns:** Task representing the force unmount operation

## Usage Examples

### Basic Unmount with Changes Saved
```csharp
// Inject the service via dependency injection
private readonly IWindowsImageUnmountService _unmountService;

// Unmount and save changes
await _unmountService.UnmountImageAsync(mountedImage, saveChanges: true, progress);
```

### Unmount Without Saving Changes
```csharp
// Unmount and discard changes
await _unmountService.UnmountImageAsync(mountedImage, saveChanges: false, progress);
```

### Force Unmount (Emergency)
```csharp
// Force unmount when normal unmount fails
try
{
    await _unmountService.UnmountImageAsync(mountedImage, true, progress);
}
catch (Exception)
{
    // If normal unmount fails, try force unmount
    await _unmountService.ForceUnmountImageAsync(mountedImage, progress);
}
```

### Unmount All Images
```csharp
// Unmount all currently mounted images
await _unmountService.UnmountAllImagesAsync(saveChanges: true, progress);
```

## Features

- **Save Changes Control**: Option to save or discard changes when unmounting
- **Progress Reporting**: Detailed progress updates during unmount operations
- **Force Unmount**: Emergency unmount capability for stuck mounts
- **Batch Operations**: Ability to unmount all images at once
- **Error Handling**: Comprehensive error handling with specific error messages
- **PowerShell Integration**: Uses native PowerShell Dismount-WindowsImage cmdlets

## Dependencies

- `Bucket.Models.MountedImageInfo`: Model for mounted image information
- `IWindowsImageMountService`: Required for checking mount status and getting mount information
- PowerShell with Windows imaging cmdlets (Dismount-WindowsImage)

## Related Files

- [`WindowsImageUnmountService.md`](./WindowsImageUnmountService.md) - Implementation documentation
- [`IWindowsImageMountService.md`](./IWindowsImageMountService.md) - Mount service interface
- [`MountedImageInfo.md`](../../Models/MountedImageInfo.md) - Mounted image model

## Best Practices

1. **Always Use Progress Reporting**: Provide progress updates for long-running unmount operations
2. **Handle Cancellation**: Support cancellation tokens for responsive UI
3. **Save Changes by Default**: Default to saving changes unless explicitly discarding
4. **Use Force Unmount Sparingly**: Only use force unmount when normal unmount fails
5. **Error Handling**: Always wrap unmount operations in try-catch blocks
6. **Refresh UI**: Refresh mounted images list after unmount operations

## Error Handling

The service handles various error scenarios:

- **Access Denied**: Provides clear message about administrator privileges
- **Cmdlet Not Available**: Checks for PowerShell imaging module availability
- **Image Not Mounted**: Validates that the image is actually mounted
- **Mount In Use**: Handles cases where mount directory is being accessed
- **Directory Cleanup**: Graceful handling of directory cleanup failures

## Security Considerations

- Requires administrator privileges for most unmount operations
- Validates mount paths to prevent directory traversal
- Handles file system permissions appropriately
- Protects against force deletion of non-empty directories (except in force mode)

## Performance Notes

- Unmount operations can be time-consuming for large images
- Progress reporting provides user feedback during long operations
- Batch unmount operations are optimized for multiple images
- Cleanup operations scan directory structure efficiently

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 