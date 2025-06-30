# IWindowsImageMountService Interface Documentation

## Overview
Interface for Windows image mounting operations. Provides functionality to mount and manage Windows images with support for multi-mount scenarios. This service follows the Single Responsibility Principle by focusing exclusively on mounting operations, with unmounting handled by a separate service.

## Location
- **File**: `src/Services/WindowsImage/IWindowsImageMountService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition
```csharp
public interface IWindowsImageMountService
```

## Methods

### MountImageAsync
```csharp
Task<MountedImageInfo> MountImageAsync(string imagePath, int index, string imageName, string editionName, IProgress<string> progress = null, CancellationToken cancellationToken = default);
```
Mounts a Windows image index to a unique directory.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `index`: The index number to mount
- `imageName`: The friendly name of the image
- `editionName`: The name of the Windows edition
- `progress`: The progress reporter (optional)
- `cancellationToken`: The cancellation token (optional)

**Returns:** The mounted image information

### GetMountedImagesAsync
```csharp
Task<List<MountedImageInfo>> GetMountedImagesAsync(CancellationToken cancellationToken = default);
```
Gets a list of all currently mounted images using `Get-WindowsImage -Mounted`.

**Parameters:**
- `cancellationToken`: The cancellation token (optional)

**Returns:** A list of mounted image information

### OpenMountDirectoryAsync
```csharp
Task OpenMountDirectoryAsync(MountedImageInfo mountedImage);
```
Opens the mount directory for a mounted image in Windows Explorer.

**Parameters:**
- `mountedImage`: The mounted image whose directory to open

### IsImageMountedAsync
```csharp
Task<bool> IsImageMountedAsync(string imagePath, int index, CancellationToken cancellationToken = default);
```
Checks if a specific image index is currently mounted.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `index`: The index number to check
- `cancellationToken`: The cancellation token (optional)

**Returns:** True if the image is mounted, false otherwise

### GetMountDirectoryPath
```csharp
string GetMountDirectoryPath(string imagePath, int index);
```
Gets the mount directory path for a specific image and index.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `index`: The index number

**Returns:** The mount directory path

### CleanupOrphanedMountDirectoriesAsync
```csharp
Task CleanupOrphanedMountDirectoriesAsync(IProgress<string> progress = null, CancellationToken cancellationToken = default);
```
Cleans up orphaned mount directories that are not currently active.

**Parameters:**
- `progress`: Optional progress reporter
- `cancellationToken`: The cancellation token (optional)

**Returns:** A task representing the cleanup operation

## Usage Examples

### Basic Mount Operation
```csharp
var mountService = serviceProvider.GetService<IWindowsImageMountService>();

var mountedImage = await mountService.MountImageAsync(
    @"C:\Images\install.wim",
    1,
    "Windows 11 Pro",
    "Windows 11 Pro",
    progress,
    cancellationToken);

Console.WriteLine($"Mounted to: {mountedImage.MountPath}");
```

### Checking Mount Status
```csharp
bool isMounted = await mountService.IsImageMountedAsync(
    @"C:\Images\install.wim", 
    1);

if (isMounted)
{
    Console.WriteLine("Image is already mounted");
}
```

### Getting All Mounted Images
```csharp
var mountedImages = await mountService.GetMountedImagesAsync();
foreach (var mount in mountedImages)
{
    Console.WriteLine($"{mount.DisplayText} -> {mount.MountPath}");
}
```

### Opening Mount Directory
```csharp
await mountService.OpenMountDirectoryAsync(mountedImage);
```

### Cleaning Up Orphaned Directories
```csharp
var progress = new Progress<string>(message => Console.WriteLine(message));
await mountService.CleanupOrphanedMountDirectoriesAsync(progress);
```

## Features
- **Multi-Mount Support**: Supports mounting multiple indices from the same or different WIM files
- **Unique Mount Paths**: Creates unique mount directories for each image/index combination
- **Progress Reporting**: Supports progress reporting for long-running operations
- **Cancellation Support**: All async operations support cancellation tokens
- **System Integration**: Uses PowerShell `Get-WindowsImage -Mounted` for accurate status
- **Single Responsibility**: Focuses exclusively on mounting operations

## Dependencies
- `Bucket.Models.MountedImageInfo` for mount information representation
- PowerShell DISM module for mount operations
- Windows Imaging API support

## Related Files
- [`MountedImageInfo.md`](../../Models/MountedImageInfo.md) - Model for mount information
- [`WindowsImageMountService.md`](./WindowsImageMountService.md) - Implementation class
- [`IWindowsImageUnmountService.md`](./IWindowsImageUnmountService.md) - Unmounting operations
- [`Constants.md`](../../Common/Constants.md) - Mount directory path constants

## Best Practices
- Always check if an image is already mounted before mounting
- Use progress reporting for long-running mount operations
- Handle cancellation tokens appropriately in UI scenarios
- Validate image paths and indices before operations
- Use unique mount directories to avoid conflicts
- Use the separate unmount service for dismounting operations

## Error Handling
- Throw `ArgumentException` for invalid parameters
- Throw `FileNotFoundException` for missing image files
- Throw `InvalidOperationException` for mount failures
- Handle PowerShell execution errors gracefully
- Provide detailed error messages for troubleshooting

## Security Considerations
- Validate file paths to prevent directory traversal attacks
- Ensure mount directories are within allowed locations
- Check permissions before mount operations
- Handle administrator privilege requirements

## Architecture Notes
This service follows the Single Responsibility Principle (SRP) by focusing exclusively on mounting operations. Unmounting operations are handled by the separate `IWindowsImageUnmountService` interface, providing better separation of concerns and testability.

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 