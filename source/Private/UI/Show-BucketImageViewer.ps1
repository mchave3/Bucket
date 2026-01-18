function Show-BucketImageViewer
{
    <#
      .SYNOPSIS
      Displays an interactive viewer for browsing imported WIM images.

      .DESCRIPTION
      This function provides a live, interactive interface for viewing imported WIM images
      using Spectre.Console's live rendering capabilities. The layout consists of a header,
      a left panel showing the list of images, and a right panel showing detailed index
      information for the selected image. Navigation is performed using arrow keys.

      .EXAMPLE
      Show-BucketImageViewer

      Opens the interactive image viewer.

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
            Write-SpectreHost '[yellow]No images imported yet.[/]'
            Write-SpectreHost '[grey]Use "Import Image" to add a WIM file.[/]'
            Write-SpectreHost ''
            Read-SpectreConfirm -Prompt 'Press Enter to continue' -DefaultAnswer 'y' | Out-Null
            return New-BucketNavResult -Action Back
        }

        # Build the layout structure:
        # +----------------------------------+
        # |             Header               |
        # +----------------------------------+
        # |   Image List   |  Index Details  |
        # |    (ratio 2)   |    (ratio 4)    |
        # +----------------------------------+
        # |             Footer               |
        # +----------------------------------+

        $layout = New-SpectreLayout -Name 'root' -Rows @(
            (New-SpectreLayout -Name 'header' -MinimumSize 3 -Ratio 1 -Data 'empty')
            (New-SpectreLayout -Name 'content' -Ratio 10 -Columns @(
                (New-SpectreLayout -Name 'imageList' -Ratio 2 -Data 'empty')
                (New-SpectreLayout -Name 'indexDetails' -Ratio 4 -Data 'empty')
            ))
            (New-SpectreLayout -Name 'footer' -MinimumSize 3 -Ratio 1 -Data 'empty')
        )

        Invoke-SpectreLive -Data $layout -ScriptBlock {
            param (
                [Spectre.Console.LiveDisplayContext] $Context
            )

            # State
            $imageList = @($images)
            $selectedIndex = 0
            $selectedImageIndexTab = 0  # For navigating indexes within selected image

            while ($true)
            {
                $selectedImage = $imageList[$selectedIndex]

                # Build header panel
                $headerContent = '[deepskyblue1]Image Viewer[/] - [grey]Use Up/Down to navigate images, Left/Right for indexes, Esc to exit[/]'
                $headerPanel = $headerContent | Format-SpectrePanel -Expand -Border Rounded

                # Build image list panel
                $imageListLines = @()
                for ($i = 0; $i -lt $imageList.Count; $i++)
                {
                    $img = $imageList[$i]
                    $sizeGB = [math]::Round($img.FileSize / 1GB, 2)
                    $prefix = if ($i -eq $selectedIndex) { '[deepskyblue1]> [/]' } else { '  ' }
                    $highlight = if ($i -eq $selectedIndex) { '[white]' } else { '[grey]' }
                    $safeImageFileName = Get-SpectreEscapedText $img.FileName
                    $imageListLines += "$prefix$highlight$safeImageFileName[/] [grey]($sizeGB GB, $($img.IndexCount) idx)[/]"
                }
                $imageListContent = $imageListLines -join "`n"
                $imageListPanel = $imageListContent | Format-SpectrePanel -Expand -Header '[deepskyblue1]Images[/]' -Border Rounded

                # Build index details panel
                if ($null -ne $selectedImage -and $selectedImage.Indexes.Count -gt 0)
                {
                    # Ensure index tab is within bounds
                    $maxIndexTab = $selectedImage.Indexes.Count - 1
                    if ($selectedImageIndexTab -gt $maxIndexTab) { $selectedImageIndexTab = 0 }
                    if ($selectedImageIndexTab -lt 0) { $selectedImageIndexTab = $maxIndexTab }

                    $idx = $selectedImage.Indexes[$selectedImageIndexTab]

                    $detailLines = @(
                        "[deepskyblue1]Image:[/] $($selectedImage.FileName)"
                        "[deepskyblue1]Index:[/] $($idx.ImageIndex) of $($selectedImage.IndexCount)"
                        ''
                        "[white]$($idx.ImageName)[/]"
                        "[grey]$($idx.ImageDescription)[/]"
                        ''
                        "[deepskyblue1]Architecture:[/] $($idx.Architecture)"
                        "[deepskyblue1]Version:[/] $($idx.Version)"
                        "[deepskyblue1]Edition:[/] $($idx.EditionId)"
                        "[deepskyblue1]Install Type:[/] $($idx.InstallationType)"
                        "[deepskyblue1]Size:[/] $([math]::Round($idx.ImageSize / 1GB, 2)) GB"
                        ''
                        "[deepskyblue1]Languages:[/] $($idx.Languages -join ', ')"
                        "[deepskyblue1]SP Build:[/] $($idx.SPBuild)"
                    )
                    $detailContent = $detailLines -join "`n"
                }
                else
                {
                    $detailContent = '[grey]No index information available.[/]'
                }
                $indexDetailsPanel = $detailContent | Format-SpectrePanel -Expand -Header '[deepskyblue1]Index Details[/]' -Border Rounded

                # Build footer panel
                $footerContent = "[grey]Image $($selectedIndex + 1) of $($imageList.Count) | Index $($selectedImageIndexTab + 1) of $($selectedImage.IndexCount) | ID: $($selectedImage.Id)[/]"
                $footerPanel = $footerContent | Format-SpectrePanel -Expand -Border Rounded

                # Update layout
                $layout['header'].Update($headerPanel) | Out-Null
                $layout['imageList'].Update($imageListPanel) | Out-Null
                $layout['indexDetails'].Update($indexDetailsPanel) | Out-Null
                $layout['footer'].Update($footerPanel) | Out-Null

                # Refresh display
                $Context.Refresh()

                # Handle input
                [Console]::TreatControlCAsInput = $true
                $keyInfo = [Console]::ReadKey($true)

                if ($keyInfo.Key -eq 'Escape')
                {
                    return
                }
                elseif ($keyInfo.Key -eq 'C' -and $keyInfo.Modifiers -eq 'Control')
                {
                    return
                }
                elseif ($keyInfo.Key -eq 'DownArrow')
                {
                    $selectedIndex = ($selectedIndex + 1) % $imageList.Count
                    $selectedImageIndexTab = 0  # Reset to first index when changing image
                }
                elseif ($keyInfo.Key -eq 'UpArrow')
                {
                    $selectedIndex = ($selectedIndex - 1 + $imageList.Count) % $imageList.Count
                    $selectedImageIndexTab = 0  # Reset to first index when changing image
                }
                elseif ($keyInfo.Key -eq 'RightArrow')
                {
                    $maxIdx = $selectedImage.Indexes.Count - 1
                    $selectedImageIndexTab = [math]::Min($selectedImageIndexTab + 1, $maxIdx)
                }
                elseif ($keyInfo.Key -eq 'LeftArrow')
                {
                    $selectedImageIndexTab = [math]::Max($selectedImageIndexTab - 1, 0)
                }
            }
        }

        return New-BucketNavResult -Action Back
    }
}
