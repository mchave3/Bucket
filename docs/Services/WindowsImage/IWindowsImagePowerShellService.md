# IWindowsImagePowerShellService Interface Documentation

## Overview

Interface for Windows image PowerShell operations and JSON parsing. This service handles interaction with PowerShell Get-WindowsImage cmdlet and parsing of results to extract Windows image metadata and indices information.

## Location

- **File**: `src/Services/WindowsImage/IWindowsImagePowerShellService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition

```csharp
public interface IWindowsImagePowerShellService
```

## Methods

### AnalyzeImageAsync

```csharp
Task<List<WindowsImageIndex>> AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```

Analyzes a WIM/ESD file and extracts its indices asynchronously using PowerShell Get-WindowsImage cmdlet.

**Parameters:**
- `imagePath`: The path to the WIM/ESD file
- `progress`: Optional progress reporter for tracking analysis operations
- `cancellationToken`: Optional cancellation token for async operation

**Returns:** A collection of Windows image indices

### GetImageIndexDetailsAsync

```csharp
Task<WindowsImageIndex> GetImageIndexDetailsAsync(string imagePath, int index, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```

Gets detailed information for a specific Windows image index using PowerShell commands.

**Parameters:**
- `imagePath`: The path to the image file
- `index`: The index number to get detailed information for
- `progress`: Optional progress reporter for tracking operations
- `cancellationToken`: Optional cancellation token for async operation

**Returns:** A WindowsImageIndex object with detailed information, or null if not found

### IsValidImageFormat

```csharp
bool IsValidImageFormat(string extension)
```

Validates if the file format is supported for Windows imaging.

**Parameters:**
- `extension`: The file extension to validate (e.g., ".wim", ".esd")

**Returns:** True if the format is supported, false otherwise

## Usage Examples

```csharp
// Injecting the service
public class WindowsImageAnalysisService
{
    private readonly IWindowsImagePowerShellService _powerShellService;

    public WindowsImageAnalysisService(IWindowsImagePowerShellService powerShellService)
    {
        _powerShellService = powerShellService;
    }

    // Analyze a Windows image file
    public async Task<List<WindowsImageIndex>> AnalyzeImageFileAsync(string imagePath)
    {
        var progress = new Progress<string>(message => Logger.Information(message));

        if (!_powerShellService.IsValidImageFormat(Path.GetExtension(imagePath)))
        {
            throw new ArgumentException("Unsupported image format");
        }

        return await _powerShellService.AnalyzeImageAsync(imagePath, progress);
    }

    // Get specific index details
    public async Task<WindowsImageIndex> GetIndexDetailsAsync(string imagePath, int index)
    {
        var progress = new Progress<string>(message => Logger.Information(message));
        return await _powerShellService.GetImageIndexDetailsAsync(imagePath, index, progress);
    }

    // Validate image format before processing
    public bool CanProcessImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return _powerShellService.IsValidImageFormat(extension);
    }
}
```

## Features

- **PowerShell Integration**: Leverages Windows PowerShell Get-WindowsImage cmdlet
- **JSON Parsing**: Converts PowerShell output to structured data objects
- **Progress Reporting**: Provides feedback during potentially long-running operations
- **Format Validation**: Validates supported Windows image formats
- **Async Operations**: All operations are asynchronous to prevent UI blocking
- **Index Analysis**: Extracts comprehensive metadata from image indices

## Dependencies

- **Bucket.Models**: WindowsImageIndex model
- **System.Management.Automation**: PowerShell execution (if used)
- **System.Threading**: Cancellation token support
- **System.Threading.Tasks**: Async operations support

## Implementation

This interface is implemented by [`WindowsImagePowerShellService`](./WindowsImagePowerShellService.md).

## Related Files

- [`WindowsImagePowerShellService.md`](./WindowsImagePowerShellService.md) - Implementation
- [`WindowsImageIndex.md`](../../Models/WindowsImageIndex.md) - Data model
- [`WindowsImageService.md`](../WindowsImageService.md) - Main coordinator service
- [`IWindowsImageFileService.md`](./IWindowsImageFileService.md) - File operations

## Best Practices

- Always validate image formats before processing
- Use progress reporting for long-running PowerShell operations
- Handle PowerShell execution errors gracefully
- Validate PowerShell output before parsing
- Use cancellation tokens for responsive UI

## Error Handling

- PowerShell execution failures
- Invalid image file formats
- Corrupted or incomplete image files
- PowerShell output parsing errors
- Cancellation during long operations
- Access denied for PowerShell operations

## Security Considerations

- **PowerShell Execution**: Ensure secure PowerShell execution environment
- **File Path Validation**: Validate image file paths before PowerShell operations
- **Output Sanitization**: Sanitize PowerShell output before parsing
- **Privilege Requirements**: May require elevated privileges for some operations

## Performance Notes

- PowerShell operations can be slow for large image files
- Consider caching results for frequently accessed images
- Use progress reporting to keep users informed
- Implement timeouts for PowerShell operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
