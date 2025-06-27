# Image Reordering Feature Documentation

## Overview

The Image Reordering feature allows users to manually sort their Windows images by moving them up or down in the list. This functionality provides users with control over how their images are displayed and organized within the Bucket application.

## Location

- **View**: `src/Views/ImageManagementPage.xaml`
- **ViewModel**: `src/ViewModels/ImageManagementViewModel.cs`
- **Feature Area**: Image Management

## User Interface

### Master Header Controls

The reordering controls are located in the master panel header, to the right of the "Windows Images" title:

```xml
<StackPanel Orientation="Horizontal" Spacing="4">
    <Button Content="&#xE70E;"
            ToolTipService.ToolTip="Move selected image up"
            Command="{Binding MoveImageUpCommand}"
            IsEnabled="{Binding CanMoveImageUp}" />
    <Button Content="&#xE70D;"
            ToolTipService.ToolTip="Move selected image down"
            Command="{Binding MoveImageDownCommand}"
            IsEnabled="{Binding CanMoveImageDown}" />
</StackPanel>
```

### Button Styling

- **Style**: `SubtleButtonStyle` for minimal visual impact
- **Size**: 32x32 pixels for compact layout
- **Icons**: Fluent icons (E70E for up arrow, E70D for down arrow)
- **Font**: Segoe Fluent Icons for consistent appearance
- **Tooltips**: Descriptive tooltips for accessibility

## Functionality

### Move Up Operation

- **Action**: Moves the selected image one position up in the list
- **Availability**: Enabled when an image is selected and it's not at the top
- **Effect**: Updates both main collection and filtered collection
- **Feedback**: Status message confirms the operation

### Move Down Operation

- **Action**: Moves the selected image one position down in the list
- **Availability**: Enabled when an image is selected and it's not at the bottom
- **Effect**: Updates both main collection and filtered collection
- **Feedback**: Status message confirms the operation

### State Management

- **Dynamic Enabling**: Buttons automatically enable/disable based on position
- **Selection Awareness**: Commands respond to selection changes
- **Collection Synchronization**: Both main and filtered collections stay in sync

## Implementation Details

### ViewModel Properties

```csharp
/// <summary>
/// Gets whether the selected image can be moved up in the list.
/// </summary>
public bool CanMoveImageUp => SelectedImage != null && Images.IndexOf(SelectedImage) > 0;

/// <summary>
/// Gets whether the selected image can be moved down in the list.
/// </summary>
public bool CanMoveImageDown => SelectedImage != null && Images.IndexOf(SelectedImage) < Images.Count - 1;
```

### Commands

```csharp
/// <summary>
/// Gets the command to move the selected image up in the list.
/// </summary>
public ICommand MoveImageUpCommand { get; }

/// <summary>
/// Gets the command to move the selected image down in the list.
/// </summary>
public ICommand MoveImageDownCommand { get; }
```

### Move Methods

```csharp
private async Task MoveImageUp()
{
    if (SelectedImage == null) return;

    var currentIndex = Images.IndexOf(SelectedImage);
    if (currentIndex > 0)
    {
        // Safe logging with null protection
        var imageName = SelectedImage.Name ?? "Unnamed Image";
        Logger.Information("Moving image '{ImageName}' up from position {From} to {To}",
            imageName, currentIndex, currentIndex - 1);

        // Move in the main collection
        Images.Move(currentIndex, currentIndex - 1);

        // Update the filtered collection to maintain consistency
        var filteredIndex = FilteredImages.IndexOf(SelectedImage);
        if (filteredIndex >= 0 && filteredIndex > 0)
        {
            FilteredImages.Move(filteredIndex, filteredIndex - 1);
        }

        // Update command states
        UpdateMoveCommandStates();

        // Save the new order to file for persistence
        try
        {
            await _windowsImageService.SaveImagesOrderAsync(Images.ToList());
            StatusMessage = $"Moved '{imageName}' up";
            Logger.Information("Successfully saved new order after moving image up");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save image order after moving up");
            StatusMessage = $"Moved '{imageName}' up (order not saved)";
        }
    }
}
```

## Usage Scenarios

### Basic Reordering

1. **Select an Image**: Click on an image in the master list
2. **Move Up**: Click the up arrow button to move the image higher in the list
3. **Move Down**: Click the down arrow button to move the image lower in the list
4. **Visual Feedback**: Status message confirms the move operation

### Keyboard Navigation

While not currently implemented, the feature is designed to support future keyboard shortcuts:
- **Ctrl+Up**: Move selected image up
- **Ctrl+Down**: Move selected image down

### Filtered Lists

The reordering feature works seamlessly with filtered lists:
- Moving an image updates both the main collection and filtered view
- Positions are maintained correctly when switching between filtered and full views
- Search functionality continues to work normally after reordering

## Benefits

### User Experience

- **Customizable Organization**: Users can arrange images according to their preferences
- **Visual Control**: Clear visual feedback with arrow buttons and tooltips
- **Intuitive Interface**: Standard up/down arrows follow common UI conventions
- **Responsive Design**: Buttons automatically enable/disable based on context

### Technical Benefits

- **Collection Synchronization**: Maintains consistency between multiple views
- **Performance**: Uses efficient ObservableCollection.Move() method
- **Logging**: All operations are logged for troubleshooting
- **State Management**: Proper command state updates prevent invalid operations

## Accessibility

### Screen Reader Support

- **Tooltips**: Descriptive tooltips provide screen reader information
- **Command Names**: Clear command names for assistive technology
- **State Indication**: Button enabled/disabled states are properly communicated

### Keyboard Support

- **Tab Navigation**: Buttons are included in tab order
- **Focus Indicators**: Standard focus visuals are maintained
- **Future Enhancement**: Keyboard shortcuts can be added later

## Error Handling

### Validation

- **Null Checks**: Methods validate that an image is selected
- **Boundary Checks**: Prevents moving beyond list boundaries
- **Collection Integrity**: Ensures collections remain synchronized

### Error Scenarios

```csharp
// Safe move operation with validation
private void MoveImageUp()
{
    if (SelectedImage == null) return; // No selection

    var currentIndex = Images.IndexOf(SelectedImage);
    if (currentIndex > 0) // Not at top
    {
        // Perform move operation
    }
    // No action needed if already at top
}
```

## Future Enhancements

### Planned Features

- **Drag and Drop**: Visual drag and drop reordering
- **Keyboard Shortcuts**: Ctrl+Up/Down for power users
- **Multiple Selection**: Move multiple images at once
- **Sort Options**: Automatic sorting by name, date, size, etc.

### Persistence

Currently, the reordering is session-based. Future enhancements could include:
- **Save Order**: Persist custom order between sessions
- **Order Profiles**: Multiple saved ordering schemes
- **Import Order**: Apply ordering when importing new images

## Performance Considerations

### Efficient Operations

- **Move vs Remove/Insert**: Uses ObservableCollection.Move() for better performance
- **Minimal UI Updates**: Only necessary property changes trigger UI updates
- **Collection Synchronization**: Updates both collections efficiently

### Large Collections

For large image collections, consider:
- **Virtualization**: UI virtualization for better scroll performance
- **Batch Operations**: Future support for batch moves
- **Progress Indication**: For operations on many items

## Persistence and Data Management

### Automatic Persistence

The reordering feature automatically saves the new order to the JSON file after each move operation:

```csharp
// Save the new order to file for persistence
try
{
    await _windowsImageService.SaveImagesOrderAsync(Images.ToList());
    StatusMessage = $"Moved '{imageName}' up";
    Logger.Information("Successfully saved new order after moving image up");
}
catch (Exception ex)
{
    Logger.Error(ex, "Failed to save image order after moving up");
    StatusMessage = $"Moved '{imageName}' up (order not saved)";
}
```

### Benefits of Persistence

- **Session Continuity**: Image order is preserved between application sessions
- **User Preferences**: Custom ordering reflects user workflows and preferences
- **Data Integrity**: Order changes are immediately saved to prevent data loss
- **Error Recovery**: Clear feedback when save operations fail

### Technical Implementation

- **Async Operations**: Save operations don't block the UI
- **Error Handling**: Graceful degradation when save fails
- **Service Delegation**: Uses WindowsImageService.SaveImagesOrderAsync()
- **JSON Storage**: Order is persisted in the images.json metadata file

## Related Documentation

- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - Complete ViewModel documentation
- [`ImageManagementPage.md`](../Views/ImageManagementPage.md) - View implementation details
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Image model structure
- [`Navigation-Bar-Management.md`](../Navigation-Bar-Management.md) - Overall navigation patterns

## Testing Considerations

### Test Scenarios

1. **Basic Movement**: Test up/down operations with single selection
2. **Boundary Conditions**: Test movement at top and bottom of list
3. **Empty Lists**: Verify behavior with no images
4. **Filtered Lists**: Test movement with active search filters
5. **State Updates**: Verify command enable/disable states
6. **Collection Sync**: Ensure filtered and main collections stay synchronized

### Edge Cases

- **Single Image**: Verify buttons are disabled appropriately
- **Rapid Operations**: Test multiple quick moves
- **Selection Changes**: Verify state updates when selection changes
- **Filtering During Move**: Test behavior when filtering after moves

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
