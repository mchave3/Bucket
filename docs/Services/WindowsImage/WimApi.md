# WimApi Class Documentation

## Overview

Native Windows Imaging API (WIMGAPI) interop declarations for low-level WIM file operations. Provides P/Invoke declarations for accessing WIM metadata directly through the Windows native wimgapi.dll library. This class is the foundation for all WIM file metadata operations in Bucket.

## Location

- **File**: `src/Services/WindowsImage/WimApi.cs`
- **Namespace**: `Bucket.Services.WindowsImage`

## Class Definition

```csharp
internal static class WimApi
```

## Constants

### WIM Access Flags
- **`WIM_GENERIC_READ`** (`0x80000000`): Generic read access to WIM files
- **`WIM_GENERIC_WRITE`** (`0x40000000`): Generic write access to WIM files

### WIM Creation Disposition
- **`WIM_OPEN_EXISTING`** (`3`): Opens an existing WIM file

### WIM Flags
- **`WIM_FLAG_SHARE_WRITE`** (`0x00000002`): Allows shared write access

### WIM Compression Types
- **`WIM_COMPRESS_NONE`** (`0`): No compression for WIM operations

## Native API Methods

### `WIMCreateFile`
```csharp
internal static extern IntPtr WIMCreateFile(
    string pszWimPath,
    uint dwDesiredAccess,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    uint dwCompressionType,
    int pdwCreationResult)
```

Creates a handle to a WIM file for read/write operations.

**Parameters:**
- `pszWimPath`: Path to the WIM file
- `dwDesiredAccess`: Desired access (read/write)
- `dwCreationDisposition`: Creation disposition flags
- `dwFlagsAndAttributes`: File flags and attributes
- `dwCompressionType`: Compression type for operations
- `pdwCreationResult`: Creation result output

**Returns:** Handle to WIM file or `IntPtr.Zero` on failure

### `WIMCloseHandle`
```csharp
internal static extern bool WIMCloseHandle(IntPtr hObject)
```

Closes a WIM handle and releases resources.

**Parameters:**
- `hObject`: Handle to close

**Returns:** True on success, false on failure

### `WIMGetImageInformation`
```csharp
internal static extern bool WIMGetImageInformation(
    IntPtr hWim,
    out IntPtr ppvImageInfo,
    out IntPtr pcbImageInfo)
```

Retrieves XML metadata from a WIM file.

**Parameters:**
- `hWim`: Handle to the WIM file
- `ppvImageInfo`: Pointer to receive image information
- `pcbImageInfo`: Pointer to receive size of image information

**Returns:** True on success, false on failure

### `WIMSetImageInformation`
```csharp
internal static extern bool WIMSetImageInformation(
    IntPtr hWim,
    IntPtr pvImageInfo,
    uint cbImageInfo)
```

Sets XML metadata for a WIM file.

**Parameters:**
- `hWim`: Handle to the WIM file
- `pvImageInfo`: Pointer to image information data
- `cbImageInfo`: Size of image information data

**Returns:** True on success, false on failure

### `WIMSetTemporaryPath`
```csharp
internal static extern bool WIMSetTemporaryPath(
    IntPtr hWim,
    string pszPath)
```

Sets the temporary path for WIM operations.

**Parameters:**
- `hWim`: Handle to the WIM file
- `pszPath`: Path for temporary files

**Returns:** True on success, false on failure

## Helper Methods

### `PtrToStringUni`
```csharp
internal static string PtrToStringUni(IntPtr ptr)
```

Converts a pointer to a Unicode string safely.

**Parameters:**
- `ptr`: Pointer to Unicode string

**Returns:** Managed string or null if pointer is invalid

### `GetLastErrorMessage`
```csharp
internal static string GetLastErrorMessage()
```

Gets the last Win32 error message for debugging.

**Returns:** Error message for the last Win32 error

## Usage Examples

### Basic WIM File Access
```csharp
var wimHandle = WimApi.WIMCreateFile(
    wimFilePath,
    WimApi.WIM_GENERIC_READ,
    WimApi.WIM_OPEN_EXISTING,
    WimApi.WIM_FLAG_SHARE_WRITE,
    WimApi.WIM_COMPRESS_NONE,
    0);

if (wimHandle == IntPtr.Zero)
{
    var error = WimApi.GetLastErrorMessage();
    Logger.Error("Failed to open WIM file: {Error}", error);
    return;
}

try
{
    // Perform WIM operations
}
finally
{
    WimApi.WIMCloseHandle(wimHandle);
}
```

### Reading WIM Metadata
```csharp
IntPtr infoPtr, sizePtr;
var success = WimApi.WIMGetImageInformation(wimHandle, out infoPtr, out sizePtr);

if (success)
{
    var xmlInfo = WimApi.PtrToStringUni(infoPtr);
    // Process XML metadata
}
```

### Writing WIM Metadata
```csharp
var xmlBytes = Encoding.Unicode.GetBytes(updatedXml);
var xmlBuffer = Marshal.AllocHGlobal(xmlBytes.Length);

try
{
    Marshal.Copy(xmlBytes, 0, xmlBuffer, xmlBytes.Length);

    var success = WimApi.WIMSetImageInformation(
        wimHandle,
        xmlBuffer,
        (uint)xmlBytes.Length);
}
finally
{
    Marshal.FreeHGlobal(xmlBuffer);
}
```

## Features

### Low-Level WIM Access
- **Direct API Access**: Bypasses higher-level abstractions for maximum control
- **Metadata Operations**: Read and write WIM XML metadata directly
- **Handle Management**: Proper resource cleanup and error handling

### Memory Management
- **Pointer Safety**: Safe conversion between native and managed memory
- **Resource Cleanup**: Automatic handle and memory cleanup
- **Error Handling**: Comprehensive Win32 error reporting

### Performance Optimizations
- **Native Speed**: Direct native API calls for maximum performance
- **Minimal Overhead**: Thin wrapper over Windows APIs
- **Efficient Operations**: Optimized for metadata read/write scenarios

## Dependencies

### System Requirements
- **Windows 10/11**: Required for WIMGAPI support
- **wimgapi.dll**: Windows Imaging API library (part of Windows)

### Framework Dependencies
- **System.Runtime.InteropServices**: For P/Invoke declarations
- **System.Text**: For string encoding operations

## Related Files

- [`WindowsImageIndexEditingService.md`](./WindowsImageIndexEditingService.md): Primary consumer of these APIs
- [`WindowsImageMetadataService.md`](./WindowsImageMetadataService.md): Higher-level metadata operations

## Best Practices

### Handle Management
- Always close WIM handles in finally blocks
- Check for `IntPtr.Zero` return values
- Use using statements where possible for automatic cleanup

### Error Handling
- Always check return values from WIM API calls
- Use `GetLastErrorMessage()` for detailed error information
- Log errors with sufficient context for debugging

### Memory Safety
- Free allocated memory with `Marshal.FreeHGlobal()`
- Validate pointers before dereferencing
- Use safe string conversion methods

## Security Considerations

### File Access
- Validates file paths to prevent unauthorized access
- Requires appropriate file system permissions
- Handles locked files gracefully

### Memory Safety
- Prevents buffer overflows with proper size checking
- Uses safe pointer conversion methods
- Validates all pointer operations

## Performance Notes

### Native Performance
- Direct native API calls provide maximum speed
- Minimal managed/unmanaged transition overhead
- Optimized for metadata operations

### Memory Usage
- Efficient pointer-based operations
- Minimal managed heap allocations
- Proper resource cleanup prevents leaks

## Troubleshooting

### Common Issues
- **Access Denied**: Check file permissions and administrative rights
- **File In Use**: Ensure other processes aren't locking the WIM file
- **Invalid Handle**: Verify WIM file exists and is not corrupted

### Error Codes
- Use `GetLastErrorMessage()` for Win32 error details
- Check Windows Event Log for system-level errors
- Validate WIM file integrity with DISM tools

---

**Note**: This documentation was generated automatically by AI and reflects the current implementation. Please verify against the actual source code and report any discrepancies.
