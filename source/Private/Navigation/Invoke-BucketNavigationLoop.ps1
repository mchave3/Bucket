function Invoke-BucketNavigationLoop
{
    <#
      .SYNOPSIS
      Runs the main navigation loop for the Bucket application.

      .DESCRIPTION
      This function implements the core navigation engine using a stack-based approach.
      It manages screen transitions, handles navigation actions (Navigate, Back, Exit, Refresh),
      and ensures proper Clear-Host calls between screens. The loop continues until the
      navigation stack is empty or an Exit action is received.

      .EXAMPLE
      Invoke-BucketNavigationLoop

      Starts the navigation loop beginning from the MainMenu screen.

      .OUTPUTS
      [void] This function runs the application loop and does not return output.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param()

    process
    {
        Write-Verbose -Message 'Starting Bucket navigation loop...'
        Write-BucketLog -Message 'Starting navigation loop' -Level Info

        # Push the initial screen onto the stack
        $initialFrame = [PSCustomObject]@{
            Name      = 'MainMenu'
            Arguments = @{}
        }
        $script:NavigationStack.Push($initialFrame)

        # Main application loop
        while ($script:NavigationStack.Count -gt 0)
        {
            $currentFrame = $script:NavigationStack.Peek()
            $screenName = $currentFrame.Name
            $screenArgs = $currentFrame.Arguments

            Write-Verbose -Message "Displaying screen: $screenName"
            Write-BucketLog -Message "Navigating to screen: $screenName" -Level Debug

            # Look up the screen function
            if (-not $script:ScreenRegistry.ContainsKey($screenName))
            {
                Write-BucketLog -Message "Unknown screen requested: $screenName" -Level Warning
                Write-Warning -Message "Unknown screen: $screenName. Returning to previous screen."
                [void]$script:NavigationStack.Pop()
                continue
            }

            $screenFunction = $script:ScreenRegistry[$screenName]

            # Execute the screen within error handling
            $navResult = $null
            try
            {
                Write-BucketLog -Message "Executing screen function: $screenFunction" -Level Debug
                $navResult = & $screenFunction @screenArgs
                Write-BucketLog -Message "Screen '$screenName' returned action: $($navResult.Action)" -Level Debug
            }
            catch
            {
                $errorMessage = "Error in screen '$screenName': $($_.Exception.Message)"
                $errorDetails = $_.ScriptStackTrace
                Write-BucketLog -Message $errorMessage -Level Error
                Write-BucketLog -Message "Stack trace: $errorDetails" -Level Error
                Write-Warning -Message $errorMessage
                # On error, pop the current screen and try to go back
                [void]$script:NavigationStack.Pop()
                Clear-Host
                continue
            }

            # Validate navigation result
            if ($null -eq $navResult)
            {
                Write-BucketLog -Message "Screen '$screenName' returned null navigation result" -Level Warning
                Write-Warning -Message "Screen '$screenName' returned invalid navigation result. Going back."
                [void]$script:NavigationStack.Pop()
                Clear-Host
                continue
            }

            $hasAction = $false
            if ($navResult -is [hashtable])
            {
                $hasAction = $navResult.ContainsKey('Action')
            }
            else
            {
                $hasAction = ($null -ne $navResult.PSObject.Properties['Action'])
            }

            if (-not $hasAction)
            {
                Write-BucketLog -Message "Screen '$screenName' returned result without Action property" -Level Warning
                Write-Warning -Message "Screen '$screenName' returned invalid navigation result. Going back."
                [void]$script:NavigationStack.Pop()
                Clear-Host
                continue
            }

            # Process the navigation action
            switch ($navResult.Action)
            {
                'Navigate'
                {
                    $newFrame = [PSCustomObject]@{
                        Name      = $navResult.Target
                        Arguments = if ($navResult.Arguments) { $navResult.Arguments } else { @{} }
                    }
                    $script:NavigationStack.Push($newFrame)
                    Clear-Host
                }
                'Back'
                {
                    [void]$script:NavigationStack.Pop()
                    Clear-Host
                }
                'Exit'
                {
                    Write-Verbose -Message 'Exit requested. Clearing navigation stack.'
                    Write-BucketLog -Message 'User requested exit. Clearing navigation stack.' -Level Info
                    $script:NavigationStack.Clear()
                }
                'Refresh'
                {
                    # Just clear and let the loop redraw the current screen
                    Clear-Host
                }
                default
                {
                    Write-BucketLog -Message "Unknown navigation action: $($navResult.Action)" -Level Warning
                    Write-Warning -Message "Unknown navigation action: $($navResult.Action). Going back."
                    [void]$script:NavigationStack.Pop()
                    Clear-Host
                }
            }
        }

        Write-Verbose -Message 'Navigation loop ended.'
        Write-BucketLog -Message 'Navigation loop ended' -Level Info
    }
}
