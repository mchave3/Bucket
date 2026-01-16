function Get-BucketImportedImages
{
    <#
      .SYNOPSIS
      Returns the list of imported WIM images from the metadata.

      .DESCRIPTION
      This function retrieves the list of WIM images that have been imported into Bucket.
      It reads from the in-memory BucketState.Metadata.Images collection which is loaded
      from metadata.json during initialization. This provides fast access to image
      information without re-reading the actual WIM files.

      .EXAMPLE
      $images = Get-BucketImportedImages
      $images | ForEach-Object { Write-Host $_.FileName }

      Gets all imported images and displays their file names.

      .EXAMPLE
      $image = Get-BucketImportedImages -Id 'abc123-def456-...'

      Gets a specific image by its unique ID.

      .PARAMETER Id
      Optional. The unique identifier of a specific image to retrieve.

      .OUTPUTS
      [object[]] An array of image metadata objects, or a single object if Id is specified.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'Function returns a collection of images.')]
    [CmdletBinding()]
    [OutputType([object[]])]
    param
    (
        [Parameter()]
        [string]
        $Id
    )

    process
    {
        $state = Get-BucketState
        $images = $state.Metadata.Images

        # Handle case where Images might be null or not an array
        if ($null -eq $images)
        {
            $images = @()
        }

        # If a specific ID is requested, filter to that image
        if (-not [string]::IsNullOrEmpty($Id))
        {
            $result = $images | Where-Object { $_.Id -eq $Id }
            return $result
        }

        return $images
    }
}
