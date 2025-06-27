# ImageDetailsViewModel Class Documentation

## Overview
ViewModel for the Windows Image Details page that provides comprehensive functionality for viewing, editing, and managing detailed information about Windows images and their editions.

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
Gets or sets the image information being displayed in the details view.

### HasSourceIso
```csharp
public bool HasSourceIso { get; }
```
Gets whether the image has a source ISO path, used for conditional UI display.

## Commands

### EditMetadataCommand
```csharp
public IAsyncRelayCommand EditMetadataCommand { get; }
```
Command to open the metadata editing interface.

### ExportImageCommand
```csharp
public IAsyncRelayCommand ExportImageCommand { get; }
```
Command to export the Windows image to a new location.

### SelectAllIndicesCommand
```csharp
public IRelayCommand SelectAllIndicesCommand { get; }
```
Command to select all Windows editions/indices for operations.

### SelectNoIndicesCommand
```csharp
public IRelayCommand SelectNoIndicesCommand { get; }
```
Command to deselect all Windows editions/indices.

### ApplyUpdatesCommand
```csharp
public IAsyncRelayCommand ApplyUpdatesCommand { get; }
```
Command to apply Windows updates to selected editions.

### MountImageCommand
```csharp
public IAsyncRelayCommand MountImageCommand { get; }
```
Command to mount the Windows image for exploration.

### ExtractFilesCommand
```csharp
public IAsyncRelayCommand ExtractFilesCommand { get; }
```
Command to extract files from the Windows image.

### RenameImageCommand
```csharp
public IAsyncRelayCommand RenameImageCommand { get; }
```
Command to rename the Windows image.

### EditIndexCommand
```csharp
public IAsyncRelayCommand<WindowsImageIndex> EditIndexCommand { get; }
```
Command to edit a specific Windows image index.

## Methods

### SetImageInfo
```csharp
public void SetImageInfo(WindowsImageInfo imageInfo)
```
Sets the image information to display and updates related properties.

## Usage Examples

### Navigation with Parameter
```csharp
// Navigate to image details page with image data
var navService = App.GetService<IJsonNavigationService>();
navService.NavigateTo(typeof(Views.ImageDetailsPage), selectedImage);
```

### ViewModel Setup in Page
```csharp
public sealed partial class ImageDetailsPage : Page
{
    public ImageDetailsViewModel ViewModel { get; }

    public ImageDetailsPage()
    {
        ViewModel = App.GetService<ImageDetailsViewModel>();
        this.DataContext = ViewModel;
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is WindowsImageInfo imageInfo)
        {
            ViewModel.SetImageInfo(imageInfo);
        }
    }
}
```

### Data Binding in XAML
```xml
<!-- Display image information -->
<TextBlock Text="{x:Bind ViewModel.ImageInfo.Name, Mode=OneWay}" />
<TextBlock Text="{x:Bind ViewModel.ImageInfo.FormattedFileSize, Mode=OneWay}" />

<!-- Command binding -->
<Button Content="Select All" Command="{x:Bind ViewModel.SelectAllIndicesCommand}" />
<Button Content="Export Image" Command="{x:Bind ViewModel.ExportImageCommand}" />

<!-- Conditional visibility -->
<TextBlock Text="{x:Bind ViewModel.ImageInfo.SourceIsoPath, Mode=OneWay}"
           Visibility="{x:Bind ViewModel.HasSourceIso, Mode=OneWay}" />
```

## Features

### Index Selection Management
- **Select All**: Marks all Windows editions as included for operations
- **Select None**: Clears selection from all Windows editions
- **Individual Selection**: Each edition can be toggled independently

### Image Information Display
- **Comprehensive Metadata**: File path, size, dates, type, source ISO
- **Edition Details**: Complete information about each Windows edition
- **Visual Organization**: Information grouped in logical sections

### Action Commands
- **Future Extensibility**: Commands are designed for advanced operations
- **User Feedback**: Informational dialogs for unimplemented features
- **Error Handling**: Comprehensive error handling with user-friendly messages

## Dependencies

- **CommunityToolkit.Mvvm**: MVVM infrastructure and commands
- **Bucket.Models.WindowsImageInfo**: Domain model for Windows images
- **Microsoft.UI.Xaml.Controls**: WinUI 3 dialog support
- **DevWinUI Navigation Services**: Page navigation functionality

## Related Files

- [`ImageDetailsPage.md`](../Views/ImageDetailsPage.md) - Associated view
- [`ImageManagementViewModel.md`](./ImageManagementViewModel.md) - Parent page ViewModel
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Primary domain model

## Best Practices

### ViewModel Initialization
```csharp
// Always initialize through dependency injection
var viewModel = App.GetService<ImageDetailsViewModel>();

// Set image data during navigation
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    if (e.Parameter is WindowsImageInfo imageInfo)
    {
        ViewModel.SetImageInfo(imageInfo);
    }
}
```

### Error Handling
```csharp
// Commands include comprehensive error handling
try
{
    // Perform operation
}
catch (Exception ex)
{
    Logger.Error(ex, "Operation failed for image: {Name}", ImageInfo.Name);
    await ShowErrorDialogAsync("Operation Error", ex.Message);
}
```

### Property Updates
```csharp
// Notify dependent properties when ImageInfo changes
public void SetImageInfo(WindowsImageInfo imageInfo)
{
    ImageInfo = imageInfo;
    OnPropertyChanged(nameof(HasSourceIso));
}
```

## Error Handling

### Common Scenarios
1. **Null Image Info**: Commands check for null ImageInfo before proceeding
2. **Navigation Errors**: Handles cases where no image parameter is provided
3. **Operation Failures**: User-friendly error messages for failed operations

### Error Recovery
- **Graceful Degradation**: Commands disable appropriately when operations are unavailable
- **User Feedback**: Clear error messages with actionable information
- **Logging**: Comprehensive logging for troubleshooting

## Performance Considerations

### Memory Management
- **Efficient Data Binding**: Uses x:Bind for better performance
- **Lazy Loading**: Commands are created only when needed
- **Resource Cleanup**: Proper disposal of resources and event handlers

### UI Responsiveness
- **Async Operations**: All potentially long-running operations are async
- **Progress Reporting**: User feedback for operations that take time
- **Non-blocking UI**: Commands don't block the UI thread

## Security Considerations

- **Input Validation**: Validates image information before display
- **Safe Operations**: Commands prevent unauthorized actions through proper validation
- **Error Disclosure**: Avoids exposing sensitive system information in errors

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
