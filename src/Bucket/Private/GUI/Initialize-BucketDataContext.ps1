<#
.SYNOPSIS
    Initializes the global data context for the Bucket application

.DESCRIPTION
    Creates and initializes the global data context object used throughout the Bucket application.
    This includes version information, working directories, and other global settings.

.NOTES
    Name:        Initialize-BucketDataContext.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket
#>

function Initialize-BucketDataContext {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,
        
        [Parameter(Mandatory = $true)]
        [string]$BucketVersion
    )
    
    process {
        Write-BucketLog -Data "Initializing global data context" -Level Info
        
        # Create the global data context
        $script:globalDataContext  = [PSCustomObject]@{
            BucketVersion          = $BucketVersion
            WorkingDirectory       = $WorkingDirectory
            MountDirectory         = Join-Path -Path $WorkingDirectory -ChildPath "Mount"
            CompletedWimsDirectory = Join-Path -Path $WorkingDirectory -ChildPath "CompletedWIMs"
        }
        
        Write-BucketLog -Data "Global data context initialized with version: $BucketVersion" -Level Info
        
        return $script:globalDataContext
    }
}
