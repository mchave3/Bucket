# WindowsImageIndex Class Documentation

## Overview
Represents a Windows image index with its associated metadata. This class models individual Windows editions contained within a WIM or ESD file, providing detailed information about each available installation option.

## Location
- **File**: `src/Models/WindowsImageIndex.cs`
- **Namespace**: `Bucket.Models`

## Class Definition
```csharp
public class WindowsImageIndex
```

## Properties

### Core Properties
- **`Index`** (int): The index number of the Windows image
- **`Name`** (string): The display name of the Windows edition (e.g., "Windows 11 Pro")
- **`Description`** (string): The detailed description of the Windows edition
- **`Architecture`** (string): The architecture of the Windows image (x86, x64, ARM64)
- **`SizeMB`** (double): The size of the image in megabytes
- **`IsIncluded`** (bool): Whether this index is included/selected for operations

### Computed Properties
- **`FormattedSize`** (string): Gets the formatted size string for display purposes (e.g., "4.2 GB")
- **`DisplayText`** (string): Gets the formatted display text for the index (e.g., "Index 1: Windows 11 Pro")
- **`IndexDisplayText`** (string): Gets the index display text for the blue badge (e.g., "Index 5:")

## Usage Examples

### Creating a new Windows image index
```csharp
var index = new WindowsImageIndex
{
    Index = 1,
    Name = "Windows 11 Pro",
    Description = "Windows 11 Pro for business and advanced users",
    Architecture = "x64",
    SizeMB = 4256.7,
    IsIncluded = true
};

// Display formatted information
Console.WriteLine(index.DisplayText); // "Index 1: Windows 11 Pro"
Console.WriteLine(index.FormattedSize); // "4256.7 MB"
```

### Working with collections
```csharp
var indices = new List<WindowsImageIndex>();
// ... populate indices from DISM output

// Find specific editions
var proEditions = indices.Where(i => i.Name.Contains("Pro")).ToList();
var includedIndices = indices.Where(i => i.IsIncluded).ToList();
```

## Features

- **Automatic Formatting**: Provides formatted display strings for UI binding
- **Selection Support**: Built-in selection state for import/export operations
- **Architecture Awareness**: Tracks processor architecture for compatibility
- **Size Management**: Handles size information in appropriate units

## Dependencies

- **System namespaces**: Standard .NET types
- **No external dependencies**: Self-contained model class

## Related Files
- [`WindowsImageInfo.md`](./WindowsImageInfo.md) - Container for multiple indices
- [`ImageManagementViewModel.md`](../ViewModels/ImageManagementViewModel.md) - ViewModel that uses this model
- [`WindowsImageService.md`](../Services/WindowsImageService.md) - Service that creates instances

## Best Practices

### Data Validation
- Always validate index numbers are positive
- Ensure architecture strings are standardized (x86, x64, ARM64)
- Check that size values are reasonable

### UI Binding
```csharp
// Use computed properties for data binding
<TextBlock Text="{Binding DisplayText}" />
<TextBlock Text="{Binding FormattedSize}" />
```

### Selection Management
```csharp
// Legacy property (IsIncluded) - maintained for compatibility
index.IsIncluded = !index.IsIncluded;

// New single-selection property (IsSelected) - used in modern UI
index.IsSelected = true; // Select this index
otherIndices.ForEach(i => i.IsSelected = false); // Deselect others

// Single-selection pattern (recommended)
void SelectIndex(WindowsImageIndex targetIndex, List<WindowsImageIndex> allIndices)
{
    allIndices.ForEach(i => i.IsSelected = false); // Clear all
    targetIndex.IsSelected = true; // Select target
}
```

## Error Handling

### Common Scenarios
- **Invalid Index**: Handle cases where index numbers are 0 or negative
- **Missing Names**: Provide fallback display text when Name is empty
- **Size Calculations**: Handle edge cases where size information is unavailable

### Example Error Handling
```csharp
public string SafeDisplayText => string.IsNullOrEmpty(Name)
    ? $"Index {Index}: Unknown Edition"
    : DisplayText;
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
