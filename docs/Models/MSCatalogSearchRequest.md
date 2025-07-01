# MSCatalogSearchRequest Class Documentation

## Overview
Represents a search request for the Microsoft Update Catalog with comprehensive filtering and search options. Supports both custom query searches and operating system-based searches.

## Location
- **File**: `src/Models/MSCatalogSearchRequest.cs`
- **Namespace**: `Bucket.Models`

## Class Definition
```csharp
public class MSCatalogSearchRequest
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Mode` | `SearchMode` | - | The search mode (OperatingSystem or SearchQuery) |
| `SearchQuery` | `string` | - | The search query (used in SearchQuery mode) |
| `OperatingSystem` | `string` | - | The operating system to search for (used in OperatingSystem mode) |
| `Version` | `string` | - | The OS version (e.g., "24H2", "23H2") |
| `UpdateType` | `string` | - | The update type (e.g., "Cumulative Updates", "Security Updates") |
| `Architecture` | `string` | "All" | The architecture filter (e.g., "x64", "x86", "ARM64", "All") |
| `StrictSearch` | `bool` | false | Whether to use strict search (exact phrase matching) |
| `AllPages` | `bool` | false | Whether to search through all available pages |
| `IncludePreview` | `bool` | false | Whether to include preview updates |
| `IncludeDynamic` | `bool` | false | Whether to include dynamic updates |
| `ExcludeFramework` | `bool` | false | Whether to exclude .NET Framework updates |
| `GetFramework` | `bool` | false | Whether to only show .NET Framework updates |
| `FromDate` | `DateTime?` | null | The from date filter |
| `ToDate` | `DateTime?` | null | The to date filter |
| `MinSize` | `double?` | null | The minimum size filter in MB |
| `MaxSize` | `double?` | null | The maximum size filter in MB |
| `SortBy` | `string` | "Date" | The field to sort by (e.g., "Date", "Size", "Title", "Classification", "Product") |
| `Descending` | `bool` | true | Whether to sort in descending order |

## Enums

### SearchMode
```csharp
public enum SearchMode
{
    OperatingSystem,  // Search by operating system parameters
    SearchQuery       // Search using a custom query string
}
```

## Usage Examples

### Operating System Search
```csharp
var request = new MSCatalogSearchRequest
{
    Mode = SearchMode.OperatingSystem,
    OperatingSystem = "Windows 11",
    Version = "22H2",
    UpdateType = "Cumulative Updates",
    Architecture = "x64",
    ExcludeFramework = true,
    AllPages = true
};
```

### Custom Query Search
```csharp
var request = new MSCatalogSearchRequest
{
    Mode = SearchMode.SearchQuery,
    SearchQuery = "KB5031354",
    IncludePreview = false,
    FromDate = DateTime.Now.AddMonths(-6),
    SortBy = "Date"
};
```

### Advanced Filtering
```csharp
var request = new MSCatalogSearchRequest
{
    Mode = SearchMode.OperatingSystem,
    OperatingSystem = "Windows Server 2022",
    IncludePreview = false,
    IncludeDynamic = false,
    FromDate = new DateTime(2023, 1, 1),
    ToDate = DateTime.Now,
    MinSize = 100,  // 100 MB minimum
    MaxSize = 1000, // 1000 MB maximum
    SortBy = "Size",
    Descending = true
};
```

## Features
- Dual search modes for flexibility
- Comprehensive filtering options
- Date range filtering
- Size range filtering
- Architecture-specific filtering
- Preview and dynamic update filtering
- Customizable sorting options
- Support for .NET Framework-specific searches

## Dependencies
- System namespace for DateTime type

## Related Files
- [`MSCatalogUpdate.md`](./MSCatalogUpdate.md) - Update entry model
- [`IMSCatalogService.md`](../Services/MSCatalog/IMSCatalogService.md) - Service interface
- [`MSCatalogService.md`](../Services/MSCatalog/MSCatalogService.md) - Service implementation

## Best Practices
1. Choose appropriate search mode based on use case
2. Use date filters to limit results to recent updates
3. Apply architecture filters when targeting specific systems
4. Use size filters to manage download bandwidth
5. Exclude preview updates for production deployments
6. Set AllPages carefully as it may result in many API calls

## Error Handling
- Validate date ranges (FromDate should be before ToDate)
- Ensure size values are positive numbers
- Validate architecture values against known options
- Handle null values appropriately for optional properties

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 