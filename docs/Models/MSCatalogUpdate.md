# MSCatalogUpdate Class Documentation

## Overview
Represents a Microsoft Update Catalog update entry with comprehensive information about Windows updates including title, classification, size, and download details.

## Location
- **File**: `src/Models/MSCatalogUpdate.cs`
- **Namespace**: `Bucket.Models`

## Class Definition
```csharp
public class MSCatalogUpdate
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Title` | `string` | The title of the update |
| `Products` | `string` | The products this update applies to |
| `Classification` | `string` | The classification of the update (e.g., Security Updates, Critical Updates) |
| `LastUpdated` | `DateTime` | The last updated date |
| `Version` | `string` | The version of the update |
| `Size` | `string` | The size of the update as a formatted string (e.g., "302 MB") |
| `SizeInBytes` | `long` | The size of the update in bytes |
| `Guid` | `string` | The unique GUID of the update |
| `FileNames` | `string[]` | The file names associated with this update |
| `IsSelected` | `bool` | Whether this update is selected for download |
| `OperatingSystem` | `string` | The operating system this update is for (populated when searching by OS) |
| `OSVersion` | `string` | The OS version this update is for (populated when searching by OS) |
| `UpdateType` | `string` | The update type (populated when searching by update type) |

## Usage Examples

### Creating a New Update Entry
```csharp
var update = new MSCatalogUpdate
{
    Title = "2023-10 Cumulative Update for Windows 11 Version 22H2 (KB5031354)",
    Products = "Windows 11",
    Classification = "Security Updates",
    LastUpdated = DateTime.Now,
    Version = "n/a",
    Size = "302 MB",
    SizeInBytes = 316669952,
    Guid = "12345678-1234-1234-1234-123456789012",
    OperatingSystem = "Windows 11",
    OSVersion = "22H2",
    UpdateType = "Cumulative Update"
};
```

### Checking if Update is Selected
```csharp
if (update.IsSelected)
{
    // Process selected update
    Console.WriteLine($"Downloading: {update.Title}");
}
```

### Working with File Names
```csharp
if (update.FileNames != null && update.FileNames.Length > 0)
{
    foreach (var fileName in update.FileNames)
    {
        Console.WriteLine($"File: {fileName}");
    }
}
```

## Features
- Represents complete update metadata from Microsoft Update Catalog
- Supports selection state for batch operations
- Includes both formatted and raw size information
- Tracks OS-specific information when available
- Stores associated file names for download operations

## Dependencies
- System namespace for DateTime type

## Related Files
- [`MSCatalogSearchRequest.md`](./MSCatalogSearchRequest.md) - Search request parameters
- [`IMSCatalogService.md`](../Services/MSCatalog/IMSCatalogService.md) - Service interface for catalog operations
- [`MSCatalogService.md`](../Services/MSCatalog/MSCatalogService.md) - Service implementation

## Best Practices
1. Always check for null values in optional properties like FileNames
2. Use SizeInBytes for calculations and Size for display
3. Validate GUID format when creating instances manually
4. Consider using IsSelected for batch operations
5. Populate OS-specific properties when searching by operating system

## Error Handling
- Ensure DateTime values are valid when setting LastUpdated
- Validate GUID format matches expected pattern
- Handle null or empty FileNames arrays gracefully
- Check for negative SizeInBytes values

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 