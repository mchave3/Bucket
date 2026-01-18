function Start-Bucket
{
    <#
      .SYNOPSIS
      Starts the Bucket WIM image provisioning application with interactive terminal UI.

      .DESCRIPTION
      This function is the main entry point for the Bucket module. It performs the following:
      1. Checks for administrative privileges and auto-elevates if necessary
      2. Initializes the application state, logging, and navigation systems
      3. Displays a progress spinner during initialization
      4. Launches the main navigation loop with the interactive menu system

      The application provides a stack-based navigation experience for managing WIM images,
      applying Windows updates, and configuring provisioning options.

      .EXAMPLE
      Start-Bucket

      Starts the Bucket application. If not running as administrator, it will prompt for elevation.

      .EXAMPLE
      Start-Bucket -Verbose

      Starts the Bucket application with verbose logging output.

      .PARAMETER Elevated
      Internal switch used to indicate the process was re-launched with elevation.
      This prevents infinite elevation loops.

      .OUTPUTS
      [void] This function runs the interactive application and does not return output.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Interactive application entry point, ShouldProcess would break the user experience.')]
    [CmdletBinding()]
    [OutputType([void])]
    param
    (
        [Parameter()]
        [switch]
        $Elevated
    )

    process
    {
        $OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

        # Ensure the user's PowerShell profile is configured for full Unicode output (PwshSpectreConsole requirement)
        try
        {
            Enable-BucketUnicodeProfile -Force
        }
        catch
        {
            Write-Verbose -Message "Failed to update PowerShell profile for UTF-8: $_"
        }

        # Check for administrative privileges
        if (-not (Test-BucketAdminPrivilege))
        {
            if ($Elevated)
            {
                # We were supposed to be elevated but aren't - something went wrong
                Write-Warning -Message 'Failed to obtain administrative privileges. Bucket requires elevation.'
                return
            }

            Write-Verbose -Message 'Not running as administrator. Requesting elevation...'
            Start-BucketElevated
            return
        }

        # Clear the screen for a fresh start
        Clear-Host

        # Show initialization with spinner
        $initResult = Invoke-SpectreCommandWithStatus -Title 'Initializing Bucket...' -Spinner 'Dots' -ScriptBlock {
            # Initialize application state (paths, config, metadata)
            Initialize-BucketState

            # Mark session as elevated
            $script:BucketState.Session.IsElevated = $true

            # Initialize logging
            Initialize-BucketLogging
            Write-BucketLog -Message 'Bucket application started.' -Level Info

            # Initialize navigation system
            Initialize-BucketNavigation
            Write-BucketLog -Message 'Navigation system initialized.' -Level Debug

            # Small delay for visual feedback
            Start-Sleep -Milliseconds 500

            return $true
        }

        if (-not $initResult)
        {
            Write-Warning -Message 'Initialization failed. Please check the logs for details.'
            return
        }

        Write-BucketLog -Message 'Initialization complete. Launching main menu.' -Level Info

        # Clear screen before showing main menu
        Clear-Host

        # Start the main navigation loop
        try
        {
            Invoke-BucketNavigationLoop
        }
        catch
        {
            Write-BucketLog -Message "Fatal error: $_" -Level Error
            Write-Warning -Message "An unexpected error occurred: $_"
        }
        finally
        {
            Write-BucketLog -Message 'Bucket application exited.' -Level Info
            Write-Verbose -Message 'Bucket session ended.'
        }
    }
}
