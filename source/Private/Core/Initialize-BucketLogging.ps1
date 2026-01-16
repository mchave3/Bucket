function Initialize-BucketLogging
{
    <#
      .SYNOPSIS
      Initializes the logging system for the Bucket application.

      .DESCRIPTION
      This function sets up the logging infrastructure for Bucket, creating the log file
      for the current session. It ensures the Logs directory exists and creates a
      timestamped log file that can be used throughout the session.

      .EXAMPLE
      Initialize-BucketLogging

      Initializes logging and creates a new session log file.

      .OUTPUTS
      [void] This function initializes script-scoped logging variables.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param()

    process
    {
        $state = Get-BucketState
        $logsPath = $state.Paths.Logs

        # Create log file name with timestamp
        $timestamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
        $logFileName = "Bucket_$timestamp.log"
        $script:CurrentLogFile = Join-Path -Path $logsPath -ChildPath $logFileName

        # Write initial log entry
        $initMessage = "Bucket session started at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        Add-Content -Path $script:CurrentLogFile -Value $initMessage -Encoding UTF8

        Write-Verbose -Message "Log file created: $script:CurrentLogFile"
    }
}
