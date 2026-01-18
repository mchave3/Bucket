function Show-BucketImportWim
{
    <#
      .SYNOPSIS
      Displays the Import WIM screen and imports a WIM into the Bucket image store.

      .DESCRIPTION
      This screen provides a dedicated UI for importing a WIM file. It guides the user
      through selecting a WIM, previews indexes in a table, and then runs the import with
      a live progress card showing step-by-step status and details.

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

            # Step model
            $steps = @(
                [pscustomobject]@{ Id = 'Validate'; Title = 'Validate'; Status = 'Pending'; Detail = 'Checking catalog and inputs...'; Percent = $null }
                [pscustomobject]@{ Id = 'Copy'; Title = 'Copy File'; Status = 'Pending'; Detail = 'Copying to repository...'; Percent = 0 }
                [pscustomobject]@{ Id = 'Save'; Title = 'Update Catalog'; Status = 'Pending'; Detail = 'Writing metadata...'; Percent = $null }
            )

            $activeStepId = 'Validate'
            $overallStatus = 'Running'
            $detailLines = @(
                "[grey]Source:[/] $safeFileName"
                "[grey]Destination:[/] $safeDestPath"
                "[grey]Indexes:[/] $($metadata.IndexCount)"
            )

            function Set-Step
            {
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
            }

            function Render-StepList
            {
                $lines = @()
                foreach ($s in $steps)
                {
                    switch ($s.Status)
                    {
                        'Pending' { $icon = '[grey]○[/]'; $color = 'grey' }
                        'Running' { $icon = '[deepskyblue1]●[/]'; $color = 'white' }
                        'Done'    { $icon = '[green]●[/]'; $color = 'grey' }
                        'Failed'  { $icon = '[red]×[/]'; $color = 'red' }
                        default   { $icon = '[grey]?[/]'; $color = 'grey' }
                    }

                    $pct = ''
                    if ($null -ne $s.Percent)
                    {
                        $pct = " [grey]($($s.Percent)%) [/]"
                    }

                    $detail = ''
                    if (-not [string]::IsNullOrWhiteSpace($s.Detail))
                    {
                        $safeDetail = ('' + $s.Detail).Replace('[', '[[').Replace(']', ']]')
                        $detail = "`n[grey]  $safeDetail[/]"
                    }

                    $lines += "$icon [$color]$($s.Title)[/]$pct$detail"
                }

                return ($lines -join "`n") |
                    Format-SpectrePanel -Expand -Header '[deepskyblue1]Steps[/]' -Border Rounded -Color 'DeepSkyBlue1'
            }

            function Render-Details
            {
                $content = ($detailLines -join "`n")
                return $content |
                    Format-SpectrePanel -Expand -Header '[deepskyblue1]Details[/]' -Border Rounded -Color 'DeepSkyBlue1'
            }

            function Render-Footer
            {
                switch ($overallStatus)
                {
                    'Running' { $footer = '[grey]Working... please wait[/]' }
                    'Failed'  { $footer = '[grey]Press Enter to return[/]' }
                    'Done'    { $footer = '[grey]Press Enter to return[/]' }
                    default   { $footer = '[grey]Press Enter to return[/]' }
                }

                return $footer |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded
            }

            # Progress card layout
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

                $layout['header'].Update($headerPanel) | Out-Null

                $update = {
                    $layout['left'].Update((Render-StepList)) | Out-Null
                    $layout['right'].Update((Render-Details)) | Out-Null
                    $layout['footer'].Update((Render-Footer)) | Out-Null
                    $Context.Refresh()
                }

                & $update

                try
                {
                    # Step 1: Validate
                    Set-Step -Id 'Validate' -Status 'Running' -Detail 'Checking for duplicates...'
                    & $update

                    $existingImages = Get-BucketImportedImages
                    $duplicate = $existingImages | Where-Object { $_.FileName -eq $fileName }
                    if ($duplicate)
                    {
                        Set-Step -Id 'Validate' -Status 'Failed' -Detail 'Duplicate filename found.'
                        $overallStatus = 'Failed'
                        $detailLines += "[red]Duplicate: an image with this filename already exists.[/]"
                        & $update
                        return
                    }

                    Set-Step -Id 'Validate' -Status 'Done' -Detail 'OK'
                    & $update

                    # Step 2: Copy (stream copy for live updates)
                    Set-Step -Id 'Copy' -Status 'Running' -Detail 'Copying bytes...'
                    & $update

                    $sourceInfo = Get-Item -Path $path
                    $totalBytes = [int64]$sourceInfo.Length
                    $copiedBytes = 0
                    $bufferSize = 4MB
                    $buffer = [byte[]]::new($bufferSize)
                    $sw = [System.Diagnostics.Stopwatch]::StartNew()
                    $lastUiUpdateMs = 0

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
                                if (($percent -ne $steps[1].Percent) -or (($nowMs - $lastUiUpdateMs) -ge 200))
                                {
                                    $lastUiUpdateMs = $nowMs

                                    $copiedGb = [math]::Round($copiedBytes / 1GB, 2)
                                    $totalGb = [math]::Round($totalBytes / 1GB, 2)
                                    $speedMb = 0
                                    if ($sw.Elapsed.TotalSeconds -gt 0)
                                    {
                                        $speedMb = [math]::Round(($copiedBytes / 1MB) / $sw.Elapsed.TotalSeconds, 1)
                                    }

                                    Set-Step -Id 'Copy' -Status 'Running' -Percent $percent -Detail ("{0}% ({1} / {2} GB) @ {3} MB/s" -f $percent, $copiedGb, $totalGb, $speedMb)

                                    $detailLines = @(
                                        "[grey]Source:[/] $safeFileName"
                                        "[grey]Destination:[/] $safeDestPath"
                                        "[grey]Progress:[/] $percent%"
                                        "[grey]Copied:[/] $copiedGb / $totalGb GB"
                                        "[grey]Speed:[/] $speedMb MB/s"
                                        "[grey]Elapsed:[/] $([int]$sw.Elapsed.TotalSeconds)s"
                                    )

                                    & $update
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
                        Set-Step -Id 'Copy' -Status 'Failed' -Detail 'Destination file missing after copy.'
                        $overallStatus = 'Failed'
                        & $update
                        return
                    }

                    Set-Step -Id 'Copy' -Status 'Done' -Percent 100 -Detail 'File copied.'
                    & $update

                    # Step 3: Save metadata
                    Set-Step -Id 'Save' -Status 'Running' -Detail 'Writing metadata.json...'
                    & $update

                    if ($null -eq $state.Metadata.Images)
                    {
                        $state.Metadata.Images = @()
                    }
                    $state.Metadata.Images = @($state.Metadata.Images) + $metadata
                    Save-BucketMetadata

                    Set-Step -Id 'Save' -Status 'Done' -Detail 'Catalog updated.'
                    $overallStatus = 'Done'

                    $safeImageId = ('' + $metadata.Id).Replace('[', '[[').Replace(']', ']]')
                    $detailLines = @(
                        "[bold green]Import complete[/]"
                        ""
                        "[grey]File:[/] $safeFileName"
                        "[grey]Image ID:[/] $safeImageId"
                        "[grey]Indexes:[/] $($metadata.IndexCount)"
                        "[grey]Destination:[/] $safeDestPath"
                    )

                    & $update
                }
                catch
                {
                    $safeError = ('' + $_).Replace('[', '[[').Replace(']', ']]')
                    Set-Step -Id $activeStepId -Status 'Failed' -Detail $safeError
                    $overallStatus = 'Failed'
                    $detailLines = @(
                        '[bold red]Import failed[/]'
                        ''
                        "[grey]$safeError[/]"
                    )
                    & $update
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
