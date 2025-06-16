<#
.SYNOPSIS
    Logs messages with different severity levels for the Bucket module.

.DESCRIPTION
    Writes log messages with specified severity levels (Verbose, Debug, Info, Warning, Error, Fatal).
    This function centralizes logging for the Bucket module, ensuring consistent logging behavior
    across all module components. Logs can be directed to appropriate PowerShell streams based on
    their severity level. Each log message is automatically prefixed with the name of the calling
    function for better traceability and debugging.

.NOTES
    Name:        Write-BucketLog.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     25.6.3.4
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Write-BucketLog -Data "Operation completed successfully" -Level Info
    # Output: [Initialize-BucketMainPage] Operation completed successfully

.EXAMPLE
    Write-BucketLog -Data "Failed to connect to resource" -Level Error
    # Output: [Import-BucketISO] Failed to connect to resource

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
        #region Caller Detection
        # Extract caller information for better traceability
        $callStack = Get-PSCallStack
        $caller = "Unknown"

        # Start from index 1 (skip Write-BucketLog itself)
        for ($i = 1; $i -lt $callStack.Count; $i++) {
            $currentCaller = $callStack[$i].FunctionName

            # Skip <ScriptBlock> entries and look for the actual function name
            if ($currentCaller -ne '<ScriptBlock>' -and $currentCaller -ne 'Write-BucketLog') {
                $caller = $currentCaller
                break
            }
        }

        # Clean up the caller name by removing <Process> and other unwanted suffixes
        if ($caller -match '^(.+)<.+>$') {
            $caller = $matches[1]
        }
        #endregion Caller Detection

        $logMessage = "[$caller] $Data"

        # Log the data with the specified level
        switch ($Level) {
            'Verbose' { Write-VerboseLog $logMessage }  # Use for implementation details (e.g., "Creating temporary variables")
            'Debug'   { Write-DebugLog $logMessage }    # Use for important values (e.g., "Working path: X")
            'Info'    { Write-InfoLog $logMessage }     # Use for main steps (e.g., "Initialization complete")
            'Warning' { Write-WarningLog $logMessage }  # Use for suboptimal situations (e.g., "Folder already exists, using existing folder")
            'Error'   { Write-ErrorLog $logMessage }    # Use for operation failures (e.g., "Unable to create folder")
            'Fatal'   { Write-FatalLog $logMessage }    # Use for critical conditions (e.g., "Incompatible configuration detected")
        }
    }
}
