function Show-BucketImageManagement
{
    <#
      .SYNOPSIS
      Displays the Image Management sub-menu for managing WIM/ISO/ESD images.

      .DESCRIPTION
      This function renders the Image Management screen, providing options to view,
      import, and delete images stored in the Bucket Images directory. Supports
      viewing imported images in an interactive browser, importing new WIM files,
      and deleting existing images from the catalog.

      .EXAMPLE
      $result = Show-BucketImageManagement

      Displays the Image Management menu and returns a navigation result.

      .OUTPUTS
      [hashtable] A navigation result object for the navigation engine.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    process
    {
        Write-BucketLog -Message 'Entering Image Management screen' -Level Debug

        $choices = @(
            'View Available Images'
            'Import Image (WIM)'
            'Delete Image'
        )

        $navigationMap = @{
            'Import Image (WIM)' = 'ImportWim'
        }

        $result = Read-BucketMenu -Title 'Image Management' -Subtitle 'Manage your WIM images.' -Choices $choices -NavigationMap $navigationMap

        Write-BucketLog -Message "Image Management menu result - Action: $($result.Action), Selection: $($result.Selection)" -Level Debug

        # Handle menu selections
        if ($result.Action -eq 'Selection')
        {
            switch ($result.Selection)
            {
                'View Available Images'
                {
                    Write-BucketLog -Message 'User selected: View Available Images' -Level Debug
                    $images = Get-BucketImportedImages
                    if ($images.Count -eq 0)
                    {
                        Write-SpectreHost ''
                        Write-SpectreHost '[yellow]No images imported yet.[/]'
                        Write-SpectreHost "[grey]Use 'Import Image (WIM)' to add an image first.[/]"
                        Write-SpectreHost ''
                        Read-SpectreConfirm -Prompt 'Press Enter to continue' -DefaultAnswer 'y' | Out-Null
                        return New-BucketNavResult -Action Refresh
                    }
                    Show-BucketImageViewer
                    return New-BucketNavResult -Action Refresh
                }
                'Import Image (WIM)'
                {
                    # NavigationMap handles this choice.
                    return New-BucketNavResult -Action Navigate -Target 'ImportWim'
                }
                'Delete Image'
                {
                    Write-BucketLog -Message 'User selected: Delete Image' -Level Debug
                    $images = Get-BucketImportedImages
                    if ($images.Count -eq 0)
                    {
                        Write-SpectreHost ''
                        Write-SpectreHost '[yellow]No images imported yet.[/]'
                        Write-SpectreHost "[grey]Use 'Import Image (WIM)' to add an image first.[/]"
                        Write-SpectreHost ''
                        Read-SpectreConfirm -Prompt 'Press Enter to continue' -DefaultAnswer 'y' | Out-Null
                        return New-BucketNavResult -Action Refresh
                    }
                    $deleteResult = Show-BucketDeleteImageMenu
                    return $deleteResult
                }
                default
                {
                    Write-SpectreHost "[yellow]'$($result.Selection)' is not yet implemented.[/]"
                    Start-Sleep -Seconds 1
                    return New-BucketNavResult -Action Refresh
                }
            }
        }

        return $result
    }
}
