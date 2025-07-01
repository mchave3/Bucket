using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bucket.Models;

namespace Bucket.Services.MSCatalog
{
    /// <summary>
    /// Interface for the Microsoft Update Catalog service
    /// </summary>
    public interface IMSCatalogService
    {
        /// <summary>
        /// Searches for updates in the Microsoft Update Catalog
        /// </summary>
        /// <param name="request">The search request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of matching updates</returns>
        Task<IEnumerable<MSCatalogUpdate>> SearchUpdatesAsync(MSCatalogSearchRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a single update
        /// </summary>
        /// <param name="update">The update to download</param>
        /// <param name="destinationPath">The destination folder path</param>
        /// <param name="progress">Progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if download succeeded, false otherwise</returns>
        Task<bool> DownloadUpdateAsync(MSCatalogUpdate update, string destinationPath, IProgress<double> progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads multiple updates
        /// </summary>
        /// <param name="updates">The updates to download</param>
        /// <param name="destinationPath">The destination folder path</param>
        /// <param name="progress">Progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if all downloads succeeded, false otherwise</returns>
        Task<bool> DownloadMultipleUpdatesAsync(IEnumerable<MSCatalogUpdate> updates, string destinationPath, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports updates to an Excel file
        /// </summary>
        /// <param name="updates">The updates to export</param>
        /// <param name="filePath">The file path to save the Excel file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if export succeeded, false otherwise</returns>
        Task<bool> ExportToExcelAsync(IEnumerable<MSCatalogUpdate> updates, string filePath, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents download progress for multiple files
    /// </summary>
    public class DownloadProgress
    {
        /// <summary>
        /// Gets or sets the current file being downloaded
        /// </summary>
        public string CurrentFile { get; set; }

        /// <summary>
        /// Gets or sets the current file index (1-based)
        /// </summary>
        public int CurrentFileIndex { get; set; }

        /// <summary>
        /// Gets or sets the total number of files
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the current file progress (0-100)
        /// </summary>
        public double CurrentFileProgress { get; set; }

        /// <summary>
        /// Gets or sets the overall progress (0-100)
        /// </summary>
        public double OverallProgress { get; set; }

        /// <summary>
        /// Gets or sets the current download speed in bytes per second
        /// </summary>
        public long BytesPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the estimated time remaining
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }
} 