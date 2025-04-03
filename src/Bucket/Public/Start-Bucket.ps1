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

    process {
        if ($PSCmdlet.ShouldProcess("Starting logger and initializing log files")) {
            New-Logger |
                Set-MinimumLevel -Value Verbose | # You can change this value later to filter log messages
                # Here you can add as many sinks as you want - see https://github.com/PoShLog/PoShLog/wiki/Sinks for all available sinks
                Add-SinkConsole |   # Tell logger to write log messages to console
                Add-SinkFile -Path 'C:\Data\my_awesome.log' | # Tell logger to write log messages into file
                Start-Logger # Start the logger
        }

        # Get the current version of the Bucket module
        Get-BucketVersion
    }
}
