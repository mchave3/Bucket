function Show-BucketSubmenuHeader
{
    <#
      .SYNOPSIS
      Displays a styled panel header for submenu screens.

      .DESCRIPTION
      This function renders a centered, styled panel header using Spectre Console
      for submenu screens. Unlike the main menu which uses Figlet text, submenus
      use a cleaner panel-based header that integrates better with the navigation flow.
      Uses Out-SpectreHost to render without polluting the PowerShell pipeline.

      .EXAMPLE
      Show-BucketSubmenuHeader -Title 'Image Management'

      Displays a panel header with "Image Management" centered.

      .EXAMPLE
      Show-BucketSubmenuHeader -Title 'Settings' -Subtitle 'Configure Bucket module settings.'

      Displays a panel header with title and subtitle.

      .PARAMETER Title
      The title text to display in the header panel.

      .PARAMETER Subtitle
      Optional subtitle/description text displayed below the title.

      .PARAMETER Color
      The accent color for the panel border. Defaults to Cyan1.

      .OUTPUTS
      [void] This function outputs directly to the console without polluting the pipeline.
    #>
    [CmdletBinding()]
    [OutputType([void])]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]
        $Title,

        [Parameter()]
        [string]
        $Subtitle,

        [Parameter()]
        [string]
        $Color = 'Cyan1'
    )

    process
    {
        # Build content: title with optional subtitle
        if ([string]::IsNullOrWhiteSpace($Subtitle))
        {
            $content = "[bold $Color]$Title[/]"
        }
        else
        {
            $content = "[bold $Color]$Title[/]`n[grey]$Subtitle[/]"
        }

        # Create centered, aligned panel and render via Out-SpectreHost (no pipeline pollution)
        $content |
            Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
            Format-SpectrePanel -Expand -Border Rounded -Color $Color |
            Out-SpectreHost

        # Add spacing after header
        Write-Host ''
    }
}
