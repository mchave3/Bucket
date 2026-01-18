function Initialize-BucketNavigation
{
    <#
      .SYNOPSIS
      Initializes the navigation stack and screen registry for the Bucket application.

      .DESCRIPTION
      This function sets up the script-scoped navigation stack (a Stack of PSCustomObjects)
      and the screen registry (a hashtable mapping screen names to function names).
      Each stack frame contains the screen name and optional arguments.

      .EXAMPLE
      Initialize-BucketNavigation

      Initializes the navigation system with an empty stack and the default screen registry.

      .OUTPUTS
      [void] This function initializes script-scoped variables and does not return output.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param()

    process
    {
        Write-Verbose -Message 'Initializing Bucket navigation system...'

        # Initialize the navigation stack with PSCustomObject frames
        $script:NavigationStack = [System.Collections.Generic.Stack[PSCustomObject]]::new()

        # Initialize the screen registry mapping screen names to function names
        $script:ScreenRegistry = @{
            'MainMenu'          = 'Show-BucketMainMenu'
            'StartProvisioning' = 'Show-BucketStartProvisioning'
            'ImageManagement'   = 'Show-BucketImageManagement'
            'ImportWim'         = 'Show-BucketImportWim'
            'UpdateManagement'  = 'Show-BucketUpdateManagement'
            'Settings'          = 'Show-BucketSettings'
            'About'             = 'Show-BucketAbout'
        }

        # Validate that each registered screen maps to an existing function (fail fast)
        foreach ($screen in $script:ScreenRegistry.GetEnumerator())
        {
            $screenName = $screen.Key
            $screenFunctionName = $screen.Value

            $screenCommand = Get-Command -Name $screenFunctionName -CommandType Function -ErrorAction SilentlyContinue
            if (-not $screenCommand)
            {
                throw "Screen '$screenName' references missing function '$screenFunctionName'."
            }
        }

        Write-Verbose -Message "Registered $($script:ScreenRegistry.Count) screens."
    }
}
