# IWindowsImageMetadataService Interface Documentation

## Overview

Interface for Windows image metadata management. This service handles loading, saving, and managing Windows image metadata collections stored as JSON files. It provides CRUD operations for image metadata and manages the persistence layer.

## Location

- **File**: `src/Services/WindowsImage/IWindowsImageMetadataService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition

```csharp
public interface IWindowsImageMetadataService
```

## Methods

### GetImagesAsync

```csharp
Task<ObservableCollection<WindowsImageInfo>> GetImagesAsync(CancellationToken cancellationToken = default)
```

Gets all available Windows images asynchronously from the metadata storage.

**Parameters:**
- `cancellationToken`: Optional cancellation token for async operation

**Returns:** A collection of Windows image information

### SaveImagesAsync

```csharp
Task SaveImagesAsync(IEnumerable<WindowsImageInfo> images, CancellationToken cancellationToken = default)
```

Saves the Windows images metadata asynchronously to persistent storage.

**Parameters:**
- `images`: The collection of images to save
- `cancellationToken`: Optional cancellation token for async operation

### RemoveImageAsync

```csharp
Task RemoveImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default)
```

Removes an image from the metadata collection and updates persistent storage.

**Parameters:**
- `imageInfo`: The image to remove
- `cancellationToken`: Optional cancellation token for async operation

### UpdateImageAsync

```csharp
Task UpdateImageAsync(WindowsImageInfo imageInfo, CancellationToken cancellationToken = default)
```

Updates an existing image in the metadata collection and saves to persistent storage.

**Parameters:**
- `imageInfo`: The image to update
- `cancellationToken`: Optional cancellation token for async operation

### GetImagesDataPath

```csharp
string GetImagesDataPath()
```

Gets the path to the images data file.

**Returns:** The path to the images metadata file

### GetImagesDirectory

```csharp
string GetImagesDirectory()
```

Gets the path to the images directory.

**Returns:** The path to the images directory

## Usage Examples

```csharp
// Injecting the service
public class ImageManagementService
{
    private readonly IWindowsImageMetadataService _metadataService;

    public ImageManagementService(IWindowsImageMetadataService metadataService)
    {
        _metadataService = metadataService;
    }

    // Load all images
    public async Task<ObservableCollection<WindowsImageInfo>> LoadImagesAsync()
    {
        return await _metadataService.GetImagesAsync();
    }

    // Add a new image
    public async Task AddImageAsync(WindowsImageInfo newImage)
    {
        var images = await _metadataService.GetImagesAsync();
        images.Add(newImage);
        await _metadataService.SaveImagesAsync(images);
    }

    // Remove an image
    public async Task RemoveImageAsync(WindowsImageInfo imageToRemove)
    {
        await _metadataService.RemoveImageAsync(imageToRemove);
    }

    // Update image metadata
    public async Task UpdateImageAsync(WindowsImageInfo updatedImage)
    {
        await _metadataService.UpdateImageAsync(updatedImage);
    }
}
```

## Features

- **JSON Persistence**: Stores metadata in JSON format for easy readability and debugging
- **Observable Collections**: Returns ObservableCollection for automatic UI binding
- **Async Operations**: All operations are asynchronous to prevent UI blocking
- **CRUD Operations**: Complete Create, Read, Update, Delete functionality
- **Path Management**: Centralized path management for data files and directories
- **Cancellation Support**: All async operations support cancellation tokens

## Dependencies

- **System.Collections.ObjectModel**: Observable collections
- **Bucket.Models**: WindowsImageInfo model
- **System.Threading**: Cancellation token support
- **System.Threading.Tasks**: Async operations support

## Implementation

This interface is implemented by [`WindowsImageMetadataService`](./WindowsImageMetadataService.md).

## Related Files

- [`WindowsImageMetadataService.md`](./WindowsImageMetadataService.md) - Implementation
- [`WindowsImageInfo.md`](../../Models/WindowsImageInfo.md) - Data model
- [`WindowsImageService.md`](../WindowsImageService.md) - Main coordinator service
- [`IWindowsImageFileService.md`](./IWindowsImageFileService.md) - File operations

## Best Practices

- Always use async methods to avoid blocking the UI thread
- Handle cancellation tokens appropriately in long-running operations
- Use ObservableCollection binding for automatic UI updates
- Validate image data before persistence operations
- Handle file system exceptions gracefully

## Error Handling

- File not found scenarios for metadata files
- JSON parsing/serialization errors
- Access denied exceptions for file operations
- Invalid image data validation
- Cancellation during async operations

## Security Considerations

- **File Path Validation**: Ensure metadata file paths are secure
- **Data Validation**: Validate image metadata before persistence
- **Access Control**: Ensure appropriate file system permissions

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
