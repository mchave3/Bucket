# IWindowsImageFileService Interface Documentation

## Overview
Interface for Windows image file operations, copying, validation, and naming utilities. This service handles all file-related operations for Windows images including copying files to managed directories, generating unique file names, and validating file paths for security.

## Location
- **File**: `src/Services/WindowsImage/IWindowsImageFileService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition
```csharp
public interface IWindowsImageFileService
```

## Methods

### CopyImageToManagedDirectoryAsync
```csharp
Task<string> CopyImageToManagedDirectoryAsync(string sourcePath, string targetName, IProgress<string> progress = null, CancellationToken cancellationToken = default)
```
Copies an image file to the managed images directory.

**Parameters:**
- `sourcePath`: The source image file path
- `targetName`: The target file name (without extension)
- `progress`: Optional progress reporter for tracking copy operation
- `cancellationToken`: Cancellation token for async operation

**Returns:** The path to the copied image file

### GenerateUniqueFileName
```csharp
string GenerateUniqueFileName(string baseName, string extension)
```
Generates a unique file name based on the base name and extension, resolving conflicts with a counter.

**Parameters:**
- `baseName`: The base name for the file (without extension)
- `extension`: The file extension (e.g., ".wim", ".esd")

**Returns:** A unique file name that doesn't conflict with existing files

### GenerateUniqueFilePath
```csharp
string GenerateUniqueFilePath(string baseName, string extension)
```
Generates the full path for a unique file name.

**Parameters:**
- `baseName`: The base name for the file (without extension)
- `extension`: The file extension (e.g., ".wim", ".esd")

**Returns:** The full path to the unique file

### SanitizeFileName
```csharp
string SanitizeFileName(string fileName)
```
Sanitizes a file name by removing or replacing invalid characters.

**Parameters:**
- `fileName`: The file name to sanitize

**Returns:** A sanitized file name safe for the file system

### IsValidFileName
```csharp
bool IsValidFileName(string fileName)
```
Validates if a file name is valid for the current file system.

**Parameters:**
- `fileName`: The file name to validate

**Returns:** True if the file name is valid, false otherwise

### IsValidFilePath
```csharp
bool IsValidFilePath(string filePath)
```
Validates file path to prevent directory traversal attacks.

**Parameters:**
- `filePath`: The file path to validate

**Returns:** True if the path is safe, false otherwise

## Usage Examples

```csharp
// Injecting the service
public class WindowsImageService
{
    private readonly IWindowsImageFileService _fileService;

    public WindowsImageService(IWindowsImageFileService fileService)
    {
        _fileService = fileService;
    }

    // Copy an image file safely
    public async Task<string> ImportImageAsync(string sourcePath, string targetName)
    {
        var progress = new Progress<string>(message => Logger.Information(message));
        return await _fileService.CopyImageToManagedDirectoryAsync(
            sourcePath, targetName, progress);
    }

    // Generate unique file name
    public string CreateUniqueImageName(string baseName)
    {
        return _fileService.GenerateUniqueFileName(baseName, ".wim");
    }

    // Validate user input
    public bool ValidateUserFileName(string fileName)
    {
        return _fileService.IsValidFileName(fileName) &&
               _fileService.IsValidFilePath(fileName);
    }
}
```

## Features
- **Async File Operations**: Supports large file copying with progress reporting and cancellation
- **Unique Name Generation**: Automatically resolves file name conflicts
- **Security Validation**: Prevents directory traversal and validates file names
- **File System Safety**: Ensures compatibility with Windows file system requirements
- **Progress Reporting**: Provides feedback during long-running file operations

## Dependencies
- **System.IO**: File system operations
- **System.Threading**: Cancellation token support
- **System.Threading.Tasks**: Async operations support

## Implementation
This interface is implemented by [`WindowsImageFileService`](./WindowsImageFileService.md).

## Related Files
- [`WindowsImageFileService.md`](./WindowsImageFileService.md) - Implementation
- [`WindowsImageService.md`](../WindowsImageService.md) - Main coordinator service
- [`IWindowsImageMetadataService.md`](./IWindowsImageMetadataService.md) - Metadata operations
- [`IWindowsImagePowerShellService.md`](./IWindowsImagePowerShellService.md) - PowerShell operations

## Best Practices
- Always use the async methods for file operations to avoid blocking the UI
- Provide progress reporting for large file operations
- Validate file names and paths before processing
- Use cancellation tokens to allow users to cancel long operations
- Handle file system exceptions appropriately

## Security Considerations
- **Path Validation**: Use `IsValidFilePath` to prevent directory traversal attacks
- **File Name Sanitization**: Use `SanitizeFileName` to ensure safe file names
- **Input Validation**: Always validate user-provided file names and paths

## Error Handling
- File not found scenarios
- Access denied exceptions
- Disk space limitations
- Invalid path or file name formats
- Cancellation during file operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
