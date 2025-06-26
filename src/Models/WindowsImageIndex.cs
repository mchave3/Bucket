using CommunityToolkit.Mvvm.ComponentModel;

namespace Bucket.Models;

/// <summary>
/// Represents a Windows image index with its associated metadata.
/// Contains both basic information (available from listing all indices) and detailed information (available when querying a specific index).
/// </summary>
public partial class WindowsImageIndex : ObservableObject
{
    #region Basic Properties (Available from image listing)

    /// <summary>
    /// Gets or sets the index number of the Windows image.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the display name of the Windows edition.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the Windows edition.
    /// </summary>
    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets the architecture of the Windows image (x86, x64, ARM64).
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the image in megabytes.
    /// </summary>
    public double SizeMB { get; set; }

    /// <summary>
    /// Gets or sets whether this index is included/selected for operations.
    /// </summary>
    [ObservableProperty]
    private bool _isIncluded = true;

    #endregion

    #region Detailed Properties (Available from specific index query)

    /// <summary>
    /// Gets or sets whether WIMBoot is enabled for this image.
    /// </summary>
    public bool? WIMBoot { get; set; }

    /// <summary>
    /// Gets or sets the Windows version information.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service pack build number.
    /// </summary>
    public int? SPBuild { get; set; }

    /// <summary>
    /// Gets or sets the service pack level.
    /// </summary>
    public int? SPLevel { get; set; }

    /// <summary>
    /// Gets or sets the Windows edition identifier.
    /// </summary>
    public string EditionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the installation type (e.g., Client, Server).
    /// </summary>
    public string InstallationType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product type (e.g., WinNT).
    /// </summary>
    public string ProductType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product suite information.
    /// </summary>
    public string ProductSuite { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the system root directory (typically WINDOWS).
    /// </summary>
    public string SystemRoot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HAL (Hardware Abstraction Layer) information.
    /// </summary>
    public string Hal { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of directories in the image.
    /// </summary>
    public int? DirectoryCount { get; set; }

    /// <summary>
    /// Gets or sets the number of files in the image.
    /// </summary>
    public int? FileCount { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the image.
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the last modification date and time of the image.
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// Gets or sets the supported languages.
    /// </summary>
    public string Languages { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether detailed information has been loaded for this index.
    /// </summary>
    public bool HasDetailedInfo { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets the formatted size string for display purposes.
    /// </summary>
    public string FormattedSize => $"{SizeMB:F1} MB";

    /// <summary>
    /// Gets the formatted display text for the index.
    /// </summary>
    public string DisplayText => $"Index {Index}: {Name}";

    /// <summary>
    /// Gets the formatted version string with build information.
    /// </summary>
    public string FormattedVersion
    {
        get
        {
            if (string.IsNullOrEmpty(Version)) return "Unknown";

            var versionText = Version;
            if (SPBuild.HasValue && SPBuild.Value > 0)
            {
                versionText += $" (Build {SPBuild})";
            }
            return versionText;
        }
    }

    /// <summary>
    /// Gets formatted file and directory count information.
    /// </summary>
    public string FormattedCounts
    {
        get
        {
            if (!FileCount.HasValue || !DirectoryCount.HasValue)
                return "N/A";

            return $"{FileCount:N0} files, {DirectoryCount:N0} directories";
        }
    }

    /// <summary>
    /// Gets the formatted creation time string.
    /// </summary>
    public string FormattedCreatedTime => CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

    /// <summary>
    /// Gets the formatted modification time string.
    /// </summary>
    public string FormattedModifiedTime => ModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Partial method called when Name property changes.
    /// </summary>
    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayText));
    }

    /// <summary>
    /// Partial method called when IsIncluded property changes.
    /// </summary>
    partial void OnIsIncludedChanged(bool value)
    {
        // This can be used to notify parent objects about selection changes
    }

    #endregion
}
