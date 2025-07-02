# MSCatalogService Class Documentation

## Overview
Implementation of the Microsoft Update Catalog service that provides functionality to search, download, and export Windows updates from the Microsoft Update Catalog website through web scraping and HTML parsing. Features robust error handling, comprehensive logging, and enhanced parsing capabilities.

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

Initializes a new instance with configured HttpClient including appropriate user agent headers for optimal compatibility with Microsoft Update Catalog.

## Constants
- `CatalogUrl`: "https://www.catalog.update.microsoft.com"
- `SearchUrl`: "https://www.catalog.update.microsoft.com/Search.aspx"
- `DownloadUrl`: "https://www.catalog.update.microsoft.com/DownloadDialog.aspx"

## Public Methods

### SearchUpdatesAsync
Performs a comprehensive search of the Microsoft Update Catalog with advanced pagination and filtering support.

```csharp
public async Task<IEnumerable<MSCatalogUpdate>> SearchUpdatesAsync(MSCatalogSearchRequest request, CancellationToken cancellationToken = default)
```

**Enhanced Features:**
- Builds optimized search queries based on mode (custom query or OS-based)
- Supports multi-page results with configurable limits
- Advanced filtering with detailed logging for troubleshooting
- Robust HTML parsing with fallback mechanisms
- Comprehensive error handling and recovery

### DownloadUpdateAsync
Downloads a single update file with real-time progress reporting.

```csharp
public async Task<bool> DownloadUpdateAsync(MSCatalogUpdate update, string destinationPath, IProgress<double> progress = null, CancellationToken cancellationToken = default)
```

**Key Features:**
- Retrieves download links via POST request to DownloadDialog
- Creates destination directories automatically
- Provides real-time progress reporting
- Handles large file downloads efficiently with streaming
- Robust error handling with detailed logging

### DownloadMultipleUpdatesAsync
Manages batch downloads with comprehensive progress tracking.

```csharp
public async Task<bool> DownloadMultipleUpdatesAsync(IEnumerable<MSCatalogUpdate> updates, string destinationFolder, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default)
```

**Enhanced Features:**
- Sequential download of multiple files with individual progress
- Per-file and overall progress reporting
- Continues processing on individual failures
- Detailed logging for each download operation
- Returns comprehensive success status for entire batch

### ExportToExcelAsync
Exports update information to a professionally formatted Excel file.

```csharp
public async Task<bool> ExportToExcelAsync(IEnumerable<MSCatalogUpdate> updates, string filePath, CancellationToken cancellationToken = default)
```

**Key Features:**
- Creates structured Excel worksheets with professional formatting
- Auto-formats columns and headers for readability
- Includes all relevant update metadata
- Handles large datasets efficiently
- Comprehensive error handling

## Private Methods (Enhanced Implementation)

### BuildSearchQuery
Constructs optimized search query strings based on request parameters with intelligent defaults.

### ExtractTotalPages
Parses HTML to determine total number of result pages with fallback mechanisms.

### ParseUpdates
Extracts update information from HTML table rows with comprehensive debugging and error recovery.

**Enhanced Features:**
- Detailed logging for HTML structure analysis
- Multiple fallback strategies for different table formats
- Comprehensive row validation and debugging
- Performance optimization for large result sets

### ParseUpdateRow
Converts individual HTML table row to MSCatalogUpdate object with robust parsing.

**Major Enhancements:**
- **Multi-pattern GUID extraction**: Supports both `"guid"` and `'guid'` formats
- **Robust date parsing**: Tries 7+ different date formats including US/EU formats
- **Enhanced size parsing**: Regex-based cleaning and extraction
- **Comprehensive validation**: Detailed logging for each parsing step
- **Fallback mechanisms**: Graceful handling of malformed data

**Supported Date Formats:**
- `M/d/yyyy` (US format: 6/26/2025)
- `d/M/yyyy` (EU format: 26/6/2025)
- `MM/dd/yyyy` (US with leading zeros)
- `dd/MM/yyyy` (EU with leading zeros)
- `yyyy-MM-dd` (ISO format)
- Mixed formats and general parsing fallback

### ExtractAdditionalInfo
Extracts OS, version, and update type information from title strings with enhanced pattern matching.

**Enhanced Features:**
- Windows 11/10 detection with version extraction
- Server OS identification (2019, 2022)
- Update type classification (Cumulative, Dynamic, Feature, .NET Framework)
- File name extraction from titles
- Comprehensive logging for extraction process

### ExtractWindowsVersion
Parses Windows version information from update titles with multiple pattern support.

**Enhanced Patterns:**
- Version patterns (22H2, 21H1, etc.)
- Build number detection
- Fallback mechanisms for various title formats

### ParseSize
Converts human-readable size strings to bytes with enhanced parsing.

**Enhanced Features:**
- Regex-based size extraction and cleaning
- Support for KB, MB, GB, TB units
- Handles malformed size strings gracefully
- Detailed logging for size conversion

### ApplyFilters
Applies comprehensive filtering to search results with detailed logging.

**Enhanced Filtering with Detailed Logs:**
- Preview/Dynamic update filtering with count tracking
- .NET Framework inclusion/exclusion with detailed logs
- Date range filtering with verbose logging for excluded items
- Size filtering (min/max) with byte conversion
- Architecture filtering with case-insensitive matching
- Before/after counts for each filter step

### ApplySorting
Sorts results based on specified criteria with multiple sort options.

**Supported Sort Criteria:**
- Date (ascending/descending)
- Size (ascending/descending)
- Title (alphabetical)
- Classification
- Product

### GetDownloadLinksAsync
Retrieves actual download URLs for updates with enhanced error handling.

### SanitizeFileName
Ensures file names are valid for the file system with comprehensive character replacement.

## Usage Examples

### Basic Search with Enhanced Error Handling
```csharp
var service = new MSCatalogService();
var request = new MSCatalogSearchRequest
{
    Mode = SearchMode.OperatingSystem,
    OperatingSystem = "Windows 11",
    Version = "23H2",
    UpdateType = "Cumulative Update",
    Architecture = "x64",
    IncludePreview = false,
    FromDate = DateTime.Now.AddMonths(-6),
    ToDate = DateTime.Now
};

try
{
    var updates = await service.SearchUpdatesAsync(request);
    Logger.Information("Found {Count} updates", updates.Count());
}
catch (Exception ex)
{
    Logger.Error(ex, "Search failed");
}
```

### Download with Comprehensive Progress Tracking
```csharp
var progress = new Progress<double>(p => 
{
    Console.WriteLine($"Download Progress: {p:P1}");
    Logger.Debug("Download progress: {Progress:P1}", p);
});

try
{
    bool success = await service.DownloadUpdateAsync(
        update, 
        @"C:\Updates\update.cab", 
        progress);
    
    if (success)
    {
        Logger.Information("Download completed successfully");
    }
    else
    {
        Logger.Warning("Download failed - check logs for details");
    }
}
catch (Exception ex)
{
    Logger.Error(ex, "Download error occurred");
}
```

### Batch Download with Progress Monitoring
```csharp
var batchProgress = new Progress<DownloadProgress>(p =>
{
    Logger.Information("Batch Progress: {CurrentFile}/{TotalFiles} - {OverallProgress:P1}", 
        p.CurrentFileIndex, p.TotalFiles, p.OverallProgress);
});

var success = await service.DownloadMultipleUpdatesAsync(
    selectedUpdates, 
    @"C:\Updates\", 
    batchProgress);
```

## Features

### Core Capabilities
- Advanced HTML parsing with HtmlAgilityPack
- Robust error handling and comprehensive logging
- Real-time progress reporting for long operations
- Automatic directory creation and file management
- Intelligent file name sanitization
- Multi-page pagination support
- Excel export with professional formatting

### Enhanced Parsing & Reliability
- **Multi-format date parsing**: Handles US, EU, and ISO date formats
- **Robust GUID extraction**: Multiple regex patterns for different onclick formats
- **Smart size parsing**: Regex-based cleaning and unit conversion
- **Comprehensive validation**: Detailed logging for troubleshooting
- **Fallback mechanisms**: Graceful handling of malformed HTML

### Advanced Filtering & Sorting
- Preview/Dynamic update filtering
- .NET Framework inclusion/exclusion
- Date range filtering with detailed logging
- Size-based filtering (min/max in MB)
- Architecture-specific filtering
- Multiple sort criteria with ascending/descending options

### Debugging & Monitoring
- **Comprehensive logging**: Debug, Information, Warning, and Error levels
- **Performance tracking**: Timing information for operations
- **Filter analytics**: Before/after counts for each filter
- **HTML structure analysis**: Detailed table and row parsing logs
- **Progress reporting**: Real-time updates for long operations

## Dependencies
- **HtmlAgilityPack**: HTML parsing and manipulation
- **EPPlus**: Excel file generation and formatting
- **System.Net.Http**: Web requests and downloads
- **System.Web**: URL encoding and HTML utilities
- **Serilog**: Comprehensive logging framework
- **System.Text.RegularExpressions**: Pattern matching and extraction

## Error Handling

### Network & Web Errors
- HTTP request failures with retry logic
- Timeout handling for large downloads
- Connection issues with detailed logging
- Invalid response handling

### Parsing & Data Errors
- Malformed HTML graceful handling
- Invalid date format fallbacks
- Missing GUID recovery mechanisms
- Size parsing error handling

### File System Errors
- Directory creation failures
- Disk space validation
- File permission issues
- Path sanitization and validation

## Performance Considerations

### Optimizations
- Streaming downloads for large files (8192-byte buffer)
- Efficient HTML parsing with targeted XPath queries
- Minimal memory usage for Excel exports
- Lazy evaluation for large result sets

### Scalability
- Configurable pagination limits
- Memory-efficient batch processing
- Progress reporting without performance impact
- Optimized regex compilation for repeated use

## Security Considerations

### Web Security
- HTTPS enforcement for all communications
- Proper user agent headers for compatibility
- Safe HTML content handling
- Input validation and sanitization

### File System Security
- Path traversal prevention
- File name sanitization
- Directory permission validation
- Secure temporary file handling

## Logging & Debugging

### Log Levels Used
- **Debug**: Detailed parsing steps, filter counts, HTML analysis
- **Information**: Operation start/completion, result counts
- **Warning**: Non-critical issues, fallback usage
- **Error**: Critical failures, exceptions with context

### Key Logging Points
- Search query construction and execution
- HTML parsing and table analysis
- Filter application with before/after counts
- Download progress and completion status
- Error conditions with full context

## Related Files
- [`IMSCatalogService.md`](./IMSCatalogService.md) - Service interface
- [`MSCatalogUpdate.md`](../../Models/MSCatalogUpdate.md) - Update model
- [`MSCatalogSearchRequest.md`](../../Models/MSCatalogSearchRequest.md) - Search request model

## Best Practices

### Service Usage
1. **Always dispose** of service when done (implements IDisposable)
2. **Use cancellation tokens** for all async operations
3. **Handle pagination limits** appropriately for large searches
4. **Validate file paths** before initiating downloads
5. **Monitor progress** for long-running operations

### Error Handling
1. **Catch specific exceptions** rather than generic Exception
2. **Log errors with context** for troubleshooting
3. **Implement retry logic** for transient failures
4. **Validate inputs** before processing
5. **Handle partial failures** gracefully in batch operations

### Performance
1. **Use appropriate buffer sizes** for downloads
2. **Limit concurrent operations** to avoid overwhelming servers
3. **Monitor memory usage** for large result sets
4. **Cache results** when appropriate
5. **Use streaming** for large file operations

### Debugging
1. **Enable Debug logging** for troubleshooting
2. **Monitor filter effectiveness** through logs
3. **Validate HTML structure** when parsing fails
4. **Check network connectivity** for download issues
5. **Review progress reports** for operation monitoring

---

**Note**: This documentation reflects the current implementation with all recent enhancements including robust parsing, comprehensive logging, and enhanced error handling. The service has been thoroughly tested and debugged to handle various edge cases and provide reliable operation. 