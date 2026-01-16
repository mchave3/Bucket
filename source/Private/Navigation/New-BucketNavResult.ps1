function New-BucketNavResult
{
    <#
      .SYNOPSIS
      Creates a standardized navigation result object for screen transitions.

      .DESCRIPTION
      This function creates a navigation result hashtable that screens return to indicate
      what action the navigation engine should take. Valid actions are Navigate (go to a new screen),
      Back (return to previous screen), Exit (close application), and Refresh (redraw current screen).

      .EXAMPLE
      return New-BucketNavResult -Action Navigate -Target 'ImageManagement'

      Returns a navigation result that tells the engine to navigate to the ImageManagement screen.

      .EXAMPLE
      return New-BucketNavResult -Action Back

      Returns a navigation result that tells the engine to go back to the previous screen.

      .PARAMETER Action
      The navigation action to perform. Valid values are Navigate, Back, Exit, and Refresh.

      .PARAMETER Target
      The target screen name when Action is Navigate. Required for Navigate action.

      .PARAMETER Arguments
      Optional hashtable of arguments to pass to the target screen.

      .OUTPUTS
      [hashtable] A navigation result object with Action, Target, and Arguments properties.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Function creates a data object, does not change system state.')]
    [CmdletBinding()]
    [OutputType([hashtable])]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateSet('Navigate', 'Back', 'Exit', 'Refresh')]
        [string]
        $Action,

        [Parameter()]
        [string]
        $Target,

        [Parameter()]
        [hashtable]
        $Arguments = @{}
    )

    process
    {
        if ($Action -eq 'Navigate' -and [string]::IsNullOrWhiteSpace($Target))
        {
            throw "Target parameter is required when Action is 'Navigate'."
        }

        return @{
            Action    = $Action
            Target    = $Target
            Arguments = $Arguments
        }
    }
}
