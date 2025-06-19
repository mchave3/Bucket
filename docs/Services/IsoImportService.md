# IsoImportService Class Documentation

## Overview

A specialized service for importing Windows images from ISO files and direct WIM/ESD files. This service handles the complete workflow of mounting ISO files, extracting Windows installation images, copying them to the managed directory, and registering them with the application.

## Location

- **File**: `src/Services/IsoImportService.cs`
- **Namespace**: `Bucket.Services`

## Class Definition

```csharp
public class IsoImportService
```

## Methods

### ImportFromIsoAsync()

```csharp
public async Task<WindowsImageInfo> ImportFromIsoAsync(
    StorageFile isoFile,
    string customName = "",
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

Imports a Windows image from an ISO file by mounting it, finding the install.wim/install.esd, and copying it to the managed directory.

**Parameters**:
- `isoFile`: The ISO file to import from
- `customName`: Optional custom name for the imported image
- `progress`: Progress reporter for the operation
- `cancellationToken`: Cancellation token for the operation

**Returns**: The imported WindowsImageInfo object

**Process**:
1. Mounts the ISO file using PowerShell
2. Scans for Windows image files (install.wim, install.esd, boot.wim)
3. Copies the main installation image to the managed directory
4. Analyzes the image with DISM
5. Registers the image in the application database
6. Dismounts the ISO file

### ImportFromWimAsync()

```csharp
public async Task<WindowsImageInfo> ImportFromWimAsync(
    StorageFile wimFile,
    string customName = "",
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
```

Imports a Windows image directly from a WIM or ESD file.

**Parameters**:
- `wimFile`: The WIM/ESD file to import
- `customName`: Optional custom name for the imported image
- `progress`: Progress reporter for the operation
- `cancellationToken`: Cancellation token for the operation

**Returns**: The imported WindowsImageInfo object

## Features

### ISO File Support
- Mounts ISO files using PowerShell Mount-DiskImage
- Automatically detects drive letter after mounting
- Safely dismounts ISO files after processing
- Handles both standard and custom ISO structures

### Windows Image Detection
- Searches multiple common locations for Windows images
- Prioritizes install.wim and install.esd files
- Supports boot.wim images as well
- Handles both WIM and ESD formats

### Progress Reporting
- Detailed progress messages throughout the import process
- File copy progress with percentage and size information
- Cancellation support for long-running operations
- User-friendly status messages

### File Management
- Copies images to the managed ImportedWIMs directory
- Generates unique filenames with timestamps
- Preserves original file extensions
- Validates file integrity during copy

## Usage Examples

### Basic ISO Import
```csharp
var isoImportService = new IsoImportService();
var progress = new Progress<string>(message => Console.WriteLine(message));

try
{
    var imageInfo = await isoImportService.ImportFromIsoAsync(
        isoFile,
        "Windows 11 Pro",
        progress,
        CancellationToken.None);

    Console.WriteLine($"Imported: {imageInfo.Name}");
}
catch (Exception ex)
{
    Console.WriteLine($"Import failed: {ex.Message}");
}
```

### WIM Import with Cancellation
```csharp
var cts = new CancellationTokenSource();
var progress = new Progress<string>();

try
{
    var imageInfo = await isoImportService.ImportFromWimAsync(
        wimFile,
        "", // Auto-generate name
        progress,
        cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Import was cancelled");
}
```

### Progress Monitoring
```csharp
var progress = new Progress<string>(message =>
{
    // Update UI with progress
    ProgressLabel.Text = message;
    Logger.Information("Import progress: {Message}", message);
});

var imageInfo = await isoImportService.ImportFromIsoAsync(
    isoFile,
    customName,
    progress,
    cancellationToken);
```

## Dependencies

### External Dependencies
- **Windows.Storage**: For StorageFile handling
- **System.Diagnostics**: For PowerShell process execution
- **System.IO**: For file system operations

### Internal Dependencies
- **WindowsImageService**: For image analysis and registration
- **Constants**: For directory paths configuration
- **LoggerSetup.Logger**: For comprehensive logging

### System Requirements
- **PowerShell**: Required for ISO mounting/dismounting
- **DISM**: Used by WindowsImageService for image analysis
- **Administrative Rights**: May be required for ISO mounting
- **Disk Space**: Sufficient space in ImportedWIMs directory

## Related Files

- [`FilePickerService.md`](./FilePickerService.md) - Provides file selection
- [`WindowsImageService.md`](./WindowsImageService.md) - Handles image analysis
- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - Coordinates the import process
- [`Constants.md`](../Common/Constants.md) - Defines directory paths

## Best Practices

### Error Handling
- Always use try-catch blocks when calling import methods
- Handle OperationCanceledException for user cancellations
- Check available disk space before starting imports
- Validate ISO/WIM files before processing

### Performance Considerations
- Large ISO files can take significant time to process
- Always provide progress reporting for user feedback
- Consider running imports on background threads
- Implement cancellation for better user experience

### Resource Management
- Service automatically handles ISO mounting/dismounting
- Temporary files are cleaned up automatically
- Failed imports don't leave partial files
- Cancellation properly cleans up resources

## Error Handling

### Common Error Scenarios

1. **File Access Issues**
   - ISO file is locked or in use
   - Insufficient permissions to mount ISO
   - Target directory is read-only

2. **ISO Mounting Failures**
   - PowerShell execution disabled
   - No available drive letters
   - Corrupted ISO file

3. **Disk Space Issues**
   - Insufficient space for image copy
   - Temporary space exhausted during processing

4. **Image Analysis Failures**
   - Invalid or corrupted WIM/ESD files
   - DISM not available or failing
   - Unsupported image formats

### Recommended Error Handling

```csharp
try
{
    // Check available disk space first
    var driveInfo = new DriveInfo(Path.GetPathRoot(Constants.ImportedWIMsDirectoryPath));
    if (driveInfo.AvailableSpace < requiredSpace)
    {
        throw new InvalidOperationException("Insufficient disk space");
    }

    var imageInfo = await isoImportService.ImportFromIsoAsync(
        isoFile, customName, progress, cancellationToken);
}
catch (OperationCanceledException)
{
    // User cancelled - normal flow
    Logger.Information("Import cancelled by user");
}
catch (UnauthorizedAccessException)
{
    // Permission issues
    await ShowErrorDialog("Administrator rights may be required for ISO mounting");
}
catch (FileNotFoundException ex)
{
    // Missing dependencies
    await ShowErrorDialog($"Required component not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Business logic errors
    await ShowErrorDialog($"Import failed: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected errors
    Logger.Error(ex, "Unexpected error during import");
    await ShowErrorDialog("An unexpected error occurred during import");
}
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
