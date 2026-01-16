function Remove-BucketWim
{
    <#
      .SYNOPSIS
      Removes an imported WIM image from the Bucket image store.

      .DESCRIPTION
      This function removes a WIM image that was previously imported into Bucket.
      It deletes both the WIM file from the Images directory and removes the
      corresponding metadata entry from metadata.json. Supports ShouldProcess
      for confirmation prompts.

      .EXAMPLE
      Remove-BucketWim -Id 'abc123-def456-...'

      Removes the image with the specified ID after confirmation.

      .EXAMPLE
      Remove-BucketWim -Id 'abc123-def456-...' -Force

      Removes the image without prompting for confirmation.

      .EXAMPLE
      Get-BucketImportedImages | Select-Object -First 1 | Remove-BucketWim

      Removes the first imported image using pipeline input.

      .PARAMETER Id
      The unique identifier of the image to remove.

      .PARAMETER Force
      Suppresses the confirmation prompt.

      .OUTPUTS
      [bool] True if the removal was successful, False otherwise.
    #>
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
    [OutputType([bool])]
    param
    (
        [Parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)]
        [string]
        $Id,

        [Parameter()]
        [switch]
        $Force
    )

    process
    {
        $state = Get-BucketState

        # Find the image by ID
        $image = Get-BucketImportedImages -Id $Id

        if ($null -eq $image)
        {
            Write-SpectreHost "[red]Image not found with ID: $Id[/]"
            return $false
        }

        $fileName = $image.FileName
        $filePath = Join-Path -Path $state.Paths.Images -ChildPath $fileName

        # Check ShouldProcess (handles -WhatIf, -Confirm, and -Force via ConfirmPreference)
        if ($Force)
        {
            $ConfirmPreference = 'None'
        }

        if (-not $PSCmdlet.ShouldProcess($fileName, 'Remove WIM image'))
        {
            return $false
        }

        try
        {
            # Step 1: Delete the file if it exists
            if (Test-Path -Path $filePath -PathType Leaf)
            {
                Write-Verbose -Message "Deleting file: $filePath"
                Remove-Item -Path $filePath -Force
                Write-SpectreHost "[green]Deleted file:[/] $fileName"
            }
            else
            {
                Write-SpectreHost "[yellow]File not found (already deleted?):[/] $fileName"
            }

            # Step 2: Remove from metadata
            $currentImages = @($state.Metadata.Images)
            $updatedImages = $currentImages | Where-Object { $_.Id -ne $Id }

            # Handle the case where filtering returns $null for single item
            if ($null -eq $updatedImages)
            {
                $updatedImages = @()
            }

            $state.Metadata.Images = @($updatedImages)

            # Step 3: Save updated metadata
            Save-BucketMetadata

            Write-SpectreHost "[green]Removed image from catalog:[/] $fileName"
            Write-BucketLog -Message "Removed WIM image: $fileName (ID: $Id)" -Level Information

            return $true
        }
        catch
        {
            Write-SpectreHost "[red]Failed to remove image: $_[/]"
            Write-BucketLog -Message "Remove-BucketWim failed for $fileName : $_" -Level Error
            return $false
        }
    }
}
