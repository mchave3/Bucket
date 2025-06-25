using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Interface for Windows image PowerShell operations and JSON parsing.
/// Handles interaction with PowerShell Get-WindowsImage cmdlet and parsing of results.
/// </summary>
public interface IWindowsImagePowerShellService
{
    /// <summary>
    /// Analyzes a WIM/ESD file and extracts its indices asynchronously using PowerShell Get-WindowsImage.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image indices.</returns>
    Task<List<WindowsImageIndex>> AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information for a specific Windows image index.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <param name="index">The index number to get detailed information for.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A WindowsImageIndex object with detailed information, or null if not found.</returns>
    Task<WindowsImageIndex> GetImageIndexDetailsAsync(string imagePath, int index, IProgress<string> progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the file format is supported for Windows imaging.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>True if the format is supported, false otherwise.</returns>
    bool IsValidImageFormat(string extension);
}
