# WindowsImageIndexEditingService Class Documentation

## Overview

Service for editing Windows image index metadata using native Windows Imaging API (WIMGAPI). Provides functionality to modify index names and descriptions directly in WIM files with user progress feedback and automatic UI refresh. Replicates the behavior found in WinToolkit v1 but adapted for the modern Bucket application architecture.

## Location

- **File**: `src/Services/WindowsImage/WindowsImageIndexEditingService.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Class Definition

```csharp
public class WindowsImageIndexEditingService : IWindowsImageIndexEditingService
```

## Dependencies

- **WimApi**: Native P/Invoke declarations for Windows Imaging API
- **ILogger**: Global logging through LoggerSetup
- **System.IO**: File and directory operations
- **System.Text.RegularExpressions**: XML parsing and manipulation

## Methods

### UpdateIndexNameAsync

```csharp
Task<bool> UpdateIndexNameAsync(string wimFilePath, int index, string currentName, string newName, CancellationToken cancellationToken = default)
```

Updates the name of a specific Windows image index in the WIM file.

**Parameters:**
- `wimFilePath`: Path to the WIM file
- `index`: Index number to modify (1-based)
- `currentName`: Current name of the index
- `newName`: New name for the index
- `cancellationToken`: Cancellation token for async operation

**Returns:** `true` if successful, `false` otherwise

### UpdateIndexDescriptionAsync

```csharp
Task<bool> UpdateIndexDescriptionAsync(string wimFilePath, int index, string currentDescription, string newDescription, CancellationToken cancellationToken = default)
```

Updates the description of a specific Windows image index in the WIM file.

### UpdateIndexMetadataAsync

```csharp
Task<bool> UpdateIndexMetadataAsync(string wimFilePath, int index, string currentName, string newName, string currentDescription, string newDescription, CancellationToken cancellationToken = default)
```

Updates both name and description of a specific Windows image index in a single operation for better performance.

### IsWimFileAccessible

```csharp
bool IsWimFileAccessible(string wimFilePath)
```

Validates if a WIM file is accessible and not in use by another process.

## Usage Examples

### Basic Index Name Update

```csharp
var editingService = App.GetService<IWindowsImageIndexEditingService>();

bool success = await editingService.UpdateIndexNameAsync(
    @"C:\Images\install.wim",
    1,
    "Windows 11 Pro",
    "Windows 11 Professional Custom");

if (success)
{
    Logger.Information("Index name updated successfully");
}
```

### Complete Metadata Update

```csharp
bool success = await editingService.UpdateIndexMetadataAsync(
    wimFilePath: @"C:\Images\install.wim",
    index: 2,
    currentName: "Windows 11 Home",
    newName: "Windows 11 Home Edition",
    currentDescription: "Standard home edition",
    newDescription: "Customized home edition with additional drivers");
```

### File Accessibility Check

```csharp
if (!editingService.IsWimFileAccessible(@"C:\Images\install.wim"))
{
    // Handle file access issues
    MessageBox.Show("WIM file is in use by another process");
}
```

## Features

### Native WIM API Integration

- **Direct WIM Manipulation**: Uses Windows native WIMGAPI for direct file modification
- **Efficient Operations**: Modifies metadata without extracting/rebuilding the entire WIM
- **Transactional Safety**: All-or-nothing updates to prevent corruption

### XML Metadata Processing

- **Intelligent Parsing**: Handles various XML structures and edge cases
- **Pattern Matching**: Uses regex patterns to locate and replace specific metadata fields
- **Field Addition**: Automatically adds missing metadata fields when needed

### Error Handling and Validation

- **File Access Validation**: Checks if WIM files are accessible before attempting modifications
- **Comprehensive Logging**: Detailed logging for troubleshooting and audit trails
- **Graceful Failure**: Returns false on failure without throwing exceptions

### Performance Optimizations

- **Async Operations**: All operations are async to prevent UI blocking
- **Minimal Memory Usage**: Efficient XML processing without loading entire WIM into memory
- **Temporary File Management**: Automatic cleanup of temporary files and directories

## Technical Implementation

### WIM API Workflow

1. **Open WIM File**: Creates handle with appropriate access permissions
2. **Retrieve Metadata**: Gets current XML metadata from WIM file
3. **Parse and Modify**: Uses regex to locate and update specific fields
4. **Write Back**: Updates WIM file with modified XML metadata
5. **Cleanup**: Closes handles and cleans temporary files

### XML Structure Handling

The service handles XML structures like:

```xml
<IMAGE INDEX="1">
    <NAME>Windows 11 Pro</NAME>
    <DESCRIPTION>Professional edition</DESCRIPTION>
    <DISPLAYNAME>Windows 11 Professional</DISPLAYNAME>
    <DISPLAYDESCRIPTION>Full-featured professional edition</DISPLAYDESCRIPTION>
    <!-- Other metadata fields -->
</IMAGE>
```

### Regex Patterns

- **Field Replacement**: `<FIELDTYPE>CurrentValue</FIELDTYPE>` → `<FIELDTYPE>NewValue</FIELDTYPE>`
- **Empty Field Population**: `<FIELDTYPE></FIELDTYPE>` → `<FIELDTYPE>NewValue</FIELDTYPE>`
- **Field Addition**: Adds new fields after `<IMAGE INDEX="n">` tag

## Error Handling

### Common Error Scenarios

1. **File Access Denied**: WIM file is locked by another process
2. **Invalid WIM Format**: Corrupted or non-standard WIM files
3. **Missing Index**: Specified index does not exist in WIM
4. **Disk Space**: Insufficient space for temporary files

### Error Recovery

- **Access Waiting**: Waits up to 5 seconds for file access availability
- **Rollback Capability**: Can revert changes if partial updates fail
- **Validation Checks**: Pre-validates inputs and file states

## Security Considerations

- **File Path Validation**: Prevents directory traversal attacks
- **Access Control**: Respects Windows file permissions
- **Temporary File Security**: Creates temporary files in secure user locations

## Performance Notes

- **Optimal for Small Changes**: Designed for metadata updates, not bulk operations
- **Memory Efficient**: Streams XML processing to minimize memory usage
- **Concurrent Safety**: Thread-safe for multiple simultaneous operations on different files

## Related Files

- [`IWindowsImageIndexEditingService.md`](./IWindowsImageIndexEditingService.md) - Service interface
- [`WimApi.md`](./WimApi.md) - Native API declarations
- [`ImageDetailsViewModel.md`](../ViewModels/ImageDetailsViewModel.md) - Primary consumer
- [`EditIndexDialog.md`](../Views/Dialogs/EditIndexDialog.md) - UI dialog

## Best Practices

### Service Usage

```csharp
// Always check file accessibility first
if (!service.IsWimFileAccessible(wimPath))
{
    // Handle access issues
    return;
}

// Use combined updates when changing multiple fields
bool success = await service.UpdateIndexMetadataAsync(
    wimPath, index, oldName, newName, oldDesc, newDesc);

// Always handle failures gracefully
if (!success)
{
    Logger.Error("Failed to update WIM metadata");
    // Implement fallback or user notification
}
```

### Integration Patterns

```csharp
// In ViewModels - inject service via constructor
public ImageDetailsViewModel(IWindowsImageIndexEditingService editingService)
{
    _editingService = editingService;
}

// In UI event handlers - always use async/await
private async void EditButton_Click(object sender, RoutedEventArgs e)
{
    var success = await _editingService.UpdateIndexNameAsync(...);
    if (success)
    {
        // Update UI
    }
}
```

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
