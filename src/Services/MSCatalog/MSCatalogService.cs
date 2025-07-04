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

        try
        {
            // Build the search query
            var searchQuery = BuildSearchQuery(request);
            Logger.Debug("Built search query: {Query}", searchQuery);

            // Keep searching until all pages are processed or limit reached
            var maxPagesToProcess = request.AllPages ? 40 : request.MaxPages; // Microsoft limit is 40 pages
            var hasNextPage = true;
            
            while (hasNextPage && currentPage <= maxPagesToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var url = $"{SearchUrl}?q={HttpUtility.UrlEncode(searchQuery)}";
                if (currentPage > 1)
                {
                    // Use 'p' parameter like PowerShell module (page number is 0-based for p parameter)
                    url += $"&p={currentPage - 1}";
                }

                Logger.Debug("Fetching page {CurrentPage}: {Url}", currentPage, url);
                var response = await _httpClient.GetStringAsync(url, cancellationToken);

                // Log response details for debugging
                Logger.Debug("Response received - Length: {Length} characters", response.Length);
                Logger.Verbose("Raw HTML response (first 1000 chars): {HtmlSnippet}", response.Length > 1000 ? response.Substring(0, 1000) : response);

                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // No need to extract total pages upfront like PowerShell module
                // We check for next page link on each iteration

                // Parse updates from the current page
                var pageUpdates = ParseUpdates(doc, request);
                updates.AddRange(pageUpdates);

                Logger.Debug("Found {Count} updates on page {Page}", pageUpdates.Count, currentPage);

                // Debug: Log all navigation links found
                var allLinks = doc.DocumentNode.SelectNodes("//a[contains(@id, 'Page') or contains(text(), 'Next') or contains(text(), 'Previous')]");
                if (allLinks != null)
                {
                    Logger.Debug("Found {Count} navigation links:", allLinks.Count);
                    foreach (var link in allLinks)
                    {
                        var id = link.GetAttributeValue("id", "no-id");
                        var text = link.InnerText?.Trim() ?? "no-text";
                        var onclick = link.GetAttributeValue("onclick", "no-onclick");
                        Logger.Debug("  Link: id='{Id}', text='{Text}', onclick='{OnClick}'", id, text, onclick);
                    }
                }
                
                // Check if there's a next page (try multiple selectors)
                var nextPageNode = doc.DocumentNode.SelectSingleNode("//a[@id='ctl00_catalogBody_nextPageLink']") ??
                                  doc.DocumentNode.SelectSingleNode("//a[@id='ctl00_catalogBody_nextPageLinkText']") ??
                                  doc.DocumentNode.SelectSingleNode("//a[contains(text(), 'Next')]") ??
                                  doc.DocumentNode.SelectSingleNode("//a[contains(@onclick, 'nextPageLinkText')]");
                
                hasNextPage = nextPageNode != null;
                
                if (hasNextPage)
                {
                    Logger.Debug("Next page link found (selector: {Selector}), continuing to page {NextPage}", 
                        nextPageNode.GetAttributeValue("id", "no-id"), currentPage + 1);
                }
                else
                {
                    Logger.Debug("No next page link found, stopping pagination");
                }

                currentPage++;
                
                // Microsoft Catalog limit is 40 pages (like in PowerShell module)
                if (currentPage > 40)
                {
                    Logger.Warning("Reached Microsoft Catalog page limit (40 pages), stopping");
                    break;
                }
            }

            Logger.Debug("Before filtering: {Count} updates", updates.Count);
            
            // Apply filters
            updates = ApplyFilters(updates, request).ToList();
            
            Logger.Debug("After filtering: {Count} updates", updates.Count);

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
        // Like PowerShell module, we don't need to determine total pages upfront
        // We just check for next page link on each iteration
        // Return 1 for first page, pagination logic will handle the rest
        return 1;
    }

    private List<MSCatalogUpdate> ParseUpdates(HtmlDocument doc, MSCatalogSearchRequest request)
    {
        var updates = new List<MSCatalogUpdate>();

        Logger.Debug("Starting to parse updates from HTML document");
        
        // Log all table elements for debugging
        var allTables = doc.DocumentNode.SelectNodes("//table");
        Logger.Debug("Found {TableCount} table elements in document", allTables?.Count ?? 0);
        
        if (allTables != null)
        {
            for (int i = 0; i < allTables.Count; i++)
            {
                var tableId = allTables[i].GetAttributeValue("id", "no-id");
                var tableClass = allTables[i].GetAttributeValue("class", "no-class");
                Logger.Debug("Table {Index}: id='{Id}', class='{Class}'", i, tableId, tableClass);
            }
        }

        // Find the results table
        var table = doc.DocumentNode.SelectSingleNode("//table[@id='ctl00_catalogBody_updateMatches']");
        if (table == null)
        {
            Logger.Warning("Could not find results table with id 'ctl00_catalogBody_updateMatches'");
            
            // Try alternative selectors
            var alternativeTable = doc.DocumentNode.SelectSingleNode("//table[contains(@id, 'updateMatches')]");
            if (alternativeTable != null)
            {
                Logger.Debug("Found alternative table with id: {Id}", alternativeTable.GetAttributeValue("id", ""));
                table = alternativeTable;
            }
            else
            {
                // Look for any table with results-like content
                var resultsTables = doc.DocumentNode.SelectNodes("//table[.//tr[contains(@class, 'result')]]");
                if (resultsTables != null && resultsTables.Count > 0)
                {
                    Logger.Debug("Found {Count} tables with result-like rows", resultsTables.Count);
                    table = resultsTables[0];
                }
            }
            
            if (table == null)
            {
                Logger.Error("No suitable results table found in the document");
                return updates;
            }
        }

        Logger.Debug("Found results table with id: {TableId}", table.GetAttributeValue("id", "no-id"));

        // Log all rows in the table for debugging
        var allRows = table.SelectNodes(".//tr");
        Logger.Debug("Found {RowCount} total rows in results table", allRows?.Count ?? 0);
        
        if (allRows != null)
        {
            for (int i = 0; i < Math.Min(allRows.Count, 5); i++) // Log first 5 rows
            {
                var rowClass = allRows[i].GetAttributeValue("class", "no-class");
                var cellCount = allRows[i].SelectNodes("./td")?.Count ?? 0;
                Logger.Debug("Row {Index}: class='{Class}', cells={CellCount}", i, rowClass, cellCount);
            }
        }

        // Parse each row (skip header)
        var rows = table.SelectNodes(".//tr[@class='resultsBackGround' or @class='resultsBackGroundHighlight']");
        if (rows == null)
        {
            Logger.Warning("Could not find result rows with expected classes");
            
            // Try alternative row selectors
            var alternativeRows = table.SelectNodes(".//tr[contains(@class, 'result')]");
            if (alternativeRows != null)
            {
                Logger.Debug("Found {Count} rows with alternative selector", alternativeRows.Count);
                rows = alternativeRows;
            }
            else
            {
                // Try to get all rows except the first (header)
                var allTableRows = table.SelectNodes(".//tr");
                if (allTableRows != null && allTableRows.Count > 1)
                {
                    rows = new HtmlNodeCollection(allTableRows[0]);
                    for (int i = 1; i < allTableRows.Count; i++)
                    {
                        rows.Add(allTableRows[i]);
                    }
                    Logger.Debug("Using all rows except header: {Count} rows", rows.Count);
                }
            }
            
            if (rows == null)
            {
                Logger.Error("No result rows found with any selector");
                return updates;
            }
        }

        Logger.Debug("Found {RowCount} result rows to parse", rows.Count);

        foreach (var row in rows)
        {
            try
            {
                var update = ParseUpdateRow(row);
                if (update != null)
                {
                    updates.Add(update);
                    Logger.Debug("Successfully parsed update: {Title}", update.Title);
                }
                else
                {
                    Logger.Debug("ParseUpdateRow returned null for a row");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error parsing update row");
            }
        }

        Logger.Debug("Completed parsing updates: {Count} updates found", updates.Count);
        return updates;
    }

    private MSCatalogUpdate ParseUpdateRow(HtmlNode row)
    {
        var cells = row.SelectNodes("./td");
        if (cells == null || cells.Count < 5)
        {
            Logger.Debug("Row has insufficient cells: {CellCount} (minimum 5 required)", cells?.Count ?? 0);
            if (cells != null)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    Logger.Debug("Cell {Index}: '{Content}'", i, cells[i].InnerText.Trim());
                }
            }
            return null;
        }

        Logger.Debug("Parsing row with {CellCount} cells", cells.Count);
        var update = new MSCatalogUpdate();

        // Title (column 2)
        var titleLink = cells[1].SelectSingleNode(".//a");
        if (titleLink != null)
        {
            update.Title = HttpUtility.HtmlDecode(titleLink.InnerText.Trim());
            Logger.Debug("Extracted title: {Title}", update.Title);
            
            // Extract GUID from onclick attribute
            var onclick = titleLink.GetAttributeValue("onclick", "");
            Logger.Debug("Link onclick attribute: {OnClick}", onclick);
            // Try multiple GUID patterns
            var guidMatch = Regex.Match(onclick, @"""([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})""");
            if (!guidMatch.Success)
            {
                guidMatch = Regex.Match(onclick, @"'([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})'");
            }
            if (guidMatch.Success)
            {
                update.Guid = guidMatch.Groups[1].Value;
                Logger.Debug("Extracted GUID: {Guid}", update.Guid);
            }
            else
            {
                Logger.Debug("No GUID found in onclick attribute");
            }
        }
        else
        {
            Logger.Debug("No title link found in cell 1");
            Logger.Debug("Cell 1 content: {Content}", cells[1].InnerText.Trim());
        }

        // Products (column 3)
        update.Products = HttpUtility.HtmlDecode(cells[2].InnerText.Trim());
        Logger.Debug("Extracted products: {Products}", update.Products);

        // Classification (column 4)
        update.Classification = HttpUtility.HtmlDecode(cells[3].InnerText.Trim());
        Logger.Debug("Extracted classification: {Classification}", update.Classification);

        // Last Updated (column 5)
        var lastUpdatedText = cells[4].InnerText.Trim();
        Logger.Debug("Last updated text: {LastUpdatedText}", lastUpdatedText);
        
        // Try multiple date formats
        var dateFormats = new[]
        {
            "M/d/yyyy",      // US format: 6/26/2025
            "d/M/yyyy",      // EU format: 26/6/2025
            "MM/dd/yyyy",    // US format with leading zeros
            "dd/MM/yyyy",    // EU format with leading zeros
            "yyyy-MM-dd",    // ISO format
            "M/dd/yyyy",     // Mixed format
            "dd/M/yyyy"      // Mixed format
        };
        
        bool dateParsed = false;
        foreach (var format in dateFormats)
        {
            if (DateTime.TryParseExact(lastUpdatedText, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var lastUpdated))
            {
                update.LastUpdated = lastUpdated;
                Logger.Debug("Parsed last updated with format '{Format}': {LastUpdated}", format, lastUpdated);
                dateParsed = true;
                break;
            }
        }
        
        if (!dateParsed)
        {
            // Try general parsing as fallback
            if (DateTime.TryParse(lastUpdatedText, out var lastUpdated))
            {
                update.LastUpdated = lastUpdated;
                Logger.Debug("Parsed last updated with general parsing: {LastUpdated}", lastUpdated);
            }
            else
            {
                Logger.Debug("Failed to parse last updated date with any format");
            }
        }

        // Version (column 6) - may not exist
        if (cells.Count > 5)
        {
            update.Version = HttpUtility.HtmlDecode(cells[5].InnerText.Trim());
            Logger.Debug("Extracted version: {Version}", update.Version);
        }

        // Size (column 7) - may not exist
        if (cells.Count > 6)
        {
            var sizeText = cells[6].InnerText.Trim();
            // Clean up size text - remove extra whitespace and newlines
            sizeText = Regex.Replace(sizeText, @"\s+", " ");
            // Extract just the size part (before any additional info)
            var sizeMatch = Regex.Match(sizeText, @"([\d.,]+\s*[KMGT]?B)");
            if (sizeMatch.Success)
            {
                update.Size = sizeMatch.Groups[1].Value.Trim();
                update.SizeInBytes = ParseSize(update.Size);
                Logger.Debug("Extracted size: {Size} ({SizeInBytes} bytes)", update.Size, update.SizeInBytes);
            }
            else
            {
                update.Size = sizeText;
                update.SizeInBytes = ParseSize(sizeText);
                Logger.Debug("Extracted size (raw): {Size} ({SizeInBytes} bytes)", update.Size, update.SizeInBytes);
            }
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
        var originalCount = updates.Count();
        Logger.Debug("Starting filters with {Count} updates", originalCount);
        
        // Filter by preview
        if (!request.IncludePreview)
        {
            var beforeCount = updates.Count();
            updates = updates.Where(u => !u.Title.Contains("Preview", StringComparison.OrdinalIgnoreCase));
            var afterCount = updates.Count();
            Logger.Debug("Preview filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        // Filter by dynamic
        if (!request.IncludeDynamic)
        {
            var beforeCount = updates.Count();
            updates = updates.Where(u => !u.Title.Contains("Dynamic", StringComparison.OrdinalIgnoreCase));
            var afterCount = updates.Count();
            Logger.Debug("Dynamic filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        // Filter by framework
        if (request.ExcludeFramework)
        {
            var beforeCount = updates.Count();
            updates = updates.Where(u => !u.Title.Contains(".NET Framework", StringComparison.OrdinalIgnoreCase));
            var afterCount = updates.Count();
            Logger.Debug("Framework exclude filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }
        else if (request.GetFramework)
        {
            var beforeCount = updates.Count();
            updates = updates.Where(u => u.Title.Contains(".NET Framework", StringComparison.OrdinalIgnoreCase));
            var afterCount = updates.Count();
            Logger.Debug("Framework only filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        // Filter by date
        if (request.FromDate.HasValue)
        {
            var beforeCount = updates.Count();
            Logger.Debug("Applying FromDate filter: {FromDate}", request.FromDate.Value);
            updates = updates.Where(u => {
                var include = u.LastUpdated >= request.FromDate.Value;
                if (!include)
                {
                    Logger.Verbose("Excluding update '{Title}' - date {UpdateDate} < {FromDate}", u.Title, u.LastUpdated, request.FromDate.Value);
                }
                return include;
            });
            var afterCount = updates.Count();
            Logger.Debug("FromDate filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        if (request.ToDate.HasValue)
        {
            var beforeCount = updates.Count();
            Logger.Debug("Applying ToDate filter: {ToDate}", request.ToDate.Value);
            updates = updates.Where(u => {
                var include = u.LastUpdated <= request.ToDate.Value;
                if (!include)
                {
                    Logger.Verbose("Excluding update '{Title}' - date {UpdateDate} > {ToDate}", u.Title, u.LastUpdated, request.ToDate.Value);
                }
                return include;
            });
            var afterCount = updates.Count();
            Logger.Debug("ToDate filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        // Filter by size
        if (request.MinSize.HasValue)
        {
            var beforeCount = updates.Count();
            var minSizeInBytes = (long)(request.MinSize.Value * 1024 * 1024);
            Logger.Debug("Applying MinSize filter: {MinSize} MB ({MinSizeBytes} bytes)", request.MinSize.Value, minSizeInBytes);
            updates = updates.Where(u => u.SizeInBytes >= minSizeInBytes);
            var afterCount = updates.Count();
            Logger.Debug("MinSize filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        if (request.MaxSize.HasValue)
        {
            var beforeCount = updates.Count();
            var maxSizeInBytes = (long)(request.MaxSize.Value * 1024 * 1024);
            Logger.Debug("Applying MaxSize filter: {MaxSize} MB ({MaxSizeBytes} bytes)", request.MaxSize.Value, maxSizeInBytes);
            updates = updates.Where(u => u.SizeInBytes <= maxSizeInBytes);
            var afterCount = updates.Count();
            Logger.Debug("MaxSize filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        // Filter by architecture
        if (!string.IsNullOrWhiteSpace(request.Architecture) && request.Architecture != "All")
        {
            var beforeCount = updates.Count();
            Logger.Debug("Applying Architecture filter: {Architecture}", request.Architecture);
            updates = updates.Where(u => u.Title.Contains(request.Architecture, StringComparison.OrdinalIgnoreCase));
            var afterCount = updates.Count();
            Logger.Debug("Architecture filter: {Before} → {After} (removed {Removed})", beforeCount, afterCount, beforeCount - afterCount);
        }

        var finalCount = updates.Count();
        Logger.Debug("Filtering completed: {Original} → {Final} updates", originalCount, finalCount);
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