namespace Bucket.Updater.Models
{
    /// <summary>
    /// Contains complete information about an available software update
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// Version number of the update
        /// </summary>
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// Git tag name for the release
        /// </summary>
        public string TagName { get; set; } = string.Empty;
        
        /// <summary>
        /// Display name of the release
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Release notes and changelog
        /// </summary>
        public string Body { get; set; } = string.Empty;
        
        /// <summary>
        /// When the release was published
        /// </summary>
        public DateTime PublishedAt { get; set; }
        
        /// <summary>
        /// Whether this is a prerelease version
        /// </summary>
        public bool IsPrerelease { get; set; }
        
        /// <summary>
        /// List of downloadable assets for this release
        /// </summary>
        public List<UpdateAsset> Assets { get; set; } = new();
        
        /// <summary>
        /// Direct download URL for the primary installer
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Size of the primary installer file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Release channel (Release or Nightly)
        /// </summary>
        public UpdateChannel Channel { get; set; }
        
        /// <summary>
        /// Target system architecture
        /// </summary>
        public SystemArchitecture Architecture { get; set; }
    }
}