# WindowsImageInfo Class Documentation

## Overview

Represents a Windows image file (WIM/ESD) with its associated indices and metadata. This class serves as a container for managing Windows installation media, tracking both the file information and the available Windows editions contained within.

## Location

- **File**: `src/Models/WindowsImageInfo.cs`
- **Namespace**: `Bucket.Models`

## Class Definition

```csharp
public class WindowsImageInfo
```

## Properties

### Core Properties

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

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
