# WindowsImageService Class Documentation (Refactored Architecture)

## Overview
**IMPORTANT**: This service has been refactored into a distributed architecture using dependency injection and specialized services. The WindowsImageService now acts as a coordinator that orchestrates multiple focused services, each with a single responsibility.

The refactored WindowsImageService provides comprehensive functionality to analyze, import, manage, and extract detailed information from WIM/ESD files, including ISO mounting and import operations, through a clean separation of concerns.

## Architecture

### New Structure
The WindowsImageService has been split into specialized services:

- **WindowsImageFileService**: File operations and path management
- **WindowsImageMetadataService**: JSON metadata persistence and management
- **WindowsImagePowerShellService**: PowerShell execution and parsing
- **WindowsImageIsoService**: ISO mounting and extraction operations
- **WindowsImageService**: Main coordinator service (this class)

### Dependency Injection
The service now uses constructor dependency injection:

```csharp
public WindowsImageService(
    IWindowsImageMetadataService metadataService,
    IWindowsImageFileService fileService,
    IWindowsImagePowerShellService powerShellService,
    IWindowsImageIsoService isoService)
```

## Location
- **File**: `src/Services/WindowsImageService.cs`
- **Namespace**: `Bucket.Services`
- **Related Services**: `src/Services/WindowsImage/` (specialized services)

## Class Definition
```csharp
public class WindowsImageService
```

## Key Features

### Service Coordination
- Orchestrates multiple specialized services
- Provides unified API for higher-level operations
- Handles cross-service validation and error management
- Maintains backward compatibility with existing code

### Image Collection Management
- Delegates to WindowsImageMetadataService for persistence
- Load and save Windows image collections
- Manage image metadata and indexing
- Automatic discovery of existing images

### Direct Import Operations
- Coordinates file operations through WindowsImageFileService
- Uses PowerShell service for image analysis
- Copy files to managed directory with progress reporting
- Generate unique names with timestamps

### ISO Import Operations
- Delegates to WindowsImageIsoService for ISO operations
- Mount/dismount ISO files automatically
- Extract Windows images from mounted ISOs
- Support for install.wim, install.esd, and boot.wim files
- Robust error handling and cleanup

### Image Analysis
- Uses WindowsImagePowerShellService for deep analysis
- PowerShell Get-WindowsImage integration
- Extract detailed metadata for each image index
- Support for multiple architectures and editions

## Core Methods

### Public Methods - Collection Management

#### `GetImagesAsync(CancellationToken cancellationToken = default)`
```csharp
public async Task<ObservableCollection<WindowsImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default)
```
**Purpose**: Gets all available Windows images asynchronously.

**Parameters**:
- `cancellationToken`: Optional cancellation token

**Returns**: Collection of Windows image information

#### `SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)`
```csharp
public async Task SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)
```
**Purpose**: Saves the Windows images metadata asynchronously.

### Public Methods - ISO Import Operations

#### `ImportFromIsoAsync(StorageFile isoFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)`
```csharp
public async Task<WindowsImageInfo> ImportFromIsoAsync(StorageFile isoFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
**Purpose**: Imports a Windows image from an ISO file with full mount/dismount cycle.

**Parameters**:
- `isoFile`: The ISO file to import from
- `customName`: Optional custom name for the imported image
- `progress`: Progress reporter for the operation
- `cancellationToken`: Cancellation token for the operation

**Returns**: The imported Windows image information

**Process**:
1. Mount the ISO file
2. Scan for Windows image files (install.wim/install.esd)
3. Copy the main image file with progress reporting
4. Analyze and import the image
5. Dismount the ISO and cleanup

#### `ImportFromWimAsync(StorageFile wimFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)`
```csharp
public async Task<WindowsImageInfo> ImportFromWimAsync(StorageFile wimFile, string customName = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
**Purpose**: Imports a Windows image directly from a WIM/ESD file.

**Parameters**:
- `wimFile`: The WIM/ESD file to import
- `customName`: Optional custom name for the imported image
- `progress`: Progress reporter for the operation
- `cancellationToken`: Cancellation token for the operation

**Returns**: The imported Windows image information

### Public Methods - Image Analysis

#### `AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)`
```csharp
public async Task<List<WindowsImageIndex>> AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
**Purpose**: Analyzes a WIM/ESD file and extracts its indices using PowerShell Get-WindowsImage.

#### `GetImageIndexDetailsAsync(string imagePath, int index, IProgress<string> progress = null, CancellationToken cancellationToken = default)`
```csharp
public async Task<WindowsImageIndex> GetImageIndexDetailsAsync(string imagePath, int index, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
**Purpose**: Gets detailed information for a specific Windows image index.

### Public Methods - Image Management

#### `ImportImageAsync(string imagePath, string name, string sourceIsoPath = "", bool copyToManagedDirectory = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)`
```csharp
public async Task<WindowsImageInfo> ImportImageAsync(string imagePath, string name, string sourceIsoPath = "", bool copyToManagedDirectory = true, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
**Purpose**: Imports a new Windows image and adds it to the collection.

#### `DeleteImageAsync(WindowsImageInfo imageInfo, bool deleteFromDisk = false, CancellationToken cancellationToken = default)`
```csharp
public async Task DeleteImageAsync(WindowsImageInfo imageInfo, bool deleteFromDisk = false, CancellationToken cancellationToken = default)
```
**Purpose**: Deletes a Windows image from the collection and optionally from disk.

## Private Utility Methods

### ISO Mount/Dismount Operations
- `MountIsoAsync()`: Mounts ISO files with drive letter detection
- `DismountIsoAsync()`: Dismounts ISO files with timeout handling
- `TryAlternativeDismountAsync()`: Fallback dismount method
- `IsIsoMountedAsync()`: Checks if ISO is already mounted
- `FindWindowsImagesAsync()`: Searches for Windows image files in mounted ISO

### File Operations
- `CopyFileWithProgressAsync()`: Chunked file copy with progress reporting
- `FormatBytes()`: Human-readable byte formatting

### PowerShell Integration
- `ParsePowerShellOutput()`: Parses JSON output from Get-WindowsImage
- `ParseImageElement()`: Parses individual image elements from JSON

## Usage Examples

### Import from ISO
```csharp
var imageService = new WindowsImageService();
var progress = new Progress<string>(message => Console.WriteLine(message));

try
{
    var result = await imageService.ImportFromIsoAsync(
        isoFile,
        customName: "Windows 11 Pro",
        progress: progress,
        cancellationToken: cancellationToken);

    Console.WriteLine($"Imported: {result.Name} with {result.IndexCount} editions");
}
catch (Exception ex)
{
    Console.WriteLine($"Import failed: {ex.Message}");
}
```

### Import from WIM/ESD
```csharp
var imageService = new WindowsImageService();
var progress = new Progress<string>(message => Console.WriteLine(message));

try
{
    var result = await imageService.ImportFromWimAsync(
        wimFile,
        customName: "Custom Image",
        progress: progress,
        cancellationToken: cancellationToken);

    Console.WriteLine($"Imported: {result.Name}");
}
catch (Exception ex)
{
    Console.WriteLine($"Import failed: {ex.Message}");
}
```

### Analyze Image
```csharp
var imageService = new WindowsImageService();
var progress = new Progress<string>(message => Console.WriteLine(message));

try
{
    var indices = await imageService.AnalyzeImageAsync(
        @"C:\Images\install.wim",
        progress,
        cancellationToken);

    foreach (var index in indices)
    {
        Console.WriteLine($"Index {index.Index}: {index.Name} ({index.Architecture})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Analysis failed: {ex.Message}");
}
```

## Features

### Robust ISO Handling
- **Automatic Mount Detection**: Checks if ISO is already mounted before mounting
- **Drive Letter Resolution**: Automatically detects mount drive letters
- **Cleanup Guarantee**: Ensures ISOs are always dismounted, even on errors
- **Alternative Dismount**: Fallback methods if primary dismount fails
- **Timeout Protection**: Prevents hanging on dismount operations

### Progress Reporting
- **Detailed Progress**: Real-time progress updates for all long-running operations
- **File Copy Progress**: Chunked copying with percentage and byte count
- **Operation Steps**: Clear indication of current operation stage
- **Cancellation Support**: Graceful cancellation handling throughout

### Error Handling
- **Disk Space Validation**: Checks available space before operations
- **File Format Validation**: Ensures only valid WIM/ESD/ISO files are processed
- **Path Validation**: Prevents directory traversal attacks
- **Comprehensive Logging**: Structured logging for all operations
- **Graceful Degradation**: Continues operation even if non-critical steps fail

### Performance Optimization
- **Chunked File Copy**: 1MB buffer for efficient large file copying
- **Memory Management**: Careful handling of large WIM files
- **Concurrent Operations**: Proper async/await patterns throughout
- **Resource Cleanup**: Automatic disposal of streams and processes

## Dependencies

### External Dependencies
- **Windows.Storage**: For StorageFile operations
- **System.Diagnostics**: For PowerShell process execution
- **System.Text.Json**: For parsing PowerShell JSON output

### Internal Dependencies
- **Bucket.Models**: WindowsImageInfo, WindowsImageIndex models
- **Bucket.Common**: Constants, logging utilities
- **LoggerSetup**: Global logging infrastructure

## Related Files
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md): Image data model
- [`WindowsImageIndex.md`](../Models/WindowsImageIndex.md): Image index data model
- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md): Primary consumer
- [`Constants.md`](../Common/Constants.md): Configuration constants
- [`LoggerSetup.md`](../Common/LoggerSetup.md): Logging infrastructure

## Best Practices

### Service Usage
1. **Single Instance**: Use dependency injection for singleton pattern
2. **Progress Reporting**: Always provide progress reporting for UI operations
3. **Cancellation**: Implement cancellation tokens for long-running operations
4. **Error Handling**: Wrap service calls in try-catch blocks
5. **Resource Management**: Service handles all resource cleanup automatically

### Performance Considerations
1. **Large Files**: Service is optimized for multi-gigabyte WIM files
2. **Disk Space**: Always ensure sufficient disk space (2x file size recommended)
3. **Memory Usage**: Chunked operations prevent memory exhaustion
4. **Network Drives**: Avoid importing from slow network locations

### Security Considerations
1. **Administrator Rights**: ISO mounting requires elevated privileges
2. **Path Validation**: All file paths are validated against traversal attacks
3. **Temporary Files**: Automatic cleanup of temporary mount points
4. **File Permissions**: Validates file access before operations

## Error Handling

### Common Error Scenarios
- **Insufficient Disk Space**: Validates before operations
- **Invalid File Formats**: Checks file extensions and headers
- **Access Denied**: Handles permission errors gracefully
- **ISO Mount Failures**: Multiple fallback strategies
- **PowerShell Errors**: Robust parsing with error recovery
- **Network Issues**: Timeout handling for remote files

### Exception Types
- `InvalidOperationException`: Invalid file formats or states
- `UnauthorizedAccessException`: Permission issues
- `DirectoryNotFoundException`: Missing directories
- `OperationCanceledException`: User cancellation
- `FileNotFoundException`: Missing source files

## Performance Notes

### Optimization Features
- **Streaming Operations**: No unnecessary file loading into memory
- **Parallel Processing**: Where safe, operations run concurrently
- **Efficient Copying**: 1MB buffers for optimal throughput
- **Smart Caching**: Avoids re-analyzing unchanged files
- **Resource Pooling**: Reuses PowerShell processes where possible

### Recommended Usage Patterns
```csharp
// Good: Use using statements for automatic disposal
using var service = new WindowsImageService();

// Good: Always provide cancellation tokens
var result = await service.ImportFromIsoAsync(file, "", progress, cancellationToken);

// Good: Handle specific exceptions
try
{
    await service.ImportFromIsoAsync(file);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("disk space"))
{
    // Handle disk space specifically
}
```

---

**Note**: This documentation was generated automatically by AI and reflects the current state of the WindowsImageService class after the migration from IsoImportService. Please verify the information against the actual source code and report any discrepancies.
