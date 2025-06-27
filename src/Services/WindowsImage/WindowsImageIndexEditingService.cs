using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Bucket.Models;
using Bucket.Common;

namespace Bucket.Services.WindowsImage;

/// <summary>
/// Service for editing Windows image index metadata using native WIM APIs.
/// Provides functionality to modify index names and descriptions directly in WIM files.
/// </summary>
public class WindowsImageIndexEditingService : IWindowsImageIndexEditingService
{
    #region Private Fields

    private readonly string _tempDirectory;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowsImageIndexEditingService class.
    /// </summary>
    public WindowsImageIndexEditingService()
    {
        _tempDirectory = Path.Combine(Constants.TempDirectoryPath, "WIMGAPI");
        Logger.Debug("WindowsImageIndexEditingService initialized with temp directory: {TempDirectory}", _tempDirectory);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the name of a specific Windows image index.
    /// </summary>
    public async Task<bool> UpdateIndexNameAsync(string wimFilePath, int index, string currentName, string newName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wimFilePath) || !File.Exists(wimFilePath))
        {
            Logger.Error("WIM file does not exist: {WimFilePath}", wimFilePath);
            return false;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            Logger.Error("New name cannot be empty");
            return false;
        }

        if (currentName == newName)
        {
            Logger.Information("Name unchanged for index {Index}: {Name}", index, currentName);
            return true;
        }

        Logger.Information("Updating index {Index} name: '{CurrentName}' → '{NewName}' in {WimFile}",
            index, currentName, newName, Path.GetFileName(wimFilePath));

        return await Task.Run(async () =>
        {
            try
            {
                return await UpdateIndexMetadata(wimFilePath, index, "NAME", currentName, newName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update index {Index} name in {WimFile}", index, wimFilePath);
                return false;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Updates the description of a specific Windows image index.
    /// </summary>
    public async Task<bool> UpdateIndexDescriptionAsync(string wimFilePath, int index, string currentDescription, string newDescription, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wimFilePath) || !File.Exists(wimFilePath))
        {
            Logger.Error("WIM file does not exist: {WimFilePath}", wimFilePath);
            return false;
        }

        if (string.IsNullOrWhiteSpace(newDescription))
        {
            Logger.Error("New description cannot be empty");
            return false;
        }

        if (currentDescription == newDescription)
        {
            Logger.Information("Description unchanged for index {Index}", index);
            return true;
        }

        Logger.Information("Updating index {Index} description in {WimFile}",
            index, Path.GetFileName(wimFilePath));

        return await Task.Run(async () =>
        {
            try
            {
                return await UpdateIndexMetadata(wimFilePath, index, "DESCRIPTION", currentDescription, newDescription);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update index {Index} description in {WimFile}", index, wimFilePath);
                return false;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Updates both name and description of a specific Windows image index in a single operation.
    /// </summary>
    public async Task<bool> UpdateIndexMetadataAsync(string wimFilePath, int index, string currentName, string newName, string currentDescription, string newDescription, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wimFilePath) || !File.Exists(wimFilePath))
        {
            Logger.Error("WIM file does not exist: {WimFilePath}", wimFilePath);
            return false;
        }

        var nameChanged = !string.IsNullOrWhiteSpace(newName) && currentName != newName;
        var descriptionChanged = !string.IsNullOrWhiteSpace(newDescription) && currentDescription != newDescription;

        if (!nameChanged && !descriptionChanged)
        {
            Logger.Information("No changes needed for index {Index}", index);
            return true;
        }

        Logger.Information("Updating index {Index} metadata in {WimFile}: Name={NameChanged}, Description={DescriptionChanged}",
            index, Path.GetFileName(wimFilePath), nameChanged, descriptionChanged);

        return await Task.Run(async () =>
        {
            try
            {
                // Get current WIM metadata
                var originalXml = GetWimImageInfo(wimFilePath);
                if (string.IsNullOrEmpty(originalXml))
                {
                    Logger.Error("Failed to retrieve WIM metadata for {WimFile}", wimFilePath);
                    return false;
                }

                Logger.Debug("Original XML (first 500 chars): {OriginalXml}",
                    originalXml.Length > 500 ? originalXml.Substring(0, 500) + "..." : originalXml);

                var updatedXml = originalXml;

                // Update name if changed - following WinToolkit pattern: NAME + DISPLAYNAME
                if (nameChanged)
                {
                    var beforeNameChange = updatedXml;
                    // Update internal NAME field
                    updatedXml = RenameImageMetadata(updatedXml, "NAME", index, currentName, newName);
                    // Update display NAME field (what users see)
                    updatedXml = RenameImageMetadata(updatedXml, "DISPLAYNAME", index, currentName, newName);
                    Logger.Debug("XML after NAME + DISPLAYNAME change (first 500 chars): {UpdatedXml}",
                        updatedXml.Length > 500 ? updatedXml.Substring(0, 500) + "..." : updatedXml);
                    Logger.Debug("NAME + DISPLAYNAME change applied: {Changed}", beforeNameChange != updatedXml);
                }

                // Update description if changed - following WinToolkit pattern: DESCRIPTION + DISPLAYDESCRIPTION
                if (descriptionChanged)
                {
                    var beforeDescChange = updatedXml;
                    // Update internal DESCRIPTION field
                    updatedXml = RenameImageMetadata(updatedXml, "DESCRIPTION", index, currentDescription, newDescription);
                    // Update display DESCRIPTION field (what users see)
                    updatedXml = RenameImageMetadata(updatedXml, "DISPLAYDESCRIPTION", index, currentDescription, newDescription);
                    Logger.Debug("XML after DESCRIPTION + DISPLAYDESCRIPTION change (first 500 chars): {UpdatedXml}",
                        updatedXml.Length > 500 ? updatedXml.Substring(0, 500) + "..." : updatedXml);
                    Logger.Debug("DESCRIPTION + DISPLAYDESCRIPTION change applied: {Changed}", beforeDescChange != updatedXml);
                }

                Logger.Information("Final XML to be written (first 200 chars): {FinalXml}",
                    updatedXml.Length > 200 ? updatedXml.Substring(0, 200) + "..." : updatedXml);

                // Write updated metadata back to WIM
                var result = await SetWimImageInfo(wimFilePath, updatedXml);

                // Verify that the changes were persisted
                return result && VerifyMetadataChanges(wimFilePath, index, newName, newDescription);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update index {Index} metadata in {WimFile}", index, wimFilePath);
                return false;
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Validates if a WIM file is accessible and not in use by another process.
    /// </summary>
    public bool IsWimFileAccessible(string wimFilePath)
    {
        if (string.IsNullOrWhiteSpace(wimFilePath) || !File.Exists(wimFilePath))
        {
            return false;
        }

        try
        {
            // Try to open the file with read access to check if it's locked
            using var stream = new FileStream(wimFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return stream.CanRead;
        }
        catch (IOException)
        {
            // File is likely in use by another process
            return false;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Error checking WIM file accessibility: {WimFilePath}", wimFilePath);
            return false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates a specific metadata field for an index in a WIM file.
    /// </summary>
    private async Task<bool> UpdateIndexMetadata(string wimFilePath, int index, string metadataType, string currentValue, string newValue)
    {
        try
        {
            // Get current metadata
            var originalXml = GetWimImageInfo(wimFilePath);
            if (string.IsNullOrEmpty(originalXml))
            {
                Logger.Error("Failed to retrieve WIM metadata for {WimFile}", wimFilePath);
                return false;
            }

            // Update the specific metadata field - following WinToolkit pattern
            var updatedXml = RenameImageMetadata(originalXml, metadataType, index, currentValue, newValue);

            // Also update the corresponding DISPLAY field if it's NAME or DESCRIPTION
            if (metadataType == "NAME")
            {
                updatedXml = RenameImageMetadata(updatedXml, "DISPLAYNAME", index, currentValue, newValue);
                Logger.Debug("Updated both NAME and DISPLAYNAME fields for index {Index}", index);
            }
            else if (metadataType == "DESCRIPTION")
            {
                updatedXml = RenameImageMetadata(updatedXml, "DISPLAYDESCRIPTION", index, currentValue, newValue);
                Logger.Debug("Updated both DESCRIPTION and DISPLAYDESCRIPTION fields for index {Index}", index);
            }

            // Write updated metadata back to WIM
            var success = await SetWimImageInfo(wimFilePath, updatedXml);

            if (success)
            {
                Logger.Information("Successfully updated {MetadataType} for index {Index} in {WimFile}", metadataType, index, wimFilePath);

                // Brief verification that the file was updated - wait for file system sync
                Thread.Sleep(500);
                var verificationXml = GetWimImageInfo(wimFilePath);
                if (!string.IsNullOrEmpty(verificationXml))
                {
                    // Parse the verification XML to check the specific index
                    var verified = VerifySpecificFieldChange(verificationXml, index, metadataType, newValue);
                    if (verified)
                    {
                        Logger.Information("Verification successful: {MetadataType} change persisted in {WimFile}", metadataType, wimFilePath);
                    }
                    else
                    {
                        Logger.Warning("Verification failed: {MetadataType} change may not have persisted in {WimFile}", metadataType, wimFilePath);
                        success = false; // Mark as failed if verification fails
                    }
                }
                else
                {
                    Logger.Warning("Could not retrieve WIM metadata for verification");
                }
            }

            return success;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to update {MetadataType} for index {Index} in {WimFile}", metadataType, index, wimFilePath);
            return false;
        }
    }

    /// <summary>
    /// Retrieves the XML metadata from a WIM file using native APIs.
    /// </summary>
    private string GetWimImageInfo(string wimFilePath)
    {
        var wimHandle = IntPtr.Zero;

        try
        {
            // Wait for file to be available if it's in use
            WaitForFileAccess(wimFilePath);

            // Open WIM file with read access
            wimHandle = WimApi.WIMCreateFile(
                wimFilePath,
                WimApi.WIM_GENERIC_READ,
                WimApi.WIM_OPEN_EXISTING,
                WimApi.WIM_FLAG_SHARE_WRITE,
                WimApi.WIM_COMPRESS_NONE,
                0);

            if (wimHandle == IntPtr.Zero)
            {
                var error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to open WIM file {WimFile}: {Error}", wimFilePath, error);
                return null;
            }

            // Get image information
            IntPtr infoPtr;
            IntPtr sizePtr;
            var success = WimApi.WIMGetImageInformation(wimHandle, out infoPtr, out sizePtr);

            if (!success)
            {
                var error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to get WIM image information for {WimFile}: {Error}", wimFilePath, error);
                return null;
            }

            // Convert pointer to string
            var xmlInfo = WimApi.PtrToStringUni(infoPtr);
            Logger.Debug("Retrieved WIM metadata XML ({Length} characters) from {WimFile}",
                xmlInfo?.Length ?? 0, Path.GetFileName(wimFilePath));

            return xmlInfo;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception while getting WIM image info for {WimFile}", wimFilePath);
            return null;
        }
        finally
        {
            if (wimHandle != IntPtr.Zero)
            {
                WimApi.WIMCloseHandle(wimHandle);
            }
        }
    }

    /// <summary>
    /// Sets the XML metadata for a WIM file using native APIs.
    /// Follows the exact pattern used by WinToolkit for maximum compatibility.
    /// </summary>
    private async Task<bool> SetWimImageInfo(string wimFilePath, string xmlInfo)
    {
        var wimHandle = IntPtr.Zero;
        var xmlBuffer = IntPtr.Zero;

        try
        {
            // Clean up XML exactly like WinToolkit does
            var cleanXml = xmlInfo;

            Logger.Debug("Original XML length: {OriginalLength}", cleanXml.Length);

            // Remove newlines exactly like WinToolkit
            cleanXml = ReplaceIgnoreCase(cleanXml, Environment.NewLine, "");

            // Remove extra spaces exactly like WinToolkit
            while (cleanXml.Contains("> ", StringComparison.OrdinalIgnoreCase))
            {
                cleanXml = ReplaceIgnoreCase(cleanXml, "> ", ">");
            }

            Logger.Debug("Cleaned XML length: {CleanedLength}", cleanXml.Length);

            // Log portions of XML around our target index to verify modifications
            var indexSearch = $"<IMAGE INDEX=\"1\">";
            var indexPos = cleanXml.IndexOf(indexSearch, StringComparison.OrdinalIgnoreCase);
            if (indexPos >= 0)
            {
                var contextStart = Math.Max(0, indexPos - 50);
                var contextLength = Math.Min(800, cleanXml.Length - contextStart); // Reduced context for cleaner logs
                var context = cleanXml.Substring(contextStart, contextLength);
                Logger.Debug("XML around INDEX 1: {XmlContext}", context);
            }

            // Delete and recreate temporary directory first (like WinToolkit does with true parameter)
            CleanupAndRecreateTempDirectory();

            // Open WIM file with write access (exactly like WinToolkit)
            wimHandle = WimApi.WIMCreateFile(
                wimFilePath,
                WimApi.WIM_GENERIC_WRITE,
                WimApi.WIM_OPEN_EXISTING,
                WimApi.WIM_FLAG_SHARE_WRITE,
                WimApi.WIM_COMPRESS_NONE,
                0);

            if (wimHandle == IntPtr.Zero)
            {
                var error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to open WIM file for writing {WimFile}: {Error}", wimFilePath, error);
                return false;
            }

            // Set temporary path for WIM operations (directory exists from recreate step)
            // WinToolkit always calls this after ensuring directory exists
            var tempSetResult = WimApi.WIMSetTemporaryPath(wimHandle, _tempDirectory);
            if (!tempSetResult)
            {
                var error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to set temporary path {TempPath}: {Error}", _tempDirectory, error);
                return false; // Fail if we can't set the temp path like WinToolkit expects
            }
            Logger.Debug("Successfully set temporary path: {TempPath}", _tempDirectory);

            // Convert string to Unicode byte array (like WinToolkit)
            var xmlBytes = Encoding.Unicode.GetBytes(cleanXml);
            var xmlLength = xmlBytes.Length;

            // Allocate unmanaged memory for XML
            xmlBuffer = Marshal.AllocHGlobal(xmlLength);
            Marshal.Copy(xmlBytes, 0, xmlBuffer, xmlLength);

            // Set the image information
            var success = WimApi.WIMSetImageInformation(wimHandle, xmlBuffer, (uint)xmlLength);

            if (!success)
            {
                var error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to set WIM image information for {WimFile}: {Error}", wimFilePath, error);
                return false;
            }

            Logger.Information("Successfully updated WIM metadata for {WimFile}", Path.GetFileName(wimFilePath));

            // Force a delay to ensure the file is properly closed and written
            await Task.Delay(500, CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception while setting WIM image info for {WimFile}", wimFilePath);
            return false;
        }
        finally
        {
            // Free the allocated XML buffer
            if (xmlBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(xmlBuffer);
            }

            if (wimHandle != IntPtr.Zero)
            {
                WimApi.WIMCloseHandle(wimHandle);
                Logger.Debug("WIM handle closed for {WimFile}", Path.GetFileName(wimFilePath));
            }

            // Clean up temporary directory exactly like WinToolkit does (with false parameter)
            // This means delete the directory completely without recreating it
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                    Logger.Debug("Deleted temporary directory completely: {TempDirectory}", _tempDirectory);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Error during final temp directory cleanup: {TempDirectory}", _tempDirectory);
            }

            // Force garbage collection (like WinToolkit's FreeRAM call)
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Give more time for the file system to sync
            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// Modifies XML metadata to rename a specific field for a given index.
    /// Based on the exact RenameImage logic from WinToolkit.
    /// </summary>
    private string RenameImageMetadata(string xmlData, string fieldType, int index, string currentValue, string newValue)
    {
        Logger.Debug("RenameImageMetadata called with fieldType={FieldType}, index={Index}, currentValue={CurrentValue}, newValue={NewValue}",
            fieldType, index, currentValue, newValue);

        var nData = "";
        try
        {
            // Use WinToolkit's EXACT logic - Regex.Split with "<IMAGE INDEX="
            var splitParts = Regex.Split(xmlData, "<IMAGE INDEX=");
            Logger.Debug("Split XML into {PartCount} parts", splitParts.Length);

            foreach (var D in splitParts)
            {
                var DE = D;

                // WinToolkit logic: if section starts with quote, add back the IMAGE INDEX prefix
                if (DE.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    DE = "<IMAGE INDEX=" + DE;
                    Logger.Debug("Reconstructed section with IMAGE INDEX prefix, first 100 chars: {FirstChars}",
                        DE.Length > 100 ? DE.Substring(0, 100) + "..." : DE);
                }

                // Check if this is the section for our target index (exactly like WinToolkit)
                if (DE.StartsWith($"<IMAGE INDEX=\"{index}\">", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Information("Found target section for index {Index}", index);
                    Logger.Debug("Target section content (first 300 chars): {SectionContent}",
                        DE.Length > 300 ? DE.Substring(0, 300) + "..." : DE);

                    var originalDE = DE;

                    // Check if the field exists (exactly like WinToolkit logic)
                    if (DE.Contains($"<{fieldType}>", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Debug("Field {FieldType} exists in section", fieldType);

                        // Extract current value from XML to see what we're actually looking for
                        var fieldPattern = $"<{fieldType}>(.*?)</{fieldType}>";
                        var fieldMatch = Regex.Match(DE, fieldPattern, RegexOptions.IgnoreCase);
                        var actualCurrentValue = fieldMatch.Success ? fieldMatch.Groups[1].Value : "";

                        Logger.Information("Field {FieldType} - searching for: '{SearchValue}', found in XML: '{ActualValue}'",
                            fieldType, currentValue, actualCurrentValue);

                        // Case 1: Field exists with current value - replace it
                        var searchPattern = $"<{fieldType}>{currentValue}</{fieldType}>";
                        if (DE.Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
                        {
                            var before = DE;
                            DE = ReplaceIgnoreCase(DE, searchPattern, $"<{fieldType}>{newValue}</{fieldType}>");
                            Logger.Information("Replaced existing {FieldType} field with value for index {Index}. Changed: {Changed}",
                                fieldType, index, before != DE);
                        }
                        else
                        {
                            Logger.Warning("Could not find exact match for {FieldType} field. Expected: '{Expected}', but XML contains: '{Actual}'",
                                fieldType, searchPattern, $"<{fieldType}>{actualCurrentValue}</{fieldType}>");

                            // Try to replace with the actual current value instead
                            var actualPattern = $"<{fieldType}>{actualCurrentValue}</{fieldType}>";
                            var before = DE;
                            DE = ReplaceIgnoreCase(DE, actualPattern, $"<{fieldType}>{newValue}</{fieldType}>");
                            Logger.Information("Replaced {FieldType} using actual XML value for index {Index}. Changed: {Changed}",
                                fieldType, index, before != DE);
                        }

                        // Case 2: Field exists but is empty - replace it
                        if (DE.Contains($"<{fieldType}></{fieldType}>", StringComparison.OrdinalIgnoreCase))
                        {
                            var before = DE;
                            DE = ReplaceIgnoreCase(DE, $"<{fieldType}></{fieldType}>", $"<{fieldType}>{newValue}</{fieldType}>");
                            Logger.Information("Replaced empty {FieldType} field for index {Index}. Changed: {Changed}",
                                fieldType, index, before != DE);
                        }
                    }
                    else
                    {
                        // Case 3: Field doesn't exist - add it after the IMAGE INDEX tag
                        var before = DE;
                        DE = ReplaceIgnoreCase(DE, $"<IMAGE INDEX=\"{index}\">", $"<IMAGE INDEX=\"{index}\">\n    <{fieldType}>{newValue}</{fieldType}>");
                        Logger.Information("Added new {FieldType} field for index {Index}. Changed: {Changed}",
                            fieldType, index, before != DE);
                    }

                    if (originalDE != DE)
                    {
                        Logger.Debug("Modified section content (first 300 chars): {ModifiedContent}",
                            DE.Length > 300 ? DE.Substring(0, 300) + "..." : DE);
                    }
                }

                // WinToolkit logic: accumulate ALL sections directly
                nData += DE;
            }

            Logger.Debug("Final XML length: {FinalLength}, Original length: {OriginalLength}", nData.Length, xmlData.Length);
        }
        catch (Exception ex)
        {
            // Return original data on any error (exactly like WinToolkit)
            Logger.Warning(ex, "Error processing XML metadata for fieldType={FieldType}, index={Index}", fieldType, index);
            nData = xmlData;
        }

        return nData;
    }

    /// <summary>
    /// Case-insensitive string replacement matching WinToolkit's ReplaceIgnoreCase behavior.
    /// </summary>
    private string ReplaceIgnoreCase(string input, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue))
            return input;

        // Try simple replace first (like WinToolkit does)
        var standardReplace = input.Replace(oldValue, newValue);
        if (!input.Contains(oldValue, StringComparison.OrdinalIgnoreCase))
        {
            return standardReplace;
        }

        try
        {
            var sb = new StringBuilder();
            var currentIndex = 0;
            var searchIndex = input.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);

            while (searchIndex != -1)
            {
                sb.Append(input.Substring(currentIndex, searchIndex - currentIndex));
                sb.Append(newValue);
                currentIndex = searchIndex + oldValue.Length;
                searchIndex = input.IndexOf(oldValue, currentIndex, StringComparison.OrdinalIgnoreCase);
            }

            sb.Append(input.Substring(currentIndex));
            return sb.ToString();
        }
        catch
        {
            return input;
        }
    }

    /// <summary>
    /// Waits for a file to become accessible if it's currently in use.
    /// </summary>
    private void WaitForFileAccess(string filePath)
    {
        const int maxWaitTime = 5000; // 5 seconds
        const int sleepInterval = 100; // 100ms
        var elapsedTime = 0;

        while (elapsedTime < maxWaitTime)
        {
            if (IsWimFileAccessible(filePath))
            {
                return;
            }

            Logger.Debug("File in use, waiting... ({ElapsedTime}ms)", elapsedTime);
            Thread.Sleep(sleepInterval);
            elapsedTime += sleepInterval;
        }

        Logger.Warning("File still in use after {MaxWaitTime}ms: {FilePath}", maxWaitTime, filePath);
    }

    /// <summary>
    /// Ensures the temporary directory exists for WIM operations.
    /// </summary>
    private void EnsureTempDirectory()
    {
        try
        {
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
                Logger.Debug("Created temporary directory: {TempDirectory}", _tempDirectory);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to create temporary directory: {TempDirectory}", _tempDirectory);
        }
    }

    /// <summary>
    /// Cleans up and recreates the temporary directory used for WIM operations (like WinToolkit's true parameter).
    /// </summary>
    private void CleanupAndRecreateTempDirectory()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
                Logger.Debug("Deleted temporary directory: {TempDirectory}", _tempDirectory);
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Failed to delete temporary directory (attempting to continue): {TempDirectory}", _tempDirectory);
        }

        try
        {
            Directory.CreateDirectory(_tempDirectory);
            Logger.Debug("Recreated temporary directory: {TempDirectory}", _tempDirectory);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to recreate temporary directory: {TempDirectory}", _tempDirectory);
        }
    }

    /// <summary>
    /// Cleans up the temporary directory used for WIM operations.
    /// </summary>
    private void CleanupTempDirectory()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
                Logger.Debug("Cleaned up temporary directory: {TempDirectory}", _tempDirectory);
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Failed to cleanup temporary directory (non-critical): {TempDirectory}", _tempDirectory);
            // Non-critical error, continue
        }
    }

    /// <summary>
    /// Verifies that the metadata changes were actually persisted to the WIM file.
    /// </summary>
    private bool VerifyMetadataChanges(string wimFilePath, int index, string expectedName, string expectedDescription)
    {
        try
        {
            // Wait a moment for any file system caching to complete
            Thread.Sleep(500);

            var xmlInfo = GetWimImageInfo(wimFilePath);
            if (string.IsNullOrEmpty(xmlInfo))
            {
                Logger.Warning("Could not retrieve WIM metadata for verification");
                return false;
            }

            Logger.Debug("XML after writing to WIM (first 500 chars): {VerificationXml}",
                xmlInfo.Length > 500 ? xmlInfo.Substring(0, 500) + "..." : xmlInfo);

            // Parse the XML to find the specific index
            var imageSections = Regex.Split(xmlInfo, "<IMAGE INDEX=", RegexOptions.IgnoreCase);
            foreach (var section in imageSections)
            {
                if (section.StartsWith($"\"{index}\">", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the name and description from this section
                    var nameMatch = Regex.Match(section, "<NAME>(.*?)</NAME>", RegexOptions.IgnoreCase);
                    var descMatch = Regex.Match(section, "<DESCRIPTION>(.*?)</DESCRIPTION>", RegexOptions.IgnoreCase);

                    var actualName = nameMatch.Success ? nameMatch.Groups[1].Value : "";
                    var actualDescription = descMatch.Success ? descMatch.Groups[1].Value : "";

                    var nameMatches = actualName.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
                    var descMatches = actualDescription.Equals(expectedDescription, StringComparison.OrdinalIgnoreCase);

                    Logger.Information("Verification for index {Index}: Name={NameMatch} (expected: '{ExpectedName}', actual: '{ActualName}'), Description={DescMatch} (expected: '{ExpectedDesc}', actual: '{ActualDesc}')",
                        index, nameMatches, expectedName, actualName, descMatches, expectedDescription, actualDescription);

                    return nameMatches && descMatches;
                }
            }

            Logger.Warning("Could not find index {Index} in WIM metadata during verification", index);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during metadata verification for {WimFile} index {Index}", wimFilePath, index);
            return false;
        }
    }

    /// <summary>
    /// Verifies that a specific field was changed to the expected value for a given index.
    /// </summary>
    private bool VerifySpecificFieldChange(string xmlInfo, int index, string fieldType, string expectedValue)
    {
        try
        {
            // Parse the XML to find the specific index using the same logic as WinToolkit
            var imageSections = Regex.Split(xmlInfo, "<IMAGE INDEX=", RegexOptions.IgnoreCase);
            foreach (var section in imageSections)
            {
                var currentSection = section;
                if (currentSection.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "<IMAGE INDEX=" + currentSection;
                }

                if (currentSection.StartsWith($"<IMAGE INDEX=\"{index}\">", StringComparison.OrdinalIgnoreCase))
                {
                    // Look for the specific field
                    var fieldStart = $"<{fieldType}>";
                    var fieldEnd = $"</{fieldType}>";

                    var startIndex = currentSection.IndexOf(fieldStart, StringComparison.OrdinalIgnoreCase);
                    if (startIndex != -1)
                    {
                        startIndex += fieldStart.Length;
                        var endIndex = currentSection.IndexOf(fieldEnd, startIndex, StringComparison.OrdinalIgnoreCase);
                        if (endIndex != -1)
                        {
                            var actualValue = currentSection.Substring(startIndex, endIndex - startIndex);
                            var matches = string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);

                            Logger.Debug("Field verification for {FieldType} on index {Index}: expected='{Expected}', actual='{Actual}', matches={Matches}",
                                fieldType, index, expectedValue, actualValue, matches);

                            return matches;
                        }
                    }

                    Logger.Warning("Could not find {FieldType} field in index {Index} during verification", fieldType, index);
                    return false;
                }
            }

            Logger.Warning("Could not find index {Index} during field verification", index);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during field verification for {FieldType} on index {Index}", fieldType, index);
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Waits for a file to become accessible by repeatedly trying to access it.
    /// </summary>
    private void WaitForFileAccess(string filePath, int maxWaitTime = 5000)
    {
        var elapsedTime = 0;
        const int retryInterval = 100;

        while (elapsedTime < maxWaitTime)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return; // File is accessible
            }
            catch (IOException)
            {
                // File is still in use, wait and retry
                Thread.Sleep(retryInterval);
                elapsedTime += retryInterval;
            }
        }

        Logger.Warning("File still in use after {MaxWaitTime}ms: {FilePath}", maxWaitTime, filePath);
    }

    #endregion
}
