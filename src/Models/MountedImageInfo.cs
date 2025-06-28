using CommunityToolkit.Mvvm.ComponentModel;

namespace Bucket.Models;

/// <summary>
/// Represents information about a mounted Windows image.
/// </summary>
public partial class MountedImageInfo : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for this mount.
    /// </summary>
    [ObservableProperty]
    private string id = string.Empty;

    /// <summary>
    /// Gets or sets the path to the WIM/ESD file.
    /// </summary>
    [ObservableProperty]
    private string imagePath = string.Empty;

    /// <summary>
    /// Gets or sets the index number that is mounted.
    /// </summary>
    [ObservableProperty]
    private int index;

    /// <summary>
    /// Gets or sets the mount directory path.
    /// </summary>
    [ObservableProperty]
    private string mountPath = string.Empty;

    /// <summary>
    /// Gets or sets the friendly name of the image.
    /// </summary>
    [ObservableProperty]
    private string imageName = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Windows edition.
    /// </summary>
    [ObservableProperty]
    private string editionName = string.Empty;

    /// <summary>
    /// Gets or sets the mount timestamp.
    /// </summary>
    [ObservableProperty]
    private DateTime mountedAt = DateTime.Now;

    /// <summary>
    /// Gets or sets the mount status.
    /// </summary>
    [ObservableProperty]
    private MountStatus status = MountStatus.Mounted;

    /// <summary>
    /// Gets the display text for the mount.
    /// </summary>
    public string DisplayText => $"{ImageName} - Index {Index} ({EditionName})";

    /// <summary>
    /// Gets the formatted mount time.
    /// </summary>
    public string FormattedMountTime => MountedAt.ToString("yyyy-MM-dd HH:mm:ss");
}

/// <summary>
/// Represents the status of a mounted image.
/// </summary>
public enum MountStatus
{
    /// <summary>
    /// The image is successfully mounted.
    /// </summary>
    Mounted,

    /// <summary>
    /// The image is currently being mounted.
    /// </summary>
    Mounting,

    /// <summary>
    /// The image is currently being unmounted.
    /// </summary>
    Unmounting,

    /// <summary>
    /// The mount has an error.
    /// </summary>
    Error
} 