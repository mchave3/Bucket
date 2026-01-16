function Read-BucketMenu
{
    <#
      .SYNOPSIS
      Displays a menu and returns a standardized navigation result based on user selection.

      .DESCRIPTION
      This function provides consistent menu behavior across all Bucket screens using
      Spectre Live rendering. It displays a header panel with title and optional subtitle,
      a keyboard-navigable menu, and automatically adds Back and Exit options based on
      context. Returns a properly formatted navigation result that the navigation engine
      can process.

      .EXAMPLE
      $result = Read-BucketMenu -Title 'Image Management' -Subtitle 'Manage your WIM images.' -Choices @('View Images', 'Import Image', 'Delete Image') -NavigationMap @{
          'View Images' = 'ViewImages'
          'Import Image' = 'ImportImage'
          'Delete Image' = 'DeleteImage'
      }

      Displays a menu with header panel and choices, returns a navigation result.

      .PARAMETER Title
      The title text to display in the header panel.

      .PARAMETER Subtitle
      Optional subtitle/description text displayed below the title in the header.

      .PARAMETER Choices
      An array of menu choice strings to display.

      .PARAMETER NavigationMap
      A hashtable mapping choice strings to target screen names for navigation.

      .PARAMETER ShowBack
      Whether to show the Back option. Defaults to true when navigation stack has more than one item.

      .PARAMETER ShowExit
      Whether to show the Exit option. Defaults to true.

      .PARAMETER Color
      The accent color for the header and selected option. Defaults to Cyan1.

      .OUTPUTS
      [hashtable] A navigation result object compatible with the navigation engine.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]
        $Title,

        [Parameter()]
        [string]
        $Subtitle,

        [Parameter(Mandatory = $true)]
        [string[]]
        $Choices,

        [Parameter()]
        [hashtable]
        $NavigationMap = @{},

        [Parameter()]
        [Nullable[bool]]
        $ShowBack = $null,

        [Parameter()]
        [bool]
        $ShowExit = $true,

        [Parameter()]
        [string]
        $Color = 'Cyan1'
    )

    process
    {
        # Call the live menu to get user selection
        $liveMenuParams = @{
            Title    = $Title
            Choices  = $Choices
            ShowBack = $ShowBack
            ShowExit = $ShowExit
            Color    = $Color
        }

        if (-not [string]::IsNullOrWhiteSpace($Subtitle))
        {
            $liveMenuParams['Subtitle'] = $Subtitle
        }

        $result = Read-BucketLiveMenu @liveMenuParams

        # Process the result and apply navigation mapping
        switch ($result.Action)
        {
            'Back'
            {
                return New-BucketNavResult -Action Back
            }
            'Exit'
            {
                return New-BucketNavResult -Action Exit
            }
            'Selection'
            {
                $selection = $result.Selection
                # Check if we have a navigation mapping for this choice
                if ($NavigationMap.ContainsKey($selection))
                {
                    return New-BucketNavResult -Action Navigate -Target $NavigationMap[$selection]
                }
                else
                {
                    # Return the selection as-is for screens to handle
                    return @{
                        Action    = 'Selection'
                        Selection = $selection
                    }
                }
            }
            default
            {
                # Pass through any other actions
                return $result
            }
        }
    }
}
