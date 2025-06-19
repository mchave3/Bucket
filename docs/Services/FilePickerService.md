# FilePickerService Class Documentation

## Overview

A service class that provides file and folder picker functionality for WinUI 3 applications. This service handles the Windows Runtime interop required for file pickers to work properly in WinUI 3 applications, including ISO files, WIM/ESD files, and folder selection.

## Location

- **File**: `src/Services/FilePickerService.cs`
- **Namespace**: `Bucket.Services`

## Class Definition

```csharp
public class FilePickerService
```

## Methods

### PickIsoFileAsync()

```csharp
public async Task<StorageFile> PickIsoFileAsync()
```

Opens a file picker dialog specifically configured for selecting ISO files.

**Returns**: The selected ISO file, or null if the user cancels the operation.

**Example Usage**:
```csharp
var filePickerService = new FilePickerService();
var isoFile = await filePickerService.PickIsoFileAsync();
if (isoFile != null)
{
    // Process the selected ISO file
    Console.WriteLine($"Selected: {isoFile.Name}");
}
```

### PickWimFileAsync()

```csharp
public async Task<StorageFile> PickWimFileAsync()
```

Opens a file picker dialog configured for selecting WIM or ESD files.

**Returns**: The selected WIM/ESD file, or null if the user cancels the operation.

**Example Usage**:
```csharp
var filePickerService = new FilePickerService();
var wimFile = await filePickerService.PickWimFileAsync();
if (wimFile != null)
{
    // Process the selected WIM file
    Console.WriteLine($"Selected: {wimFile.Name}");
}
```

### PickFolderAsync()

```csharp
public async Task<StorageFolder> PickFolderAsync()
```

Opens a folder picker dialog for selecting destination folders.

**Returns**: The selected folder, or null if the user cancels the operation.

**Example Usage**:
```csharp
var filePickerService = new FilePickerService();
var folder = await filePickerService.PickFolderAsync();
if (folder != null)
{
    // Use the selected folder
    Console.WriteLine($"Selected folder: {folder.Name}");
}
```

## Features

### WinUI 3 Compatibility
- Properly initializes file pickers with window handles for WinUI 3
- Handles the required Windows Runtime interop operations
- Compatible with packaged and unpackaged WinUI 3 applications

### File Type Filtering
- **ISO files**: Filters to show only .iso files
- **Windows Images**: Filters to show .wim and .esd files
- **Folders**: Configured for folder selection with wildcard filter

### User Experience
- Uses appropriate starting locations (Computer folder)
- Provides clear file type filtering
- Handles user cancellation gracefully

### Logging Integration
- Comprehensive logging of all operations
- Structured logging with relevant context
- Error logging with full exception details

## Usage Examples

### Basic ISO Import Workflow
```csharp
var filePickerService = new FilePickerService();

// Pick ISO file
var isoFile = await filePickerService.PickIsoFileAsync();
if (isoFile != null)
{
    // Pick destination folder
    var destFolder = await filePickerService.PickFolderAsync();
    if (destFolder != null)
    {
        // Process the import
        await ProcessIsoImport(isoFile, destFolder);
    }
}
```

### Error Handling Pattern
```csharp
try
{
    var file = await filePickerService.PickIsoFileAsync();
    if (file != null)
    {
        // Process file
    }
    else
    {
        // User cancelled - this is normal
        Logger.Information("File selection cancelled by user");
    }
}
catch (Exception ex)
{
    // Handle unexpected errors
    Logger.Error(ex, "Failed to open file picker");
    throw;
}
```

## Dependencies

### External Dependencies
- **Windows.Storage**: For StorageFile and StorageFolder types
- **Windows.Storage.Pickers**: For file and folder picker functionality
- **WinRT.Interop**: For Windows Runtime interop operations
- **Microsoft.UI.Xaml**: For accessing the main window

### Internal Dependencies
- **App.MainWindow**: For obtaining window handle
- **LoggerSetup.Logger**: For logging operations (via global using)

## Related Files

- [`IsoImportService.md`](./IsoImportService.md) - Uses this service for file selection
- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - Integrates this service
- [`WindowsImageService.md`](./WindowsImageService.md) - Works with selected files

## Best Practices

### Service Lifecycle
- Create new instances when needed (lightweight service)
- No need for dependency injection for this simple service
- Service methods are stateless and thread-safe

### Error Handling
- Always check for null return values (user cancellation)
- Wrap calls in try-catch blocks for unexpected errors
- Log both successful selections and cancellations

### Integration Patterns
- Use with progress reporting for long operations
- Combine with validation before processing files
- Consider file access permissions in your workflow

## Error Handling

### Common Error Scenarios

1. **User Cancellation**: Returns null - not an error, handle gracefully
2. **Permission Issues**: May throw UnauthorizedAccessException
3. **Window Handle Issues**: May fail if App.MainWindow is null
4. **Platform Compatibility**: May not work on non-Windows platforms

### Recommended Error Handling
```csharp
try
{
    var file = await filePickerService.PickIsoFileAsync();
    if (file == null)
    {
        // User cancelled - normal flow
        return;
    }

    // Validate file access
    var properties = await file.GetBasicPropertiesAsync();
    if (properties.Size == 0)
    {
        throw new InvalidOperationException("Selected file is empty");
    }

    // Continue with processing
}
catch (UnauthorizedAccessException)
{
    // Handle permission issues
    await ShowErrorDialog("Access denied to the selected file");
}
catch (Exception ex)
{
    // Handle unexpected errors
    Logger.Error(ex, "Unexpected error during file selection");
    throw;
}
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
