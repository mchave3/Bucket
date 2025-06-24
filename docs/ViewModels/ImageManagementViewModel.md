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

### Computed Properties

- **`FilteredImagesCountText`** (string): Formatted count text for filtered images ("1 image" or "X images")
- **`SelectedImageDisplayName`** (string): Display name for selected image or "No image selected"

## Commands

### Primary Actions

- **`RefreshCommand`**: Refreshes the images list from storage
- **`ImportFromIsoCommand`**: Initiates full ISO import process with progress tracking
- **`ImportFromWimCommand`**: Initiates direct WIM file import with analysis

### Import Operations

```csharp
// ISO Import with automatic progress dialog and cancellation support
await viewModel.ImportFromIsoCommand.ExecuteAsync(null);

// WIM Direct Import with standard picker and progress tracking
await viewModel.ImportFromWimCommand.ExecuteAsync(null);
```

The ISO import process includes:
1. File picker for ISO selection
2. Progress dialog with cancellation support
3. Automatic ISO mounting and dismounting
4. Windows image detection and extraction
5. Image analysis and registration
6. Error handling with user-friendly messages

### Selection Actions

- **`DeleteSelectedCommand`**: Removes selected image from collection only
- **`DeleteSelectedFromDiskCommand`**: Removes selected image from collection and disk
- **`ViewImageDetailsCommand`**: Displays detailed information about an image

### Detail Panel Actions

- **`ExtractSelectedIndicesCommand`**: Extracts selected indices from an image (placeholder)
- **`MountImageCommand`**: Mounts an image for modification (placeholder)
- **`ValidateImageCommand`**: Validates an image's integrity (placeholder)

## Methods

### Public Methods

- **`InitializeAsync()`**: Initializes the ViewModel by loading existing images
- **`UpdateSearchFilter(string searchText)`**: Updates the search filter and refreshes filtered images

### Private Helper Methods

- **`RefreshImagesAsync()`**: Refreshes the images collection from the service
- **`ImportFromIsoAsync()`**: Imports images from an ISO file using IsoImportService with full progress tracking
- **`ImportFromWimAsync()`**: Imports a WIM or ESD file using WindowsImageService directly
- **`DeleteSelectedImageAsync()`**: Deletes selected image from collection only
- **`DeleteSelectedImageFromDiskAsync()`**: Deletes selected image from both collection and disk
- **`ViewImageDetails(WindowsImageInfo image)`**: Views detailed information about specified image
- **`LoadIndexDetailsAsync(WindowsImageIndex index)`**: Loads detailed information for a specific image index
- **`FilterImages()`**: Filters the images based on search text using case-insensitive matching
- **`CanDeleteSelected()`**: Checks if selected image can be deleted (returns true if image is selected)

### Enhanced Dialog Helper Methods

- **`ShowErrorDialogAsync(string title, string message)`**: Shows error dialog with OK button
- **`ShowInfoDialogAsync(string title, string message)`**: Shows information dialog for user feedback
- **`ShowConfirmationDialogAsync(string title, string message)`**: Shows Yes/No confirmation dialog
- **`ShowImportProgressDialogAsync(string title, Func<IProgress<string>, CancellationToken, Task> importOperation)`**:
  - Shows cancellable progress dialog for import operations
  - Supports real-time progress updates and user cancellation
  - Handles both successful completion and cancellation scenarios
  - Automatically closes dialog on operation completion

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

### File Import Capabilities

- **ISO Import**: Custom FilePicker with window handle for WinUI 3 compatibility
- **WIM Direct Import**: Standard FileOpenPicker for WIM/ESD/SWM files
- **Robust File Selection**: Proper error handling and user feedback

### Automatic Filtering

- **Real-time Search**: Filters images as user types
- **Multi-field Search**: Searches across name, type, and edition information
- **Case-insensitive**: Provides user-friendly search experience

### Command Management

- **Async Commands**: All operations support async/await pattern
- **Conditional Enabling**: Commands automatically enable/disable based on state
- **Progress Reporting**: Long operations provide user feedback

### Error Management

- **User-friendly Messages**: Displays appropriate error dialogs
- **Graceful Degradation**: Continues operation when possible
- **Comprehensive Logging**: All operations are logged for troubleshooting
- **Progress Dialog Support**: Shows cancellable progress dialogs for long operations

### Import Progress Management

- **Cancellable Operations**: Import operations can be cancelled by users
- **Real-time Progress**: Shows live progress updates during import
- **Error Propagation**: Properly handles and displays import errors

### State Management

- **Property Change Notification**: Implements INotifyPropertyChanged
- **Dependent Property Updates**: Related properties update automatically
- **UI Synchronization**: Maintains consistent UI state

## Dependencies

### External Dependencies

- **CommunityToolkit.Mvvm**: MVVM infrastructure and commands
- **Microsoft.UI.Xaml.Controls**: WinUI 3 dialog support
- **Windows.Storage.Pickers**: File picker functionality
- **WinRT.Interop**: Window handle interop for file pickers

### Internal Dependencies

- **Bucket.Models.WindowsImageInfo**: Domain model for Windows images
- **Bucket.Services.WindowsImageService**: Core business logic service for image management
- **Bucket.Services.IsoImportService**: Specialized service for ISO import operations
- **Bucket.Common.Constants**: Configuration constants and paths
- **App.GetService\<IJsonNavigationService\>()**: Navigation service for detail views

### System Dependencies

- **PowerShell**: Required for ISO operations via IsoImportService
- **DISM**: Required for Windows image analysis
- **Administrator Rights**: May be required for ISO mounting operations

## Related Files

- [`ImageManagementPage.md`](../Views/ImageManagementPage.md) - Associated view implementation
- [`WindowsImageService.md`](../Services/WindowsImageService.md) - Core business logic service
- [`IsoImportService.md`](../Services/IsoImportService.md) - ISO import functionality
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Primary domain model
- [`ImageDetailsPage.md`](../Views/ImageDetailsPage.md) - Detail view for images

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

### Exception Management

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

## Common Error Scenarios

### Service Integration Issues

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

// Progress dialogs with cancellation
await ShowImportProgressDialogAsync("Importing...", async (progress, cancellationToken) =>
{
    // Long-running import operation
    progress.Report("Starting import...");
    await SomeImportOperationAsync(progress, cancellationToken);
});
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
