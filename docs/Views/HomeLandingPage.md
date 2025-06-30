# HomeLandingPage Class Documentation

## Overview

The home landing page serves as the main dashboard for the Bucket application, providing users with an overview of currently mounted Windows images and quick access to mount management operations. This page displays real-time information about mounted images and allows users to perform actions directly from the home screen.

## Location

- **File**: `src/Views/HomeLandingPage.xaml` and `src/Views/HomeLandingPage.xaml.cs`
- **Namespace**: `Bucket.Views`

## Class Definition

```csharp
public sealed partial class HomeLandingPage : Page
```

## Properties

### ViewModel
```csharp
public MainViewModel ViewModel { get; }
```
- **Type**: `MainViewModel`
- **Purpose**: Provides data binding and command handling for mounted images
- **Initialization**: Retrieved via dependency injection from `App.GetService<MainViewModel>()`

## Methods

### Constructor
```csharp
public HomeLandingPage()
```
Initializes a new instance of the HomeLandingPage class, sets up the ViewModel, and initializes the component.

## Usage Examples

### Navigation to Home Page
```csharp
// Navigate to home page (typically set as default)
Frame.Navigate(typeof(HomeLandingPage));
```

### XAML Structure
```xaml
<Page x:Class="Bucket.Views.HomeLandingPage">
    <Grid>
        <!-- Header with DevWinUI AllLandingPage -->
        <dev:AllLandingPage HeaderText="Bucket" />
        
        <!-- Mounted Images Section -->
        <Grid>
            <!-- Content when images are mounted -->
            <ScrollViewer Visibility="{ShowMountedImagesList}">
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
                    <!-- Mounted images list -->
                </Border>
            </ScrollViewer>
            
            <!-- Empty State - Full Height -->
            <Grid Visibility="{ShowEmptyState}">
                <!-- Refresh button in top-right corner -->
                <Button HorizontalAlignment="Right" VerticalAlignment="Top" />
                <!-- Centered empty state content -->
                <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" />
            </Grid>
        </Grid>
    </Grid>
</Page>
```

## Features

### Mounted Images Management
- **Real-time Display**: Shows currently mounted Windows images with details
- **Quick Actions**: CommandBar with Open, Save, and Discard operations
- **Empty State**: Informative message when no images are mounted
- **Auto-refresh**: Automatically loads mounted images on page initialization

### User Interface Components
- **Application Header**: DevWinUI AllLandingPage component with app branding
- **Mounted Images Card**: Displays list of mounted images with metadata
- **CommandBar Actions**: Modern action buttons for each mounted image
- **Empty State Display**: Full-height centered message when no images are mounted
- **Dual Refresh Access**: Refresh button available in both mounted and empty states

### Command Integration
- **Refresh**: Updates the list of mounted images
- **Open**: Opens mount directory in Windows Explorer
- **Save**: Unmounts image while saving changes
- **Discard**: Unmounts image while discarding changes

## Dependencies

### External Dependencies
- **Microsoft.UI.Xaml**: Core WinUI 3 framework
- **DevWinUI**: Third-party UI components for header section

### Internal Dependencies
- **MainViewModel**: Primary ViewModel for mounted image management
- **MountedImageInfo**: Model representing mounted image data
- **WindowsImageMountService**: Service for mount operations
- **WindowsImageUnmountService**: Service for unmount operations

## Related Files

- [`MainViewModel.md`](../ViewModels/MainViewModel.md): ViewModel handling mounted images
- [`MountedImageInfo.md`](../Models/MountedImageInfo.md): Data model for mounted images
- [`ImageManagementPage.md`](./ImageManagementPage.md): Page for mounting new images
- [`ImageDetailsPage.md`](./ImageDetailsPage.md): Detailed image management

## Best Practices

### Data Binding
- Use x:Bind for better performance with compiled bindings
- Implement proper Mode=OneWay for read-only data
- Use ElementName binding for command parameters

### User Experience
- Provide clear visual feedback for empty states
- Use CommandBar for consistent action presentation
- Display relevant metadata (mount path, timestamp, etc.)

### Performance
- Lazy load mounted images data
- Use async commands for all operations
- Implement proper error handling with user dialogs

## Error Handling

### Common Error Scenarios
1. **Mount Service Unavailable**: Service injection or initialization failures
2. **Permission Issues**: Insufficient rights to access mount directories
3. **Unmount Failures**: Images in use or system conflicts

### Error Recovery
- Graceful fallback to empty state on data load failures
- User-friendly error dialogs for operation failures
- Automatic retry mechanisms for transient failures

## UI Structure

### Header Section
- **DevWinUI AllLandingPage**: Application branding and version display
- **Navigation Integration**: Part of the overall navigation system

### Main Content Area
- **Dual Layout System**: Grid-based layout with conditional visibility
- **Mounted State**: ScrollViewer with images card when images are present
- **Empty State**: Full-height grid with centered content when no images
- **Refresh Button**: Available in both states (card header and top-right corner)
- **Images List**: ItemsControl with DataTemplate for each mounted image

### Per-Image Actions (CommandBar)
- **Open Button**: Opens mount directory in Explorer
- **Save Button**: Unmounts with changes saved
- **Discard Button**: Unmounts with changes discarded

### Responsive Design
- Adapts to different window sizes
- Maintains readability and usability across screen resolutions
- Proper spacing and alignment for touch and mouse interaction

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
