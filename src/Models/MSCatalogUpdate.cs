using System;

namespace Bucket.Models
{
    /// <summary>
    /// Represents a Microsoft Update Catalog update entry
    /// </summary>
    public class MSCatalogUpdate
    {
        /// <summary>
        /// Gets or sets the title of the update
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the products this update applies to
        /// </summary>
        public string Products { get; set; }

        /// <summary>
        /// Gets or sets the classification of the update (e.g., Security Updates, Critical Updates)
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Gets or sets the last updated date
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the version of the update
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the size of the update as a formatted string (e.g., "302 MB")
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Gets or sets the size of the update in bytes
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the unique GUID of the update
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the file names associated with this update
        /// </summary>
        public string[] FileNames { get; set; }

        /// <summary>
        /// Gets or sets whether this update is selected for download
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the operating system this update is for (populated when searching by OS)
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the OS version this update is for (populated when searching by OS)
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// Gets or sets the update type (populated when searching by update type)
        /// </summary>
        public string UpdateType { get; set; }
    }
} 