# EditIndexDialog Class Documentation

## Overview

Dialog component for editing Windows image index properties (name and description). Provides a user-friendly interface for modifying index metadata that will be written both to the WIM file and the application's metadata storage.

## Location

- **File**: `src/Views/Dialogs/EditIndexDialog.xaml.cs`
- **XAML**: `src/Views/Dialogs/EditIndexDialog.xaml`
- **Namespace**: `Bucket.Views.Dialogs`

## Class Definition

```csharp
public sealed partial class EditIndexDialog : ContentDialog
```

## Properties

### Display Properties (Read-Only)

- **`Index`** (`int`): The index number (for display purposes)
- **`Architecture`** (`string`): The Windows architecture (x86, x64, ARM64)

### Editable Properties

- **`IndexName`** (`string`): The display name of the Windows edition
- **`IndexDescription`** (`string`): The detailed description of the Windows edition

## Methods

### Constructor

```csharp
public EditIndexDialog(WindowsImageIndex imageIndex)
```

Initializes the dialog with existing index information.

**Parameters:**

- `imageIndex`: The Windows image index to edit

**Behavior:**

- Copies current values from the image index
- Sets up validation event handlers
- Logs dialog creation

### Core Methods

#### `UpdateImageIndex(WindowsImageIndex imageIndex)`

```csharp
public void UpdateImageIndex(WindowsImageIndex imageIndex)
```

Updates the provided image index with the edited values.

**Parameters:**

- `imageIndex`: The image index object to update

**Behavior:**

- Trims whitespace from input values
- Updates the index name and description
- Logs the changes made

#### `ShowValidationError(string title, string message)`

```csharp
private async void ShowValidationError(string title, string message)
```

Displays validation errors to the user in a modal dialog.

## Event Handlers

### `EditIndexDialog_PrimaryButtonClick`

Validates user input before allowing dialog closure:

**Validation Rules:**

1. **Name Required**: Ensures the index name is not empty or whitespace
2. **Length Limits**: Enforces maximum length constraints (UI-defined)

**Error Handling:**

- Shows user-friendly error messages
- Cancels dialog closure on validation failure
- Provides specific guidance for correction

## Usage Examples

### Basic Usage

```csharp
var dialog = new EditIndexDialog(imageIndex);

// Get XamlRoot from main window
if (App.MainWindow?.Content is FrameworkElement element)
{
    dialog.XamlRoot = element.XamlRoot;
}

var result = await dialog.ShowAsync();

if (result == ContentDialogResult.Primary)
{
    dialog.UpdateImageIndex(imageIndex);

    // Continue with WIM file updates and metadata persistence
    // This is typically handled by the calling ViewModel
}
```

### Integration with ImageDetailsViewModel

```csharp
// In ImageDetailsViewModel.EditIndex method
private async Task EditIndex(WindowsImageIndex imageIndex)
{
    var dialog = new EditIndexDialog(imageIndex);
    dialog.XamlRoot = GetXamlRoot();

    var result = await dialog.ShowAsync();

    if (result == ContentDialogResult.Primary)
    {
        // Update the in-memory model
        dialog.UpdateImageIndex(imageIndex);

        // Update the original object in collection
        var originalIndex = ImageInfo.Indices?.FirstOrDefault(i => i.Index == imageIndex.Index);
        if (originalIndex != null)
        {
            originalIndex.Name = imageIndex.Name;
            originalIndex.Description = imageIndex.Description;
        }

        // Proceed with WIM file and metadata updates
        // (Handled by specialized services)
    }
}
```

## Features

### Validation Features

- **Required Field Validation**: Ensures name is provided
- **Real-time Input**: Updates as user types
- **Error Prevention**: Prevents submission of invalid data

### Data Binding Features

- **Two-Way Binding**: Properties automatically sync with UI
- **Property Notification**: Updates reflected immediately
- **Trim Handling**: Automatically cleans whitespace

### User Experience Features

- **Clear Layout**: Shows index information prominently
- **Helpful Placeholders**: Guides user input
- **Multiline Description**: Supports detailed descriptions
- **Character Limits**: Prevents excessively long entries

## Dependencies

### Required Services

- **Logger**: For operation logging

### Model Dependencies

- [`WindowsImageIndex`](../../Models/WindowsImageIndex.md): Core index data model

### Framework Dependencies

- **Microsoft.UI.Xaml.Controls**: ContentDialog base class

## Related Files

- [`RenameImageDialog.md`](./RenameImageDialog.md): Similar dialog for image renaming
- [`ImageDetailsViewModel.md`](../../ViewModels/ImageDetailsViewModel.md): Primary consumer of this dialog
- [`WindowsImageIndex.md`](../../Models/WindowsImageIndex.md): Data model being modified
- [`WindowsImageIndexEditingService.md`](../../Services/WindowsImage/WindowsImageIndexEditingService.md): Service that persists changes to WIM files

## Best Practices

### When to Use

- When users need to customize Windows edition names
- When providing meaningful descriptions for deployment scenarios
- When organizing multiple editions within a single WIM file

### Implementation Guidelines

- Always validate required fields
- Provide clear feedback for validation errors
- Keep descriptions concise but informative
- Use consistent naming conventions

### Data Flow

1. **Dialog Creation**: Initialize with current index values
2. **User Input**: Collect name and description changes
3. **Validation**: Ensure data quality before submission
4. **Model Update**: Apply changes to in-memory objects
5. **Service Integration**: Let services handle persistence

## Error Handling

### Validation Errors

- **Empty Name**: Clear message about required field
- **Invalid Characters**: Guidance on acceptable characters
- **Length Limits**: Information about maximum allowed length

### System Errors

- The dialog focuses on UI validation only
- Persistence errors are handled by calling services
- Network/file system errors are managed at service level

## Security Considerations

### Input Validation

- Sanitizes input for safe storage
- Prevents injection of problematic characters
- Enforces reasonable length limits

### Data Integrity

- Maintains referential integrity with index numbers
- Preserves read-only properties
- Validates against data model constraints

## Performance Notes

### UI Responsiveness

- Validation is fast and non-blocking
- Property binding is efficient
- Dialog creation is lightweight

### Memory Usage

- Minimal memory footprint
- Efficient string handling
- No large object allocations

---

**Note**: This documentation was generated automatically by AI and reflects the current implementation. Please verify against the actual source code and report any discrepancies.
