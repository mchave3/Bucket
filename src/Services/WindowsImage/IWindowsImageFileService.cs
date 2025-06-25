namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image file operations, copying, validation, and naming utilities.
/// Handles all file-related operations for Windows images.
/// </summary>
public interface IWindowsImageFileService
{
    /// <summary>
    /// Copies an image file to the managed images directory.
    /// </summary>
    /// <param name="sourcePath">The source image file path.</param>
    /// <param name="targetName">The target file name (without extension).</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path to the copied image file.</returns>
    Task<string> CopyImageToManagedDirectoryAsync(string sourcePath, string targetName, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique file name based on the base name and extension, resolving conflicts with a counter.
    /// </summary>
    /// <param name="baseName">The base name for the file (without extension).</param>
    /// <param name="extension">The file extension (e.g., ".wim", ".esd").</param>
    /// <returns>A unique file name that doesn't conflict with existing files.</returns>
    string GenerateUniqueFileName(string baseName, string extension);

    /// <summary>
    /// Generates the full path for a unique file name.
    /// </summary>
    /// <param name="baseName">The base name for the file (without extension).</param>
    /// <param name="extension">The file extension (e.g., ".wim", ".esd").</param>
    /// <returns>The full path to the unique file.</returns>
    string GenerateUniqueFilePath(string baseName, string extension);

    /// <summary>
    /// Sanitizes a file name by removing or replacing invalid characters.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>A sanitized file name safe for the file system.</returns>
    string SanitizeFileName(string fileName);

    /// <summary>
    /// Validates if a file name is valid for the current file system.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>True if the file name is valid, false otherwise.</returns>
    bool IsValidFileName(string fileName);

    /// <summary>
    /// Validates file path to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>True if the path is safe, false otherwise.</returns>
    bool IsValidFilePath(string filePath);
}
