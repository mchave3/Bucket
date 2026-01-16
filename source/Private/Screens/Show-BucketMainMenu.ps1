function Show-BucketMainMenu
{
    <#
      .SYNOPSIS
      Displays the Bucket main menu and returns a navigation result based on user selection.

      .DESCRIPTION
      This function renders the main menu screen for the Bucket application, featuring
      the Figlet header and main navigation options. It uses the Read-BucketMenu helper
      to display choices and handle navigation consistently.

      .EXAMPLE
      $result = Show-BucketMainMenu

      Displays the main menu and returns a navigation result.

      .OUTPUTS
      [hashtable] A navigation result object for the navigation engine.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    process
    {
        # Display the header
        Show-BucketHeader

        # Define menu choices and their navigation targets
        $choices = @(
            'Start Provisioning'
            'Image Management'
            'Update Management'
            'Settings'
            'About'
        )

        $navigationMap = @{
            'Start Provisioning' = 'StartProvisioning'
            'Image Management'   = 'ImageManagement'
            'Update Management'  = 'UpdateManagement'
            'Settings'           = 'Settings'
            'About'              = 'About'
        }

        # Display menu (no Back option for main menu, but show Exit)
        $result = Read-BucketMenu -Title 'Main Menu' -Choices $choices -NavigationMap $navigationMap -ShowBack $false -ShowExit $true

        return $result
    }
}
