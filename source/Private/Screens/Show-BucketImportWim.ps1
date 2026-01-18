function Show-BucketImportWim
{
    <#
      .SYNOPSIS
      Displays the Import WIM screen and imports a WIM into the Bucket image store.

      .DESCRIPTION
      This screen provides a dedicated, clean UI for importing a WIM file. It guides the
      user through selecting a WIM, previews the WIM indexes in a table, asks for a final
      confirmation, performs the copy + metadata save with status spinners, then shows a
      success/failure summary panel and waits for Enter before returning to Image Management.

      .EXAMPLE
      $result = Show-BucketImportWim

      Opens the import screen, imports the selected WIM, then returns a navigation result.

      .OUTPUTS
      [hashtable] A navigation result object for the navigation engine.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    process
    {
        try
        {
            Write-BucketLog -Message 'Entering Import WIM screen' -Level Debug

            # Tell Import-BucketWim to suppress chatty output when invoked from a Screen.
            $env:BUCKET_INTERACTIVE_SCREEN = '1'

            Clear-Host

            $header = "[bold deepskyblue1]Import WIM Image[/]`n[grey]Select a WIM, preview indexes, then confirm import.[/]"
            $headerPanel = $header |
                Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                Format-SpectrePanel -Expand -Border Rounded -Color 'DeepSkyBlue1'

            $headerPanel | Out-SpectreHost
            Write-SpectreHost ''

            $path = Read-BucketFilePath -Title 'Select WIM Image' -Filter 'WIM files (*.wim)|*.wim|All files (*.*)|*.*'

            if ([string]::IsNullOrWhiteSpace($path))
            {
                Write-SpectreHost '[yellow]Import cancelled.[/]'
                Start-Sleep -Milliseconds 700
                return New-BucketNavResult -Action Refresh
            }

            if (-not (Test-Path -Path $path -PathType Leaf))
            {
                $safePath = ('' + $path).Replace('[', '[[').Replace(']', ']]')
                ("[bold red]FILE NOT FOUND[/]`n`n[grey]$safePath[/]") |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color 'Red' |
                    Out-SpectreHost

                Read-SpectrePause -Message 'Press Enter to return...'
                return New-BucketNavResult -Action Refresh
            }

            if ([System.IO.Path]::GetExtension($path).ToLower() -ne '.wim')
            {
                ('[bold red]INVALID FILE TYPE[/]`n`n[grey]Please select a .wim file.[/]') |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color 'Red' |
                    Out-SpectreHost

                Read-SpectrePause -Message 'Press Enter to return...'
                return New-BucketNavResult -Action Refresh
            }

            $fileName = [System.IO.Path]::GetFileName($path)
            $safeFileName = ('' + $fileName).Replace('[', '[[').Replace(']', ']]')

            $metadata = Invoke-SpectreCommandWithStatus -Title 'Analyzing WIM structure...' -Spinner 'Dots' -ScriptBlock {
                Get-BucketWimMetadata -ImagePath $path
            }

            if ($null -eq $metadata)
            {
                ('[bold red]ANALYSIS FAILED[/]`n`n[grey]Failed to extract image metadata.[/]') |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color 'Red' |
                    Out-SpectreHost

                Read-SpectrePause -Message 'Press Enter to return...'
                return New-BucketNavResult -Action Refresh
            }

            # Preview table of indexes
            $indexRows = foreach ($idx in $metadata.Indexes)
            {
                [pscustomobject]@{
                    IDX  = $idx.ImageIndex
                    Name = $idx.ImageName
                    Arch = $idx.Architecture
                    Size = ("{0} GB" -f ([math]::Round($idx.ImageSize / 1GB, 2)))
                }
            }

            $table = Format-SpectreTable -Data $indexRows

            $summaryText = "[grey]File:[/] $safeFileName`n[grey]Indexes:[/] $($metadata.IndexCount)"
            $summaryPanel = $summaryText |
                Format-SpectreAligned -HorizontalAlignment Left -VerticalAlignment Top |
                Format-SpectrePanel -Expand -Header '[deepskyblue1]Preview[/]' -Border Rounded -Color 'DeepSkyBlue1'

            Clear-Host
            $headerPanel | Out-SpectreHost
            Write-SpectreHost ''
            $summaryPanel | Out-SpectreHost
            Write-SpectreHost ''
            $table | Out-SpectreHost
            Write-SpectreHost ''

            $selection = Read-SpectreSelection -Message 'Import WIM' -Choices @('Import', 'Cancel') -Color 'DeepSkyBlue1'
            if ($selection -ne 'Import')
            {
                Write-SpectreHost '[yellow]Import cancelled.[/]'
                Start-Sleep -Milliseconds 700
                return New-BucketNavResult -Action Refresh
            }

            # Prevent duplicate by filename
            $existingImages = Get-BucketImportedImages
            $duplicate = $existingImages | Where-Object { $_.FileName -eq $fileName }
            if ($duplicate)
            {
                ("[bold yellow]ALREADY IMPORTED[/]`n`n[grey]An image with this filename already exists:[/] $safeFileName") |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color 'Yellow' |
                    Out-SpectreHost

                Read-SpectrePause -Message 'Press Enter to return...'
                return New-BucketNavResult -Action Refresh
            }

            $state = Get-BucketState
            $destinationPath = Join-Path -Path $state.Paths.Images -ChildPath $fileName

            $copyOk = Invoke-SpectreCommandWithStatus -Title "Copying $safeFileName..." -Spinner 'Dots' -ScriptBlock {
                Copy-Item -Path $path -Destination $destinationPath -Force
                Test-Path -Path $destinationPath -PathType Leaf
            }

            if (-not $copyOk)
            {
                ('[bold red]COPY FAILED[/]`n`n[grey]Failed to copy image file.[/]') |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color 'Red' |
                    Out-SpectreHost

                Read-SpectrePause -Message 'Press Enter to return...'
                return New-BucketNavResult -Action Refresh
            }

            if ($null -eq $state.Metadata.Images)
            {
                $state.Metadata.Images = @()
            }
            $state.Metadata.Images = @($state.Metadata.Images) + $metadata

            Invoke-SpectreCommandWithStatus -Title 'Saving metadata...' -Spinner 'Dots' -ScriptBlock {
                Save-BucketMetadata
                $true
            } | Out-Null

            Clear-Host

            $safeImageId = ('' + $metadata.Id).Replace('[', '[[').Replace(']', ']]')
            $successLines = @(
                '[bold green]IMPORT SUCCESSFUL[/]'
                ''
                "[grey]File:[/] $safeFileName"
                "[grey]Image ID:[/] $safeImageId"
                "[grey]Indexes:[/] $($metadata.IndexCount)"
                ''
                '[grey]The image is now available in the catalog.[/]'
            )

            ($successLines -join "`n") |
                Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                Format-SpectrePanel -Expand -Border Rounded -Color 'Green' |
                Out-SpectreHost

            Read-SpectrePause -Message 'Press Enter to return to Image Management...'

            return New-BucketNavResult -Action Refresh
        }
        catch
        {
            $safeError = ('' + $_).Replace('[', '[[').Replace(']', ']]')
            Write-BucketLog -Message "Import WIM screen failed: $_" -Level Error

            Clear-Host
            ("[bold red]IMPORT FAILED[/]`n`n[grey]$safeError[/]") |
                Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                Format-SpectrePanel -Expand -Border Rounded -Color 'Red' |
                Out-SpectreHost

            Read-SpectrePause -Message 'Press Enter to return to Image Management...'
            return New-BucketNavResult -Action Refresh
        }
        finally
        {
            # Avoid leaking the flag to subsequent commands.
            Remove-Item -Path env:BUCKET_INTERACTIVE_SCREEN -ErrorAction SilentlyContinue
        }
    }
}
