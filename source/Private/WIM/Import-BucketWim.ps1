function Import-BucketWim
{
    <#
      .SYNOPSIS
      Imports a WIM image file into the Bucket image store.

      .DESCRIPTION
      This function guides the user through importing a WIM image file. It displays
      a file picker dialog to select the WIM file, extracts metadata using DISM,
      copies the file to the Bucket Images directory, and saves the metadata to
      the metadata.json file. Progress is displayed using Spectre console spinners.

      .EXAMPLE
      Import-BucketWim

      Opens a file picker, imports the selected WIM, and returns success status.

      .EXAMPLE
      Import-BucketWim -Path 'C:\Images\install.wim'

      Imports the specified WIM file without showing a file picker.

      .PARAMETER Path
      Optional. Direct path to a WIM file to import. If not specified, a file picker is shown.

      .OUTPUTS
      [bool] True if the import was successful, False otherwise.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param
    (
        [Parameter()]
        [string]
        $Path
    )

    process
    {
        $state = Get-BucketState

        # Step 1: Get the WIM file path
        if ([string]::IsNullOrEmpty($Path))
        {
            Write-SpectreHost '[grey]Select a WIM image file to import...[/]'

            $Path = Read-BucketFilePath -Title 'Select WIM Image' -Filter 'WIM files (*.wim)|*.wim|All files (*.*)|*.*'

            if ([string]::IsNullOrEmpty($Path))
            {
                Write-SpectreHost '[yellow]Import cancelled.[/]'
                return $false
            }
        }

        # Validate the file exists
        if (-not (Test-Path -Path $Path -PathType Leaf))
        {
            $safePath = ('' + $Path).Replace('[', '[[').Replace(']', ']]')
            Write-SpectreHost "[red]File not found: $safePath[/]"
            return $false
        }

        # Validate it's a .wim file
        if ([System.IO.Path]::GetExtension($Path).ToLower() -ne '.wim')
        {
            Write-SpectreHost '[red]Selected file is not a WIM image.[/]'
            return $false
        }

        $fileName = [System.IO.Path]::GetFileName($Path)
        $safeFileName = ('' + $fileName).Replace('[', '[[').Replace(']', ']]')

        # If called from an interactive Screen, keep this function quiet to avoid dumping
        # raw output under the menu. The Screen owns all rendering.
        $isInteractiveScreenCall = -not [string]::IsNullOrWhiteSpace($env:BUCKET_INTERACTIVE_SCREEN)
        if (-not $isInteractiveScreenCall)
        {
            Write-SpectreHost "[deepskyblue1]Importing:[/] $safeFileName"
            Write-SpectreHost ''
        }

        try
        {
            # Step 2: Extract metadata
            $metadata = Invoke-SpectreCommandWithStatus -Title 'Extracting image metadata...' -ScriptBlock {
                Get-BucketWimMetadata -ImagePath $Path
            }

            if ($null -eq $metadata)
            {
                if (-not $isInteractiveScreenCall)
                {
                    Write-SpectreHost '[red]Failed to extract image metadata.[/]'
                }
                return $false
            }

            if (-not $isInteractiveScreenCall)
            {
                Write-SpectreHost "[green]Found $($metadata.IndexCount) image index(es)[/]"

                # Display index summary
                foreach ($index in $metadata.Indexes)
                {
                    $size = [math]::Round($index.ImageSize / 1GB, 2)
                    $safeImageName = ('' + $index.ImageName).Replace('[', '[[').Replace(']', ']]')
                    $safeArchitecture = ('' + $index.Architecture).Replace('[', '[[').Replace(']', ']]')
                    Write-SpectreHost "  [grey]($($index.ImageIndex))[/] $safeImageName [grey]($safeArchitecture, $size GB)[/]"
                }
                Write-SpectreHost ''
            }

            # Step 3: Check for duplicates
            $existingImages = Get-BucketImportedImages
            $duplicate = $existingImages | Where-Object { $_.FileName -eq $fileName }
            if ($duplicate)
            {
                if (-not $isInteractiveScreenCall)
                {
                    Write-SpectreHost "[yellow]An image with this filename already exists: $safeFileName[/]"
                    Write-SpectreHost '[yellow]Please delete the existing image first or rename the source file.[/]'
                }
                return $false
            }

            # Step 4: Copy the file to Images directory
            $destinationPath = Join-Path -Path $state.Paths.Images -ChildPath $fileName

            Invoke-SpectreCommandWithStatus -Title "Copying $safeFileName..." -ScriptBlock {
                Copy-Item -Path $Path -Destination $destinationPath -Force
            }

            if (-not (Test-Path -Path $destinationPath -PathType Leaf))
            {
                if (-not $isInteractiveScreenCall)
                {
                    Write-SpectreHost '[red]Failed to copy image file.[/]'
                }
                return $false
            }

            if (-not $isInteractiveScreenCall)
            {
                Write-SpectreHost '[green]File copied successfully.[/]'
            }

            # Step 5: Add to metadata
            # Ensure Images is an array
            if ($null -eq $state.Metadata.Images)
            {
                $state.Metadata.Images = @()
            }

            # Add the new image metadata
            $state.Metadata.Images = @($state.Metadata.Images) + $metadata

            # Step 6: Save metadata
            Invoke-SpectreCommandWithStatus -Title 'Saving metadata...' -ScriptBlock {
                Save-BucketMetadata
            }

            if (-not $isInteractiveScreenCall)
            {
                Write-SpectreHost ''
                $safeImageId = ('' + $metadata.Id).Replace('[', '[[').Replace(']', ']]')
                Write-SpectreHost "[green]Successfully imported:[/] $safeFileName"
                Write-SpectreHost "[grey]Image ID: $safeImageId[/]"
            }

            return $true
        }
        catch
        {
            $safeError = ('' + $_).Replace('[', '[[').Replace(']', ']]')
            if (-not $isInteractiveScreenCall)
            {
                Write-SpectreHost "[red]Import failed: $safeError[/]"
            }
            Write-BucketLog -Message "Import-BucketWim failed: $_" -Level Error
            return $false
        }
    }
}
