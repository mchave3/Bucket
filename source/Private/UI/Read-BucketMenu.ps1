function Read-BucketMenu
{
    <#
      .SYNOPSIS
      Displays a menu and returns a standardized navigation result based on user selection.

      .DESCRIPTION
      This function wraps Read-SpectreSelection to provide consistent menu behavior across
      all Bucket screens. It automatically adds Back and Exit options based on context
      (Back is only shown when not at the root menu) and returns a properly formatted
      navigation result that the navigation engine can process.

      .EXAMPLE
      $result = Read-BucketMenu -Title 'Image Management' -Choices @('View Images', 'Import Image', 'Delete Image') -NavigationMap @{
          'View Images' = 'ViewImages'
          'Import Image' = 'ImportImage'
          'Delete Image' = 'DeleteImage'
      }

      Displays a menu with the given choices plus Back/Exit, and returns a navigation result.

      .PARAMETER Title
      The title/message to display above the menu choices.

      .PARAMETER Choices
      An array of menu choice strings to display.

      .PARAMETER NavigationMap
      A hashtable mapping choice strings to target screen names for navigation.

      .PARAMETER ShowBack
      Whether to show the Back option. Defaults to true when navigation stack has more than one item.

      .PARAMETER ShowExit
      Whether to show the Exit option. Defaults to true.

      .PARAMETER Color
      The accent color for the selected option. Defaults to Cyan1.

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
        # Build the full choices list
        $menuChoices = [System.Collections.ArrayList]::new()

        # Add the main choices
        foreach ($choice in $Choices)
        {
            [void]$menuChoices.Add($choice)
        }

        # Determine if we should show Back based on navigation stack depth
        $shouldShowBack = $ShowBack
        if ($null -eq $shouldShowBack)
        {
            $shouldShowBack = ($script:NavigationStack.Count -gt 1)
        }

        # Add Back option if appropriate
        if ($shouldShowBack)
        {
            [void]$menuChoices.Add('Back')
        }

        # Add Exit option
        if ($ShowExit)
        {
            [void]$menuChoices.Add('Exit')
        }

        # Display the menu and get selection
        $selection = Read-SpectreSelection -Message $Title -Choices $menuChoices.ToArray() -Color $Color -PageSize 10

        # Process the selection and return navigation result
        switch ($selection)
        {
            'Back'
            {
                return New-BucketNavResult -Action Back
            }
            'Exit'
            {
                return New-BucketNavResult -Action Exit
            }
            default
            {
                # Check if we have a navigation mapping for this choice
                if ($NavigationMap.ContainsKey($selection))
                {
                    return New-BucketNavResult -Action Navigate -Target $NavigationMap[$selection]
                }
                else
                {
                    # Return the selection as-is in a special result for screens to handle
                    return @{
                        Action    = 'Selection'
                        Selection = $selection
                    }
                }
            }
        }
    }
}
