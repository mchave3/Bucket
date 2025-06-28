# MainViewModel Class Documentation

## Overview

The `MainViewModel` class serves as the primary view model for the main window of the Bucket application. It manages mounted Windows images and provides functionality for viewing, unmounting, and opening mount directories. This ViewModel follows MVVM architecture patterns and integrates with specialized Windows image services.

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
[ObservableProperty]
private ObservableCollection<MountedImageInfo> mountedImages = new();
```

- **Type**: `ObservableCollection<MountedImageInfo>`
- **Purpose**: Collection of currently mounted Windows images
- **Access**: Public (via generated property)
- **Binding**: Supports two-way data binding for UI lists

## Commands

### RefreshMountedImagesCommand

```csharp
public IAsyncRelayCommand RefreshMountedImagesCommand { get; }
```

- **Purpose**: Refreshes the list of mounted Windows images
- **Type**: Async command without parameters
- **Implementation**: Calls `RefreshMountedImagesAsync()`

### OpenMountDirectoryCommand

```csharp
public IAsyncRelayCommand<MountedImageInfo> OpenMountDirectoryCommand { get; }
```

- **Purpose**: Opens the mount directory for a specific mounted image in Windows Explorer
- **Type**: Async command with `MountedImageInfo` parameter
- **Implementation**: Calls `OpenMountDirectoryAsync(MountedImageInfo)`

### UnmountImageCommand

```csharp
public IAsyncRelayCommand<MountedImageInfo> UnmountImageCommand { get; }
```

- **Purpose**: Unmounts a specific Windows image with progress dialog
- **Type**: Async command with `MountedImageInfo` parameter
- **Implementation**: Calls `UnmountImageAsync(MountedImageInfo)`

## Constructor

```csharp
public MainViewModel(IWindowsImageMountService mountService, IWindowsImageUnmountService unmountService)
```

**Parameters:**
- `mountService`: Service for mounting operations and retrieving mounted images
- `unmountService`: Service for unmounting operations

**Features:**
- Dependency injection of specialized services
- Automatic loading of mounted images on startup
- Command initialization

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
        Command="{Binding RefreshMountedImagesCommand}" />

<!-- Open folder button for specific image -->
<Button Content="Open Folder"
        Command="{Binding DataContext.OpenMountDirectoryCommand, ElementName=PageRoot}"
        CommandParameter="{x:Bind}" />

<!-- Unmount button for specific image -->
<Button Content="Unmount"
        Command="{Binding DataContext.UnmountImageCommand, ElementName=PageRoot}"
        CommandParameter="{x:Bind}" />
```

### Data Binding for Image List

```xml
<ListView ItemsSource="{Binding MountedImages}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:MountedImageInfo">
            <Grid>
                <TextBlock Text="{x:Bind DisplayText}" />
                <TextBlock Text="{x:Bind MountPath}" />
            </Grid>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

## Features

### Mounted Image Management

1. **Automatic Refresh**: Loads mounted images on ViewModel initialization
2. **Real-time Updates**: Refreshes image list after unmount operations
3. **Error Handling**: Comprehensive error handling with user-friendly dialogs
4. **Progress Reporting**: Shows progress dialogs during long-running operations

### User Interface Integration

1. **Command Pattern**: All operations exposed as bindable commands
2. **Observable Collections**: Automatic UI updates when image list changes
3. **Parameter Binding**: Commands accept specific image parameters
4. **Dialog Management**: Handles progress and error dialogs

### Service Integration

1. **Mount Service**: Uses `IWindowsImageMountService` for querying and directory operations
2. **Unmount Service**: Uses `IWindowsImageUnmountService` for dismounting operations
3. **Dependency Injection**: Services injected via constructor
4. **Service Coordination**: Coordinates between mount and unmount services

## Dependencies

### Core Dependencies

- **CommunityToolkit.Mvvm**: MVVM infrastructure and source generators
- **Microsoft.UI.Xaml**: WinUI 3 framework for dialogs
- **Bucket.Services.WindowsImage**: Windows image management services

### Service Dependencies

- **IWindowsImageMountService**: Mount operations and image querying
- **IWindowsImageUnmountService**: Unmount operations

### Model Dependencies

- **MountedImageInfo**: Model representing mounted image information

## Private Methods

### RefreshMountedImagesAsync

```csharp
private async Task RefreshMountedImagesAsync()
```

- **Purpose**: Retrieves current mounted images and updates the collection
- **Error Handling**: Logs errors but doesn't throw exceptions
- **Performance**: Clears and repopulates collection efficiently

### OpenMountDirectoryAsync

```csharp
private async Task OpenMountDirectoryAsync(MountedImageInfo mountedImage)
```

- **Purpose**: Opens Windows Explorer to the mount directory
- **Validation**: Checks for null parameters
- **Error Handling**: Shows error dialog if operation fails

### UnmountImageAsync

```csharp
private async Task UnmountImageAsync(MountedImageInfo mountedImage)
```

- **Purpose**: Unmounts image with progress dialog and error handling
- **Features**:
  - Progress dialog with indeterminate progress bar
  - Automatic list refresh after successful unmount
  - Error dialog for failures
  - Comprehensive logging

### ShowUnmountProgressDialogAsync

```csharp
private async Task ShowUnmountProgressDialogAsync(MountedImageInfo mountedImage)
```

- **Purpose**: Shows progress dialog during unmount operation
- **Features**:
  - Indeterminate progress bar
  - Cancel button support
  - Automatic dialog closure on completion

### Dialog Helper Methods

- `ShowErrorDialogAsync`: Displays error messages to user
- `ShowInfoDialogAsync`: Displays informational messages

## Related Files

- [`IWindowsImageMountService.md`](../Services/WindowsImage/IWindowsImageMountService.md) - Mount service interface
- [`IWindowsImageUnmountService.md`](../Services/WindowsImage/IWindowsImageUnmountService.md) - Unmount service interface
- [`MountedImageInfo.md`](../Models/MountedImageInfo.md) - Mounted image model
- [`HomeLandingPage.md`](../Views/HomeLandingPage.md) - Main page using this ViewModel

## Best Practices

### ViewModel Design

1. **Service Injection**: Use constructor injection for all dependencies
2. **Command Pattern**: Expose all operations as commands for UI binding
3. **Error Handling**: Always provide user feedback for errors
4. **Logging**: Log all operations for debugging and monitoring

### UI Integration

1. **DataContext Setting**: Always set DataContext in code-behind
2. **Command Binding**: Use proper ElementName binding for commands with parameters
3. **Progress Feedback**: Show progress for long-running operations
4. **Error Display**: Use dialogs for error communication

### Performance

1. **Async Operations**: All file/service operations are async
2. **Collection Updates**: Efficient collection management
3. **Resource Cleanup**: Proper disposal of resources
4. **Background Loading**: Initial data loading doesn't block UI

## Error Handling

### Common Scenarios

- **Service Unavailable**: Handles cases where services fail to initialize
- **Mount Operation Failures**: Comprehensive error reporting for mount/unmount failures
- **Permission Issues**: Handles administrator privilege requirements
- **File System Errors**: Graceful handling of file system access issues

### Error Recovery

- **Retry Mechanisms**: Users can retry operations via refresh command
- **Graceful Degradation**: Application continues functioning even if some operations fail
- **User Feedback**: Clear error messages guide users to solutions

## Security Considerations

- **Path Validation**: All mount paths are validated before operations
- **Privilege Escalation**: Handles administrator privilege requirements appropriately
- **File System Access**: Validates permissions before file operations

## Performance Considerations

- **Lazy Loading**: Images loaded only when needed
- **Efficient Updates**: Collection updates minimize UI redraws
- **Background Operations**: Long-running operations don't block UI thread
- **Memory Management**: Proper disposal of resources and event handlers

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
