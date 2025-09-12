namespace Bucket.Updater.Models
{
    /// <summary>
    /// Represents a downloadable asset from a software release
    /// </summary>
    public class UpdateAsset
    {
        /// <summary>
        /// Name of the asset file
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Download URL for the asset
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Size of the asset in bytes
        /// </summary>
        public long Size { get; set; }
        
        /// <summary>
        /// MIME content type of the asset
        /// </summary>
        public string ContentType { get; set; } = string.Empty;
    }
}