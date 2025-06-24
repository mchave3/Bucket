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
1. Checks if ISO is already mounted to avoid conflicts
2. Mounts the ISO file using PowerShell with improved reliability
3. Scans for Windows image files (install.wim, install.esd, boot.wim)
4. Copies the main installation image to the managed directory with progress
5. Analyzes the image with DISM through WindowsImageService
6. Registers the image in the application database
7. Safely dismounts the ISO file with multiple fallback methods

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

### Private Helper Methods

#### MountIsoAsync()
Mounts an ISO file with automatic detection of existing mounts and improved PowerShell execution.

#### DismountIsoAsync()
Dismounts an ISO file with timeout protection and multiple fallback strategies.

#### TryAlternativeDismountAsync()
Alternative dismount method using `-Force` parameter for stubborn ISOs.

#### IsIsoMountedAsync()
Checks if an ISO file is currently mounted to prevent conflicts.

#### FindWindowsImagesAsync()
Searches for Windows image files in mounted ISO with support for multiple locations.

#### CopyFileWithProgressAsync()
Copies files with detailed progress reporting and cancellation support.

## Features

### Enhanced ISO Mounting & Dismounting

- **Smart Mount Detection**: Automatically detects if ISO is already mounted
- **Improved PowerShell Execution**: Uses `-ExecutionPolicy Bypass -NoProfile` for reliability
- **Timeout Protection**: Prevents hanging on dismount operations (30s primary, 15s fallback)
- **Multiple Dismount Strategies**: Primary dismount with fallback using `-Force` parameter
- **Process Management**: Automatic process termination for unresponsive operations
- **Proper Path Escaping**: Handles file paths with special characters (apostrophes)

### Advanced Windows Image Detection

- **Multi-Location Search**: Scans both `/sources` and root directories
- **Format Support**: Handles install.wim, install.esd, and boot.wim files
- **Intelligent Prioritization**: Prefers install.wim/install.esd over boot.wim
- **Duplicate Filtering**: Removes duplicate files from search results

### Comprehensive Progress Reporting

- **Detailed Status Messages**: Step-by-step progress throughout the import process
- **File Copy Progress**: Real-time percentage and size information during copy
- **Cancellation Support**: Graceful handling of user cancellations
- **User-Friendly Messages**: Clear, actionable status updates

### Robust File Management

- **Unique Filenames**: Timestamp-based naming to prevent conflicts
- **Directory Creation**: Automatic creation of destination directories
- **Streaming Copy**: 1MB buffer for efficient large file transfers
- **Progress Integration**: File copy integrated with overall progress reporting
- **Error Recovery**: Proper cleanup on operation failures

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
   - ISO file is locked or in use by another process
   - Insufficient permissions to mount ISO (requires admin rights)
   - Target directory is read-only or access denied
   - Network paths not accessible

2. **ISO Mounting Failures**
   - PowerShell execution policy restrictions
   - No available drive letters for mounting
   - Corrupted or invalid ISO file format
   - Virtual drive services not available

3. **Disk Space Issues**
   - Insufficient space for image copy operation
   - Temporary space exhausted during processing
   - Drive full during large file operations

4. **Image Analysis Failures**
   - Invalid or corrupted WIM/ESD files within ISO
   - DISM not available or failing to execute
   - Unsupported image formats or versions
   - Missing Windows imaging components

5. **Timeout and Cancellation Issues**
   - ISO dismount operations timing out (now handled with 30s + 15s fallbacks)
   - User cancellation during long operations
   - Process hanging during PowerShell execution
   - Network timeouts for remote files

### Enhanced Error Handling Features

- **Timeout Protection**: Automatic timeout handling for dismount operations
- **Multiple Fallback Strategies**: Primary and alternative dismount methods
- **Process Management**: Automatic termination of unresponsive processes
- **Graceful Cancellation**: Proper cleanup on user cancellation
- **Detailed Logging**: Comprehensive error logging for troubleshooting

### Recommended Error Handling

```csharp
try
{
    // Check available disk space first
    var driveInfo = new DriveInfo(Path.GetPathRoot(Constants.ImportedWIMsDirectoryPath));
    var estimatedSize = new FileInfo(isoFile.Path).Length; // Rough estimate
    if (driveInfo.AvailableSpace < estimatedSize * 2) // Need 2x for safety
    {
        throw new InvalidOperationException("Insufficient disk space for import operation");
    }

    var imageInfo = await isoImportService.ImportFromIsoAsync(
        isoFile, customName, progress, cancellationToken);

    Logger.Information("Successfully imported {ImageName} with {IndexCount} indices",
        imageInfo.Name, imageInfo.IndexCount);
}
catch (OperationCanceledException)
{
    // User cancelled - normal flow, cleanup handled automatically
    Logger.Information("Import cancelled by user");
    StatusMessage = "Import operation cancelled";
}
catch (UnauthorizedAccessException ex)
{
    // Permission issues - common with ISO mounting
    Logger.Error(ex, "Permission denied during ISO import");
    await ShowErrorDialog("Administrator Rights Required",
        "ISO mounting requires administrator privileges. Please run the application as administrator.");
}
catch (FileNotFoundException ex)
{
    // Missing dependencies or files
    Logger.Error(ex, "Required component not found");
    await ShowErrorDialog("Missing Component",
        $"Required component not found: {ex.Message}\n\nPlease ensure PowerShell and DISM are available.");
}
catch (InvalidOperationException ex)
{
    // Business logic errors (disk space, invalid files, etc.)
    Logger.Error(ex, "Import operation failed");
    await ShowErrorDialog("Import Failed",
        $"The import operation could not be completed:\n\n{ex.Message}");
}
catch (TimeoutException ex)
{
    // Timeout errors (handled automatically now, but may still occur)
    Logger.Error(ex, "Operation timed out");
    await ShowErrorDialog("Operation Timeout",
        "The import operation timed out. This may indicate system issues or very large files.");
}
catch (Exception ex)
{
    // Unexpected errors
    Logger.Error(ex, "Unexpected error during ISO import");
    await ShowErrorDialog("Unexpected Error",
        $"An unexpected error occurred during import. Please check the logs for details.\n\nError: {ex.Message}");
}
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
