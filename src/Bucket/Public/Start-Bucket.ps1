<#
.SYNOPSIS
    Brief description of the script purpose

.DESCRIPTION
    Detailed description of what the script/function does

.NOTES
    Name:        Start-Bucket.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Example of how to use this script/function
#>
function Start-Bucket {

    #Requires -Version 5.1
    #Requires -Modules PoShLog

    [CmdletBinding(SupportsShouldProcess)]
    param(

    )

    begin {
        Clear-Host
    }

    process {
        if ($PSCmdlet.ShouldProcess("Starting Bucket", "Initialize Bucket module")) {
            # Start the Bucket module Pre-flight checks and setup
            Invoke-BucketPreFlight
        }

        # Get the current version of the Bucket module
        Get-BucketVersion
    }
}
