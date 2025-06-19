namespace Bucket.Models;

/// <summary>
/// Represents a Windows image index with its associated metadata.
/// </summary>
public class WindowsImageIndex
{
    /// <summary>
    /// Gets or sets the index number of the Windows image.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the display name of the Windows edition.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the Windows edition.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the architecture of the Windows image (x86, x64, ARM64).
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the image in megabytes.
    /// </summary>
    public double SizeMB { get; set; }    /// <summary>
    /// Gets or sets whether this index is included/selected for operations.
    /// </summary>
    public bool IsIncluded { get; set; } = true;

    /// <summary>
    /// Gets the formatted size string for display purposes.
    /// </summary>
    public string FormattedSize => $"{SizeMB:F1} MB";

    /// <summary>
    /// Gets the formatted display text for the index.
    /// </summary>
    public string DisplayText => $"Index {Index}: {Name}";
}
