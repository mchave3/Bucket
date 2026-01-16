function Show-BucketDeleteImageMenu
{
    <#
      .SYNOPSIS
      Displays a menu to select and delete an imported image.

      .DESCRIPTION
      This helper function displays a list of imported images and allows the user
      to select one for deletion. It handles confirmation and delegates the actual
      deletion to Remove-BucketWim.

      .EXAMPLE
      $result = Show-BucketDeleteImageMenu

      Displays the delete image menu and returns a navigation result.

      .OUTPUTS
      [hashtable] A navigation result object for the navigation engine.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    process
    {
        $images = Get-BucketImportedImages

        if ($images.Count -eq 0)
        {
            Write-SpectreHost ''
            Write-SpectreHost '[yellow]No images available to delete.[/]'
            Start-Sleep -Seconds 2
            return New-BucketNavResult -Action Refresh
        }

        Write-SpectreHost ''
        Write-SpectreHost '[grey]Select an image to delete:[/]'
        Write-SpectreHost ''

        # Build choice list with image details
        $imageChoices = @()
        foreach ($img in $images)
        {
            $sizeGB = [math]::Round($img.FileSize / 1GB, 2)
            $imageChoices += "$($img.FileName) ($sizeGB GB, $($img.IndexCount) indexes)"
        }

        # Add cancel option
        $allChoices = $imageChoices + @('Cancel')

        $selection = Read-SpectreSelection -Message 'Delete Image' -Choices $allChoices -Color 'Red'

        if ($selection -eq 'Cancel')
        {
            return New-BucketNavResult -Action Refresh
        }

        # Find the selected image
        $selectedIndex = $imageChoices.IndexOf($selection)
        if ($selectedIndex -ge 0 -and $selectedIndex -lt $images.Count)
        {
            $selectedImage = $images[$selectedIndex]

            # Confirm deletion
            Write-SpectreHost ''
            $confirm = Read-SpectreConfirm -Prompt "Delete '$($selectedImage.FileName)'? This cannot be undone" -DefaultAnswer 'n'

            if ($confirm)
            {
                Write-SpectreHost ''
                $deleteResult = Remove-BucketWim -Id $selectedImage.Id -Force
                if ($deleteResult)
                {
                    Write-SpectreHost ''
                }
                Start-Sleep -Seconds 2
            }
            else
            {
                Write-SpectreHost '[grey]Deletion cancelled.[/]'
                Start-Sleep -Seconds 1
            }
        }

        return New-BucketNavResult -Action Refresh
    }
}
