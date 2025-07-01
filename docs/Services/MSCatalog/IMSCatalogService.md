# IMSCatalogService Interface Documentation

## Overview
Interface for the Microsoft Update Catalog service that provides methods for searching, downloading, and exporting Windows updates from the Microsoft Update Catalog.

## Location
- **File**: `src/Services/MSCatalog/IMSCatalogService.cs`
- **Namespace**: `Bucket.Services.MSCatalog`

## Interface Definition
```csharp
public interface IMSCatalogService
```

## Methods

### SearchUpdatesAsync
Searches for updates in the Microsoft Update Catalog based on the provided search criteria.

```csharp
Task<IEnumerable<MSCatalogUpdate>> SearchUpdatesAsync(
    MSCatalogSearchRequest request, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `request`: The search request parameters
- `cancellationToken`: Cancellation token for async operation

**Returns:** A collection of matching updates

### DownloadUpdateAsync
Downloads a single update to the specified location.

```csharp
Task<bool> DownloadUpdateAsync(
    MSCatalogUpdate update, 
    string destinationPath, 
    IProgress<double> progress = null, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `update`: The update to download
- `destinationPath`: The destination folder path
- `progress`: Optional progress reporter (0-1 range)
- `cancellationToken`: Cancellation token

**Returns:** True if download succeeded, false otherwise

### DownloadMultipleUpdatesAsync
Downloads multiple updates with progress tracking.

```csharp
Task<bool> DownloadMultipleUpdatesAsync(
    IEnumerable<MSCatalogUpdate> updates, 
    string destinationPath, 
    IProgress<DownloadProgress> progress = null, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `updates`: The updates to download
- `destinationPath`: The destination folder path
- `progress`: Optional progress reporter for multi-file downloads
- `cancellationToken`: Cancellation token

**Returns:** True if all downloads succeeded, false otherwise

### ExportToExcelAsync
Exports update information to an Excel file.

```csharp
Task<bool> ExportToExcelAsync(
    IEnumerable<MSCatalogUpdate> updates, 
    string filePath, 
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `updates`: The updates to export
- `filePath`: The file path to save the Excel file
- `cancellationToken`: Cancellation token

**Returns:** True if export succeeded, false otherwise

## Supporting Classes

### DownloadProgress
Represents download progress for multiple files.

```csharp
public class DownloadProgress
{
    public string CurrentFile { get; set; }
    public int CurrentFileIndex { get; set; }
    public int TotalFiles { get; set; }
    public double CurrentFileProgress { get; set; }
    public double OverallProgress { get; set; }
    public long BytesPerSecond { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}
```

## Usage Examples

### Basic Search
```csharp
var service = GetService<IMSCatalogService>();
var request = new MSCatalogSearchRequest
{
    Mode = SearchMode.SearchQuery,
    SearchQuery = "Windows 11 cumulative"
};

var updates = await service.SearchUpdatesAsync(request);
```

### Download with Progress
```csharp
var progress = new Progress<double>(percent =>
{
    Console.WriteLine($"Download progress: {percent:P0}");
});

bool success = await service.DownloadUpdateAsync(
    update, 
    @"C:\Downloads", 
    progress, 
    cancellationToken);
```

### Batch Download
```csharp
var downloadProgress = new Progress<DownloadProgress>(p =>
{
    Console.WriteLine($"File {p.CurrentFileIndex}/{p.TotalFiles}: {p.CurrentFile}");
    Console.WriteLine($"Overall: {p.OverallProgress:P0}");
});

bool allSuccess = await service.DownloadMultipleUpdatesAsync(
    selectedUpdates,
    @"C:\Downloads\Updates",
    downloadProgress,
    cancellationToken);
```

### Export Results
```csharp
bool exported = await service.ExportToExcelAsync(
    searchResults,
    @"C:\Reports\UpdateCatalog.xlsx");
```

## Implementation Requirements
- Must handle HTML parsing of Microsoft Update Catalog website
- Should implement proper error handling and retry logic
- Must support cancellation for long-running operations
- Should validate file paths and handle file system errors
- Must properly dispose of HTTP resources

## Dependencies
- `MSCatalogUpdate` model
- `MSCatalogSearchRequest` model
- `DownloadProgress` class
- System.Threading for async operations

## Related Files
- [`MSCatalogService.md`](./MSCatalogService.md) - Service implementation
- [`MSCatalogUpdate.md`](../../Models/MSCatalogUpdate.md) - Update model
- [`MSCatalogSearchRequest.md`](../../Models/MSCatalogSearchRequest.md) - Search request model

## Best Practices
1. Always provide cancellation tokens for long operations
2. Use progress reporting for better user experience
3. Handle network failures gracefully
4. Validate inputs before calling methods
5. Dispose of service properly if it implements IDisposable

## Error Handling
- Network errors should be caught and logged
- Invalid file paths should return false, not throw
- HTML parsing errors should be handled gracefully
- Download failures should not stop batch operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 