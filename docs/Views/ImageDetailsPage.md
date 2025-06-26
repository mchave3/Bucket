# ImageDetailsPage Class Documentation

## Overview

A comprehensive page for viewing and editing detailed information about Windows images, including metadata, Windows editions, and available operations. This page provides an in-depth view of image properties and allows users to manage individual Windows editions within the image.

## Location

- **File**: `src/Views/ImageDetailsPage.xaml` and `src/Views/ImageDetailsPage.xaml.cs`
- **Namespace**: `Bucket.Views`

## Class Definition

```csharp
public sealed partial class ImageDetailsPage : Page
```

## Properties

### ViewModel

```csharp
public ImageDetailsViewModel ViewModel { get; }
```

Gets the ViewModel for this page, providing data binding and command functionality.

## Methods

### Constructor

```csharp
public ImageDetailsPage()
```

Initializes a new instance of the ImageDetailsPage class and sets up dependency injection.

### OnNavigatedTo

```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
```

Called when the page is navigated to, handles passing image information to the ViewModel.

## Usage Examples

### Navigation to Details Page

```csharp
// Navigate from image management with image parameter
var navService = App.GetService<IJsonNavigationService>();
navService.NavigateTo(typeof(Views.ImageDetailsPage), selectedImage);
```

### XAML Structure

```xml
<Page x:Class="Bucket.Views.ImageDetailsPage">
    <!-- Header with image name and actions -->
    <Grid>
        <TextBlock Text="{x:Bind ViewModel.ImageInfo.Name, Mode=OneWay}"
                   Style="{StaticResource TitleTextBlockStyle}" />

        <!-- Action buttons -->
        <Button Content="Edit Metadata"
                Command="{x:Bind ViewModel.EditMetadataCommand}" />
    </Grid>

    <!-- Scrollable content with information cards -->
    <ScrollViewer>
        <!-- Image information card -->
        <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
            <!-- File details -->
        </Border>

        <!-- Windows editions card -->
        <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
            <ListView ItemsSource="{x:Bind ViewModel.ImageInfo.Indices, Mode=OneWay}">
                <!-- Edition templates with checkboxes -->
            </ListView>
        </Border>
    </ScrollViewer>
</Page>
```

### Code-behind Implementation

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

## Features

### Comprehensive Image Information

- **File Details**: Path, size, creation/modification dates, image type
- **Source Information**: ISO source path if imported from ISO
- **Visual Organization**: Information grouped in styled cards

### Windows Editions Management

- **Edition Listing**: All Windows editions/indices in the image
- **Individual Selection**: Checkboxes for each edition
- **Bulk Operations**: Select all/none functionality
- **Edition Details**: Name, description, architecture, size

### Action Commands

- **Metadata Editing**: Edit image metadata
- **Image Export**: Export image to new location
- **Update Application**: Apply Windows updates
- **Image Mounting**: Mount for exploration
- **File Extraction**: Extract files from image

### Modern UI Design

- **Fluent Design**: Follows WinUI 3 and Fluent Design principles
- **Responsive Layout**: Adapts to different window sizes
- **Accessibility**: Proper automation properties and keyboard navigation
- **Theme Support**: Works with light and dark themes

## Data Binding

### Image Information Binding

```xml
<!-- Basic information -->
<TextBlock Text="{x:Bind ViewModel.ImageInfo.Name, Mode=OneWay}" />
<TextBlock Text="{x:Bind ViewModel.ImageInfo.FormattedFileSize, Mode=OneWay}" />
<TextBlock Text="{x:Bind ViewModel.ImageInfo.CreatedDate, Mode=OneWay}" />

<!-- Conditional display -->
<TextBlock Text="{x:Bind ViewModel.ImageInfo.SourceIsoPath, Mode=OneWay}"
           Visibility="{x:Bind ViewModel.HasSourceIso, Mode=OneWay}" />
```

### Collection Binding

```xml
<!-- Windows editions list -->
<ListView ItemsSource="{x:Bind ViewModel.ImageInfo.Indices, Mode=OneWay}"
          SelectionMode="None">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:WindowsImageIndex">
            <CheckBox IsChecked="{x:Bind IsIncluded, Mode=TwoWay}" />
            <TextBlock Text="{x:Bind DisplayText}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### Command Binding

```xml
<!-- Action buttons -->
<Button Content="Select All" Command="{x:Bind ViewModel.SelectAllIndicesCommand}" />
<Button Content="Export Image" Command="{x:Bind ViewModel.ExportImageCommand}" />
<Button Content="Apply Updates" Command="{x:Bind ViewModel.ApplyUpdatesCommand}" />
```

## Dependencies

### External Dependencies

- **Microsoft.UI.Xaml**: Core WinUI 3 framework
- **Bucket.ViewModels.ImageDetailsViewModel**: Page ViewModel
- **Bucket.Models**: Domain models for data binding

### Internal Dependencies

- **Navigation Service**: For page navigation
- **Dependency Injection**: ViewModel resolution through DI container
- **Logging**: Comprehensive logging for debugging and monitoring

## Related Files

- [`ImageDetailsViewModel.md`](../ViewModels/ImageDetailsViewModel.md) - Associated ViewModel
- [`ImageManagementPage.md`](./ImageManagementPage.md) - Parent page
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Primary data model

## Best Practices

### Navigation Pattern

```csharp
// Always pass image data as navigation parameter
navService.NavigateTo(typeof(ImageDetailsPage), imageInfo);

// Handle parameter in OnNavigatedTo
protected override void OnNavigatedTo(NavigationEventArgs e)
{
    if (e.Parameter is WindowsImageInfo imageInfo)
    {
        ViewModel.SetImageInfo(imageInfo);
    }
}
```

### Data Binding Performance

```xml
<!-- Use x:Bind for better performance -->
<TextBlock Text="{x:Bind ViewModel.ImageInfo.Name, Mode=OneWay}" />

<!-- Specify binding mode explicitly -->
<CheckBox IsChecked="{x:Bind IsIncluded, Mode=TwoWay}" />
```

### Accessibility Support

```xml
<!-- Provide automation properties -->
<Button AutomationProperties.Name="Edit Image Metadata"
        AutomationProperties.HelpText="Opens dialog to edit image metadata" />

<!-- Support keyboard navigation -->
<ListView AutomationProperties.Name="Windows Editions List"
          AutomationProperties.ItemType="Windows Edition" />
```

## Error Handling

### Common Error Scenarios

1. **No Image Parameter**: Handles navigation without image data
2. **Invalid Image Data**: Validates image information before display
3. **Command Failures**: User-friendly error messages for failed operations

### Error Recovery

- **Graceful Degradation**: Page displays appropriate messages for missing data
- **User Feedback**: Clear error dialogs for operation failures
- **Navigation Fallback**: Returns to parent page if critical errors occur

## Performance Notes

### Page Load Optimization

- **Fast Initialization**: Minimal processing during page construction
- **Efficient Data Binding**: Uses x:Bind for better performance
- **Lazy Loading**: Commands and resources loaded as needed

### Memory Management

- **Proper Cleanup**: ViewModel properly disposed when page is unloaded
- **Efficient Collections**: Uses ObservableCollection for automatic updates
- **Resource Management**: Images and large objects handled efficiently

## Security Considerations

- **Parameter Validation**: Validates navigation parameters before use
- **Safe Display**: Prevents injection attacks through data binding
- **Operation Security**: Commands prevent unauthorized actions through proper validation

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
