using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bucket.Models;
using HtmlAgilityPack;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Bucket.Services.MSCatalog;

public class MSCatalogService : IMSCatalogService
{
    private readonly HttpClient _httpClient;
    private const string CatalogUrl = "https://www.catalog.update.microsoft.com";
    private const string SearchUrl = "https://www.catalog.update.microsoft.com/Search.aspx";
    private const string DownloadUrl = "https://www.catalog.update.microsoft.com/DownloadDialog.aspx";

    public MSCatalogService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }

    public async Task<IEnumerable<MSCatalogUpdate>> SearchUpdatesAsync(MSCatalogSearchRequest request, CancellationToken cancellationToken = default)
    {
        Logger.Information("Starting MS Catalog search with request: {@Request}", request);
        
        var updates = new List<MSCatalogUpdate>();
        var currentPage = 1;
        var totalPages = 1;

        try
        {
            // Build the search query
            var searchQuery = BuildSearchQuery(request);
            Logger.Debug("Built search query: {Query}", searchQuery);

            // Keep searching until all pages are processed or limit reached
            var maxPagesToProcess = request.AllPages ? int.MaxValue : 5;
            while (currentPage <= totalPages && currentPage <= maxPagesToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var url = $"{SearchUrl}?q={HttpUtility.UrlEncode(searchQuery)}";
                if (currentPage > 1)
                {
                    url += $"&PageNo={currentPage}";
                }

                Logger.Debug("Fetching page {CurrentPage}: {Url}", currentPage, url);
                var response = await _httpClient.GetStringAsync(url, cancellationToken);

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // Extract total pages on first request
                if (currentPage == 1)
                {
                    totalPages = ExtractTotalPages(doc);
                    Logger.Information("Total pages available: {TotalPages}", totalPages);
                }

                // Parse updates from the current page
                var pageUpdates = ParseUpdates(doc, request);
                updates.AddRange(pageUpdates);

                Logger.Debug("Found {Count} updates on page {Page}", pageUpdates.Count, currentPage);

                currentPage++;
            }

            // Apply filters
            updates = ApplyFilters(updates, request).ToList();

            // Apply sorting
            updates = ApplySorting(updates, request).ToList();

            Logger.Information("Search completed. Found {TotalCount} updates after filtering", updates.Count);
            return updates;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error searching MS Catalog");
            throw;
        }
    }

    public async Task<bool> DownloadUpdateAsync(MSCatalogUpdate update, string destinationPath, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Starting download for update: {Title} ({Guid})", update.Title, update.Guid);

        try
        {
            // Get download links
            var downloadLinks = await GetDownloadLinksAsync(update.Guid, cancellationToken);
            if (!downloadLinks.Any())
            {
                Logger.Warning("No download links found for update {Guid}", update.Guid);
                return false;
            }

            // Create destination directory
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Download the first link (usually there's only one)
            var downloadUrl = downloadLinks.First();
            Logger.Debug("Downloading from URL: {Url}", downloadUrl);

            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progress != null;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[8192];
            var totalRead = 0L;
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                totalRead += read;

                if (canReportProgress)
                {
                    progress.Report((double)totalRead / totalBytes);
                }
            }

            Logger.Information("Successfully downloaded update to: {Path}", destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error downloading update {Guid}", update.Guid);
            return false;
        }
    }

    public async Task<bool> DownloadMultipleUpdatesAsync(IEnumerable<MSCatalogUpdate> updates, string destinationFolder, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default)
    {
        var updatesList = updates.ToList();
        var successCount = 0;
        var totalCount = updatesList.Count;

        Logger.Information("Starting download of {Count} updates", totalCount);

        for (var i = 0; i < totalCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var update = updatesList[i];
            var fileName = SanitizeFileName(update.FileNames?.FirstOrDefault() ?? $"{update.Guid}.cab");
            var filePath = Path.Combine(destinationFolder, fileName);

            var downloadProgress = new Progress<double>(p =>
            {
                progress?.Report(new DownloadProgress
                {
                    CurrentFile = fileName,
                    CurrentFileProgress = p * 100,
                    TotalFiles = totalCount,
                    CurrentFileIndex = i + 1,
                    OverallProgress = ((i + p) / totalCount) * 100
                });
            });

            var success = await DownloadUpdateAsync(update, filePath, downloadProgress, cancellationToken);
            if (success)
            {
                successCount++;
            }
        }

        Logger.Information("Download completed. Successfully downloaded {Success}/{Total} updates", successCount, totalCount);
        return successCount == totalCount;
    }

    public async Task<bool> ExportToExcelAsync(IEnumerable<MSCatalogUpdate> updates, string filePath, CancellationToken cancellationToken = default)
    {
        Logger.Information("Exporting {Count} updates to Excel: {Path}", updates.Count(), filePath);

        try
        {
            await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("MS Catalog Updates");

                // Add headers
                var headers = new[] { "Title", "Products", "Classification", "Last Updated", "Version", "Size", "Update Type", "OS", "OS Version", "GUID" };
                for (var i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Add data
                var row = 2;
                foreach (var update in updates)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    worksheet.Cells[row, 1].Value = update.Title;
                    worksheet.Cells[row, 2].Value = update.Products;
                    worksheet.Cells[row, 3].Value = update.Classification;
                    worksheet.Cells[row, 4].Value = update.LastUpdated;
                    worksheet.Cells[row, 5].Value = update.Version;
                    worksheet.Cells[row, 6].Value = update.Size;
                    worksheet.Cells[row, 7].Value = update.UpdateType;
                    worksheet.Cells[row, 8].Value = update.OperatingSystem;
                    worksheet.Cells[row, 9].Value = update.OSVersion;
                    worksheet.Cells[row, 10].Value = update.Guid;

                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the file
                var fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }, cancellationToken);

            Logger.Information("Successfully exported updates to Excel");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error exporting to Excel");
            return false;
        }
    }

    private string BuildSearchQuery(MSCatalogSearchRequest request)
    {
        if (request.Mode == SearchMode.SearchQuery)
        {
            return request.SearchQuery ?? string.Empty;
        }

        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.OperatingSystem))
        {
            queryParts.Add(request.OperatingSystem);
        }

        if (!string.IsNullOrWhiteSpace(request.Version))
        {
            queryParts.Add(request.Version);
        }

        if (!string.IsNullOrWhiteSpace(request.UpdateType))
        {
            queryParts.Add(request.UpdateType);
        }

        if (!string.IsNullOrWhiteSpace(request.Architecture) && request.Architecture != "All")
        {
            queryParts.Add(request.Architecture);
        }

        return string.Join(" ", queryParts);
    }

    private int ExtractTotalPages(HtmlDocument doc)
    {
        // Find the pagination control
        var paginationNode = doc.DocumentNode.SelectSingleNode("//div[@class='resultsPageNumberGroup']");
        if (paginationNode == null)
        {
            return 1;
        }

        // Extract page numbers
        var pageLinks = paginationNode.SelectNodes(".//a[@class='resultsPageNumber']");
        if (pageLinks == null || !pageLinks.Any())
        {
            return 1;
        }

        var maxPage = 1;
        foreach (var link in pageLinks)
        {
            if (int.TryParse(link.InnerText.Trim(), out var pageNum))
            {
                maxPage = Math.Max(maxPage, pageNum);
            }
        }

        return maxPage;
    }

    private List<MSCatalogUpdate> ParseUpdates(HtmlDocument doc, MSCatalogSearchRequest request)
    {
        var updates = new List<MSCatalogUpdate>();

        // Find the results table
        var table = doc.DocumentNode.SelectSingleNode("//table[@id='ctl00_catalogBody_updateMatches']");
        if (table == null)
        {
            return updates;
        }

        // Parse each row (skip header)
        var rows = table.SelectNodes(".//tr[@class='resultsBackGround' or @class='resultsBackGroundHighlight']");
        if (rows == null)
        {
            return updates;
        }

        foreach (var row in rows)
        {
            try
            {
                var update = ParseUpdateRow(row);
                if (update != null)
                {
                    updates.Add(update);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error parsing update row");
            }
        }

        return updates;
    }

    private MSCatalogUpdate ParseUpdateRow(HtmlNode row)
    {
        var cells = row.SelectNodes("./td");
        if (cells == null || cells.Count < 5)
        {
            return null;
        }

        var update = new MSCatalogUpdate();

        // Title (column 2)
        var titleLink = cells[1].SelectSingleNode(".//a");
        if (titleLink != null)
        {
            update.Title = HttpUtility.HtmlDecode(titleLink.InnerText.Trim());
            
            // Extract GUID from onclick attribute
            var onclick = titleLink.GetAttributeValue("onclick", "");
            var guidMatch = Regex.Match(onclick, @"'([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})'");
            if (guidMatch.Success)
            {
                update.Guid = guidMatch.Groups[1].Value;
            }
        }

        // Products (column 3)
        update.Products = HttpUtility.HtmlDecode(cells[2].InnerText.Trim());

        // Classification (column 4)
        update.Classification = HttpUtility.HtmlDecode(cells[3].InnerText.Trim());

        // Last Updated (column 5)
        if (DateTime.TryParse(cells[4].InnerText.Trim(), out var lastUpdated))
        {
            update.LastUpdated = lastUpdated;
        }

        // Version (column 6) - may not exist
        if (cells.Count > 5)
        {
            update.Version = HttpUtility.HtmlDecode(cells[5].InnerText.Trim());
        }

        // Size (column 7) - may not exist
        if (cells.Count > 6)
        {
            var sizeText = cells[6].InnerText.Trim();
            update.Size = sizeText;
            update.SizeInBytes = ParseSize(sizeText);
        }

        // Extract additional info from title
        ExtractAdditionalInfo(update);

        return update;
    }

    private void ExtractAdditionalInfo(MSCatalogUpdate update)
    {
        if (string.IsNullOrEmpty(update.Title))
        {
            return;
        }

        // Extract OS information
        if (update.Title.Contains("Windows 11", StringComparison.OrdinalIgnoreCase))
        {
            update.OperatingSystem = "Windows 11";
            ExtractWindowsVersion(update, "Windows 11");
        }
        else if (update.Title.Contains("Windows 10", StringComparison.OrdinalIgnoreCase))
        {
            update.OperatingSystem = "Windows 10";
            ExtractWindowsVersion(update, "Windows 10");
        }
        else if (update.Title.Contains("Server 2022", StringComparison.OrdinalIgnoreCase))
        {
            update.OperatingSystem = "Windows Server 2022";
        }
        else if (update.Title.Contains("Server 2019", StringComparison.OrdinalIgnoreCase))
        {
            update.OperatingSystem = "Windows Server 2019";
        }

        // Extract update type
        if (update.Title.Contains("Cumulative Update", StringComparison.OrdinalIgnoreCase))
        {
            update.UpdateType = "Cumulative Update";
        }
        else if (update.Title.Contains("Dynamic Update", StringComparison.OrdinalIgnoreCase))
        {
            update.UpdateType = "Dynamic Update";
        }
        else if (update.Title.Contains("Feature Update", StringComparison.OrdinalIgnoreCase))
        {
            update.UpdateType = "Feature Update";
        }
        else if (update.Title.Contains(".NET Framework", StringComparison.OrdinalIgnoreCase))
        {
            update.UpdateType = ".NET Framework";
        }

        // Extract file names from title (usually in parentheses)
        var fileNameMatch = Regex.Match(update.Title, @"\(([^)]+\.(cab|msu|exe|zip))\)", RegexOptions.IgnoreCase);
        if (fileNameMatch.Success)
        {
            update.FileNames = new[] { fileNameMatch.Groups[1].Value };
        }
    }

    private void ExtractWindowsVersion(MSCatalogUpdate update, string osName)
    {
        // Look for version patterns like "Version 22H2", "Version 21H1"
        var versionMatch = Regex.Match(update.Title, @"Version\s+(\d{2}H\d)", RegexOptions.IgnoreCase);
        if (versionMatch.Success)
        {
            update.OSVersion = versionMatch.Groups[1].Value;
        }
        else
        {
            // Look for build numbers
            var buildMatch = Regex.Match(update.Title, @"(\d{5}\.\d+)", RegexOptions.IgnoreCase);
            if (buildMatch.Success)
            {
                update.OSVersion = buildMatch.Groups[1].Value;
            }
        }
    }

    private long ParseSize(string sizeText)
    {
        if (string.IsNullOrEmpty(sizeText))
        {
            return 0;
        }

        sizeText = sizeText.Trim().ToUpper();
        var match = Regex.Match(sizeText, @"([\d.]+)\s*(KB|MB|GB|TB)?");
        if (!match.Success)
        {
            return 0;
        }

        if (!double.TryParse(match.Groups[1].Value, out var value))
        {
            return 0;
        }

        var unit = match.Groups[2].Value;
        return unit switch
        {
            "KB" => (long)(value * 1024),
            "MB" => (long)(value * 1024 * 1024),
            "GB" => (long)(value * 1024 * 1024 * 1024),
            "TB" => (long)(value * 1024 * 1024 * 1024 * 1024),
            _ => (long)value
        };
    }

    private IEnumerable<MSCatalogUpdate> ApplyFilters(IEnumerable<MSCatalogUpdate> updates, MSCatalogSearchRequest request)
    {
        // Filter by preview
        if (!request.IncludePreview)
        {
            updates = updates.Where(u => !u.Title.Contains("Preview", StringComparison.OrdinalIgnoreCase));
        }

        // Filter by dynamic
        if (!request.IncludeDynamic)
        {
            updates = updates.Where(u => !u.Title.Contains("Dynamic", StringComparison.OrdinalIgnoreCase));
        }

        // Filter by framework
        if (request.ExcludeFramework)
        {
            updates = updates.Where(u => !u.Title.Contains(".NET Framework", StringComparison.OrdinalIgnoreCase));
        }
        else if (request.GetFramework)
        {
            updates = updates.Where(u => u.Title.Contains(".NET Framework", StringComparison.OrdinalIgnoreCase));
        }

        // Filter by date
        if (request.FromDate.HasValue)
        {
            updates = updates.Where(u => u.LastUpdated >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            updates = updates.Where(u => u.LastUpdated <= request.ToDate.Value);
        }

        // Filter by size
        if (request.MinSize.HasValue)
        {
            var minSizeInBytes = (long)(request.MinSize.Value * 1024 * 1024);
            updates = updates.Where(u => u.SizeInBytes >= minSizeInBytes);
        }

        if (request.MaxSize.HasValue)
        {
            var maxSizeInBytes = (long)(request.MaxSize.Value * 1024 * 1024);
            updates = updates.Where(u => u.SizeInBytes <= maxSizeInBytes);
        }

        // Filter by architecture
        if (!string.IsNullOrWhiteSpace(request.Architecture) && request.Architecture != "All")
        {
            updates = updates.Where(u => u.Title.Contains(request.Architecture, StringComparison.OrdinalIgnoreCase));
        }

        return updates;
    }

    private IEnumerable<MSCatalogUpdate> ApplySorting(IEnumerable<MSCatalogUpdate> updates, MSCatalogSearchRequest request)
    {
        return request.SortBy switch
        {
            "Date" => request.Descending ? updates.OrderByDescending(u => u.LastUpdated) : updates.OrderBy(u => u.LastUpdated),
            "Size" => request.Descending ? updates.OrderByDescending(u => u.SizeInBytes) : updates.OrderBy(u => u.SizeInBytes),
            "Title" => request.Descending ? updates.OrderByDescending(u => u.Title) : updates.OrderBy(u => u.Title),
            "Classification" => request.Descending ? updates.OrderByDescending(u => u.Classification) : updates.OrderBy(u => u.Classification),
            "Product" => request.Descending ? updates.OrderByDescending(u => u.Products) : updates.OrderBy(u => u.Products),
            _ => updates
        };
    }

    private async Task<List<string>> GetDownloadLinksAsync(string updateGuid, CancellationToken cancellationToken)
    {
        var links = new List<string>();

        try
        {
            // POST to download dialog
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("updateIDs", $"[{{\"size\":0,\"languages\":\"\",\"uidInfo\":\"{updateGuid}\",\"updateID\":\"{updateGuid}\"}}]"),
                new KeyValuePair<string, string>("updateIDsblocked", ""),
                new KeyValuePair<string, string>("wsusApiPresent", ""),
                new KeyValuePair<string, string>("contentImport", ""),
                new KeyValuePair<string, string>("sku", ""),
                new KeyValuePair<string, string>("serverName", ""),
                new KeyValuePair<string, string>("ssl", ""),
                new KeyValuePair<string, string>("portNumber", ""),
                new KeyValuePair<string, string>("version", "")
            });

            var response = await _httpClient.PostAsync(DownloadUrl, content, cancellationToken);
            var html = await response.Content.ReadAsStringAsync(cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract download links
            var linkNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, 'download.windowsupdate.com')]");
            if (linkNodes != null)
            {
                foreach (var link in linkNodes)
                {
                    var href = link.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(href))
                    {
                        links.Add(href);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error getting download links for {Guid}", updateGuid);
        }

        return links;
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder(fileName);

        foreach (var c in invalidChars)
        {
            sanitized.Replace(c, '_');
        }

        return sanitized.ToString();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
} 