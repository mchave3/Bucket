# ImageDetailsViewModel Class Documentation

## Overview
ViewModel for the Windows Image Details page that handles displaying and editing detailed information about Windows images, including their indices and metadata.

## Location
- **File**: `src/ViewModels/ImageDetailsViewModel.cs`
- **Namespace**: `Bucket.ViewModels`

## Class Definition
```csharp
public partial class ImageDetailsViewModel : ObservableObject
```

## Properties

### ImageInfo
```csharp
public WindowsImageInfo ImageInfo { get; set; }
```
Gets or sets the image information being displayed.

### HasSourceIso
```csharp
public bool HasSourceIso { get; }
```
Gets whether the image has a source ISO path.

### SelectedIndex
```csharp
public WindowsImageIndex? SelectedIndex { get; set; }
```
Gets or sets the currently selected index for single-selection operations. Used for mount, unmount, and other operations that require a specific Windows edition to be selected.

### MountedImages
```csharp
public ObservableCollection<MountedImageInfo> MountedImages { get; set; }
```
Gets the collection of currently mounted images for this image file.

## Commands

### EditMetadataCommand
```csharp
public IAsyncRelayCommand EditMetadataCommand { get; }
```
Command to show information about editing image metadata.

### ExportImageCommand
```csharp
public IAsyncRelayCommand ExportImageCommand { get; }
```
Command to export the image (placeholder for future implementation).

### SelectAllIndicesCommand (Deprecated)
```csharp
public IRelayCommand SelectAllIndicesCommand { get; }
```
**Note**: This command is deprecated as the UI now uses single-selection mode instead of multiple checkboxes.

### SelectNoIndicesCommand (Deprecated)
```csharp
public IRelayCommand SelectNoIndicesCommand { get; }
```
**Note**: This command is deprecated as the UI now uses single-selection mode instead of multiple checkboxes.

### ApplyUpdatesCommand
```csharp
public IAsyncRelayCommand ApplyUpdatesCommand { get; }
```
Command to apply Windows updates to the selected index. **Updated behavior**: Now works with single-selection mode using the `SelectedIndex` property.

### MountImageCommand
```csharp
public IAsyncRelayCommand MountImageCommand { get; }
```
Command to mount the selected Windows edition. **Updated behavior**: Now works with single-selection mode using the `SelectedIndex` property.

### UnmountImageCommand
```csharp
public IAsyncRelayCommand UnmountImageCommand { get; }
```
Command to unmount the mounted image with default save behavior (legacy command).

### UnmountImageSaveCommand (New)
```csharp
public IAsyncRelayCommand UnmountImageSaveCommand { get; }
```
Command to unmount the mounted image and **save all changes** to the WIM file. Executes PowerShell command `Dismount-WindowsImage -Save`.

### UnmountImageDiscardCommand (New)
```csharp
public IAsyncRelayCommand UnmountImageDiscardCommand { get; }
```
Command to unmount the mounted image and **discard all changes**. Executes PowerShell command `Dismount-WindowsImage -Discard`.

### OpenMountDirectoryCommand
```csharp
public IAsyncRelayCommand OpenMountDirectoryCommand { get; }
```
Command to open the mount directory in Windows Explorer.

### DeleteImageCommand
```csharp
public IAsyncRelayCommand DeleteImageCommand { get; }
```
Command to delete the Windows image file.

### MakeIsoCommand
```csharp
public IAsyncRelayCommand MakeIsoCommand { get; }
```
Command to create an ISO file from the Windows image.

### MergeSWMCommand
```csharp
public IAsyncRelayCommand MergeSWMCommand { get; }
```
Command to merge SWM files into a single WIM.

### RebuildImageCommand
```csharp
public IAsyncRelayCommand RebuildImageCommand { get; }
```
Command to rebuild the image with maximum compression.

### ExtractFilesCommand
```csharp
public IAsyncRelayCommand ExtractFilesCommand { get; }
```
Command to extract files from the image.

### RenameImageCommand
```csharp
public IAsyncRelayCommand RenameImageCommand { get; }
```
Command to rename the image.

### EditIndexCommand
```csharp
public IAsyncRelayCommand<WindowsImageIndex> EditIndexCommand { get; }
```
Command to edit a Windows image index. **Updated behavior**: After editing, the page now refreshes the current data instead of navigating back to Image Management.

## Methods

### SetImageInfo
```csharp
public async void SetImageInfo(WindowsImageInfo imageInfo)
```
Sets the image information to display and updates related properties. **Updated behavior**: Now automatically selects the first index by default for single-selection mode and refreshes mounted images.

### UpdateIndexSelection (New)
```csharp
public void UpdateIndexSelection(WindowsImageIndex selectedIndex)
```
Updates the selection state for single-selection mode. Clears all previous selections and sets the specified index as selected.

### RefreshMountedImagesAsync
```csharp
public async Task RefreshMountedImagesAsync()
```
Refreshes the list of mounted images for the current image file.

### RefreshImageDataAsync (New)
```csharp
private async Task RefreshImageDataAsync()
```
Refreshes the current image data to reflect any changes made. This method:
- Retrieves updated image data from the metadata service
- Updates the current ImageInfo with refreshed data
- Maintains the user's current view on the Image Details page
- Handles errors gracefully without interrupting user flow

## Usage Examples

### Basic Usage
```csharp
var viewModel = App.GetService<ImageDetailsViewModel>();
viewModel.SetImageInfo(imageInfo);
// First index is automatically selected for single-selection mode
```

### Single-Selection Index Management
```csharp
// Update the selected index (single-selection mode)
viewModel.UpdateIndexSelection(specificIndex);

// Get the currently selected index
var selectedIndex = viewModel.SelectedIndex;
```

### Unmount Operations with Save/Discard
```csharp
// Unmount and save all changes
await viewModel.UnmountImageSaveCommand.ExecuteAsync();

// Unmount and discard all changes
await viewModel.UnmountImageDiscardCommand.ExecuteAsync();
```

### Editing an Index
```csharp
// The EditIndexCommand now refreshes the current page instead of navigating away
await viewModel.EditIndexCommand.ExecuteAsync(imageIndex);
// User remains on Image Details page with updated data
```

## Features

- **Image Details Display**: Shows comprehensive information about Windows images
- **Single-Selection Mode**: Uses ListView with single selection instead of multiple checkboxes for better UX
- **Index Editing**: Edit names and descriptions of Windows image indices
- **Granular Unmount Control**: Separate Save and Discard unmount operations
- **In-Place Refresh**: After editing, data is refreshed without navigation (improved UX)
- **Progress Tracking**: Shows progress dialogs for long-running operations
- **Error Handling**: Comprehensive error handling with user-friendly dialogs
- **Validation**: Input validation for edit operations
- **Cancellation Support**: Operations can be cancelled by users
- **Auto-Selection**: Automatically selects first index when loading image details

## UI Changes (Recent Updates)

### From Multiple Selection to Single Selection
- **Before**: Multiple checkboxes allowing selection of multiple Windows editions
- **After**: ListView with single-selection mode (similar to Image Management page)
- **Benefits**: Clearer user intent, simpler operations, consistent with Image Management UX

### Enhanced Unmount Operations
- **Before**: Single "Unmount" button with default save behavior
- **After**: Two distinct buttons:
  - **"Unmount Save"** (💾 icon): Saves all changes to WIM file
  - **"Unmount Discard"** (🔄 icon): Discards all changes
- **Technical**: Uses PowerShell `Dismount-WindowsImage -Save` vs `-Discard` parameters

## Dependencies

- `IWindowsImageMetadataService`: For loading and saving image metadata
- `IWindowsImageIndexEditingService`: For editing Windows image indices
- `CommunityToolkit.Mvvm`: For MVVM infrastructure
- Various dialog services for user interaction

## Related Files
- [`ImageDetailsPage.md`](../Views/ImageDetailsPage.md)
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md)
- [`WindowsImageIndex.md`](../Models/WindowsImageIndex.md)
- [`IWindowsImageMetadataService.md`](../Services/WindowsImage/IWindowsImageMetadataService.md)
- [`EditIndexDialog.md`](../Views/Dialogs/EditIndexDialog.md)

## Best Practices

- Always check for null values before operations
- Use async/await pattern for long-running operations
- Provide progress feedback for operations that may take time
- Handle cancellation tokens properly
- Log important operations and errors

## Error Handling

The ViewModel handles various error scenarios:
- **Null References**: Checks for null ImageInfo before operations
- **File Access Errors**: Handles file system exceptions during WIM operations
- **Metadata Errors**: Graceful handling of metadata save/load failures
- **Cancellation**: Proper handling of user-cancelled operations
- **Validation Errors**: Input validation with user-friendly error messages

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
