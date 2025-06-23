# WindowsImageService Class Documentation

## Overview

The `WindowsImageService` class is a comprehensive service for managing Windows image files (WIM/ESD) and their metadata in the Bucket application. It provides functionality to analyze, import, store, and manage Windows images using PowerShell's `Get-WindowsImage` cmdlet for reliable image analysis.

**Recent Update**: The service has been significantly improved to use PowerShell's `Get-WindowsImage` cmdlet instead of DISM text parsing, providing much more reliable image analysis and index detection.

## Location

- **File**: `src/Services/WindowsImageService.cs`
- **Namespace**: `Bucket.Services`

## Class Definition

```csharp
public class WindowsImageService
```

## Key Improvements

### PowerShell Integration
- **Reliable Analysis**: Uses `Get-WindowsImage` cmdlet for structured data extraction
- **JSON Output**: Processes structured JSON instead of parsing text output
- **Better Error Handling**: Improved detection of privilege and module requirements
- **Architecture Mapping**: Automatic conversion of numeric architecture codes to readable names

## Methods

### Core Operations

#### `GetImagesAsync(CancellationToken cancellationToken = default)`
- **Returns**: `Task<ObservableCollection<WindowsImageInfo>>`
- **Purpose**: Retrieves all available Windows images from metadata storage
- **Features**: Validates file existence, cleans up missing files

#### `SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)`
- **Purpose**: Persists Windows images metadata to JSON storage
- **Features**: Atomic save operation with error handling

#### `AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)`
- **Returns**: `Task<List<WindowsImageIndex>>`
- **Purpose**: **IMPROVED** - Analyzes WIM/ESD files using PowerShell `Get-WindowsImage` cmdlet
- **Features**:
  - PowerShell JSON output parsing
  - Structured data extraction
  - Better error diagnostics
  - Progress reporting
  - Architecture code mapping (0→x86, 9→x64, 12→ARM64, etc.)

#### `ImportImageAsync(string imagePath, string name, string sourceIsoPath = "", IProgress<string> progress = null, CancellationToken cancellationToken = default)`
- **Returns**: `Task<WindowsImageInfo>`
- **Purpose**: Imports a new Windows image and adds it to the collection
- **Features**: Automatic analysis, metadata creation, persistence

#### `DeleteImageAsync(WindowsImageInfo imageInfo, bool deleteFromDisk = false, CancellationToken cancellationToken = default)`
- **Purpose**: Removes an image from the collection and optionally from disk
- **Features**: Safe deletion, metadata cleanup

## Usage Examples

### Basic Service Usage

```csharp
var service = new WindowsImageService();

// Load existing images
var images = await service.GetImagesAsync();

// Import a new image
var progress = new Progress<string>(status => Console.WriteLine(status));
var newImage = await service.ImportImageAsync(
    @"C:\Images\windows11.wim",
    "Windows 11 22H2",
    @"C:\ISOs\windows11.iso",
    progress);

// Analyze an image file
var indices = await service.AnalyzeImageAsync(@"C:\Images\test.wim", progress);
```

### Advanced Operations

```csharp
// Import with custom error handling
try
{
    var image = await service.ImportImageAsync(filePath, name);
    Logger.Information("Successfully imported {Name}", image.Name);
}
catch (FileNotFoundException)
{
    Logger.Error("Image file not found: {Path}", filePath);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("DISM failed"))
{
    Logger.Error("DISM analysis failed: {Error}", ex.Message);
}

// Delete with confirmation
if (await ConfirmDeletion(image))
{
    await service.DeleteImageAsync(image, deleteFromDisk: true);
}
```

## Features

### DISM Integration
- **Automatic Detection**: Locates and executes DISM.exe for image analysis
- **Output Parsing**: Sophisticated parsing of DISM command output
- **Error Handling**: Comprehensive DISM error detection and reporting

### Progress Reporting
- **Real-time Updates**: Provides status updates during long operations
- **Cancellation Support**: Respects cancellation tokens for user control
- **Resource Management**: Proper cleanup of processes and resources

### Data Persistence
- **JSON Storage**: Human-readable metadata storage format
- **Atomic Operations**: Safe save operations with rollback on failure
- **Validation**: Automatic cleanup of orphaned metadata

### File Management
- **Path Validation**: Comprehensive file path and existence checking
- **Size Calculation**: Automatic file size detection and formatting
- **Metadata Extraction**: File system metadata integration

## Dependencies

- **System.Collections.ObjectModel**: Collection support
- **System.Diagnostics**: Process execution for DISM
- **System.Text.Json**: Metadata serialization
- **Bucket.Models**: Domain models
- **Global Logger**: Structured logging throughout

## Related Files

- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Primary domain model
- [`WindowsImageIndex.md`](../Models/WindowsImageIndex.md) - Index information model
- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - Consumer ViewModel

## Best Practices

### Error Handling

```csharp
try
{
    var images = await service.GetImagesAsync();
}
catch (JsonException ex)
{
    Logger.Error(ex, "Corrupted metadata file");
    // Handle corrupted metadata
}
catch (UnauthorizedAccessException ex)
{
    Logger.Error(ex, "Insufficient permissions");
    // Handle permission issues
}
```

### Resource Management

```csharp
// Always use cancellation tokens for long operations
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
var result = await service.AnalyzeImageAsync(path, progress, cts.Token);
```

### Progress Reporting

```csharp
var progress = new Progress<string>(status =>
{
    progressBar.Text = status;
    Logger.Debug("Operation status: {Status}", status);
});
```

## Error Handling

### Common Scenarios

- **DISM Not Found**: Service requires DISM.exe in system PATH
- **Insufficient Permissions**: Administrative rights may be required
- **Corrupted Images**: Handle invalid or corrupted WIM/ESD files
- **Disk Space**: Check available space before operations
- **File Locks**: Handle files in use by other processes

### Example Error Recovery

```csharp
public async Task<WindowsImageInfo> SafeImportAsync(string path, string name)
{
    const int maxRetries = 3;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await service.ImportImageAsync(path, name);
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            Logger.Warning("Import attempt {Attempt} failed: {Error}", attempt, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
        }
    }

    throw new InvalidOperationException($"Failed to import after {maxRetries} attempts");
}
```

## Performance Considerations

- **Large Files**: WIM files can be 4+ GB - operations may take several minutes
- **DISM Overhead**: DISM process startup has overhead - consider batching operations
- **Memory Usage**: Parsing large DISM outputs requires adequate memory
- **Disk I/O**: Image analysis involves significant disk access

## Security Considerations

- **Path Validation**: All file paths are validated to prevent directory traversal
- **Process Security**: DISM execution is controlled and sandboxed
- **Permission Handling**: Graceful handling of permission-related errors
- **Input Sanitization**: All user inputs are validated before use

## Issue Resolution: Index Detection Problems

### Problem Description
Previously, users experienced issues where imported images showed "no indices found" even when indices were present. This was caused by unreliable text parsing of DISM command output.

### Root Cause
The original implementation used `dism.exe /Get-WimInfo` and parsed the text output line by line. This approach was fragile and could fail due to:
- Localization differences in DISM output
- Formatting variations across Windows versions
- Text encoding issues
- Output parsing edge cases

### Solution Implemented
**PowerShell Integration**: Replaced DISM text parsing with PowerShell's `Get-WindowsImage` cmdlet:

```csharp
// OLD: DISM text parsing (unreliable)
var processInfo = new ProcessStartInfo
{
    FileName = "dism.exe",
    Arguments = $"/Get-WimInfo /WimFile:\"{imagePath}\"",
    // ... parse text output line by line
};

// NEW: PowerShell JSON output (reliable)
var powerShellCommand = $"Get-WindowsImage -ImagePath '{escapedPath}' | ConvertTo-Json -Depth 3";
var processInfo = new ProcessStartInfo
{
    FileName = "powershell.exe",
    Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"{powerShellCommand}\"",
    // ... parse structured JSON output
};
```

### Benefits of the Fix
- **Structured Data**: JSON output provides consistent, parseable data structure
- **Language Independent**: No localization issues with PowerShell objects
- **Better Error Handling**: Clearer error messages and exception types
- **Comprehensive Data**: Access to all image properties without parsing artifacts
- **Reliability**: No more missing indices due to parsing failures

### Testing the Fix
After updating, users should see:
1. All image indices properly detected during import
2. Correct architecture information (x86, x64, ARM64, etc.)
3. Accurate size calculations
4. Proper image names and descriptions
5. Better error messages when issues occur

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
