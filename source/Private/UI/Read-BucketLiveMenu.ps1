function Read-BucketLiveMenu
{
    <#
      .SYNOPSIS
      Displays an interactive menu with a header using Spectre Live rendering.

      .DESCRIPTION
      This function renders a full-screen interactive menu using Spectre.Console's
      live rendering capabilities. Unlike Read-SpectreSelection, this keeps the header
      panel visible while the user navigates the menu. The layout consists of a header
      panel, a menu panel with keyboard-navigable choices, and a footer with navigation hints.

      .EXAMPLE
      $result = Read-BucketLiveMenu -Title 'Image Management' -Choices @('View Images', 'Import Image')

      Displays a menu with the given title and choices, returns the user's selection.

      .EXAMPLE
      $result = Read-BucketLiveMenu -Title 'Settings' -Subtitle 'Configure options' -Choices @('Option 1', 'Option 2') -ShowBack $true

      Displays a menu with title, subtitle, and explicit Back option.

      .PARAMETER Title
      The title text to display in the header panel.

      .PARAMETER Subtitle
      Optional subtitle/description text displayed below the title in the header.

      .PARAMETER Choices
      An array of menu choice strings to display.

      .PARAMETER ShowBack
      Whether to show the Back option. Defaults to auto-detect based on navigation stack depth.

      .PARAMETER ShowExit
      Whether to show the Exit option. Defaults to true.

      .PARAMETER Color
      The accent color for panels and selected items. Defaults to Cyan1.

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
        # Store parameters in local variables for use in scriptblock closure
        # (ScriptAnalyzer requires explicit reference before closure)
        $menuTitle = $Title
        $menuSubtitle = $Subtitle
        $menuColor = $Color

        # Build the full choices list
        $menuChoices = [System.Collections.ArrayList]::new()

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

        if ($shouldShowBack)
        {
            [void]$menuChoices.Add('Back')
        }

        if ($ShowExit)
        {
            [void]$menuChoices.Add('Exit')
        }

        $allChoices = $menuChoices.ToArray()
        $selectedIndex = 0
        $menuResult = $null

        # Build the layout structure:
        # +----------------------------------+
        # |             Header               |
        # +----------------------------------+
        # |              Menu                |
        # +----------------------------------+
        # |             Footer               |
        # +----------------------------------+

        $layout = New-SpectreLayout -Name 'root' -Rows @(
            (New-SpectreLayout -Name 'header' -MinimumSize 5 -Ratio 1 -Data 'empty')
            (New-SpectreLayout -Name 'menu' -Ratio 6 -Data 'empty')
            (New-SpectreLayout -Name 'footer' -MinimumSize 3 -Ratio 1 -Data 'empty')
        )

        Invoke-SpectreLive -Data $layout -ScriptBlock {
            param (
                [Spectre.Console.LiveDisplayContext] $Context
            )

            while ($true)
            {
                # Build header panel
                if ([string]::IsNullOrWhiteSpace($menuSubtitle))
                {
                    $headerContent = "[bold $menuColor]$menuTitle[/]"
                }
                else
                {
                    $headerContent = "[bold $menuColor]$menuTitle[/]`n[grey]$menuSubtitle[/]"
                }
                $headerPanel = $headerContent |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color $menuColor

                # Build menu panel with choices
                $menuLines = @()
                for ($i = 0; $i -lt $allChoices.Count; $i++)
                {
                    $choice = $allChoices[$i]
                    if ($i -eq $selectedIndex)
                    {
                        $menuLines += "[$menuColor]> $choice[/]"
                    }
                    else
                    {
                        $menuLines += "  [grey]$choice[/]"
                    }
                }
                $menuContent = $menuLines -join "`n"
                $menuPanel = $menuContent |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded

                # Build footer panel
                $footerContent = '[grey]Up/Down: Navigate | Enter: Select | Esc: Back[/]'
                $footerPanel = $footerContent |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded

                # Update layout
                $layout['header'].Update($headerPanel) | Out-Null
                $layout['menu'].Update($menuPanel) | Out-Null
                $layout['footer'].Update($footerPanel) | Out-Null

                # Refresh display
                $Context.Refresh()

                # Handle input
                [Console]::TreatControlCAsInput = $true
                $keyInfo = [Console]::ReadKey($true)

                if ($keyInfo.Key -eq 'Escape')
                {
                    Set-Variable -Name 'menuResult' -Value @{ Action = 'Back' } -Scope 1
                    return
                }
                elseif ($keyInfo.Key -eq 'C' -and $keyInfo.Modifiers -eq 'Control')
                {
                    Set-Variable -Name 'menuResult' -Value @{ Action = 'Back' } -Scope 1
                    return
                }
                elseif ($keyInfo.Key -eq 'DownArrow')
                {
                    $selectedIndex = ($selectedIndex + 1) % $allChoices.Count
                }
                elseif ($keyInfo.Key -eq 'UpArrow')
                {
                    $selectedIndex = ($selectedIndex - 1 + $allChoices.Count) % $allChoices.Count
                }
                elseif ($keyInfo.Key -eq 'Enter')
                {
                    $selectedChoice = $allChoices[$selectedIndex]

                    switch ($selectedChoice)
                    {
                        'Back'
                        {
                            Set-Variable -Name 'menuResult' -Value @{ Action = 'Back' } -Scope 1
                        }
                        'Exit'
                        {
                            Set-Variable -Name 'menuResult' -Value @{ Action = 'Exit' } -Scope 1
                        }
                        default
                        {
                            Set-Variable -Name 'menuResult' -Value @{
                                Action    = 'Selection'
                                Selection = $selectedChoice
                            } -Scope 1
                        }
                    }
                    return
                }
            }
        }

        return $menuResult
    }
}
