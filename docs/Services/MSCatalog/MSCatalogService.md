# MSCatalogService Class Documentation

## Overview
Implementation of the Microsoft Update Catalog service that provides functionality to search, download, and export Windows updates from the Microsoft Update Catalog website through web scraping and HTML parsing.

## Location
- **File**: `src/Services/MSCatalog/MSCatalogService.cs`
- **Namespace**: `Bucket.Services.MSCatalog`

## Class Definition
```csharp
public class MSCatalogService : IMSCatalogService
```

## Constructor
```csharp
public MSCatalogService()
```

Initializes a new instance with configured HttpClient including appropriate user agent headers.

## Constants
- `CatalogUrl`: "https://www.catalog.update.microsoft.com"
- `SearchUrl`: "https://www.catalog.update.microsoft.com/Search.aspx"
- `DownloadUrl`: "https://www.catalog.update.microsoft.com/DownloadDialog.aspx"

## Methods

### SearchUpdatesAsync
Performs a comprehensive search of the Microsoft Update Catalog with pagination support.

**Key Features:**
- Builds search queries based on mode (custom query or OS-based)
- Supports multi-page results with configurable limits
- Applies filtering and sorting to results
- Provides detailed logging throughout the process

### DownloadUpdateAsync
Downloads a single update file with progress reporting.

**Key Features:**
- Retrieves download links via POST request to DownloadDialog
- Creates destination directories automatically
- Provides real-time progress reporting
- Handles large file downloads efficiently

### DownloadMultipleUpdatesAsync
Manages batch downloads with overall progress tracking.

**Key Features:**
- Sequential download of multiple files
- Per-file and overall progress reporting
- Continues on individual failures
- Returns success status for entire batch

### ExportToExcelAsync
Exports update information to a formatted Excel file.

**Key Features:**
- Creates structured Excel worksheets
- Auto-formats columns and headers
- Includes all relevant update metadata
- Handles large datasets efficiently

## Private Methods

### BuildSearchQuery
Constructs the search query string based on request parameters.

### ExtractTotalPages
Parses HTML to determine total number of result pages.

### ParseUpdates
Extracts update information from HTML table rows.

### ParseUpdateRow
Converts individual HTML table row to MSCatalogUpdate object.

### ExtractAdditionalInfo
Extracts OS, version, and update type from title strings.

### ExtractWindowsVersion
Parses Windows version information from update titles.

### ParseSize
Converts human-readable size strings to bytes.

### ApplyFilters
Applies all configured filters to search results.

### ApplySorting
Sorts results based on specified criteria.

### GetDownloadLinksAsync
Retrieves actual download URLs for an update.

### SanitizeFileName
Ensures file names are valid for the file system.

## Usage Examples

### Basic Search Implementation
```csharp
var service = new MSCatalogService();
var request = new MSCatalogSearchRequest
{
    Mode = SearchMode.OperatingSystem,
    OperatingSystem = "Windows 11",
    Version = "22H2",
    UpdateType = "Cumulative Update"
};

var updates = await service.SearchUpdatesAsync(request);
```

### Download with Error Handling
```csharp
try
{
    var progress = new Progress<double>(p => 
        Console.WriteLine($"Progress: {p:P0}"));
    
    bool success = await service.DownloadUpdateAsync(
        update, 
        @"C:\Updates\file.cab", 
        progress);
    
    if (!success)
    {
        Logger.Warning("Download failed");
    }
}
catch (Exception ex)
{
    Logger.Error(ex, "Download error");
}
```

## Features
- HTML parsing with HtmlAgilityPack
- Robust error handling and logging
- Progress reporting for long operations
- Automatic directory creation
- File name sanitization
- Pagination support
- Comprehensive filtering options
- Excel export functionality

## Dependencies
- HtmlAgilityPack for HTML parsing
- EPPlus for Excel generation
- System.Net.Http for web requests
- System.Web for URL encoding
- Serilog for logging

## Error Handling
- Network failures logged and re-thrown
- HTML parsing errors caught and logged
- File system errors handled gracefully
- Invalid inputs validated before processing

## Performance Considerations
- Uses streaming for large file downloads
- Configurable buffer size (8192 bytes)
- Efficient HTML parsing with XPath
- Minimal memory usage for Excel exports

## Security Considerations
- Validates and sanitizes file paths
- Uses HTTPS for all communications
- Includes proper user agent headers
- Handles untrusted HTML content safely

## Related Files
- [`IMSCatalogService.md`](./IMSCatalogService.md) - Service interface
- [`MSCatalogUpdate.md`](../../Models/MSCatalogUpdate.md) - Update model
- [`MSCatalogSearchRequest.md`](../../Models/MSCatalogSearchRequest.md) - Search request model

## Best Practices
1. Dispose of service when done (implements IDisposable)
2. Use cancellation tokens for all operations
3. Handle pagination limits appropriately
4. Validate file paths before downloads
5. Log all significant operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 