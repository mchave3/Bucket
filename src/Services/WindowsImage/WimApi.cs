using System.Runtime.InteropServices;
using System.Text;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Native Windows Imaging API (WIMGAPI) interop declarations.
/// Provides P/Invoke declarations for accessing WIM metadata directly.
/// Based on the Windows Imaging API from wimgapi.dll.
/// </summary>
internal static class WimApi
{
    #region Constants

    // WIM access flags
    internal const uint WIM_GENERIC_READ = 0x80000000;
    internal const uint WIM_GENERIC_WRITE = 0x40000000;

    // WIM creation disposition
    internal const uint WIM_OPEN_EXISTING = 3;

    // WIM flags
    internal const uint WIM_FLAG_SHARE_WRITE = 0x00000002;

    // WIM compression types
    internal const uint WIM_COMPRESS_NONE = 0;

    #endregion

    #region WIM API Functions

    /// <summary>
    /// Creates a handle to a WIM file.
    /// </summary>
    /// <param name="pszWimPath">Path to the WIM file.</param>
    /// <param name="dwDesiredAccess">Desired access (read/write).</param>
    /// <param name="dwCreationDisposition">Creation disposition.</param>
    /// <param name="dwFlagsAndAttributes">Flags and attributes.</param>
    /// <param name="dwCompressionType">Compression type.</param>
    /// <param name="pdwCreationResult">Creation result (can be IntPtr.Zero).</param>
    /// <returns>Handle to the WIM file or IntPtr.Zero on failure.</returns>
    [DllImport("wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr WIMCreateFile(
        string pszWimPath,
        uint dwDesiredAccess,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        uint dwCompressionType,
        IntPtr pdwCreationResult);

    /// <summary>
    /// Closes a WIM handle.
    /// </summary>
    /// <param name="hObject">Handle to close.</param>
    /// <returns>True on success, false on failure.</returns>
    [DllImport("wimgapi.dll", SetLastError = true)]
    internal static extern bool WIMCloseHandle(IntPtr hObject);

    /// <summary>
    /// Gets the image information from a WIM file.
    /// </summary>
    /// <param name="hWim">Handle to the WIM file.</param>
    /// <param name="ppvImageInfo">Pointer to receive the image information.</param>
    /// <param name="pcbImageInfo">Pointer to receive the size of the image information.</param>
    /// <returns>True on success, false on failure.</returns>
    [DllImport("wimgapi.dll", SetLastError = true)]
    internal static extern bool WimGetImageInformation(
        IntPtr hWim,
        out IntPtr ppvImageInfo,
        out IntPtr pcbImageInfo);

    /// <summary>
    /// Sets the image information for a WIM file.
    /// </summary>
    /// <param name="hWim">Handle to the WIM file.</param>
    /// <param name="pvImageInfo">Pointer to the image information.</param>
    /// <param name="cbImageInfo">Size of the image information.</param>
    /// <returns>True on success, false on failure.</returns>
    [DllImport("wimgapi.dll", SetLastError = true)]
    internal static extern bool WimSetImageInformation(
        IntPtr hWim,
        IntPtr pvImageInfo,
        uint cbImageInfo);

    /// <summary>
    /// Sets the temporary path for WIM operations.
    /// </summary>
    /// <param name="hWim">Handle to the WIM file.</param>
    /// <param name="pszPath">Path to use for temporary files.</param>
    /// <returns>True on success, false on failure.</returns>
    [DllImport("wimgapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool WimSetTemporaryPath(IntPtr hWim, string pszPath);

    #endregion

    #region Helper Methods

    /// <summary>
    /// Converts a pointer to a Unicode string.
    /// </summary>
    /// <param name="ptr">Pointer to the Unicode string.</param>
    /// <returns>The managed string or null if the pointer is invalid.</returns>
    internal static string PtrToStringUni(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return null;

        return Marshal.PtrToStringUni(ptr);
    }

    /// <summary>
    /// Gets the last Win32 error message.
    /// </summary>
    /// <returns>The error message for the last Win32 error.</returns>
    internal static string GetLastErrorMessage()
    {
        int errorCode = Marshal.GetLastWin32Error();
        return new System.ComponentModel.Win32Exception(errorCode).Message;
    }

    #endregion
}
