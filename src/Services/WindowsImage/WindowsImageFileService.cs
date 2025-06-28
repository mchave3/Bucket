namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for Windows image file operations, copying, validation, and naming utilities.
/// Provides functionality to handle all file-related operations for Windows images including
/// file validation, sanitization, copying with progress reporting, and unique name generation.
/// </summary>
public class WindowsImageFileService : IWindowsImageFileService
{
    #region Private Fields

    private readonly string _imagesDirectory;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowsImageFileService class.
    /// </summary>
    /// <param name="imagesDirectory">The directory path where images are stored.</param>
    public WindowsImageFileService(string imagesDirectory)
    {
        _imagesDirectory = imagesDirectory ?? throw new ArgumentNullException(nameof(imagesDirectory));

        // Ensure the images directory exists
        Directory.CreateDirectory(_imagesDirectory);

        Logger.Debug("WindowsImageFileService initialized with directory: {Directory}", _imagesDirectory);
    }

    #endregion

    #region Public Methods - File Operations

    /// <summary>
    /// Copies an image file to the managed images directory.
    /// </summary>
    /// <param name="sourcePath">The source image file path.</param>
    /// <param name="targetName">The target file name (without extension).</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path to the copied image file.</returns>
    public async Task<string> CopyImageToManagedDirectoryAsync(string sourcePath, string targetName, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Copying image to managed directory: {SourcePath} -> {TargetName}", sourcePath, targetName);
        progress?.Report("Preparing to copy image file...");

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Source image file not found: {sourcePath}");
        }

        var sourceInfo = new FileInfo(sourcePath);
        var extension = sourceInfo.Extension;

        // Use unified naming system to generate unique file path
        var targetPath = GenerateUniqueFilePath(targetName, extension);

        // Ensure target directory exists
        Directory.CreateDirectory(_imagesDirectory);

        // Check available disk space
        var driveInfo = new DriveInfo(Path.GetPathRoot(_imagesDirectory));
        if (driveInfo.AvailableFreeSpace < sourceInfo.Length)
        {
            throw new IOException($"Insufficient disk space. Need {sourceInfo.Length} bytes, but only {driveInfo.AvailableFreeSpace} bytes available.");
        }

        try
        {
            // Copy the file with progress reporting
            await CopyFileWithProgressAsync(sourcePath, targetPath, progress, cancellationToken);

            progress?.Report("Copy completed");
            Logger.Information("Successfully copied image to: {TargetPath}", targetPath);

            return targetPath;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy image file: {SourcePath} -> {TargetPath}", sourcePath, targetPath);

            // Clean up partial copy if it exists
            if (File.Exists(targetPath))
            {
                try
                {
                    File.Delete(targetPath);
                }
                catch (Exception deleteEx)
                {
                    Logger.Warning(deleteEx, "Failed to clean up partial copy: {TargetPath}", targetPath);
                }
            }

            throw;
        }
    }

    #endregion

    #region Public Methods - File Naming

    /// <summary>
    /// Generates a unique file name based on the base name and extension, resolving conflicts with a counter.
    /// </summary>
    /// <param name="baseName">The base name for the file (without extension).</param>
    /// <param name="extension">The file extension (e.g., ".wim", ".esd").</param>
    /// <returns>A unique file name that doesn't conflict with existing files.</returns>
    public string GenerateUniqueFileName(string baseName, string extension)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            throw new ArgumentException("Base name cannot be null or empty.", nameof(baseName));
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));
        }

        // Ensure extension starts with a dot
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        // Sanitize the base name to remove invalid file name characters
        var sanitizedBaseName = SanitizeFileName(baseName);

        // Generate the initial target path
        var targetPath = Path.Combine(_imagesDirectory, $"{sanitizedBaseName}{extension}");

        // If no conflict, return the original name
        if (!File.Exists(targetPath))
        {
            Logger.Debug("Generated unique file name: {FileName}", $"{sanitizedBaseName}{extension}");
            Logger.Verbose("File name generation details: BaseName='{BaseName}', Sanitized='{SanitizedName}', Extension='{Extension}'", 
                baseName, sanitizedBaseName, extension);
            return $"{sanitizedBaseName}{extension}";
        }

        // Find the next available counter
        var counter = 1;
        do
        {
            var numberedName = $"{sanitizedBaseName}_{counter:D3}";
            targetPath = Path.Combine(_imagesDirectory, $"{numberedName}{extension}");

            if (!File.Exists(targetPath))
            {
                Logger.Debug("Generated unique file name with counter: {FileName}", $"{numberedName}{extension}");
                return $"{numberedName}{extension}";
            }

            counter++;
        }
        while (counter <= 999); // Limit to avoid infinite loop

        // If we reach here, we have too many conflicts
        throw new InvalidOperationException($"Unable to generate unique file name for base name '{sanitizedBaseName}'. Too many existing files.");
    }

    /// <summary>
    /// Generates the full path for a unique file name.
    /// </summary>
    /// <param name="baseName">The base name for the file (without extension).</param>
    /// <param name="extension">The file extension (e.g., ".wim", ".esd").</param>
    /// <returns>The full path to the unique file.</returns>
    public string GenerateUniqueFilePath(string baseName, string extension)
    {
        var fileName = GenerateUniqueFileName(baseName, extension);
        return Path.Combine(_imagesDirectory, fileName);
    }

    /// <summary>
    /// Sanitizes a file name by removing or replacing invalid characters.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>A sanitized file name safe for the file system.</returns>
    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "UnnamedImage";
        }

        // Replace invalid characters with underscores
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;

        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        // Remove multiple consecutive underscores
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, "_+", "_");

        // Remove leading/trailing underscores and spaces
        sanitized = sanitized.Trim('_', ' ');

        // Ensure we have something left
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "UnnamedImage";
        }

        // Truncate if too long (leave room for counter and extension)
        if (sanitized.Length > 200)
        {
            sanitized = sanitized.Substring(0, 200).TrimEnd('_', ' ');
        }

        return sanitized;
    }

    #endregion

    #region Public Methods - File Validation

    /// <summary>
    /// Validates if a file name is valid for the current file system.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>True if the file name is valid, false otherwise.</returns>
    public bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
        {
            return false;
        }

        // Check for reserved names on Windows
        string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();

        if (reservedNames.Contains(nameWithoutExtension))
        {
            return false;
        }

        // Check length (Windows limit is 255 characters for file name)
        if (fileName.Length > 255)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates file path to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>True if the path is safe, false otherwise.</returns>
    public bool IsValidFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // Check for directory traversal attempts
        if (filePath.Contains("..") || filePath.Contains("~"))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (filePath.Any(c => invalidChars.Contains(c)))
            return false;

        try
        {
            // Try to get full path - this will throw if invalid
            Path.GetFullPath(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Copies a file with progress reporting.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="destinationPath">The destination file path.</param>
    /// <param name="progress">Progress reporter for the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task CopyFileWithProgressAsync(string sourcePath, string destinationPath, IProgress<string> progress, CancellationToken cancellationToken)
    {
        Logger.Information("Copying file from {Source} to {Destination}", sourcePath, destinationPath);

        try
        {
            // Ensure destination directory exists
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var sourceInfo = new FileInfo(sourcePath);
            var totalBytes = sourceInfo.Length;
            var copiedBytes = 0L;

            const int bufferSize = 1024 * 1024; // 1MB buffer
            var buffer = new byte[bufferSize];

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    copiedBytes += bytesRead;

                    var percentComplete = (int)((double)copiedBytes / totalBytes * 100);
                    progress?.Report($"Copying file... {percentComplete}% ({FormatBytes(copiedBytes)} / {FormatBytes(totalBytes)})");

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            Logger.Information("File copied successfully: {Destination}", destinationPath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to copy file from {Source} to {Destination}", sourcePath, destinationPath);
            throw;
        }
    }

    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A formatted string representation.</returns>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:F1} {suffixes[suffixIndex]}";
    }

    #endregion
}
