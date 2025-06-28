using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Bucket.Models;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for Windows image PowerShell operations and JSON parsing.
/// Handles interaction with PowerShell Get-WindowsImage cmdlet and parsing of JSON results.
/// Provides functionality to analyze Windows images and extract detailed information.
/// </summary>
public class WindowsImagePowerShellService : IWindowsImagePowerShellService
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowsImagePowerShellService class.
    /// </summary>
    public WindowsImagePowerShellService()
    {
        Logger.Debug("WindowsImagePowerShellService initialized");
    }

    #endregion

    #region Public Methods - PowerShell Operations

    /// <summary>
    /// Analyzes a WIM/ESD file and extracts its indices asynchronously using PowerShell Get-WindowsImage.
    /// </summary>
    /// <param name="imagePath">The path to the WIM/ESD file.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Windows image indices.</returns>
    public async Task<List<WindowsImageIndex>> AnalyzeImageAsync(string imagePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Analyzing Windows image: {ImagePath}", imagePath);
        progress?.Report("Initializing image analysis...");

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        // Validate file format
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();
        if (!IsValidImageFormat(extension))
        {
            throw new NotSupportedException($"Unsupported image format: {extension}");
        }

        // Check available disk space
        var fileInfo = new FileInfo(imagePath);
        var driveInfo = new DriveInfo(Path.GetPathRoot(imagePath));
        if (driveInfo.AvailableFreeSpace < fileInfo.Length * 2) // Need at least 2x file size for operations
        {
            Logger.Warning("Low disk space detected: {AvailableSpace} bytes available, {FileSize} bytes needed",
                driveInfo.AvailableFreeSpace, fileInfo.Length * 2);
        }

        try
        {
            progress?.Report("Running PowerShell Get-WindowsImage analysis...");

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // Use PowerShell Get-WindowsImage to analyze the image
            Logger.Verbose("Executing PowerShell command: Get-WindowsImage -ImagePath '{ImagePath}' | ConvertTo-Json -Depth 10", imagePath);
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Get-WindowsImage -ImagePath '{imagePath}' | ConvertTo-Json -Depth 10\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), cancellationToken);

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString().Trim();
                Logger.Error("PowerShell Get-WindowsImage failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);

                if (errorMessage.Contains("Access is denied") || errorMessage.Contains("UnauthorizedAccess"))
                {
                    throw new UnauthorizedAccessException($"Access denied when analyzing image: {imagePath}. Please run as administrator.");
                }
                else if (errorMessage.Contains("cannot access") || errorMessage.Contains("FileNotFound"))
                {
                    throw new FileNotFoundException($"Cannot access image file: {imagePath}");
                }
                else if (errorMessage.Contains("Get-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    Logger.Fatal("Critical dependency missing: Get-WindowsImage PowerShell cmdlet not available. Application cannot function without Windows PowerShell module for imaging.");
                    throw new InvalidOperationException("The Get-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }

                throw new InvalidOperationException($"PowerShell Get-WindowsImage failed with exit code {process.ExitCode}: {errorMessage}");
            }

            progress?.Report("Parsing image analysis results...");

            var jsonOutput = outputBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(jsonOutput))
            {
                Logger.Warning("No output received from Get-WindowsImage for {ImagePath}", imagePath);
                return new List<WindowsImageIndex>();
            }

            // Parse the JSON output
            var indices = ParsePowerShellOutput(jsonOutput);

            Logger.Information("Successfully analyzed Windows image: {ImagePath}, found {Count} indices", imagePath, indices.Count);
            progress?.Report($"Analysis completed - found {indices.Count} image indices");

            return indices;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to analyze Windows image: {ImagePath}", imagePath);
            throw;
        }
    }

    /// <summary>
    /// Gets detailed information for a specific Windows image index.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <param name="index">The index number to get detailed information for.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A WindowsImageIndex object with detailed information, or null if not found.</returns>
    public async Task<WindowsImageIndex> GetImageIndexDetailsAsync(string imagePath, int index, IProgress<string> progress = null, CancellationToken cancellationToken = default)
    {
        Logger.Information("Getting detailed information for image index {Index} in {ImagePath}", index, imagePath);

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        try
        {
            progress?.Report($"Getting detailed information for index {index}...");

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            // Use PowerShell Get-WindowsImage with specific index to get detailed info
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"Get-WindowsImage -ImagePath '{imagePath}' -Index {index} | ConvertTo-Json -Depth 10\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), cancellationToken);

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString().Trim();
                Logger.Error("PowerShell Get-WindowsImage failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);

                if (errorMessage.Contains("Get-WindowsImage") && errorMessage.Contains("not recognized"))
                {
                    throw new InvalidOperationException("The Get-WindowsImage PowerShell cmdlet is not available. Please ensure the Windows PowerShell module for imaging is installed.");
                }

                throw new InvalidOperationException($"PowerShell Get-WindowsImage failed with exit code {process.ExitCode}: {errorMessage}");
            }

            progress?.Report("Parsing detailed image information...");

            var jsonOutput = outputBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(jsonOutput))
            {
                Logger.Warning("No output received from Get-WindowsImage for index {Index} in {ImagePath}", index, imagePath);
                return null;
            }

            // Parse the JSON output for detailed information
            var detailedIndices = ParsePowerShellOutput(jsonOutput);
            var detailedIndex = detailedIndices.FirstOrDefault();

            if (detailedIndex != null)
            {
                Logger.Information("Successfully loaded detailed information for index {Index}: {Name}", index, detailedIndex.Name);
            }
            else
            {
                Logger.Warning("No detailed information found for index {Index} in {ImagePath}", index, imagePath);
            }

            return detailedIndex;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to get detailed information for index {Index} in {ImagePath}", index, imagePath);
            throw;
        }
    }

    /// <summary>
    /// Validates if the file format is supported for Windows imaging.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>True if the format is supported, false otherwise.</returns>
    public bool IsValidImageFormat(string extension)
    {
        return extension switch
        {
            ".wim" => true,
            ".esd" => true,
            ".swm" => true,
            _ => false
        };
    }

    #endregion

    #region Private Methods - JSON Parsing

    /// <summary>
    /// Parses PowerShell Get-WindowsImage JSON output to extract Windows image indices.
    /// </summary>
    /// <param name="jsonOutput">The JSON output from PowerShell Get-WindowsImage cmdlet.</param>
    /// <returns>A list of Windows image indices.</returns>
    private static List<WindowsImageIndex> ParsePowerShellOutput(string jsonOutput)
    {
        var indices = new List<WindowsImageIndex>();

        try
        {
            // Handle both single object and array responses
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(jsonOutput);
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "Failed to parse PowerShell JSON output: {Output}", jsonOutput);
                throw new InvalidOperationException("Invalid JSON output from PowerShell Get-WindowsImage command", ex);
            }

            using (document)
            {
                JsonElement root = document.RootElement;

                // Handle array of images
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var imageElement in root.EnumerateArray())
                    {
                        var index = ParseImageElement(imageElement);
                        if (index != null)
                        {
                            indices.Add(index);
                        }
                    }
                }
                // Handle single image object
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    var index = ParseImageElement(root);
                    if (index != null)
                    {
                        indices.Add(index);
                    }
                }
            }

            Logger.Debug("Parsed {Count} image indices from PowerShell output", indices.Count);
            return indices;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to parse PowerShell output");
            throw;
        }
    }

    /// <summary>
    /// Parses a single image element from PowerShell JSON output.
    /// </summary>
    /// <param name="imageElement">The JSON element representing a single image.</param>
    /// <returns>A WindowsImageIndex object or null if parsing fails.</returns>
    private static WindowsImageIndex ParseImageElement(JsonElement imageElement)
    {
        try
        {
            var index = new WindowsImageIndex();

            // Extract index number
            if (imageElement.TryGetProperty("ImageIndex", out var indexProperty))
            {
                index.Index = indexProperty.GetInt32();
            }

            // Extract image name
            if (imageElement.TryGetProperty("ImageName", out var nameProperty))
            {
                index.Name = nameProperty.GetString() ?? string.Empty;
            }

            // Extract description
            if (imageElement.TryGetProperty("ImageDescription", out var descProperty))
            {
                index.Description = descProperty.GetString() ?? string.Empty;
            }

            // Extract architecture
            if (imageElement.TryGetProperty("Architecture", out var archProperty))
            {
                var archValue = archProperty.GetInt32();
                index.Architecture = archValue switch
                {
                    0 => "x86",
                    5 => "ARM",
                    6 => "IA64",
                    9 => "x64",
                    12 => "ARM64",
                    _ => archValue.ToString()
                };
            }

            // Extract size
            if (imageElement.TryGetProperty("ImageSize", out var sizeProperty))
            {
                var sizeBytes = sizeProperty.GetInt64();
                index.SizeMB = Math.Round(sizeBytes / (1024.0 * 1024.0), 1);
            }

            // Extract detailed properties (available when querying specific index)
            if (imageElement.TryGetProperty("WIMBoot", out var wimBootProperty))
            {
                index.WIMBoot = wimBootProperty.GetBoolean();
                index.HasDetailedInfo = true;
            }

            if (imageElement.TryGetProperty("Version", out var versionProperty))
            {
                index.Version = versionProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("SPBuild", out var spBuildProperty))
            {
                index.SPBuild = spBuildProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("SPLevel", out var spLevelProperty))
            {
                index.SPLevel = spLevelProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("EditionId", out var editionIdProperty))
            {
                index.EditionId = editionIdProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("InstallationType", out var installationTypeProperty))
            {
                index.InstallationType = installationTypeProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("ProductType", out var productTypeProperty))
            {
                index.ProductType = productTypeProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("ProductSuite", out var productSuiteProperty))
            {
                index.ProductSuite = GetStringOrArray(productSuiteProperty);
            }

            if (imageElement.TryGetProperty("SystemRoot", out var systemRootProperty))
            {
                index.SystemRoot = systemRootProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("Hal", out var halProperty))
            {
                index.Hal = halProperty.GetString() ?? string.Empty;
            }

            if (imageElement.TryGetProperty("DirectoryCount", out var dirCountProperty))
            {
                index.DirectoryCount = dirCountProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("FileCount", out var fileCountProperty))
            {
                index.FileCount = fileCountProperty.GetInt32();
            }

            if (imageElement.TryGetProperty("CreatedTime", out var createdTimeProperty))
            {
                var createdTimeString = createdTimeProperty.GetString();
                if (!string.IsNullOrEmpty(createdTimeString))
                {
                    // Handle both ISO 8601 format and Microsoft JSON.NET legacy format
                    DateTime createdTime;
                    if (TryParseMicrosoftJsonDate(createdTimeString, out createdTime) ||
                        DateTime.TryParse(createdTimeString, null, DateTimeStyles.RoundtripKind, out createdTime))
                    {
                        index.CreatedTime = createdTime;
                    }
                    else
                    {
                        Logger.Debug("Failed to parse CreatedTime: {CreatedTimeString}", createdTimeString);
                    }
                }
            }

            if (imageElement.TryGetProperty("ModifiedTime", out var modifiedTimeProperty))
            {
                var modifiedTimeString = modifiedTimeProperty.GetString();
                if (!string.IsNullOrEmpty(modifiedTimeString))
                {
                    // Handle both ISO 8601 format and Microsoft JSON.NET legacy format
                    DateTime modifiedTime;
                    if (TryParseMicrosoftJsonDate(modifiedTimeString, out modifiedTime) ||
                        DateTime.TryParse(modifiedTimeString, null, DateTimeStyles.RoundtripKind, out modifiedTime))
                    {
                        index.ModifiedTime = modifiedTime;
                    }
                    else
                    {
                        Logger.Debug("Failed to parse ModifiedTime: {ModifiedTimeString}", modifiedTimeString);
                    }
                }
            }

            if (imageElement.TryGetProperty("Languages", out var languagesProperty))
            {
                index.Languages = GetStringOrArray(languagesProperty);
            }

            // Only return valid indices with required properties
            if (index.Index > 0 && !string.IsNullOrEmpty(index.Name))
            {
                return index;
            }

            Logger.Warning("Skipping invalid image index: Index={Index}, Name={Name}", index.Index, index.Name);
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to parse image element from JSON");
            return null;
        }
    }

    /// <summary>
    /// Helper method to get a string value from a JsonElement that could be either a string or an array
    /// </summary>
    /// <param name="element">The JsonElement to process</param>
    /// <returns>A string representation of the value</returns>
    private static string GetStringOrArray(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;

            case JsonValueKind.Array:
                var arrayValues = new List<string>();
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var value = item.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            arrayValues.Add(value);
                        }
                    }
                }
                return string.Join(", ", arrayValues);

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return string.Empty;

            default:
                return element.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Tries to parse Microsoft JSON.NET legacy date format like "/Date(1725596151358)/"
    /// </summary>
    /// <param name="dateString">The date string to parse</param>
    /// <param name="result">The parsed DateTime if successful</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    private static bool TryParseMicrosoftJsonDate(string dateString, out DateTime result)
    {
        result = default;

        if (string.IsNullOrEmpty(dateString))
            return false;

        // Check if it matches the Microsoft JSON.NET format: /Date(timestamp)/
        if (dateString.StartsWith("/Date(") && dateString.EndsWith(")/"))
        {
            try
            {
                // Extract the timestamp part
                var timestampString = dateString.Substring(6, dateString.Length - 8);

                // Handle timezone offset if present (e.g., "/Date(1725596151358+0200)/")
                var plusIndex = timestampString.IndexOf('+');
                var minusIndex = timestampString.IndexOf('-');

                if (plusIndex > 0 || minusIndex > 0)
                {
                    // Remove timezone part for now, just use the timestamp
                    var timezoneIndex = Math.Max(plusIndex, minusIndex);
                    timestampString = timestampString.Substring(0, timezoneIndex);
                }

                // Parse the timestamp (milliseconds since Unix epoch)
                if (long.TryParse(timestampString, out var timestamp))
                {
                    // Convert from Unix timestamp (milliseconds) to DateTime
                    result = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                    return true;
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        return false;
    }

    #endregion
}
