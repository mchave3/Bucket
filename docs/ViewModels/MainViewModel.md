# MainViewModel Class Documentation

## Overview

The `MainViewModel` class serves as the primary view model for the home landing page of the Bucket application. It manages mounted Windows images and provides functionality for viewing, unmounting (with save/discard options), and opening mount directories. This ViewModel follows MVVM architecture patterns and integrates with specialized Windows image services.

## Location

- **File**: `src/ViewModels/MainViewModel.cs`
- **Namespace**: `Bucket.ViewModels`

## Class Definition

```csharp
public partial class MainViewModel : ObservableObject
```

## Properties

### MountedImages

```csharp
public ObservableCollection<MountedImageInfo> MountedImages { get; set; }
```

- **Type**: `ObservableCollection<MountedImageInfo>`
- **Purpose**: Collection of currently mounted Windows images
- **Access**: Public with automatic property change notifications
- **Binding**: Supports two-way data binding for UI lists
- **Auto-update**: Notifies `ShowEmptyState` and `ShowMountedImagesList` on changes

### ShowEmptyState

```csharp
public bool ShowEmptyState => MountedImages.Count == 0;
```

- **Type**: `bool`
- **Purpose**: Determines whether to show the empty state UI when no images are mounted
- **Access**: Read-only computed property
- **Usage**: Used for conditional visibility of empty state message

### ShowMountedImagesList

```csharp
public bool ShowMountedImagesList => MountedImages.Count > 0;
```

- **Type**: `bool`
- **Purpose**: Determines whether to show the mounted images list
- **Access**: Read-only computed property
- **Usage**: Used for conditional visibility of the images list UI

## Commands

### RefreshMountedImagesCommand

```csharp
public IAsyncRelayCommand RefreshMountedImagesCommand { get; }
```

- **Purpose**: Refreshes the list of mounted Windows images
- **Type**: Async command without parameters
- **Implementation**: Calls `RefreshMountedImagesAsync()`
- **Features**: Includes orphaned mount directory cleanup on first load

### OpenMountDirectoryCommand

```csharp
public IAsyncRelayCommand<MountedImageInfo> OpenMountDirectoryCommand { get; }
```

- **Purpose**: Opens the mount directory for a specific mounted image in Windows Explorer
- **Type**: Async command with `MountedImageInfo` parameter
- **Implementation**: Calls `OpenMountDirectoryAsync(MountedImageInfo)`

### UnmountImageSaveCommand

```csharp
public IAsyncRelayCommand<MountedImageInfo> UnmountImageSaveCommand { get; }
```

- **Purpose**: Unmounts a specific Windows image while saving changes
- **Type**: Async command with `MountedImageInfo` parameter
- **Implementation**: Calls `UnmountImageSaveAsync(MountedImageInfo)`
- **Features**: Shows progress dialog with "Unmounting and Saving Changes" title

### UnmountImageDiscardCommand

```csharp
public IAsyncRelayCommand<MountedImageInfo> UnmountImageDiscardCommand { get; }
```

- **Purpose**: Unmounts a specific Windows image while discarding changes
- **Type**: Async command with `MountedImageInfo` parameter
- **Implementation**: Calls `UnmountImageDiscardAsync(MountedImageInfo)`
- **Features**: Shows progress dialog with "Unmounting and Discarding Changes" title

## Constructor

```csharp
public MainViewModel(IWindowsImageMountService mountService, IWindowsImageUnmountService unmountService)
```

**Parameters:**
- `mountService`: Service for mounting operations and retrieving mounted images
- `unmountService`: Service for unmounting operations with save/discard options

**Features:**
- Dependency injection of specialized services
- Automatic loading of mounted images on startup
- Command initialization
- Parameter validation with `ArgumentNullException`

## Usage Examples

### Basic Implementation in Page

```csharp
public sealed partial class HomeLandingPage : Page
{
    public MainViewModel ViewModel { get; }

    public HomeLandingPage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();
        DataContext = ViewModel; // Important for command binding
    }
}
```

### Command Binding in XAML

```xml
<!-- Refresh button -->
<Button Content="Refresh" 
        Command="{x:Bind ViewModel.RefreshMountedImagesCommand}" />

<!-- CommandBar with actions for each image -->
<CommandBar Background="Transparent" DefaultLabelPosition="Right">
    <AppBarButton Label="Open" 
                  Command="{Binding DataContext.OpenMountDirectoryCommand, ElementName=PageRoot}"
                  CommandParameter="{x:Bind}" />
    <AppBarButton Label="Save" 
                  Command="{Binding DataContext.UnmountImageSaveCommand, ElementName=PageRoot}"
                  CommandParameter="{x:Bind}" />
    <AppBarButton Label="Discard" 
                  Command="{Binding DataContext.UnmountImageDiscardCommand, ElementName=PageRoot}"
                  CommandParameter="{x:Bind}" />
</CommandBar>
```

### Data Binding with Empty State

```xml
<!-- Mounted images list -->
<ItemsControl ItemsSource="{x:Bind ViewModel.MountedImages, Mode=OneWay}"
              Visibility="{x:Bind ViewModel.ShowMountedImagesList, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
    <!-- Item template -->
</ItemsControl>

<!-- Empty state -->
<StackPanel Visibility="{x:Bind ViewModel.ShowEmptyState, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
    <FontIcon Glyph="&#xE7C3;" />
    <TextBlock Text="No images are currently mounted" />
</StackPanel>
```

## Features

### Mounted Image Management

1. **Automatic Refresh**: Loads mounted images on ViewModel initialization
2. **Real-time Updates**: Refreshes image list after unmount operations
3. **Orphaned Cleanup**: Cleans up orphaned mount directories on first load
4. **Dual Unmount Options**: Separate commands for save and discard operations

### User Interface Integration

1. **Command Pattern**: All operations exposed as bindable commands
2. **Observable Collections**: Automatic UI updates when image list changes
3. **Parameter Binding**: Commands accept specific image parameters
4. **State Management**: Properties for controlling UI visibility

### Progress and Error Handling

1. **Progress Dialogs**: Shows indeterminate progress for long operations
2. **Custom Titles**: Different dialog titles for save vs discard operations
3. **Success Messages**: Differentiated success messages based on operation type
4. **Error Dialogs**: User-friendly error messages with operation context

## Dependencies

### Core Dependencies

- **CommunityToolkit.Mvvm**: MVVM infrastructure and source generators
- **Microsoft.UI.Xaml**: WinUI 3 framework for dialogs and UI elements

### Service Dependencies

- **IWindowsImageMountService**: Mount operations, image querying, and directory operations
- **IWindowsImageUnmountService**: Unmount operations with save/discard options

### Model Dependencies

- **MountedImageInfo**: Model representing mounted image information

## Private Methods

### RefreshMountedImagesAsync

```csharp
private async Task RefreshMountedImagesAsync()
```

- **Purpose**: Retrieves current mounted images and updates the collection
- **Features**: Orphaned mount directory cleanup on first load
- **Error Handling**: Logs errors but doesn't throw exceptions
- **Notifications**: Updates `ShowEmptyState` and `ShowMountedImagesList` properties

### OpenMountDirectoryAsync

```csharp
private async Task OpenMountDirectoryAsync(MountedImageInfo mountedImage)
```

- **Purpose**: Opens Windows Explorer to the mount directory
- **Validation**: Checks for null parameters
- **Error Handling**: Shows error dialog if operation fails

### UnmountImageSaveAsync

```csharp
private async Task UnmountImageSaveAsync(MountedImageInfo mountedImage)
```

- **Purpose**: Unmounts image while saving changes
- **Features**: Custom progress dialog title and logging
- **Error Handling**: Specific error dialog for save operations

### UnmountImageDiscardAsync

```csharp
private async Task UnmountImageDiscardAsync(MountedImageInfo mountedImage)
```

- **Purpose**: Unmounts image while discarding changes
- **Features**: Custom progress dialog title and logging
- **Error Handling**: Specific error dialog for discard operations

### ShowUnmountProgressDialogAsync

```csharp
private async Task ShowUnmountProgressDialogAsync(MountedImageInfo mountedImage, bool saveChanges = true, string title = "Unmounting Image")
```

- **Purpose**: Shows progress dialog and performs unmount operation
- **Parameters**: 
  - `saveChanges`: Whether to save or discard changes
  - `title`: Custom dialog title
- **Features**: 
  - Indeterminate progress bar
  - Background task execution
  - Automatic dialog closure
  - Success/failure handling
  - Differentiated success messages

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
