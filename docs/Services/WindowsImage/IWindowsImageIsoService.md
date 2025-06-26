# IWindowsImageIsoService Interface Documentation

## Overview

Interface for Windows image ISO operations including mounting, dismounting, and import operations. This service handles all ISO-related functionality for Windows image management, allowing users to work with Windows installation media and extract image files.

## Location

- **File**: `src/Services/WindowsImage/IWindowsImageIsoService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition

```csharp
public interface IWindowsImageIsoService
```

## Methods

### ImportFromIsoAsync

```csharp
Task<WindowsImageInfo> ImportFromIsoAsync(StorageFile isoFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
```

Imports a Windows image from an ISO file by mounting the ISO and extracting the contained WIM/ESD files.

**Parameters:**
- `isoFile`: The ISO file to import from
- `customName`: Optional custom name for the imported image
- `progress`: Optional progress reporter for tracking import operations
- `cancellationToken`: Optional cancellation token for async operation

**Returns:** The imported Windows image information

### ImportFromWimAsync

```csharp
Task<WindowsImageInfo> ImportFromWimAsync(StorageFile wimFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
```

Imports a Windows image directly from a WIM/ESD file without ISO mounting.

**Parameters:**
- `wimFile`: The WIM/ESD file to import
- `customName`: Optional custom name for the imported image
- `progress`: Optional progress reporter for tracking import operations
- `cancellationToken`: Optional cancellation token for async operation

**Returns:** The imported Windows image information

### MountIsoAsync

```csharp
Task<string> MountIsoAsync(string isoPath, CancellationToken cancellationToken)
```

Mounts an ISO file and returns the mount path for accessing the contents.

**Parameters:**
- `isoPath`: The path to the ISO file
- `cancellationToken`: Cancellation token for the operation

**Returns:** The mount path of the ISO

### DismountIsoAsync

```csharp
Task DismountIsoAsync(string isoPath, CancellationToken cancellationToken)
```

Dismounts an ISO file and cleans up the mount point.

**Parameters:**
- `isoPath`: The original ISO file path
- `cancellationToken`: Cancellation token for the operation

### IsIsoMountedAsync

```csharp
Task<bool> IsIsoMountedAsync(string isoPath, CancellationToken cancellationToken)
```

Checks if an ISO file is currently mounted.

**Parameters:**
- `isoPath`: The path to the ISO file
- `cancellationToken`: Cancellation token for the operation

**Returns:** True if the ISO is mounted, false otherwise

## Usage Examples

```csharp
// Injecting the service
public class WindowsImageImportService
{
    private readonly IWindowsImageIsoService _isoService;

    public WindowsImageImportService(IWindowsImageIsoService isoService)
    {
        _isoService = isoService;
    }

    // Import from ISO file
    public async Task<WindowsImageInfo> ImportWindowsImageFromIsoAsync(StorageFile isoFile, string customName = null)
    {
        var progress = new Progress<string>(message => Logger.Information(message));

        try
        {
            return await _isoService.ImportFromIsoAsync(isoFile, customName, progress);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to import image from ISO {IsoPath}", isoFile.Path);
            throw;
        }
    }

    // Import directly from WIM file
    public async Task<WindowsImageInfo> ImportWindowsImageFromWimAsync(StorageFile wimFile, string customName = null)
    {
        var progress = new Progress<string>(message => Logger.Information(message));
        return await _isoService.ImportFromWimAsync(wimFile, customName, progress);
    }

    // Mount and work with ISO
    public async Task<string> MountIsoForAnalysisAsync(string isoPath)
    {
        if (await _isoService.IsIsoMountedAsync(isoPath, CancellationToken.None))
        {
            Logger.Information("ISO is already mounted");
        }

        return await _isoService.MountIsoAsync(isoPath, CancellationToken.None);
    }

    // Clean up ISO mount
    public async Task CleanupIsoMountAsync(string isoPath)
    {
        if (await _isoService.IsIsoMountedAsync(isoPath, CancellationToken.None))
        {
            await _isoService.DismountIsoAsync(isoPath, CancellationToken.None);
        }
    }
}
```

## Features

- **ISO Mounting**: Mount and dismount ISO files for content access
- **Import Operations**: Import Windows images from ISO or WIM/ESD files
- **Progress Reporting**: Track progress of potentially long import operations
- **Mount State Management**: Check and manage ISO mount states
- **Async Operations**: All operations are asynchronous to prevent UI blocking
- **Storage File Integration**: Works with WinUI StorageFile objects

## Dependencies

- **Bucket.Models**: WindowsImageInfo model
- **Windows.Storage**: StorageFile for file operations
- **System.Threading**: Cancellation token support
- **System.Threading.Tasks**: Async operations support

## Implementation

This interface is implemented by [`WindowsImageIsoService`](./WindowsImageIsoService.md).

## Related Files

- [`WindowsImageIsoService.md`](./WindowsImageIsoService.md) - Implementation
- [`WindowsImageInfo.md`](../../Models/WindowsImageInfo.md) - Data model
- [`WindowsImageService.md`](../WindowsImageService.md) - Main coordinator service
- [`IWindowsImageFileService.md`](./IWindowsImageFileService.md) - File operations

## Best Practices

- Always dismount ISOs after use to free system resources
- Check mount status before attempting to mount or dismount
- Use progress reporting for import operations (can be slow for large files)
- Handle cancellation tokens appropriately for long operations
- Validate ISO files before attempting mount operations

## Error Handling

- ISO file not found or access denied
- Mount/dismount operation failures
- Invalid or corrupted ISO files
- Insufficient disk space for import operations
- Cancellation during long-running operations
- System-level mounting errors

## Security Considerations

- **File Path Validation**: Validate ISO and WIM file paths before operations
- **Mount Point Security**: Ensure secure handling of temporary mount points
- **Access Control**: Verify appropriate permissions for mounting operations
- **Resource Cleanup**: Always clean up mounted resources to prevent resource leaks

## Performance Notes

- ISO mounting can be slow for large files
- Import operations may take significant time for large images
- Consider implementing timeouts for mount operations
- Use progress reporting to keep users informed of long operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
