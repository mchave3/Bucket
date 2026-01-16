function Write-BucketLog
{
    <#
      .SYNOPSIS
      Writes a log entry to the current Bucket session log file.

      .DESCRIPTION
      This function appends a timestamped log entry to the current session's log file.
      It supports different log levels (Info, Warning, Error, Debug) and formats the
      output consistently. The function also outputs to the verbose stream when the
      -Verbose preference is set.

      .EXAMPLE
      Write-BucketLog -Message 'Starting image mount operation'

      Writes an info-level log entry.

      .EXAMPLE
      Write-BucketLog -Message 'Failed to mount image' -Level Error

      Writes an error-level log entry.

      .PARAMETER Message
      The message to write to the log file.

      .PARAMETER Level
      The log level for this entry. Valid values are Info, Warning, Error, and Debug.
      Defaults to Info.

      .OUTPUTS
      [void] This function writes to the log file and does not return output.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param
    (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]
        $Message,

        [Parameter()]
        [ValidateSet('Info', 'Warning', 'Error', 'Debug')]
        [string]
        $Level = 'Info'
    )

    process
    {
        if ([string]::IsNullOrEmpty($script:CurrentLogFile))
        {
            Write-Warning -Message 'Logging not initialized. Call Initialize-BucketLogging first.'
            return
        }

        $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
        $logEntry = "[$timestamp] [$Level] $Message"

        # Write to log file
        Add-Content -Path $script:CurrentLogFile -Value $logEntry -Encoding UTF8

        # Also write to verbose stream
        Write-Verbose -Message $logEntry
    }
}
