# WindowsImageMountService Class Documentation

## Overview
Implementation of the Windows image mounting service. Provides concrete functionality to mount and manage Windows images with support for multi-mount scenarios. This service handles PowerShell DISM command execution and mount directory management.

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
```csharp
public async Task<MountedImageInfo> MountImageAsync(string imagePath, int index, string imageName, string editionName, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Mounts a Windows image index to a unique directory using PowerShell DISM commands.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `index`: The index number to mount (must be > 0)
- `imageName`: The friendly name of the image
- `editionName`: The name of the Windows edition
- `progress`: Progress reporter for operation updates (optional)
- `cancellationToken`: Cancellation token (optional)

**Returns:** `MountedImageInfo` object containing mount details

**Process:**
1. Validates input parameters and file existence
2. Checks if image is already mounted
3. Creates unique mount directory
4. Executes `Mount-WindowsImage` PowerShell command
5. Verifies mount success
6. Returns populated `MountedImageInfo`

**Exceptions:**
- `ArgumentException`: Invalid parameters
- `FileNotFoundException`: Image file doesn't exist
- `InvalidOperationException`: Image already mounted or mount failed

### GetMountedImagesAsync
```csharp
public async Task<List<MountedImageInfo>> GetMountedImagesAsync(CancellationToken cancellationToken = default)
```
Gets a list of all currently mounted images using `Get-WindowsImage -Mounted`.

**Returns:** List of `MountedImageInfo` objects

**Process:**
1. Executes `Get-WindowsImage -Mounted | ConvertTo-Json -Depth 10`
2. Parses JSON output into `MountedImageInfo` objects
3. Returns populated list

### OpenMountDirectoryAsync
```csharp
public Task OpenMountDirectoryAsync(MountedImageInfo mountedImage)
```
Opens the mount directory for a mounted image in Windows Explorer.

**Parameters:**
- `mountedImage`: The mounted image whose directory to open

**Process:**
1. Validates mount directory exists
2. Launches Windows Explorer with mount path
3. Logs operation result

**Exceptions:**
- `ArgumentNullException`: mountedImage is null
- `DirectoryNotFoundException`: Mount directory doesn't exist

### IsImageMountedAsync
```csharp
public async Task<bool> IsImageMountedAsync(string imagePath, int index, CancellationToken cancellationToken = default)
```
Checks if a specific image index is currently mounted.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `index`: The index number to check
- `cancellationToken`: Cancellation token (optional)

**Returns:** True if the image is mounted, false otherwise

**Process:**
1. Gets list of mounted images
2. Searches for matching path and index
3. Returns boolean result

### GetMountDirectoryPath
```csharp
public string GetMountDirectoryPath(string imagePath, int index)
```
Gets the mount directory path for a specific image and index.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `index`: The index number

**Returns:** The calculated mount directory path

**Process:**
1. Extracts filename from image path
2. Creates safe directory name using regex
3. Combines with base mount directory and index

## Private Methods

### ExecutePowerShellCommandAsync
```csharp
private async Task ExecutePowerShellCommandAsync(string command, IProgress<string> progress, CancellationToken cancellationToken)
```
Executes a PowerShell command with progress reporting.

**Features:**
- Uses injected PowerShell service
- Reports user-friendly progress messages (e.g., "Mounting index 1, please wait...")
- Handles cancellation
- Logs command execution

### ExecutePowerShellCommandWithOutputAsync
```csharp
private async Task<string> ExecutePowerShellCommandWithOutputAsync(string command, CancellationToken cancellationToken)
```
Executes a PowerShell command and returns the output.

**Features:**
- Captures command output
- Handles cancellation
- Error handling and logging

### ParseMountedImagesJson
```csharp
private static List<MountedImageInfo> ParseMountedImagesJson(string jsonOutput)
```
Parses JSON output from `Get-WindowsImage -Mounted` into `MountedImageInfo` objects.

**Features:**
- Handles both single object and array JSON formats
- Robust JSON parsing with error handling
- Converts PowerShell output to domain models

### ParseMountedImageElement
```csharp
private static MountedImageInfo ParseMountedImageElement(JsonElement imageElement)
```
Parses a single JSON element into a `MountedImageInfo` object.

**Features:**
- Safe property extraction from JSON
- Default value handling for missing properties
- Consistent object creation

## Usage Examples

### Basic Mount Operation
```csharp
var mountService = serviceProvider.GetService<WindowsImageMountService>();

var mountedImage = await mountService.MountImageAsync(
    @"C:\Images\install.wim",
    1,
    "Windows 11 Pro",
    "Windows 11 Pro",
    new Progress<string>(msg => Console.WriteLine(msg)),
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

## Features

- **PowerShell Integration**: Uses native Windows PowerShell DISM commands
- **Progress Reporting**: Detailed progress updates during operations
- **Multi-Mount Support**: Handles multiple images mounted simultaneously
- **Unique Mount Paths**: Creates safe, unique directory names for each mount
- **JSON Parsing**: Robust parsing of PowerShell JSON output
- **Error Handling**: Comprehensive error handling with detailed logging
- **Path Safety**: Regex-based safe directory name generation
- **Mount Verification**: Verifies mount success after operations

## Dependencies

- `IWindowsImagePowerShellService`: For PowerShell command execution
- `Bucket.Models.MountedImageInfo`: Mount information model
- `Bucket.Common.Constants`: Mount directory path constants
- System.Text.Json: For parsing PowerShell JSON output
- System.Diagnostics: For launching Windows Explorer

## Related Files

- [`IWindowsImageMountService.md`](./IWindowsImageMountService.md) - Interface documentation
- [`IWindowsImagePowerShellService.md`](./IWindowsImagePowerShellService.md) - PowerShell service interface
- [`MountedImageInfo.md`](../../Models/MountedImageInfo.md) - Mount information model
- [`Constants.md`](../../Common/Constants.md) - Application constants

## Best Practices

### Service Usage
1. **Dependency Injection**: Always inject via constructor
2. **Progress Reporting**: Provide progress callbacks for long operations
3. **Cancellation Support**: Use cancellation tokens appropriately
4. **Error Handling**: Wrap operations in try-catch blocks

### Mount Management
1. **Check Before Mount**: Always check if image is already mounted
2. **Unique Directories**: Let service manage unique mount paths
3. **Directory Cleanup**: Use unmount service for proper cleanup
4. **Path Validation**: Validate image file existence before mounting

### Performance
1. **Async Operations**: All operations are async for UI responsiveness
2. **Cancellation**: Support operation cancellation
3. **Resource Management**: Proper disposal of resources
4. **Efficient Parsing**: Optimized JSON parsing for large mount lists

## Error Handling

### Common Error Scenarios
- **File Not Found**: Image file doesn't exist at specified path
- **Already Mounted**: Attempting to mount already mounted image
- **Permission Denied**: Insufficient privileges for mount operations
- **Invalid Index**: Specified index doesn't exist in image
- **Directory Creation**: Failed to create mount directory

### Error Recovery
- **Validation**: Extensive input validation prevents many errors
- **Graceful Degradation**: Service continues operating despite individual failures
- **Detailed Logging**: Comprehensive error logging for troubleshooting
- **Exception Propagation**: Meaningful exceptions with context

## Security Considerations

- **Path Validation**: Validates and sanitizes mount directory paths
- **Administrator Privileges**: Requires elevated permissions for mount operations
- **File System Access**: Validates file and directory permissions
- **Command Injection**: Uses parameterized PowerShell commands

## Performance Considerations

- **Async Operations**: Non-blocking operations for UI responsiveness
- **Efficient JSON Parsing**: Optimized parsing of PowerShell output
- **Directory Management**: Efficient mount directory creation and validation
- **Resource Usage**: Minimal memory footprint for mount tracking

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 