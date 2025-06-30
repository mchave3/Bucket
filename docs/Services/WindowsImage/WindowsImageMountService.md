# WindowsImageMountService Class Documentation

## Overview
Service for managing Windows image mounting operations. Provides functionality to mount, unmount, and manage Windows images with support for multi-mount scenarios and proper cancellation handling.

## Location
- **File**: `src/Services/WindowsImage/WindowsImageMountService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Class Definition
```csharp
public class WindowsImageMountService : IWindowsImageMountService
```

## Constructor

### WindowsImageMountService
```csharp
public WindowsImageMountService(IWindowsImagePowerShellService powerShellService)
```
Initializes a new instance of the WindowsImageMountService class.

**Parameters:**
- `powerShellService`: The PowerShell service for executing DISM commands

**Validation:**
- Throws `ArgumentNullException` if powerShellService is null
- Logs initialization for debugging

## Public Methods

### MountImageAsync
Mounts a Windows image to a specified directory with progress reporting and cancellation support.

```csharp
public async Task<MountedImageInfo> MountImageAsync(
    string imagePath, 
    int index, 
    string imageName, 
    string editionName, 
    IProgress<string> progress = null, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `imagePath`: Path to the WIM/ESD file
- `index`: Index of the Windows edition to mount
- `imageName`: Display name of the image
- `editionName`: Name of the Windows edition
- `progress`: Optional progress reporting
- `cancellationToken`: Cancellation token for operation cancellation

**Returns:** `MountedImageInfo` object containing mount details

**Cancellation Handling:**
- Properly kills PowerShell processes when cancelled
- Cleans up mount directories created during the operation
- Handles partial mounts and orphaned files

### GetMountedImagesAsync
Retrieves a list of currently mounted Windows images.

```csharp
public async Task<List<MountedImageInfo>> GetMountedImagesAsync(CancellationToken cancellationToken = default)
```

### OpenMountDirectoryAsync
Opens the mount directory in Windows Explorer.

```csharp
public Task OpenMountDirectoryAsync(MountedImageInfo mountedImage)
```

### IsImageMountedAsync
Checks if a specific image index is currently mounted.

```csharp
public async Task<bool> IsImageMountedAsync(string imagePath, int index, CancellationToken cancellationToken = default)
```

### GetMountDirectoryPath
Generates a unique mount directory path for an image and index.

```csharp
public string GetMountDirectoryPath(string imagePath, int index)
```

## Usage Examples

### Basic Mount Operation
```csharp
var mountService = serviceProvider.GetService<IWindowsImageMountService>();

try
{
    var progress = new Progress<string>(message => Console.WriteLine(message));
    var cancellationToken = new CancellationTokenSource().Token;
    
    var mountedImage = await mountService.MountImageAsync(
        @"C:\Images\install.wim",
        1,
        "Windows 11 Pro",
        "Professional",
        progress,
        cancellationToken);
        
    Console.WriteLine($"Image mounted to: {mountedImage.MountPath}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Mount operation was cancelled");
    // Mount directory is automatically cleaned up
}
```

### Check Mount Status
```csharp
bool isMounted = await mountService.IsImageMountedAsync(@"C:\Images\install.wim", 1);
if (isMounted)
{
    Console.WriteLine("Image is currently mounted");
}
```

### Get All Mounted Images
```csharp
var mountedImages = await mountService.GetMountedImagesAsync();
foreach (var mount in mountedImages)
{
    Console.WriteLine($"Mounted: {mount.ImageName} at {mount.MountPath}");
}
```

## Features

### Cancellation Support
- **Process Termination**: Properly kills PowerShell processes when operations are cancelled
- **Directory Cleanup**: Automatically removes mount directories created during cancelled operations
- **Partial Mount Handling**: Cleans up files created during partial mount operations
- **UI State Refresh**: Ensures UI button states are correctly updated after cancellation

### Error Handling
- **Permission Errors**: Detects and provides clear messages for administrator privilege requirements
- **Module Availability**: Checks for required PowerShell imaging modules
- **Path Validation**: Validates image paths and mount directories
- **Concurrent Mount Prevention**: Prevents mounting the same image index multiple times

### Mount Directory Management
- **Unique Paths**: Generates unique mount directory names based on image name and index
- **Safe Naming**: Sanitizes file names to create valid directory names
- **Cleanup on Failure**: Removes directories if mount operations fail
- **Orphaned Directory Detection**: Can identify and clean up abandoned mount directories

## Best Practices

### Using Cancellation Tokens
Always provide cancellation tokens for long-running operations:

```csharp
using var cts = new CancellationTokenSource();
// Allow user to cancel after 5 minutes
cts.CancelAfter(TimeSpan.FromMinutes(5));

try
{
    await mountService.MountImageAsync(imagePath, index, imageName, editionName, progress, cts.Token);
}
catch (OperationCanceledException)
{
    // Handle cancellation gracefully
}
```

### Progress Reporting
Implement progress reporting for better user experience:

```csharp
var progress = new Progress<string>(message =>
{
    // Update UI with progress message
    Dispatcher.Invoke(() => ProgressLabel.Text = message);
});
```

### Error Recovery
Handle common error scenarios:

```csharp
try
{
    await mountService.MountImageAsync(imagePath, index, imageName, editionName);
}
catch (UnauthorizedAccessException)
{
    // Prompt user to run as administrator
}
catch (FileNotFoundException)
{
    // Handle missing image file
}
catch (InvalidOperationException ex) when (ex.Message.Contains("already mounted"))
{
    // Handle already mounted scenario
}
```

## Error Handling

### Common Exceptions
- **ArgumentException**: Invalid parameters (empty paths, invalid index)
- **FileNotFoundException**: Image file not found
- **UnauthorizedAccessException**: Insufficient permissions
- **InvalidOperationException**: PowerShell module issues, already mounted images
- **OperationCanceledException**: User cancellation

### Cancellation Cleanup Process
1. **Process Termination**: PowerShell process is killed immediately
2. **File Cleanup**: Any files created in mount directory are removed
3. **Directory Removal**: Mount directory is deleted if it was created
4. **State Refresh**: UI state is updated to reflect cancellation
5. **Logging**: Cancellation events are logged for troubleshooting

## Related Files
- [`IWindowsImageMountService.md`](./IWindowsImageMountService.md)
- [`WindowsImageUnmountService.md`](./WindowsImageUnmountService.md)
- [`WindowsImagePowerShellService.md`](./WindowsImagePowerShellService.md)
- [`../WindowsImageService.md`](../WindowsImageService.md)

## Security Considerations

### Administrator Privileges
- Mount operations require administrator privileges
- Service detects permission issues and provides clear error messages
- Users are prompted to run as administrator when needed

### Path Validation
- Mount paths are validated to prevent directory traversal attacks
- Image paths are checked for existence before mounting
- Safe directory naming prevents invalid characters in mount paths

## Performance Notes

### Mount Operation Timing
- Mount operations can take several minutes for large images
- Progress reporting provides user feedback during long operations
- Cancellation is responsive and immediate

### Resource Management
- PowerShell processes are properly disposed
- Mount directories are cleaned up automatically
- Memory usage is optimized for large image operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 