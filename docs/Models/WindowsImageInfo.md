# WindowsImageInfo Class Documentation

## Overview

The `WindowsImageInfo` class represents a Windows image file (WIM/ESD) with its associated indices and metadata. This is an observable data model that supports proper UI binding and real-time updates in MVVM scenarios.

**Recent Update**: The class has been converted to use `ObservableObject` from CommunityToolkit.Mvvm to support proper data binding and UI notifications when properties change.

## Location

- **File**: `src/Models/WindowsImageInfo.cs`
- **Namespace**: `Bucket.Models`

## Class Definition

```csharp
public partial class WindowsImageInfo : ObservableObject
```

## Properties

### Core Observable Properties

All core properties now use `[ObservableProperty]` for automatic UI notification:

- **`Id`** (string): Unique identifier for this image
- **`Name`** (string): Display name of the image
- **`FilePath`** (string): Full path to the image file
- **`ImageType`** (string): Type of image file (WIM, ESD, etc.)
- **`CreatedDate`** (DateTime): Creation date of the image
- **`ModifiedDate`** (DateTime): Last modified date of the image
- **`FileSizeBytes`** (long): Total size of the image file in bytes
- **`SourceIsoPath`** (string): Source ISO path if imported from ISO
- **`Indices`** (ObservableCollection<WindowsImageIndex>): Collection of Windows indices

### Computed Properties

- **`FormattedFileSize`** (string): Formatted file size for display
- **`IndexCount`** (int): Number of indices in this image
- **`IncludedIndexCount`** (int): Number of included indices
- **`FileName`** (string): File name from the full path
- **`FileExists`** (bool): Whether the image file exists on disk
- **`IncludedIndicesSummary`** (string): Summary of included indices

## Usage Examples

### Creating a new Windows image info

```csharp
var imageInfo = new WindowsImageInfo
{
    Name = "Windows 11 22H2",
    FilePath = @"C:\Images\win11_22h2.wim",
    ImageType = "WIM",
    CreatedDate = DateTime.Now,
    ModifiedDate = DateTime.Now,
    FileSizeBytes = 4500000000, // ~4.5 GB
    SourceIsoPath = @"C:\ISOs\windows11.iso"
};

// Add indices
imageInfo.Indices.Add(new WindowsImageIndex
{
    Index = 1,
    Name = "Windows 11 Home",
    Architecture = "x64"
});
```

### Working with indices

```csharp
// Check available editions
Console.WriteLine($"Image contains {imageInfo.IndexCount} editions");
Console.WriteLine($"Selected: {imageInfo.IncludedIndexCount}");

// Filter included indices
var selectedIndices = imageInfo.Indices.Where(i => i.IsIncluded).ToList();
```

## Features

- **Automatic Size Formatting**: Converts bytes to appropriate units (KB, MB, GB)
- **File System Integration**: Validates file existence and tracks metadata
- **Index Management**: Maintains collection of available Windows editions
- **Selection Tracking**: Monitors which indices are selected for operations
- **ISO Source Tracking**: Remembers original ISO source for imported images

## Dependencies

- **System.Collections.ObjectModel**: For ObservableCollection support
- **Bucket.Models.WindowsImageIndex**: For index information

## Related Files

- [`WindowsImageIndex.md`](./WindowsImageIndex.md) - Individual Windows editions
- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - ViewModel using this model
- [`WindowsImageService.md`](../Services/WindowsImageService.md) - Service managing these objects

## Best Practices

### File Path Management

```csharp
// Always use absolute paths
imageInfo.FilePath = Path.GetFullPath(relativePath);

// Validate file existence before operations
if (imageInfo.FileExists)
{
    // Proceed with operations
}
```

### Index Management

```csharp
// Use ObservableCollection for UI binding
imageInfo.Indices.CollectionChanged += OnIndicesChanged;

// Bulk operations on indices
foreach (var index in imageInfo.Indices)
{
    index.IsIncluded = shouldInclude;
}
```

### Display Information

```csharp
// Use computed properties for UI
<TextBlock Text="{Binding FormattedFileSize}" />
<TextBlock Text="{Binding IncludedIndicesSummary}" />
```

## Error Handling

### File System Errors

- **Missing Files**: Check `FileExists` property before file operations
- **Access Denied**: Handle permission issues when accessing image files
- **Disk Space**: Validate available space before large file operations

### Index Validation

```csharp
// Ensure indices are valid
if (imageInfo.Indices.Any(i => i.Index <= 0))
{
    throw new InvalidOperationException("Invalid index numbers detected");
}
```

## Performance Notes

- **Large Files**: WIM files can be several GB - consider progress reporting for operations
- **Index Loading**: Defer index analysis until needed to improve startup performance
- **File Validation**: Cache file existence checks to avoid repeated file system access

## Issue Resolution: UI Binding Problems

### Problem Description
Previously, when users clicked on an image in the Image Management page, the details panel on the right side would not update properly, showing "No image selected" instead of the actual image details.

### Root Cause
The `WindowsImageInfo` class was a simple POCO (Plain Old CLR Object) without property change notifications. This meant that when the UI bound to properties like `SelectedImage.Name`, `SelectedImage.FormattedFileSize`, etc., the binding system couldn't detect when these properties changed, resulting in a static UI that didn't reflect the current selection.

### Solution Implemented
**Observable Object Pattern**: Converted `WindowsImageInfo` to inherit from `ObservableObject` and use `[ObservableProperty]` attributes:

```csharp
// OLD: Simple properties (no notifications)
public class WindowsImageInfo
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    // ... other properties
}

// NEW: Observable properties with automatic notifications
public partial class WindowsImageInfo : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string filePath = string.Empty;
    // ... other properties with [ObservableProperty]
}
```

### Property Change Notification Chain
The solution includes automatic notification for calculated properties:

- When `ImageType` changes → `ImageTypeDisplay` updates
- When `FileSizeBytes` changes → `FormattedFileSize` updates
- When `FilePath` changes → `FileName` and `FileExists` update
- When `Indices` collection changes → `IndexCount`, `IncludedIndexCount`, and `IncludedIndicesSummary` update

### Benefits of the Fix
- **Real-time UI Updates**: Image details now appear immediately when selecting an image
- **Proper Data Binding**: All calculated properties update automatically when base properties change
- **Collection Monitoring**: Changes to the indices collection trigger appropriate UI updates
- **MVVM Compliance**: Follows proper MVVM patterns with observable data models

### Testing the Fix
After updating, users should see:
1. Image details appear immediately when clicking on an image
2. All image information displayed correctly (name, size, dates, file path)
3. Windows editions/indices list populated properly
4. Real-time updates when image data changes

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
