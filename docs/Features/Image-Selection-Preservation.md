# Image Selection Preservation Feature

## Overview
This feature ensures that image selection is maintained during image reordering operations (move up/down) in the Image Management page. The feature addresses the issue where users would lose their selected image after performing move operations, requiring them to reselect the image to continue reordering.

## Location

- **Implementation**: `src/ViewModels/ImageManagementViewModel.cs`
- **Related Files**: 
  - `src/Views/ImageManagementPage.xaml` (UI binding)
  - `src/Services/WindowsImageService.cs` (data persistence)

## Problem Analysis

### Root Cause
The selection loss was not caused by refresh operations as initially suspected, but by the `ObservableCollection.Move()` operations in the `MoveImageUp()` and `MoveImageDown()` methods. When moving items in an ObservableCollection, the collection change events can interfere with WinUI ListView's selection binding, causing the SelectedImage property to be reset to null.

### Symptoms
- User selects an image for reordering
- User clicks "Move Up" or "Move Down" button
- Image moves successfully but selection is lost
- User must reselect the image to continue reordering
- This creates a frustrating workflow for users managing multiple images

## Solution Implementation

### Core Strategy
The solution preserves the selected image reference before performing move operations and restores it immediately after the moves are completed.

### Code Implementation

#### MoveImageUp Method
```csharp
private async Task MoveImageUp()
{
    if (SelectedImage == null) return;

    var currentIndex = Images.IndexOf(SelectedImage);
    if (currentIndex > 0)
    {
        // Preserve the selected image reference before moving
        var selectedImageToPreserve = SelectedImage;

        // Move in the main collection
        Images.Move(currentIndex, currentIndex - 1);

        // Update the filtered collection to maintain consistency
        var filteredIndex = FilteredImages.IndexOf(selectedImageToPreserve);
        if (filteredIndex >= 0 && filteredIndex > 0)
        {
            FilteredImages.Move(filteredIndex, filteredIndex - 1);
        }

        // Restore the selection after the move operations
        SelectedImage = selectedImageToPreserve;

        // Continue with order saving and status updates...
    }
}
```

#### MoveImageDown Method
```csharp
private async Task MoveImageDown()
{
    if (SelectedImage == null) return;

    var currentIndex = Images.IndexOf(SelectedImage);
    if (currentIndex < Images.Count - 1)
    {
        // Preserve the selected image reference before moving
        var selectedImageToPreserve = SelectedImage;

        // Move in the main collection
        Images.Move(currentIndex, currentIndex + 1);

        // Update the filtered collection to maintain consistency
        var filteredIndex = FilteredImages.IndexOf(selectedImageToPreserve);
        if (filteredIndex >= 0 && filteredIndex < FilteredImages.Count - 1)
        {
            FilteredImages.Move(filteredIndex, filteredIndex + 1);
        }

        // Restore the selection after the move operations
        SelectedImage = selectedImageToPreserve;

        // Continue with order saving and status updates...
    }
}
```

### Key Implementation Details

1. **Reference Preservation**: Store the selected image reference in a local variable before any move operations
2. **Dual Collection Updates**: Update both the main `Images` collection and the `FilteredImages` collection to maintain UI consistency
3. **Immediate Restoration**: Restore the selection immediately after move operations to prevent any UI flickering
4. **Bounds Checking**: Ensure filtered collection moves are within valid bounds

## Additional Features

### Refresh Operation Selection Preservation
The system also includes selection preservation during refresh operations using a different strategy:

```csharp
private void RestoreSelection(string selectedImagePath, IEnumerable<WindowsImageInfo> searchInCollection, string operationName)
{
    if (string.IsNullOrEmpty(selectedImagePath))
        return;

    var imageToSelect = searchInCollection.FirstOrDefault(img => img.FilePath == selectedImagePath);
    if (imageToSelect != null)
    {
        SelectedImage = imageToSelect;
        Logger.Debug("Restored selection after {Operation} to: {Name}", operationName, imageToSelect.Name);
    }
}
```

### Filter Operation Selection Preservation
Selection is also preserved during search filtering operations through the `FilterImages(bool preserveSelection)` method.

## User Experience Impact

### Before Implementation
- User selects image "Windows 11 Pro"
- User clicks "Move Down"
- Image moves but selection is lost
- User must reselect "Windows 11 Pro" to continue
- Workflow is interrupted and frustrating

### After Implementation
- User selects image "Windows 11 Pro"
- User clicks "Move Down"
- Image moves and selection is maintained
- User can immediately click "Move Down" again
- Smooth, uninterrupted workflow

## Technical Benefits

1. **Improved User Experience**: Eliminates the need to reselect images during reordering operations
2. **Workflow Efficiency**: Users can perform multiple consecutive move operations without interruption
3. **UI Consistency**: Selection state remains consistent across all operations
4. **Robust Implementation**: Handles both main and filtered collections properly

## Logging and Debugging

The implementation includes appropriate logging for troubleshooting:
- Selection restoration events are logged at Debug level
- Move operations are logged at Information level
- Failed selection restorations are logged for debugging

## Related Components

- **ImageManagementViewModel**: Contains the core implementation
- **WindowsImageInfo**: The data model used for selection tracking
- **ObservableCollection**: The collection type that triggers the selection issues
- **WinUI ListView**: The UI component that displays the selectable images

## Future Considerations

- The solution could be extended to handle other collection modification operations
- Similar patterns could be applied to other ViewModels with selectable collections
- Performance monitoring may be beneficial for large image collections

---

**Note**: This feature was implemented to address a specific user experience issue where image selection was lost during reordering operations, significantly improving the workflow for image management tasks. 