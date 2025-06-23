using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bucket.Models;

/// <summary>
/// Represents a Windows image file (WIM/ESD) with its associated indices and metadata.
/// </summary>
public partial class WindowsImageInfo : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier for this image.
    /// </summary>
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the display name of the image.
    /// </summary>
    [ObservableProperty]
    private string name = string.Empty;

    /// <summary>
    /// Gets or sets the full path to the image file.
    /// </summary>
    [ObservableProperty]
    private string filePath = string.Empty;

    /// <summary>
    /// Gets or sets the type of image file (WIM, ESD, etc.).
    /// </summary>
    [ObservableProperty]
    private string imageType = string.Empty;

    /// <summary>
    /// Gets or sets the creation date of the image.
    /// </summary>
    [ObservableProperty]
    private DateTime createdDate;

    /// <summary>
    /// Gets or sets the last modified date of the image.
    /// </summary>
    [ObservableProperty]
    private DateTime modifiedDate;

    /// <summary>
    /// Gets or sets the total size of the image file in bytes.
    /// </summary>
    [ObservableProperty]
    private long fileSizeBytes;

    /// <summary>
    /// Gets or sets the source ISO path if this image was imported from an ISO.
    /// </summary>
    [ObservableProperty]
    private string sourceIsoPath = string.Empty;

    /// <summary>
    /// Gets or sets the collection of Windows indices contained in this image.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<WindowsImageIndex> indices = new();

    /// <summary>
    /// Gets the formatted image type for display purposes.
    /// </summary>
    public string ImageTypeDisplay => string.IsNullOrEmpty(ImageType) ? "Image" : $"{ImageType} Image";

    /// <summary>
    /// Gets the formatted file size string for display purposes.
    /// </summary>
    public string FormattedFileSize
    {
        get
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            return FileSizeBytes switch
            {
                >= GB => $"{(double)FileSizeBytes / GB:F2} GB",
                >= MB => $"{(double)FileSizeBytes / MB:F1} MB",
                >= KB => $"{(double)FileSizeBytes / KB:F0} KB",
                _ => $"{FileSizeBytes} bytes"
            };
        }
    }

    /// <summary>
    /// Gets the number of indices in this image.
    /// </summary>
    public int IndexCount => Indices.Count;

    /// <summary>
    /// Gets the number of included indices.
    /// </summary>
    public int IncludedIndexCount => Indices.Count(i => i.IsIncluded);

    /// <summary>
    /// Gets the file name from the full path.
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// Gets whether the image file exists on disk.
    /// </summary>
    public bool FileExists => File.Exists(FilePath);

    /// <summary>
    /// Gets a summary of included indices for display.
    /// </summary>
    public string IncludedIndicesSummary
    {
        get
        {
            var includedCount = IncludedIndexCount;
            return includedCount == IndexCount ? $"All {IndexCount} indices" : $"{includedCount} of {IndexCount} indices";
        }
    }

    /// <summary>
    /// Partial method called when ImageType property changes.
    /// </summary>
    partial void OnImageTypeChanged(string value)
    {
        OnPropertyChanged(nameof(ImageTypeDisplay));
    }

    /// <summary>
    /// Partial method called when FileSizeBytes property changes.
    /// </summary>
    partial void OnFileSizeBytesChanged(long value)
    {
        OnPropertyChanged(nameof(FormattedFileSize));
    }

    /// <summary>
    /// Partial method called when FilePath property changes.
    /// </summary>
    partial void OnFilePathChanged(string value)
    {
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(FileExists));
    }

    /// <summary>
    /// Partial method called when Indices property changes.
    /// </summary>
    partial void OnIndicesChanged(ObservableCollection<WindowsImageIndex> value)
    {
        OnPropertyChanged(nameof(IndexCount));
        OnPropertyChanged(nameof(IncludedIndexCount));
        OnPropertyChanged(nameof(IncludedIndicesSummary));

        // Subscribe to collection changes
        if (value != null)
        {
            value.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IndexCount));
                OnPropertyChanged(nameof(IncludedIndexCount));
                OnPropertyChanged(nameof(IncludedIndicesSummary));
            };
        }
    }
}
