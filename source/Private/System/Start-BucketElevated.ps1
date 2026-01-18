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

        $moduleImportTarget = 'Bucket'
        $bucketModule = Get-Module -Name Bucket -ErrorAction SilentlyContinue
        if ($bucketModule)
        {
            $moduleManifestPath = Join-Path -Path $bucketModule.ModuleBase -ChildPath 'Bucket.psd1'
            if (Test-Path -Path $moduleManifestPath -PathType Leaf)
            {
                $moduleImportTarget = (Resolve-Path -Path $moduleManifestPath).Path
            }
        }

        $childVerbose = ($VerbosePreference -eq 'Continue')
        $childNoExit = ($childVerbose -or $moduleImportTarget -ne 'Bucket')

        if ($moduleImportTarget -eq 'Bucket')
        {
            $importCommand = 'Import-Module Bucket -ErrorAction Stop'
        }
        else
        {
            $escapedImportPath = $moduleImportTarget.Replace("'", "''")
            $importCommand = "try { Import-Module '$escapedImportPath' -Force -ErrorAction Stop } catch { Import-Module Bucket -Force -ErrorAction Stop }"
        }

        $commandParts = @()
        if ($childVerbose)
        {
            $commandParts += '$VerbosePreference = ''Continue'''
        }

        $commandParts += '$ErrorActionPreference = ''Stop'''
        $commandParts += 'try {'
        $commandParts += $importCommand
        $commandParts += 'Start-Bucket -Elevated'
        $commandParts += '} catch {'
        $commandParts += 'Write-Error -ErrorRecord $_'
        $commandParts += '[void](Read-Host ''Bucket failed to start. Press Enter to close'')'
        $commandParts += '}'

        $command = ($commandParts -join '; ')

        $arguments = @(
            '-NoProfile'
            '-NoLogo'
        )

        if ($childNoExit)
        {
            $arguments += '-NoExit'
        }

        $arguments += @(
            '-Command'
            $command
        )

        Write-Verbose -Message "Elevation command import target: $moduleImportTarget"

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
