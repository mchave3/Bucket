<#
.SYNOPSIS
    Logs messages with different severity levels for the Bucket module.

.DESCRIPTION
    Writes log messages with specified severity levels (Verbose, Debug, Info, Warning, Error, Fatal).
    This function centralizes logging for the Bucket module, ensuring consistent logging behavior
    across all module components. Logs can be directed to appropriate PowerShell streams based on
    their severity level.

.NOTES
    Name:        Write-BucketLog.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Write-BucketLog -Data "Operation completed successfully" -Level Info
    # Logs an informational message

.EXAMPLE
    Write-BucketLog -Data "Failed to connect to resource" -Level Error
    # Logs an error message

.PARAMETER Data
    The message content to be logged

.PARAMETER Level
    The severity level of the log message
    Valid values: Verbose, Debug, Info, Warning, Error, Fatal
    Default: Info
#>
function Write-BucketLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Data,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Verbose', 'Debug', 'Info', 'Warning', 'Error', 'Fatal')]
        [string]$Level = 'Info'
    )

    process {
        # Log the data with the specified level
        switch ($Level) {
            'Verbose' { Write-VerboseLog $Data }  # Use for implementation details (e.g., "Creating temporary variables")
            'Debug'   { Write-DebugLog $Data }    # Use for important values (e.g., "Working path: X")
            'Info'    { Write-InfoLog $Data }     # Use for main steps (e.g., "Initialization complete")
            'Warning' { Write-WarningLog $Data }  # Use for suboptimal situations (e.g., "Folder already exists, using existing folder")
            'Error'   { Write-ErrorLog $Data }    # Use for operation failures (e.g., "Unable to create folder")
            'Fatal'   { Write-FatalLog $Data }    # Use for critical conditions (e.g., "Incompatible configuration detected")
        }
    }
}

