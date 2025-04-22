<#
    .SYNOPSIS
    Retrieves WIM information from an XML file.

    .DESCRIPTION
    This function loads and parses a WIM information XML file, retrieving all WIM entries
    and their associated image indexes. It follows the Bucket XML structure and provides
    comprehensive error handling.

    .NOTES
    Name: Get-BucketWIMInfo.ps1
    Author: Mickaël CHAVE
    Created: 04/22/2025
    Version: 1.0.0
    Repository: https://github.com/mchave3/Bucket
    License: MIT License

    .LINK
    https://github.com/mchave3/Bucket

    .PARAMETER XmlFilePath
    The path to the XML file containing WIM information.

    .EXAMPLE
    $wimInfo = Get-BucketWIMInfo -XmlFilePath "C:\DEV\Bucket\WIMs.xml"
    Returns all WIM entries from the specified XML file.

    .EXAMPLE
    $selectedWim = (Get-BucketWIMInfo -XmlFilePath "C:\DEV\Bucket\WIMs.xml") | Where-Object { $_.FileName -eq "Windows 11 Enterprise" }
    Retrieves a specific WIM entry by name.
#>

function Get-BucketWIMInfo {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateScript({ Test-Path -Path $_ -PathType Leaf })]
        [string]$XmlFilePath
    )

    #region Initialization
    Write-BucketLog -Data "[ImageManagement] Starting WIM info retrieval from: $XmlFilePath" -Level Info
    $wimCollection = @()
    #endregion Initialization

    #region XML Processing
    try {
        # Load the XML file
        Write-BucketLog -Data "[ImageManagement] Loading WIM XML file" -Level Debug
        [xml]$xmlContent = Get-Content -Path $XmlFilePath -Encoding UTF8

        # Check if XML contains the required root element
        if (-not $xmlContent.WIMs) {
            Write-BucketLog -Data "[ImageManagement] Invalid XML format: missing WIMs root element" -Level Error
            return $null
        }

        # Process each WIM entry
        foreach ($wim in $xmlContent.WIMs.WIM) {
            if (-not $wim.FileName) {
                Write-BucketLog -Data "[ImageManagement] Skipping WIM entry with missing FileName" -Level Warning
                continue
            }

            Write-BucketLog -Data "[ImageManagement] Processing WIM entry: $($wim.FileName)" -Level Debug

            # Create WIM object with properties
            $wimObject = [PSCustomObject]@{
                FileName = $wim.FileName
                FilePath = $wim.FilePath
                FileSize = [long]$wim.FileSize
                FileDate = $wim.FileDate
                FileHash = $wim.FileHash
                FileHashType = $wim.FileHashType
                ImageCount = if ($wim.ImageCount) { [int]$wim.ImageCount } else { if ($wim.Indexes.Index) { $wim.Indexes.Index.Count } else { 0 } }
                CompressionType = $wim.CompressionType
                ImportDate = $wim.ImportDate
                Source = $wim.Source
                Notes = $wim.Notes
                Indexes = @()
            }

            # Process indexes if they exist
            if ($wim.Indexes -and $wim.Indexes.Index) {
                foreach ($index in $wim.Indexes.Index) {
                    $indexObject = [PSCustomObject]@{
                        ImageIndex = [int]$index.ImageIndex
                        ImageName = $index.ImageName
                        ImageDescription = $index.ImageDescription
                        ImageSize = [long]$index.ImageSize
                        WIMBoot = $index.WIMBoot -eq 'True'
                        Architecture = $index.Architecture
                        Hal = $index.Hal
                        Version = $index.Version
                        SPBuild = [int]$index.SPBuild
                        SPLevel = [int]$index.SPLevel
                        EditionId = $index.EditionId
                        InstallationType = $index.InstallationType
                        ProductType = $index.ProductType
                        ProductSuite = $index.ProductSuite
                        SystemRoot = $index.SystemRoot
                        DirectoryCount = [int]$index.DirectoryCount
                        FileCount = [int]$index.FileCount
                        CreatedTime = [datetime]$index.CreatedTime
                        ModifiedTime = [datetime]$index.ModifiedTime
                        Languages = $index.Languages
                        DefaultLanguage = $index.DefaultLanguage
                        AdditionalLanguages = $index.AdditionalLanguages
                        UpdateStatus = $index.UpdateStatus
                        LastModifiedBy = $index.LastModifiedBy
                        Capabilities = $index.Capabilities
                        Features = $index.Features
                        LicenseType = $index.LicenseType
                    }

                    $wimObject.Indexes += $indexObject
                    Write-BucketLog -Data "[ImageManagement] Processed image index $($index.ImageIndex) for $($wim.FileName)" -Level Verbose
                }
            }
            else {
                Write-BucketLog -Data "[ImageManagement] No image indexes found for WIM: $($wim.FileName)" -Level Debug
            }

            $wimCollection += $wimObject
        }

        Write-BucketLog -Data "[ImageManagement] Successfully processed $($wimCollection.Count) WIM entries" -Level Info
        return $wimCollection
    }
    catch {
        Write-BucketLog -Data "[ImageManagement] Error processing WIM XML: $_" -Level Error
        return $null
    }
    #endregion XML Processing
}