# SelectMountDialog Class Documentation

## Overview
A dialog for selecting which mounted image to unmount when multiple images are mounted from the same WIM file.

## Location
- **File**: `src/Views/Dialogs/SelectMountDialog.xaml` and `src/Views/Dialogs/SelectMountDialog.xaml.cs`
- **Namespace**: `Bucket.Views.Dialogs`

## Class Definition
```csharp
public sealed partial class SelectMountDialog : ContentDialog
```

## Properties

### SelectedMount
```csharp
public MountedImageInfo SelectedMount { get; private set; }
```
Gets the selected mounted image, or null if none was selected.

## Constructor

### SelectMountDialog(IEnumerable<MountedImageInfo>)
```csharp
public SelectMountDialog(IEnumerable<MountedImageInfo> mountedImages)
```
Initializes a new instance of the SelectMountDialog with the list of mounted images to choose from.

**Parameters:**
- `mountedImages`: The list of mounted images to choose from

## Usage Examples

### Basic Usage
```csharp
// When multiple images are mounted
var mountedImages = await _mountService.GetMountedImagesAsync();
var selectDialog = new SelectMountDialog(mountedImages);
var result = await selectDialog.ShowAsync();

if (result == ContentDialogResult.Primary && selectDialog.SelectedMount != null)
{
    var selectedMount = selectDialog.SelectedMount;
    // Proceed with unmounting the selected image
    await _unmountService.UnmountImageAsync(selectedMount, true, progress, cancellationToken);
}
```

### Integration with ImageDetailsViewModel
```csharp
// In UnmountImageAsync method
if (mountedImages.Count > 1)
{
    var selectDialog = new Views.Dialogs.SelectMountDialog(mountedImages);
    var result = await selectDialog.ShowAsync();
    
    if (result != ContentDialogResult.Primary || selectDialog.SelectedMount == null)
    {
        return; // User cancelled or no selection
    }
    
    selectedMount = selectDialog.SelectedMount;
}
```

## Features

### Visual Information Display
- **Index Number**: Shows the WIM index number for easy identification
- **Edition Name**: Displays the Windows edition name (e.g., "Windows 11 Pro")
- **Mount Path**: Shows the full mount directory path in monospace font

### User Interaction
- **Single Selection**: ListView with single selection mode
- **Default Selection**: First item is selected by default
- **Primary Button Control**: Enabled only when an item is selected
- **Cancel Support**: Secondary button allows cancellation

### Data Binding
- Uses `x:Bind` for efficient data binding to `MountedImageInfo` properties
- Responsive layout with proper text wrapping for long paths

## UI Structure

### Layout
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>    <!-- Instructions text -->
        <RowDefinition Height="*"/>       <!-- Mount list -->
    </Grid.RowDefinitions>
</Grid>
```

### ListView Template
Each mounted image is displayed with:
- **Index**: Bold text showing the WIM index
- **Edition**: The Windows edition name
- **Mount Path**: Monospace font path for easy reading

## Dependencies
- **Models**: `Bucket.Models.MountedImageInfo`
- **UI Framework**: WinUI 3 ContentDialog
- **Data Binding**: Uses x:Bind for performance

## Related Files
- [`MountedImageInfo.md`](../../Models/MountedImageInfo.md) - Data model for mounted images
- [`ImageDetailsViewModel.md`](../../ViewModels/ImageDetailsViewModel.md) - Main consumer of this dialog
- [`IWindowsImageUnmountService.md`](../../Services/WindowsImage/IWindowsImageUnmountService.md) - Unmount service interface

## Best Practices

### Error Handling
- Always check if `SelectedMount` is not null after dialog result
- Handle `ContentDialogResult.Secondary` (Cancel) appropriately
- Log user selections for debugging purposes

### User Experience
- Pre-select the first item for faster interaction
- Show clear identification information (Index + Edition)
- Use consistent styling with other application dialogs

### Performance
- Use `x:Bind` instead of `Binding` for better performance
- Pass `IEnumerable<MountedImageInfo>` to avoid unnecessary collection copies
- Convert to List only once in constructor

## Integration Points

### Called From
- `ImageDetailsViewModel.UnmountImageAsync()` - When multiple mounts exist
- Future implementations that need mount selection

### Returns To
- Selected `MountedImageInfo` object for unmounting
- Null if user cancels or no selection made

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 