function Start-BucketElevated
{
    <#
      .SYNOPSIS
      Relaunches the Bucket module in an elevated PowerShell session.

      .DESCRIPTION
      This function starts a new PowerShell process with administrative privileges
      using the RunAs verb. It imports the Bucket module and calls Start-Bucket
      with the -Elevated switch to prevent infinite elevation loops. The current
      process exits after launching the elevated session.

      .EXAMPLE
      Start-BucketElevated

      Launches an elevated PowerShell session with Bucket.

      .OUTPUTS
      [void] This function does not return; it exits the current process.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Internal function that launches elevation workflow, ShouldProcess would break UX.')]
    [CmdletBinding()]
    [OutputType([void])]
    param()

    process
    {
        Write-Verbose -Message 'Requesting elevation to administrative privileges...'

        $pwshPath = (Get-Process -Id $PID).Path

        $arguments = @(
            '-NoProfile'
            '-NoLogo'
            '-Command'
            'Import-Module Bucket; Start-Bucket -Elevated'
        )

        try
        {
            Start-Process -FilePath $pwshPath -ArgumentList $arguments -Verb RunAs
            Write-Verbose -Message 'Elevated process started. Exiting current session.'
        }
        catch
        {
            Write-Warning -Message "Failed to start elevated process: $_"
            throw 'Administrative privileges are required to run Bucket.'
        }
    }
}
