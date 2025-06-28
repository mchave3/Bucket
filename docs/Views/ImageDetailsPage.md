# ImageDetailsPage Class Documentation

## Overview
Page that displays detailed information about a Windows image, including its metadata, available editions (indices), and provides operations for mounting, unmounting, and managing the image. Features a modern single-selection interface for Windows editions and granular unmount control.

## Location
- **File**: `src/Views/ImageDetailsPage.xaml` and `src/Views/ImageDetailsPage.xaml.cs`
- **Namespace**: `Bucket.Views`

## Class Definition
```csharp
public sealed partial class ImageDetailsPage : Page
```

## Page Structure

### Header Section
- **Image Title**: Displays the Windows image name
- **Image Type**: Shows the image type (WIM, ESD, etc.)
- **Rename Button**: Allows renaming the image file

### Image Information Card
Displays comprehensive metadata about the Windows image:
- **File Path**: Full path to the image file (selectable text)
- **File Size**: Formatted file size display
- **Created**: Image creation date
- **Modified**: Last modification date
- **Image Type**: Technical image type
- **Source ISO**: Original ISO path (if imported from ISO) or "Direct import"

### Command Bar
Modern CommandBar with organized operations:

#### Primary Operations
- **Mount**: Mount the selected Windows edition
- **Unmount Save**: Unmount and save all changes (💾 icon)
- **Unmount Discard**: Unmount and discard all changes (🔄 icon)
- **Open Mount**: Open mount directory in Explorer

#### Secondary Operations (after separator)
- **Delete**: Delete the image file
- **Make ISO**: Create ISO from image
- **Merge SWM**: Merge SWM files
- **Rebuild**: Rebuild with maximum compression

### Windows Editions Card
Modern single-selection interface for Windows editions:

#### Header
- **Title**: "Windows Editions" (clean, minimal design)

#### Edition List (ListView)
- **Selection Mode**: Single selection (replaces multiple checkboxes)
- **Auto-Selection**: First edition selected by default
- **Card Layout**: Each edition displayed as a card with:
  - **Index Badge**: Blue accent badge showing "Index X:"
  - **Edition Name**: Windows edition name (e.g., "Windows 10 Pro")
  - **Size Information**: Formatted size display
  - **Description**: Detailed edition description
  - **Edit Button**: Pencil icon for editing edition metadata

## UI Evolution

### Before (Multiple Selection)
```
☐ Index 1: Windows 10 Pro
☑ Index 2: Windows 10 Home  
☐ Index 3: Windows 10 Enterprise
[Select All] [Select None]
[Unmount]
```

### After (Single Selection)
```
Index 1: Windows 10 Pro
▶ Index 2: Windows 10 Home     [Selected]
Index 3: Windows 10 Enterprise
[Unmount Save] [Unmount Discard]
```

## Data Binding

### ViewModel Binding
```xml
<!-- Page uses x:Bind for performance -->
<Page x:Class="Bucket.Views.ImageDetailsPage">
    <!-- ViewModel is bound via x:Bind ViewModel.* -->
</Page>
```

### Key Bindings
- **Image Info**: `{x:Bind ViewModel.ImageInfo.*, Mode=OneWay}`
- **Selected Index**: `{x:Bind ViewModel.SelectedIndex, Mode=TwoWay}`
- **Commands**: `{x:Bind ViewModel.*Command}`
- **Editions List**: `{x:Bind ViewModel.ImageInfo.Indices, Mode=OneWay}`

## Event Handlers

### EditIndexButton_Click
```csharp
private async void EditIndexButton_Click(object sender, RoutedEventArgs e)
```
Handles the edit button click for individual Windows editions.

**Process:**
1. Extracts the WindowsImageIndex from button's Tag
2. Calls ViewModel.EditIndexCommand
3. Shows edit dialog for the selected index

## Features

### Single-Selection Interface
- **Consistent UX**: Matches Image Management page selection behavior
- **Clear Intent**: User can only select one edition at a time (intuitive ListView behavior)
- **Auto-Selection**: First edition automatically selected on page load
- **Visual Feedback**: Selected item highlighted in ListView

### Granular Unmount Control
- **Save Changes**: `Dismount-WindowsImage -Save` preserves all modifications
- **Discard Changes**: `Dismount-WindowsImage -Discard` reverts to original state
- **Clear Icons**: Save (💾) and Discard (🔄) icons for immediate recognition
- **Descriptive Tooltips**: Clear explanation of each operation

### Responsive Layout
- **ScrollViewer**: Handles content overflow gracefully
- **Card Design**: Modern Fluent Design System cards
- **Proper Spacing**: Consistent 24px margins and 16px padding
- **Adaptive Content**: Content adapts to different window sizes

### Progress Feedback
- **Mount Operations**: Progress dialogs for mount/unmount operations
- **Edit Operations**: Progress feedback during index editing
- **Error Handling**: User-friendly error dialogs

## Usage Examples

### Navigation to Page
```csharp
// From Image Management or other pages
var imageInfo = GetSelectedImageInfo();
var viewModel = App.GetService<ImageDetailsViewModel>();
viewModel.SetImageInfo(imageInfo);

// Navigate to page
Frame.Navigate(typeof(ImageDetailsPage));
```

### Handling Edition Selection
```csharp
// The ListView automatically handles selection via TwoWay binding
// ViewModel.SelectedIndex is updated when user clicks on an edition
```

### Edit Index Operation
```csharp
// Triggered by edit button click
private async void EditIndexButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button && button.Tag is WindowsImageIndex index)
    {
        await ViewModel.EditIndexCommand.ExecuteAsync(index);
    }
}
```

## Dependencies

- **ViewModel**: `ImageDetailsViewModel` for data and commands
- **Models**: `WindowsImageInfo`, `WindowsImageIndex`, `MountedImageInfo`
- **Services**: Various Windows image services via ViewModel
- **Dialogs**: `EditIndexDialog` for editing operations
- **Converters**: `BooleanToVisibilityConverter` for conditional visibility

## Related Files
- [`ImageDetailsViewModel.md`](../ViewModels/ImageDetailsViewModel.md) - ViewModel documentation
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Image model
- [`WindowsImageIndex.md`](../Models/WindowsImageIndex.md) - Index model
- [`EditIndexDialog.md`](./Dialogs/EditIndexDialog.md) - Edit dialog

## Best Practices

1. **Single Selection**: Always use single selection for operations requiring one edition
2. **Progress Feedback**: Show progress for all long-running operations
3. **Error Handling**: Provide clear error messages to users
4. **Accessibility**: Ensure proper keyboard navigation and screen reader support
5. **Consistent UX**: Maintain consistency with Image Management page patterns

## Accessibility Features

- **Keyboard Navigation**: Full keyboard support for all controls
- **Screen Reader**: Proper labels and descriptions for assistive technology
- **High Contrast**: Supports high contrast themes
- **Focus Management**: Proper focus handling throughout the page

## Performance Considerations

- **x:Bind**: Uses compiled bindings for better performance
- **Virtualization**: ListView virtualizes items for large edition lists
- **Async Operations**: All long-running operations are asynchronous
- **Memory Management**: Proper disposal of resources

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
