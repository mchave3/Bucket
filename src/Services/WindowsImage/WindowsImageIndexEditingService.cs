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
        _tempDirectory = Path.Combine(Path.GetTempPath(), "Bucket", "WIMGAPI");
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

        return await Task.Run(() =>
        {
            try
            {
                return UpdateIndexMetadata(wimFilePath, index, "NAME", currentName, newName);
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

        return await Task.Run(() =>
        {
            try
            {
                return UpdateIndexMetadata(wimFilePath, index, "DESCRIPTION", currentDescription, newDescription);
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

        return await Task.Run(() =>
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

                var updatedXml = originalXml;

                // Update name if changed
                if (nameChanged)
                {
                    updatedXml = RenameImageMetadata(updatedXml, "NAME", index, currentName, newName);
                }

                // Update description if changed
                if (descriptionChanged)
                {
                    updatedXml = RenameImageMetadata(updatedXml, "DESCRIPTION", index, currentDescription, newDescription);
                }

                // Write updated metadata back to WIM
                return SetWimImageInfo(wimFilePath, updatedXml);
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
    private bool UpdateIndexMetadata(string wimFilePath, int index, string metadataType, string currentValue, string newValue)
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

            // Update the specific metadata field
            var updatedXml = RenameImageMetadata(originalXml, metadataType, index, currentValue, newValue);

            // Write updated metadata back to WIM
            return SetWimImageInfo(wimFilePath, updatedXml);
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
        IntPtr wimHandle = IntPtr.Zero;

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
                IntPtr.Zero);

            if (wimHandle == IntPtr.Zero)
            {
                string error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to open WIM file {WimFile}: {Error}", wimFilePath, error);
                return null;
            }

            // Get image information
            IntPtr infoPtr;
            IntPtr sizePtr;
            var success = WimApi.WimGetImageInformation(wimHandle, out infoPtr, out sizePtr);

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
    /// </summary>
    private bool SetWimImageInfo(string wimFilePath, string xmlInfo)
    {
        IntPtr wimHandle = IntPtr.Zero;
        IntPtr xmlBuffer = IntPtr.Zero;

        try
        {
            // Clean up XML (remove extra newlines and spaces)
            string cleanXml = xmlInfo.Replace(Environment.NewLine, "");
            while (cleanXml.Contains("> "))
            {
                cleanXml = cleanXml.Replace("> ", ">");
            }

            // Create temporary directory if needed
            EnsureTempDirectory();

            // Open WIM file with write access
            wimHandle = WimApi.WIMCreateFile(
                wimFilePath,
                WimApi.WIM_GENERIC_WRITE,
                WimApi.WIM_OPEN_EXISTING,
                WimApi.WIM_FLAG_SHARE_WRITE,
                WimApi.WIM_COMPRESS_NONE,
                IntPtr.Zero);

            if (wimHandle == IntPtr.Zero)
            {
                string error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to open WIM file for writing {WimFile}: {Error}", wimFilePath, error);
                return false;
            }

            // Set temporary path for WIM operations
            WimApi.WimSetTemporaryPath(wimHandle, _tempDirectory);

            // Convert string to Unicode byte array
            byte[] xmlBytes = Encoding.Unicode.GetBytes(cleanXml);
            int xmlLength = xmlBytes.Length;

            // Allocate unmanaged memory for XML
            xmlBuffer = Marshal.AllocHGlobal(xmlLength);
            Marshal.Copy(xmlBytes, 0, xmlBuffer, xmlLength);

            // Set the image information
            bool success = WimApi.WimSetImageInformation(wimHandle, xmlBuffer, (uint)xmlLength);

            if (!success)
            {
                string error = WimApi.GetLastErrorMessage();
                Logger.Error("Failed to set WIM image information for {WimFile}: {Error}", wimFilePath, error);
                return false;
            }

            Logger.Information("Successfully updated WIM metadata for {WimFile}", Path.GetFileName(wimFilePath));
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception while setting WIM image info for {WimFile}", wimFilePath);
            return false;
        }
        finally
        {
            if (xmlBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(xmlBuffer);
            }

            if (wimHandle != IntPtr.Zero)
            {
                WimApi.WIMCloseHandle(wimHandle);
            }

            // Clean up temporary directory
            CleanupTempDirectory();
        }
    }

    /// <summary>
    /// Modifies XML metadata to rename a specific field for a given index.
    /// Based on the RenameImage logic from WinToolkit.
    /// </summary>
    private string RenameImageMetadata(string xmlData, string fieldType, int index, string currentValue, string newValue)
    {
        try
        {
            string updatedXml = "";
            string[] imageSections = Regex.Split(xmlData, "<IMAGE INDEX=", RegexOptions.IgnoreCase);

            foreach (string section in imageSections)
            {
                string currentSection = section;

                // Add back the IMAGE INDEX tag if this is not the first section
                if (currentSection.StartsWith("\""))
                {
                    currentSection = "<IMAGE INDEX=" + currentSection;
                }

                // Check if this is the section for our target index
                if (currentSection.StartsWith($"<IMAGE INDEX=\"{index}\">", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Debug("Processing XML section for index {Index}", index);

                    // Check if the field exists with current value
                    string fieldPattern = $"<{fieldType}>{Regex.Escape(currentValue)}</{fieldType}>";
                    string emptyFieldPattern = $"<{fieldType}></{fieldType}>";

                    if (Regex.IsMatch(currentSection, fieldPattern, RegexOptions.IgnoreCase))
                    {
                        // Replace existing field with new value
                        currentSection = Regex.Replace(
                            currentSection,
                            fieldPattern,
                            $"<{fieldType}>{newValue}</{fieldType}>",
                            RegexOptions.IgnoreCase);

                        Logger.Debug("Replaced existing {FieldType} field for index {Index}", fieldType, index);
                    }
                    else if (Regex.IsMatch(currentSection, emptyFieldPattern, RegexOptions.IgnoreCase))
                    {
                        // Replace empty field with new value
                        currentSection = Regex.Replace(
                            currentSection,
                            emptyFieldPattern,
                            $"<{fieldType}>{newValue}</{fieldType}>",
                            RegexOptions.IgnoreCase);

                        Logger.Debug("Replaced empty {FieldType} field for index {Index}", fieldType, index);
                    }
                    else
                    {
                        // Add new field after the IMAGE INDEX tag
                        string insertPattern = $"<IMAGE INDEX=\"{index}\">";
                        string replacement = $"<IMAGE INDEX=\"{index}\">\n    <{fieldType}>{newValue}</{fieldType}>";

                        currentSection = Regex.Replace(
                            currentSection,
                            Regex.Escape(insertPattern),
                            replacement,
                            RegexOptions.IgnoreCase);

                        Logger.Debug("Added new {FieldType} field for index {Index}", fieldType, index);
                    }
                }

                updatedXml += currentSection;
            }

            return updatedXml;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to rename image metadata for {FieldType} on index {Index}", fieldType, index);
            return xmlData; // Return original data on failure
        }
    }

    /// <summary>
    /// Waits for a file to become accessible if it's currently in use.
    /// </summary>
    private void WaitForFileAccess(string filePath)
    {
        const int maxWaitTime = 5000; // 5 seconds
        const int sleepInterval = 100; // 100ms
        int elapsedTime = 0;

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

    #endregion
}
