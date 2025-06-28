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

### SelectAllIndicesCommand
```csharp
public IRelayCommand SelectAllIndicesCommand { get; }
```
Command to select all Windows indices.

### SelectNoIndicesCommand
```csharp
public IRelayCommand SelectNoIndicesCommand { get; }
```
Command to deselect all Windows indices.

### ApplyUpdatesCommand
```csharp
public IAsyncRelayCommand ApplyUpdatesCommand { get; }
```
Command to apply Windows updates to selected indices.

### MountImageCommand
```csharp
public IAsyncRelayCommand MountImageCommand { get; }
```
Command to mount the image.

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
public void SetImageInfo(WindowsImageInfo imageInfo)
```
Sets the image information to display and updates related properties.

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
```

### Editing an Index
```csharp
// The EditIndexCommand now refreshes the current page instead of navigating away
await viewModel.EditIndexCommand.ExecuteAsync(imageIndex);
// User remains on Image Details page with updated data
```

## Features

- **Image Details Display**: Shows comprehensive information about Windows images
- **Index Editing**: Edit names and descriptions of Windows image indices
- **In-Place Refresh**: After editing, data is refreshed without navigation (improved UX)
- **Progress Tracking**: Shows progress dialogs for long-running operations
- **Error Handling**: Comprehensive error handling with user-friendly dialogs
- **Validation**: Input validation for edit operations
- **Cancellation Support**: Operations can be cancelled by users

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
