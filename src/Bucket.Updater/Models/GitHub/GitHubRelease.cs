using System.Text.Json.Serialization;

namespace Bucket.Updater.Models.GitHub
{
    /// <summary>
    /// Represents a GitHub release with metadata and assets
    /// </summary>
    public class GitHubRelease
    {
        /// <summary>
        /// Unique identifier for the GitHub release
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// Git tag name for this release
        /// </summary>
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the release
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Release notes and description
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// When the release was published
        /// </summary>
        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Whether this is a prerelease version
        /// </summary>
        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        /// <summary>
        /// List of downloadable assets for this release
        /// </summary>
        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = new();
    }
}