# IWindowsImageIndexEditingService Interface Documentation

## Overview
Interface for Windows image index editing operations. Handles modification of index metadata (name, description) in WIM files using native WIM APIs. This service provides functionality to update image index properties without mounting the image.

## Location
- **File**: `src/Services/WindowsImage/IWindowsImageIndexEditingService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Interface Definition
```csharp
public interface IWindowsImageIndexEditingService
```

## Methods

### UpdateIndexNameAsync
```csharp
Task<bool> UpdateIndexNameAsync(string wimFilePath, int index, string currentName, string newName, CancellationToken cancellationToken = default)
```
Updates the name of a specific Windows image index.

**Parameters:**
- `wimFilePath`: The path to the WIM file
- `index`: The index number to modify
- `currentName`: The current name of the index
- `newName`: The new name for the index
- `cancellationToken`: The cancellation token (optional)

**Returns:** True if the operation was successful, false otherwise

### UpdateIndexDescriptionAsync
```csharp
Task<bool> UpdateIndexDescriptionAsync(string wimFilePath, int index, string currentDescription, string newDescription, CancellationToken cancellationToken = default)
```
Updates the description of a specific Windows image index.

**Parameters:**
- `wimFilePath`: The path to the WIM file
- `index`: The index number to modify
- `currentDescription`: The current description of the index
- `newDescription`: The new description for the index
- `cancellationToken`: The cancellation token (optional)

**Returns:** True if the operation was successful, false otherwise

### UpdateIndexMetadataAsync
```csharp
Task<bool> UpdateIndexMetadataAsync(string wimFilePath, int index, string currentName, string newName, string currentDescription, string newDescription, CancellationToken cancellationToken = default)
```
Updates both name and description of a specific Windows image index in a single operation.

**Parameters:**
- `wimFilePath`: The path to the WIM file
- `index`: The index number to modify
- `currentName`: The current name of the index
- `newName`: The new name for the index
- `currentDescription`: The current description of the index
- `newDescription`: The new description for the index
- `cancellationToken`: The cancellation token (optional)

**Returns:** True if the operation was successful, false otherwise

### IsWimFileAccessible
```csharp
bool IsWimFileAccessible(string wimFilePath)
```
Validates if a WIM file is accessible and not in use by another process.

**Parameters:**
- `wimFilePath`: The path to the WIM file

**Returns:** True if the file is accessible, false otherwise

## Usage Examples

### Update Index Name Only
```csharp
var editingService = serviceProvider.GetService<IWindowsImageIndexEditingService>();

bool success = await editingService.UpdateIndexNameAsync(
    @"C:\Images\install.wim",
    1,
    "Windows 11 Pro",
    "Windows 11 Professional Edition",
    cancellationToken);

if (success)
{
    Console.WriteLine("Index name updated successfully");
}
```

### Update Index Description Only
```csharp
bool success = await editingService.UpdateIndexDescriptionAsync(
    @"C:\Images\install.wim",
    1,
    "Old description",
    "Updated description with more details",
    cancellationToken);
```

### Update Both Name and Description
```csharp
bool success = await editingService.UpdateIndexMetadataAsync(
    @"C:\Images\install.wim",
    1,
    "Windows 11 Pro",
    "Windows 11 Professional",
    "Standard edition",
    "Professional edition with advanced features",
    cancellationToken);
```

### Check File Accessibility
```csharp
if (editingService.IsWimFileAccessible(@"C:\Images\install.wim"))
{
    // Proceed with editing operations
    await editingService.UpdateIndexNameAsync(...);
}
else
{
    Console.WriteLine("WIM file is not accessible or in use");
}
```

## Features

- **Metadata Editing**: Modify index names and descriptions without mounting
- **Atomic Operations**: Single operation to update both name and description
- **File Validation**: Check file accessibility before operations
- **Cancellation Support**: All async operations support cancellation tokens
- **Native API Integration**: Uses native WIM APIs for direct file modification
- **Non-Destructive**: Modifies metadata without affecting image content

## Dependencies

- `Bucket.Models`: For data models and structures
- Native WIM APIs: For direct WIM file manipulation
- System.Threading: For cancellation token support

## Related Files

- [`WindowsImageIndexEditingService.md`](./WindowsImageIndexEditingService.md) - Implementation class
- [`WindowsImageIndex.md`](../../Models/WindowsImageIndex.md) - Index model
- [`WimApi.md`](./WimApi.md) - Native WIM API wrapper

## Best Practices

### File Operations
1. **Check Accessibility**: Always use `IsWimFileAccessible` before editing
2. **Validate Parameters**: Ensure all parameters are valid before operations
3. **Handle Cancellation**: Support cancellation tokens for long operations
4. **Error Handling**: Check return values and handle failures gracefully

### Metadata Updates
1. **Atomic Updates**: Use `UpdateIndexMetadataAsync` for multiple changes
2. **Validation**: Validate new names and descriptions before updating
3. **Backup**: Consider backing up WIM files before modifications
4. **Verification**: Verify changes after operations complete

### Performance
1. **Batch Operations**: Group multiple updates when possible
2. **File Locking**: Minimize file access time to reduce lock duration
3. **Async Operations**: Use async methods to avoid blocking UI
4. **Resource Management**: Ensure proper disposal of file handles

## Error Handling

### Common Error Scenarios
- **File Not Found**: WIM file doesn't exist at specified path
- **File In Use**: WIM file is locked by another process
- **Invalid Index**: Specified index doesn't exist in the WIM file
- **Permission Denied**: Insufficient permissions to modify the file
- **Corrupted File**: WIM file is corrupted or invalid

### Error Recovery
- **Pre-validation**: Use `IsWimFileAccessible` to prevent many errors
- **Retry Logic**: Implement retry for transient file access issues
- **Graceful Degradation**: Handle failures without crashing application
- **User Feedback**: Provide clear error messages to users

## Security Considerations

- **File Path Validation**: Validate file paths to prevent directory traversal
- **Permission Checks**: Ensure adequate permissions before operations
- **File Integrity**: Verify file integrity before and after modifications
- **Access Control**: Respect file system access controls

## Performance Considerations

- **File Access**: Minimize file handle duration to reduce lock time
- **Batch Operations**: Combine multiple metadata updates when possible
- **Async Operations**: Use async methods for responsive UI
- **Memory Usage**: Efficient memory usage for large WIM files

## Implementation Notes

### Design Patterns
- **Interface Segregation**: Focused interface for index editing operations
- **Single Responsibility**: Handles only metadata editing, not content
- **Async Support**: All operations are async for better performance
- **Cancellation**: Full cancellation token support

### API Design
- **Boolean Returns**: Simple success/failure indication
- **Parameter Validation**: Comprehensive parameter validation
- **Current/New Pattern**: Explicit current and new values for safety
- **Optional Cancellation**: Cancellation tokens are optional parameters

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 