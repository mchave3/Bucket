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
        Show-BucketHeader

        Write-SpectreHost '[grey]Manage your WIM images.[/]'
        Write-SpectreHost ''

        $choices = @(
            'View Available Images'
            'Import Image (WIM)'
            'Delete Image'
        )

        $navigationMap = @{
            # No sub-screen navigation - actions are handled directly below
        }

        $result = Read-BucketMenu -Title 'Image Management' -Choices $choices -NavigationMap $navigationMap

        # Handle menu selections
        if ($result.Action -eq 'Selection')
        {
            switch ($result.Selection)
            {
                'View Available Images'
                {
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
                    Write-SpectreHost ''
                    $importResult = Import-BucketWim
                    if ($importResult)
                    {
                        Write-SpectreHost ''
                        Read-SpectreConfirm -Prompt 'Press Enter to continue' -DefaultAnswer 'y' | Out-Null
                    }
                    else
                    {
                        Start-Sleep -Seconds 2
                    }
                    return New-BucketNavResult -Action Refresh
                }
                'Delete Image'
                {
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
