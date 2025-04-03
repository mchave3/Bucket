<#
.SYNOPSIS
    Gets the current version of the Bucket module

.DESCRIPTION
    Retrieves the version information for the Bucket module from loaded or available modules.
    Returns the version string including prerelease information if available.

.NOTES
    Name:        Get-BucketVersion.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Get-BucketVersion
#>
function Get-BucketVersion {
    [CmdletBinding()]
    param(

    )

    process {
        # First check currently loaded module (module in use)
        $loadedModule = Get-Module -Name "Bucket"
        
        if ($loadedModule) {
            # We found the currently loaded module
            $module = $loadedModule
        }
        else {
            # If not loaded, search in available modules
            $module = Get-Module -Name "Bucket" -ListAvailable | 
                ForEach-Object {
                    # Create full semantic version including prerelease
                    $fullVersionStr = $_.Version.ToString()
                    if ($_.PrivateData.PSData.Prerelease) {
                        $fullVersionStr += "-$($_.PrivateData.PSData.Prerelease)"
                    }
                    $_ | Add-Member -MemberType NoteProperty -Name FullVersionString -Value $fullVersionStr -PassThru
                } |
                Sort-Object { [System.Management.Automation.SemanticVersion]$_.FullVersionString } -Descending |
                Select-Object -First 1

            if (-not $module) {
                # Search by partial match if exact module not found
                $module = Get-Module -ListAvailable | Where-Object { $_.Name -match "Bucket" } |
                    ForEach-Object {
                        # Create full semantic version including prerelease
                        $fullVersionStr = $_.Version.ToString()
                        if ($_.PrivateData.PSData.Prerelease) {
                            $fullVersionStr += "-$($_.PrivateData.PSData.Prerelease)"
                        }
                        $_ | Add-Member -MemberType NoteProperty -Name FullVersionString -Value $fullVersionStr -PassThru
                    } |
                    Sort-Object { [System.Management.Automation.SemanticVersion]$_.FullVersionString } -Descending |
                    Select-Object -First 1
            }
        }

        # Check version and include pre-release if available
        if ($module) {
            $script:BucketVersion = $module.Version.ToString()
            if ($module.PrivateData.PSData.Prerelease) {
                $script:BucketVersion += "-$($module.PrivateData.PSData.Prerelease)"
            }
            Write-BucketLog -Data "Bucket version: $script:BucketVersion" -Level Verbose
        }
        else {
            $script:BucketVersion = "Version not found"
            Write-BucketLog -Data "Bucket module not found or not loaded" -Level Warning
        }
    }
}
