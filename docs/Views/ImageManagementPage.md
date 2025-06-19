# ImageManagementPage Class Documentation

## Overview

A WinUI 3 page for managing Windows images and their editions. This page provides a modern, intuitive interface for viewing, importing, and managing Windows installation media, displaying detailed information about each image and its contained Windows editions.

## Location

- **File**: `src/Views/ImageManagementPage.xaml` + `src/Views/ImageManagementPage.xaml.cs`
- **Namespace**: `Bucket.Views`

## Class Definition

```csharp
public sealed partial class ImageManagementPage : Page
```

## Properties

- **`ViewModel`** (ImageManagementViewModel): Gets the associated ViewModel for this page

## Features

### Modern UI Design

- **Fluent Design**: Follows Microsoft Fluent Design System principles
- **Responsive Layout**: Adapts to different screen sizes and orientations
- **Dark/Light Theme**: Supports system theme preferences
- **Accessibility**: Implements proper accessibility features

### Image Display

- **Card-based Layout**: Each image displayed in an attractive card format
- **Rich Information**: Shows image name, type, size, and edition count
- **Index Preview**: Displays contained Windows editions inline
- **File Status**: Visual indicators for file existence and health

### Interactive Features

- **Real-time Search**: Instant filtering as user types
- **Selection Management**: Single-click selection with visual feedback
- **Context Actions**: Action buttons for each image
- **Bulk Operations**: Multiple selection support for batch operations

### Action Buttons

- **Import from ISO**: Opens ISO import wizard
- **Import WIM**: Direct WIM file import
- **Refresh**: Reload images from storage
- **Delete Options**: Remove from list or delete file completely

## Usage Examples

### XAML Structure

```xml
<Page x:Class="Bucket.Views.ImageManagementPage">
    <Page.DataContext>
        <viewmodels:ImageManagementViewModel />
    </Page.DataContext>

    <!-- Page content with data binding -->
    <ListView ItemsSource="{Binding FilteredImages}"
              SelectedItem="{Binding SelectedImage, Mode=TwoWay}" />
</Page>
```

### Code-behind Navigation

```csharp
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);

    // Initialize ViewModel when page loads
    await ViewModel.InitializeAsync();
}
```

### Custom Data Templates

```xml
<DataTemplate x:DataType="models:WindowsImageInfo">
    <Grid>
        <!-- Image information display -->
        <TextBlock Text="{x:Bind Name}" />
        <TextBlock Text="{x:Bind FormattedFileSize}" />

        <!-- Indices display -->
        <ItemsControl ItemsSource="{x:Bind Indices}">
            <!-- Index template -->
        </ItemsControl>
    </Grid>
</DataTemplate>
```

## UI Components

### Header Section

- **Page Title**: "Windows Image Management"
- **Description**: Contextual help text
- **Action Buttons**: Primary import and refresh actions

### Search and Filter

- **Search Box**: Real-time filtering with search icon
- **Results Counter**: Shows filtered vs total count
- **Clear Search**: Easy way to reset filters

### Main Content Area

- **Loading State**: Progress ring during data loading
- **Empty State**: Helpful message when no images found
- **Image List**: Scrollable list of image cards

### Status Bar

- **Status Messages**: Current operation status
- **Selected Actions**: Context-sensitive action buttons
- **Progress Indicators**: For long-running operations

## Data Binding

### Two-way Binding

```xml
<!-- Search text binding -->
<TextBox Text="{Binding SearchText, Mode=TwoWay}" />

<!-- Selection binding -->
<ListView SelectedItem="{Binding SelectedImage, Mode=TwoWay}" />
```

### Command Binding

```xml
<!-- Button commands -->
<Button Command="{Binding RefreshCommand}" />
<Button Command="{Binding DeleteSelectedCommand}"
        IsEnabled="{Binding SelectedImage, Converter={StaticResource ObjectToBooleanConverter}}" />
```

### Collection Binding

```xml
<!-- Dynamic collections -->
<ListView ItemsSource="{Binding FilteredImages}" />
<ItemsControl ItemsSource="{x:Bind SelectedImage.Indices}" />
```

## Dependencies

- **Microsoft.UI.Xaml**: WinUI 3 framework
- **Bucket.ViewModels.ImageManagementViewModel**: Page ViewModel
- **Bucket.Models**: Domain models for data binding
- **System theme resources**: For consistent styling

## Related Files

- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - Associated ViewModel
- [`WindowsImageInfo.md`](../Models/WindowsImageInfo.md) - Primary data model
- [`ImageManagementPage.xaml`] - XAML markup definition

## Best Practices

### MVVM Pattern

```csharp
// Proper ViewModel access
public ImageManagementViewModel ViewModel => (ImageManagementViewModel)DataContext;

// Avoid code-behind logic
// Put business logic in ViewModel instead
```

### Performance Optimization

```xml
<!-- Use x:Bind for better performance -->
<TextBlock Text="{x:Bind Name}" />

<!-- Virtualization for large lists -->
<ListView ItemsSource="{Binding Images}">
    <ListView.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel />
        </ItemsPanelTemplate>
    </ListView.ItemsPanel>
</ListView>
```

### Accessibility

```xml
<!-- Proper accessibility support -->
<Button AutomationProperties.Name="Import from ISO"
        AutomationProperties.HelpText="Import Windows images from ISO files" />

<ListView AutomationProperties.Name="Windows Images List"
          AutomationProperties.ItemType="Windows Image" />
```

## Styling and Theming

### Resource Usage

```xml
<!-- Use system resources for consistency -->
<TextBlock Style="{StaticResource TitleTextBlockStyle}" />
<Button Style="{StaticResource AccentButtonStyle}" />
```

### Custom Styling

```xml
<!-- Custom styles for specific needs -->
<Style x:Key="ImageCardStyle" TargetType="Border">
    <Setter Property="Background" Value="{ThemeResource CardBackgroundFillColorDefaultBrush}" />
    <Setter Property="CornerRadius" Value="8" />
</Style>
```

## Error Handling

### UI Error States

- **Loading Errors**: Display user-friendly error messages
- **Empty States**: Helpful guidance when no data available
- **Network Issues**: Graceful degradation for connectivity problems

### User Feedback

```csharp
// Error handling in code-behind (minimal)
private async void OnPageLoadError(Exception ex)
{
    Logger.Error(ex, "Failed to load page");
    // ViewModel handles most error scenarios
}
```

## Performance Considerations

### Virtualization

- **Large Collections**: Use VirtualizingStackPanel for performance
- **Image Loading**: Lazy load image thumbnails if implemented
- **Memory Management**: Dispose of resources when navigating away

### Responsive Design

- **Adaptive Layout**: Adjust layout based on window size
- **Progressive Loading**: Load essential content first
- **Smooth Animations**: Use built-in WinUI 3 animations

## Security Considerations

- **Input Validation**: Validate all user inputs at ViewModel level
- **File Access**: Handle file permission issues gracefully
- **Error Disclosure**: Avoid exposing sensitive system information

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
