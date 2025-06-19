# ImageManagementViewModel Class Documentation

## Overview

ViewModel for the Windows Image Management page. This class implements the MVVM pattern to manage the presentation logic for displaying, filtering, and manipulating Windows images. It provides a clean separation between the UI and business logic while supporting modern WinUI 3 data binding.

## Location

- **File**: `src/ViewModels/ImageManagementViewModel.cs`
- **Namespace**: `Bucket.ViewModels`

## Class Definition

```csharp
public partial class ImageManagementViewModel : ObservableObject
```

## Properties

### Collection Properties

- **`Images`** (ObservableCollection&lt;WindowsImageInfo&gt;): Complete collection of Windows images
- **`FilteredImages`** (ObservableCollection&lt;WindowsImageInfo&gt;): Filtered collection based on search criteria
- **`SelectedImage`** (WindowsImageInfo): Currently selected image in the UI

### State Properties

- **`IsLoading`** (bool): Indicates whether data is currently being loaded
- **`HasImages`** (bool): Whether any images are available
- **`StatusMessage`** (string): Current status message for display
- **`SearchText`** (string): Current search filter text

## Commands

### Primary Actions

- **`RefreshCommand`**: Refreshes the images list from storage
- **`ImportFromIsoCommand`**: Initiates ISO import process
- **`ImportFromWimCommand`**: Initiates direct WIM file import

### Selection Actions

- **`DeleteSelectedCommand`**: Removes selected image from collection only
- **`DeleteSelectedFromDiskCommand`**: Removes selected image from collection and disk
- **`ViewImageDetailsCommand`**: Displays detailed information about an image

## Usage Examples

### Basic ViewModel Initialization

```csharp
// ViewModel is automatically instantiated via XAML DataContext
// Manual initialization in code-behind:
var viewModel = new ImageManagementViewModel();
await viewModel.InitializeAsync();
```

### Data Binding in XAML

```xml
<!-- Bind to collections -->
<ListView ItemsSource="{Binding FilteredImages}"
          SelectedItem="{Binding SelectedImage, Mode=TwoWay}" />

<!-- Bind to properties -->
<TextBlock Text="{Binding StatusMessage}" />
<ProgressRing IsActive="{Binding IsLoading}" />

<!-- Bind to commands -->
<Button Content="Refresh" Command="{Binding RefreshCommand}" />
<Button Content="Delete" Command="{Binding DeleteSelectedCommand}"
        IsEnabled="{Binding SelectedImage, Converter={StaticResource ObjectToBooleanConverter}}" />
```

### Search and Filtering

```csharp
// Search is automatically applied when SearchText changes
viewModel.SearchText = "Windows 11";
// FilteredImages will automatically update

// Manual search update
viewModel.UpdateSearchFilter("Pro Edition");
```

## Features

### Automatic Filtering
- **Real-time Search**: Filters images as user types
- **Multi-field Search**: Searches across name, type, and edition information
- **Case-insensitive**: Provides user-friendly search experience

### Command Management
- **Async Commands**: All operations support async/await pattern
- **Conditional Enabling**: Commands automatically enable/disable based on state
- **Progress Reporting**: Long operations provide user feedback

### Error Handling
- **User-friendly Messages**: Displays appropriate error dialogs
- **Graceful Degradation**: Continues operation when possible
- **Comprehensive Logging**: All operations are logged for troubleshooting

### State Management
- **Property Change Notification**: Implements INotifyPropertyChanged
- **Dependent Property Updates**: Related properties update automatically
- **UI Synchronization**: Maintains consistent UI state

## Dependencies

- **CommunityToolkit.Mvvm**: MVVM infrastructure and commands
- **Bucket.Models.WindowsImageInfo**: Domain model for Windows images
- **Bucket.Services.WindowsImageService**: Business logic service
- **Microsoft.UI.Xaml.Controls**: WinUI 3 dialog support

## Related Files

- [`ImageManagementPage.md`](../Views/ImageManagementPage.md) - Associated view
- [`WindowsImageService.md`](../Services/WindowsImageService.md) - Business logic service
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Primary domain model

## Best Practices

### Command Implementation

```csharp
// Use AsyncRelayCommand for async operations
public ICommand RefreshCommand { get; }

private async Task RefreshImagesAsync()
{
    try
    {
        IsLoading = true;
        StatusMessage = "Loading images...";

        var images = await _service.GetImagesAsync();
        // Update collections
    }
    finally
    {
        IsLoading = false;
    }
}
```

### Error Handling

```csharp
private async Task SafeOperationAsync()
{
    try
    {
        // Perform operation
    }
    catch (Exception ex)
    {
        Logger.Error(ex, "Operation failed");
        await ShowErrorDialogAsync("Error", ex.Message);
        StatusMessage = $"Error: {ex.Message}";
    }
}
```

### Property Updates

```csharp
// Properties automatically trigger UI updates
public string StatusMessage
{
    get => _statusMessage;
    set => SetProperty(ref _statusMessage, value);
}

// Handle dependent property updates
private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    switch (e.PropertyName)
    {
        case nameof(SelectedImage):
            ((AsyncRelayCommand)DeleteSelectedCommand).NotifyCanExecuteChanged();
            break;
    }
}
```

## Error Handling

### Common Scenarios

- **Service Failures**: Handle WindowsImageService exceptions gracefully
- **File System Errors**: Manage file access and permission issues
- **User Cancellation**: Support cancellation of long-running operations
- **Invalid Data**: Validate and handle corrupted image metadata

### Dialog Management

```csharp
// Error dialogs
await ShowErrorDialogAsync("Operation Failed", "Details of the error");

// Confirmation dialogs
var confirmed = await ShowConfirmationDialogAsync(
    "Delete Image",
    "Are you sure you want to delete this image?");

if (confirmed)
{
    // Proceed with deletion
}
```

## Performance Considerations

### Collection Management

- **ObservableCollection**: Use for automatic UI updates
- **Filtering Performance**: Large image collections may need virtualization
- **Search Optimization**: Consider debouncing for real-time search

### Memory Management

```csharp
// Dispose of resources when ViewModel is no longer needed
public void Dispose()
{
    // Clean up event subscriptions
    PropertyChanged -= OnPropertyChanged;
}
```

### UI Responsiveness

- **Async Operations**: Keep UI responsive during long operations
- **Progress Reporting**: Provide feedback for operations > 1 second
- **Background Loading**: Load data off the UI thread

## Security Considerations

- **Input Validation**: Validate all user inputs before processing
- **File Path Security**: Prevent directory traversal attacks
- **Permission Handling**: Handle insufficient permissions gracefully
- **Error Disclosure**: Avoid exposing sensitive information in error messages

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
