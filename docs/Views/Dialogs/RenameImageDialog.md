# RenameImageDialog Class Documentation

## Overview
Dialog component for renaming Windows image files. Provides user interface for changing both the display name and physical file name of Windows image files, with comprehensive validation and error handling.

## Location
- **File**: `src/Views/Dialogs/RenameImageDialog.xaml.cs`
- **XAML**: `src/Views/Dialogs/RenameImageDialog.xaml`
- **Namespace**: `Bucket.Views.Dialogs`

## Class Definition
```csharp
public sealed partial class RenameImageDialog : ContentDialog
```

## Properties

### Display Properties (Read-Only)
- **`FileName`** (`string`): The current file name for display purposes
- **`ImageType`** (`string`): The image type display text (e.g., "WIM File")

### Editable Properties
- **`ImageName`** (`string`): The new name for the image (bound to UI input)

## Methods

### Constructor
```csharp
public RenameImageDialog(WindowsImageInfo imageInfo)
```
Initializes the dialog with existing image information.

**Parameters:**
- `imageInfo`: The Windows image information to rename

**Behavior:**
- Copies current values from the image info
- Sets up event handlers for validation
- Logs dialog creation

### Core Methods

#### `UpdateImageInfo(WindowsImageInfo imageInfo)`
```csharp
public bool UpdateImageInfo(WindowsImageInfo imageInfo)
```
Updates the image information and renames the physical file.

**Parameters:**
- `imageInfo`: The image info object to update

**Returns:**
- `true` if the operation was successful
- `false` if the operation failed

**Behavior:**
- Validates the new name
- Calculates new file path
- Checks for file conflicts
- Renames physical file if needed
- Updates the image info object
- Comprehensive logging of all operations

#### `ShowValidationError(string title, string message)`
```csharp
private async void ShowValidationError(string title, string message)
```
Displays validation errors to the user in a modal dialog.

## Event Handlers

### `RenameImageDialog_PrimaryButtonClick`
Validates user input before allowing dialog closure:

**Validation Rules:**
1. **Name Required**: Ensures the name is not empty or whitespace
2. **Invalid Characters**: Checks for filesystem-invalid characters
3. **Character Set**: Uses `Path.GetInvalidFileNameChars()` for validation

**Error Handling:**
- Shows user-friendly error messages
- Cancels dialog closure on validation failure
- Provides specific guidance for each error type

## Usage Examples

### Basic Usage
```csharp
var dialog = new RenameImageDialog(imageInfo);

// Get XamlRoot from main window
if (App.MainWindow?.Content is FrameworkElement element)
{
    dialog.XamlRoot = element.XamlRoot;
}

var result = await dialog.ShowAsync();

if (result == ContentDialogResult.Primary)
{
    var success = dialog.UpdateImageInfo(imageInfo);
    if (success)
    {
        // Handle successful rename
        await _metadataService.UpdateImageAsync(imageInfo);
    }
    else
    {
        // Handle rename failure
    }
}
```

### Integration with ViewModels
```csharp
// In ImageDetailsViewModel
private async Task RenameImageAsync()
{
    try
    {
        var dialog = new RenameImageDialog(ImageInfo);
        dialog.XamlRoot = GetXamlRoot();

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var success = dialog.UpdateImageInfo(ImageInfo);

            if (success)
            {
                await _metadataService.UpdateImageAsync(ImageInfo);
                OnPropertyChanged(nameof(ImageInfo));
            }
            else
            {
                await ShowErrorDialogAsync("Rename Failed",
                    "Failed to rename the image. The target file may already exist or be in use.");
            }
        }
    }
    catch (Exception ex)
    {
        Logger.Error(ex, "Failed to rename image: {Name}", ImageInfo.Name);
        await ShowErrorDialogAsync("Rename Error", $"Failed to rename image: {ex.Message}");
    }
}
```

## Features

### Validation Features
- **Real-time Validation**: Validates on dialog submission
- **Comprehensive Error Messages**: User-friendly error descriptions
- **File System Compatibility**: Ensures valid file names
- **Conflict Detection**: Checks for existing files

### File Management Features
- **Physical File Renaming**: Actually renames the file on disk
- **Metadata Updates**: Updates both display name and file path
- **Conflict Resolution**: Handles existing file scenarios
- **Rollback Safety**: Maintains data integrity on failures

### User Experience Features
- **Clear Interface**: Shows current file information
- **Helpful Instructions**: Explains what the rename operation does
- **Error Guidance**: Provides specific correction guidance
- **Cancellation Support**: Allows users to cancel without changes

## Dependencies

### Required Services
- **Logger**: For comprehensive operation logging
- **File System**: For physical file operations

### Model Dependencies
- [`WindowsImageInfo`](../../Models/WindowsImageInfo.md): Core image data model

### Framework Dependencies
- **Microsoft.UI.Xaml.Controls**: ContentDialog base class
- **System.IO**: File system operations

## Related Files
- [`EditIndexDialog.md`](./EditIndexDialog.md): Similar dialog for editing index metadata
- [`ImageDetailsViewModel.md`](../../ViewModels/ImageDetailsViewModel.md): Primary consumer of this dialog
- [`WindowsImageInfo.md`](../../Models/WindowsImageInfo.md): Data model being modified

## Best Practices

### When to Use
- When users need to change the display name of an image
- When organizing image collections with meaningful names
- When standardizing naming conventions

### Implementation Guidelines
- Always validate user input before processing
- Handle file system exceptions gracefully
- Update both metadata and physical files consistently
- Provide clear feedback for all operations

### Error Handling
- **File In Use**: Handle scenarios where files are locked
- **Permission Denied**: Handle access rights issues
- **Disk Space**: Handle insufficient storage scenarios
- **Invalid Names**: Provide clear character restrictions

## Security Considerations

### File System Security
- Validates file names to prevent directory traversal
- Checks file permissions before operations
- Handles locked files gracefully

### Input Validation
- Sanitizes user input for file system compatibility
- Prevents injection of invalid characters
- Ensures reasonable name length limits

## Performance Notes

### File Operations
- File operations are performed synchronously within the dialog
- Consider async operations for large files
- Minimal memory footprint for name validation

### UI Responsiveness
- Validation is fast and non-blocking
- Error dialogs are properly async
- File operations complete quickly for typical scenarios

---

**Note**: This documentation was generated automatically by AI and reflects the current implementation. Please verify against the actual source code and report any discrepancies.
