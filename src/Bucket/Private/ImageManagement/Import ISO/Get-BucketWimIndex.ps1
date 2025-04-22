<#
.SYNOPSIS
    Mounts an ISO, locates the WIM/ESD file, and extracts all image indices using native PowerShell commands.
.DESCRIPTION
    This function mounts a Windows ISO, finds the install.wim or install.esd file, extracts all image indices using Get-WindowsImage, and then cleanly dismounts the ISO. Returns an array of objects with index details for UI selection.
.NOTES
    Name:        Get-BucketWimIndex.ps1
    Author:      Mickaël CHAVE
    Created:     04/22/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License
.LINK
    https://github.com/mchave3/Bucket
.EXAMPLE
    Get-BucketWimIndex -IsoPath 'C:\Path\to\windows.iso'
#>
function Get-BucketWimIndex {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$IsoPath
    )
    process {
        try {
            Write-BucketLog -Data "[ISO Import] Mounting ISO: $IsoPath" -Level Info
            Mount-DiskImage -ImagePath $IsoPath -PassThru

            $vol = Get-DiskImage -ImagePath $IsoPath | Get-Volume
            $driveLetter = $vol.DriveLetter
            if (-not $driveLetter) {
                throw "Could not determine mounted ISO drive letter."
            }
            $sourcesPath = "$driveLetter`:\sources"
            $wimPath = Join-Path $sourcesPath 'install.wim'
            $esdPath = Join-Path $sourcesPath 'install.esd'
            if (Test-Path $wimPath) {
                $imagePath = $wimPath
            }
            elseif (Test-Path $esdPath) {
                $imagePath = $esdPath
            }
            else {
                throw "No install.wim or install.esd found in $sourcesPath."
            }
            Write-BucketLog -Data "[ISO Import] Found image file: $imagePath" -Level Debug
            $images = Get-WindowsImage -ImagePath $imagePath 2>&1
            if ($images -is [System.Management.Automation.ErrorRecord]) {
                throw $images
            }
            $result = @()
            foreach ($img in $images) {
                $result += [PSCustomObject]@{
                    Include      = $false
                    Index        = $img.ImageIndex
                    Name         = $img.ImageName
                    Description  = $img.ImageDescription
                    Architecture = $img.Architecture
                    Size         = [math]::Round($img.ImageSize/1MB, 1)
                }
            }
            Write-BucketLog -Data "[ISO Import] Extracted $($result.Count) indices from $imagePath" -Level Info
            return $result
        }
        catch {
            Write-BucketLog -Data "[ISO Import] Error extracting indices: $_" -Level Error
            throw $_
        }
        finally {
            try {
                Dismount-DiskImage -ImagePath $IsoPath
                Write-BucketLog -Data "[ISO Import] ISO dismounted: $IsoPath" -Level Info
            }
            catch {
                Write-BucketLog -Data "[ISO Import] Failed to dismount ISO: $_" -Level Warning
            }
        }
    }
}
