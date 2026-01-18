function Show-BucketImportWim
{
    <#
      .SYNOPSIS
      Displays the Import WIM screen and imports a WIM into the Bucket image store.

      .DESCRIPTION
      This screen provides a dedicated UI for importing a WIM file. It guides the user
      through selecting a WIM, previews indexes in a table, then runs the import with a
      live progress card (step-by-step status + details) and returns to Image Management.

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

            $state = Get-BucketState
            $destinationPath = Join-Path -Path $state.Paths.Images -ChildPath $fileName
            $safeDestPath = ('' + $destinationPath).Replace('[', '[[').Replace(']', ']]')

            # Step state (no nested functions: ScriptAnalyzer friendly)
            $steps = @(
                [pscustomobject]@{ Id = 'Validate'; Title = 'Validate'; Status = 'Pending'; Detail = 'Checking catalog and inputs...'; Percent = $null }
                [pscustomobject]@{ Id = 'Copy';     Title = 'Copy File'; Status = 'Pending'; Detail = 'Copying to repository...';       Percent = 0 }
                [pscustomobject]@{ Id = 'Save';     Title = 'Update Catalog'; Status = 'Pending'; Detail = 'Writing metadata...';        Percent = $null }
            )

            $detailLines = @(
                "[grey]Source:[/] $safeFileName"
                "[grey]Destination:[/] $safeDestPath"
                "[grey]Indexes:[/] $($metadata.IndexCount)"
            )

            $currentStepId = ''

            $updateStep = {
                param(
                    [Parameter(Mandatory = $true)] [string] $Id,
                    [Parameter(Mandatory = $true)] [string] $Status,
                    [Parameter()] [string] $Detail,
                    [Parameter()] $Percent
                )

                $s = $steps | Where-Object { $_.Id -eq $Id } | Select-Object -First 1
                if ($null -ne $s)
                {
                    $s.Status = $Status
                    if ($PSBoundParameters.ContainsKey('Detail')) { $s.Detail = $Detail }
                    if ($PSBoundParameters.ContainsKey('Percent')) { $s.Percent = $Percent }
                }

                $script:currentStepId = $Id
            }

            $renderStepPanel = {
                $lines = @()
                foreach ($s in $steps)
                {
                    switch ($s.Status)
                    {
                        'Pending' { $icon = '[grey][ ][/]'; $titleColor = 'grey' }
                        'Running' { $icon = '[deepskyblue1][*][/]'; $titleColor = 'white' }
                        'Done'    { $icon = '[green][x][/]'; $titleColor = 'grey' }
                        'Failed'  { $icon = '[red][!][/]'; $titleColor = 'red' }
                        default   { $icon = '[grey][?][/]'; $titleColor = 'grey' }
                    }

                    $pct = ''
                    if ($null -ne $s.Percent)
                    {
                        $pct = " [grey]($($s.Percent)%)[/]"
                    }

                    $detail = ''
                    if (-not [string]::IsNullOrWhiteSpace($s.Detail))
                    {
                        $safeDetail = ('' + $s.Detail).Replace('[', '[[').Replace(']', ']]')
                        $detail = "`n[grey]  $safeDetail[/]"
                    }

                    $lines += "$icon [$titleColor]$($s.Title)[/]$pct$detail"
                }

                return ($lines -join "`n") |
                    Format-SpectrePanel -Expand -Header '[deepskyblue1]Steps[/]' -Border Rounded -Color 'DeepSkyBlue1'
            }

            $renderDetailPanel = {
                $content = ($detailLines -join "`n")
                return $content |
                    Format-SpectrePanel -Expand -Header '[deepskyblue1]Details[/]' -Border Rounded -Color 'DeepSkyBlue1'
            }

            $renderFooterPanel = {
                return '[grey]Working... please wait[/]' |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded
            }

            $layout = New-SpectreLayout -Name 'root' -Rows @(
                (New-SpectreLayout -Name 'header' -MinimumSize 5 -Ratio 1 -Data 'empty')
                (New-SpectreLayout -Name 'content' -Ratio 10 -Columns @(
                    (New-SpectreLayout -Name 'left' -Ratio 2 -Data 'empty')
                    (New-SpectreLayout -Name 'right' -Ratio 5 -Data 'empty')
                ))
                (New-SpectreLayout -Name 'footer' -MinimumSize 3 -Ratio 1 -Data 'empty')
            )

            Clear-Host

            Invoke-SpectreLive -Data $layout -ScriptBlock {
                param([Spectre.Console.LiveDisplayContext] $Context)

                # ScriptAnalyzer sometimes misses nested usages; keep an explicit reference.
                $null = $Context

                $layout['header'].Update($headerPanel) | Out-Null
                $layout['left'].Update((& $renderStepPanel)) | Out-Null
                $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                $Context.Refresh()

                try
                {
                    # Validate
                    & $updateStep -Id 'Validate' -Status 'Running' -Detail 'Checking for duplicates...'
                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                    $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                    $Context.Refresh()

                    $existingImages = Get-BucketImportedImages
                    $duplicate = $existingImages | Where-Object { $_.FileName -eq $fileName }
                    if ($duplicate)
                    {
                        & $updateStep -Id 'Validate' -Status 'Failed' -Detail 'Duplicate filename found.'
                        $detailLines += '[red]Duplicate: an image with this filename already exists.[/]'

                        $layout['left'].Update((& $renderStepPanel)) | Out-Null
                        $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                        $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                        $Context.Refresh()
                        return
                    }

                    & $updateStep -Id 'Validate' -Status 'Done' -Detail 'OK'
                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $Context.Refresh()

                    # Copy (stream copy for live progress)
                    & $updateStep -Id 'Copy' -Status 'Running' -Detail 'Copying bytes...' -Percent 0
                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $Context.Refresh()

                    $sourceInfo = Get-Item -Path $path
                    $totalBytes = [int64]$sourceInfo.Length
                    $copiedBytes = 0

                    $bufferSize = 4MB
                    $buffer = [byte[]]::new($bufferSize)

                    $sw = [System.Diagnostics.Stopwatch]::StartNew()
                    $lastUiUpdateMs = 0
                    $lastPercent = -1

                    $source = [System.IO.File]::Open($path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
                    try
                    {
                        $dest = [System.IO.File]::Open($destinationPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
                        try
                        {
                            while (($read = $source.Read($buffer, 0, $buffer.Length)) -gt 0)
                            {
                                $dest.Write($buffer, 0, $read)
                                $copiedBytes += $read

                                $percent = 0
                                if ($totalBytes -gt 0)
                                {
                                    $percent = [math]::Floor(($copiedBytes / $totalBytes) * 100)
                                    if ($percent -gt 100) { $percent = 100 }
                                }

                                $nowMs = $sw.ElapsedMilliseconds
                                if (($percent -ne $lastPercent) -or (($nowMs - $lastUiUpdateMs) -ge 200))
                                {
                                    $lastUiUpdateMs = $nowMs
                                    $lastPercent = $percent

                                    $copiedGb = [math]::Round($copiedBytes / 1GB, 2)
                                    $totalGb = [math]::Round($totalBytes / 1GB, 2)

                                    $speedMb = 0
                                    if ($sw.Elapsed.TotalSeconds -gt 0)
                                    {
                                        $speedMb = [math]::Round(($copiedBytes / 1MB) / $sw.Elapsed.TotalSeconds, 1)
                                    }

                                    & $updateStep -Id 'Copy' -Status 'Running' -Percent $percent -Detail ("{0}% ({1}/{2} GB) @ {3} MB/s" -f $percent, $copiedGb, $totalGb, $speedMb)

                                    $detailLines = @(
                                        "[grey]Source:[/] $safeFileName"
                                        "[grey]Destination:[/] $safeDestPath"
                                        "[grey]Progress:[/] $percent%"
                                        "[grey]Copied:[/] $copiedGb / $totalGb GB"
                                        "[grey]Speed:[/] $speedMb MB/s"
                                        "[grey]Elapsed:[/] $([int]$sw.Elapsed.TotalSeconds)s"
                                    )

                                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                                    $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                                    $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                                    $Context.Refresh()
                                }
                            }
                        }
                        finally
                        {
                            $dest.Dispose()
                        }
                    }
                    finally
                    {
                        $source.Dispose()
                    }

                    if (-not (Test-Path -Path $destinationPath -PathType Leaf))
                    {
                        & $updateStep -Id 'Copy' -Status 'Failed' -Detail 'Destination file missing after copy.'

                        $layout['left'].Update((& $renderStepPanel)) | Out-Null
                        $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                        $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                        $Context.Refresh()
                        return
                    }

                    & $updateStep -Id 'Copy' -Status 'Done' -Percent 100 -Detail 'File copied.'
                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $Context.Refresh()

                    # Save
                    & $updateStep -Id 'Save' -Status 'Running' -Detail 'Writing metadata.json...'
                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $Context.Refresh()

                    if ($null -eq $state.Metadata.Images)
                    {
                        $state.Metadata.Images = @()
                    }
                    $state.Metadata.Images = @($state.Metadata.Images) + $metadata
                    Save-BucketMetadata

                    & $updateStep -Id 'Save' -Status 'Done' -Detail 'Catalog updated.'

                    $safeImageId = ('' + $metadata.Id).Replace('[', '[[').Replace(']', ']]')
                    $detailLines = @(
                        '[bold green]Import complete[/]'
                        ''
                        "[grey]File:[/] $safeFileName"
                        "[grey]Image ID:[/] $safeImageId"
                        "[grey]Indexes:[/] $($metadata.IndexCount)"
                        "[grey]Destination:[/] $safeDestPath"
                    )

                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                    $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                    $Context.Refresh()
                }
                catch
                {
                    $safeError = ('' + $_).Replace('[', '[[').Replace(']', ']]')
                    Write-BucketLog -Message "Import WIM progress failed: $_" -Level Error

                    if (-not [string]::IsNullOrWhiteSpace($script:currentStepId))
                    {
                        & $updateStep -Id $script:currentStepId -Status 'Failed' -Detail $safeError
                    }
                    $detailLines = @(
                        '[bold red]Import failed[/]'
                        ''
                        "[grey]$safeError[/]"
                    )

                    $layout['left'].Update((& $renderStepPanel)) | Out-Null
                    $layout['right'].Update((& $renderDetailPanel)) | Out-Null
                    $layout['footer'].Update((& $renderFooterPanel)) | Out-Null
                    $Context.Refresh()
                }

                # Wait for Enter to return
                while ($true)
                {
                    [Console]::TreatControlCAsInput = $true
                    $keyInfo = [Console]::ReadKey($true)
                    if ($keyInfo.Key -eq 'Enter')
                    {
                        return
                    }
                }
            }

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
