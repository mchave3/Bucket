using System;

namespace Bucket.Models
{
    /// <summary>
    /// Represents a search request for the Microsoft Update Catalog
    /// </summary>
    public class MSCatalogSearchRequest
    {
        /// <summary>
        /// Gets or sets the search mode
        /// </summary>
        public SearchMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the search query (used in SearchQuery mode)
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Gets or sets the operating system to search for (used in OperatingSystem mode)
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the OS version (e.g., "24H2", "23H2")
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the update type (e.g., "Cumulative Updates", "Security Updates")
        /// </summary>
        public string UpdateType { get; set; }

        /// <summary>
        /// Gets or sets the architecture filter (e.g., "x64", "x86", "ARM64", "All")
        /// </summary>
        public string Architecture { get; set; } = "All";

        /// <summary>
        /// Gets or sets whether to use strict search (exact phrase matching)
        /// </summary>
        public bool StrictSearch { get; set; }

        /// <summary>
        /// Gets or sets whether to search through all available pages
        /// </summary>
        public bool AllPages { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of pages to process (ignored if AllPages is true)
        /// </summary>
        public int MaxPages { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to include preview updates
        /// </summary>
        public bool IncludePreview { get; set; }

        /// <summary>
        /// Gets or sets whether to include dynamic updates
        /// </summary>
        public bool IncludeDynamic { get; set; }

        /// <summary>
        /// Gets or sets whether to exclude .NET Framework updates
        /// </summary>
        public bool ExcludeFramework { get; set; }

        /// <summary>
        /// Gets or sets whether to only show .NET Framework updates
        /// </summary>
        public bool GetFramework { get; set; }

        /// <summary>
        /// Gets or sets the from date filter
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Gets or sets the to date filter
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Gets or sets the minimum size filter in MB
        /// </summary>
        public double? MinSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size filter in MB
        /// </summary>
        public double? MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the field to sort by (e.g., "Date", "Size", "Title", "Classification", "Product")
        /// </summary>
        public string SortBy { get; set; } = "Date";

        /// <summary>
        /// Gets or sets whether to sort in descending order
        /// </summary>
        public bool Descending { get; set; } = true;
    }

    /// <summary>
    /// Defines the search mode for the catalog search
    /// </summary>
    public enum SearchMode
    {
        /// <summary>
        /// Search by operating system parameters
        /// </summary>
        OperatingSystem,

        /// <summary>
        /// Search using a custom query string
        /// </summary>
        SearchQuery
    }
} 