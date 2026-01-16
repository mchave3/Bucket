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

            # Look up the screen function
            if (-not $script:ScreenRegistry.ContainsKey($screenName))
            {
                Write-Warning -Message "Unknown screen: $screenName. Returning to previous screen."
                [void]$script:NavigationStack.Pop()
                continue
            }

            $screenFunction = $script:ScreenRegistry[$screenName]

            # Execute the screen within error handling
            $navResult = $null
            try
            {
                $navResult = & $screenFunction @screenArgs
            }
            catch
            {
                Write-Warning -Message "Error in screen '$screenName': $_"
                # On error, pop the current screen and try to go back
                [void]$script:NavigationStack.Pop()
                Clear-Host
                continue
            }

            # Validate navigation result
            if ($null -eq $navResult -or -not $navResult.ContainsKey('Action'))
            {
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
                    $script:NavigationStack.Clear()
                }
                'Refresh'
                {
                    # Just clear and let the loop redraw the current screen
                    Clear-Host
                }
                default
                {
                    Write-Warning -Message "Unknown navigation action: $($navResult.Action). Going back."
                    [void]$script:NavigationStack.Pop()
                    Clear-Host
                }
            }
        }

        Write-Verbose -Message 'Navigation loop ended.'
    }
}
