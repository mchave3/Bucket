using System.Text.Json.Serialization;

namespace Bucket.Updater.Models.GitHub
{
    /// <summary>
    /// Represents a downloadable asset from a GitHub release
    /// </summary>
    public class GitHubAsset
    {
        /// <summary>
        /// Unique identifier for the GitHub asset
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// Filename of the asset
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Direct download URL for the asset
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Size of the asset file in bytes
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>
        /// MIME content type of the asset file
        /// </summary>
        [JsonPropertyName("content_type")]
        public string ContentType { get; set; } = string.Empty;
    }
}