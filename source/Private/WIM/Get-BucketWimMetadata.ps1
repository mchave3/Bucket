function Get-BucketWimMetadata
{
    <#
      .SYNOPSIS
      Extracts metadata from a WIM image file using DISM.

      .DESCRIPTION
      This function uses the Get-WindowsImage cmdlet from the DISM module to extract
      comprehensive metadata from a WIM file. It retrieves both the general image
      information (number of indexes) and detailed per-index metadata including
      image name, description, size, architecture, version, and more.

      .EXAMPLE
      $metadata = Get-BucketWimMetadata -ImagePath 'C:\Images\install.wim'

      Extracts all metadata from the specified WIM file.

      .PARAMETER ImagePath
      The full path to the WIM image file.

      .OUTPUTS
      [PSCustomObject] A metadata object containing file info and per-index details.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'Metadata is a standard singular noun referring to data about data.')]
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateScript({ Test-Path -Path $_ -PathType Leaf })]
        [string]
        $ImagePath
    )

    process
    {
        Write-Verbose -Message "Extracting metadata from: $ImagePath"

        # Get file information
        $fileInfo = Get-Item -Path $ImagePath

        # Get the list of indexes in the WIM
        Write-Verbose -Message 'Getting image index list...'
        $imageList = Get-WindowsImage -ImagePath $ImagePath

        # Build per-index metadata
        $indexes = @()
        foreach ($image in $imageList)
        {
            Write-Verbose -Message "Getting details for index $($image.ImageIndex): $($image.ImageName)"

            # Get detailed info for this index
            $indexDetails = Get-WindowsImage -ImagePath $ImagePath -Index $image.ImageIndex

            $indexMetadata = [PSCustomObject]@{
                ImageIndex       = $indexDetails.ImageIndex
                ImageName        = $indexDetails.ImageName
                ImageDescription = $indexDetails.ImageDescription
                ImageSize        = $indexDetails.ImageSize
                Architecture     = switch ($indexDetails.Architecture)
                {
                    0 { 'x86' }
                    5 { 'arm' }
                    6 { 'ia64' }
                    9 { 'x64' }
                    12 { 'arm64' }
                    default { $indexDetails.Architecture }
                }
                Version          = $indexDetails.Version
                SPBuild          = $indexDetails.SPBuild
                SPLevel          = $indexDetails.SPLevel
                EditionId        = $indexDetails.EditionId
                InstallationType = $indexDetails.InstallationType
                Hal              = $indexDetails.Hal
                ProductType      = $indexDetails.ProductType
                ProductSuite     = $indexDetails.ProductSuite
                Languages        = @($indexDetails.Languages)
                DefaultLanguage  = $indexDetails.DefaultLanguageIndex
                WIMBoot          = $indexDetails.WIMBoot
            }

            $indexes += $indexMetadata
        }

        # Build the complete metadata object
        $metadata = [PSCustomObject]@{
            Id           = [guid]::NewGuid().ToString()
            OriginalPath = $ImagePath
            FileName     = $fileInfo.Name
            FileSize     = $fileInfo.Length
            ImportDate   = (Get-Date).ToString('o')
            IndexCount   = $imageList.Count
            Indexes      = $indexes
        }

        Write-Verbose -Message "Extracted metadata for $($imageList.Count) index(es)."

        return $metadata
    }
}
