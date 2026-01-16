function Show-BucketAbout
{
    <#
      .SYNOPSIS
      Displays the About screen with module information and credits.

      .DESCRIPTION
      This function renders the About screen, showing module name, version,
      author information, and links to the project repository. It reads
      version information from the module manifest.

      .EXAMPLE
      $result = Show-BucketAbout

      Displays the About screen and returns a navigation result.

      .OUTPUTS
      [hashtable] A navigation result object for the navigation engine.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    process
    {
        # Get module information
        $moduleInfo = Get-Module -Name Bucket -ListAvailable | Select-Object -First 1

        $version = '0.0.1'
        $author = 'Mickael CHAVE'
        $description = 'A PowerShell module for WIM image provisioning.'
        $projectUri = 'https://github.com/yourrepo/Bucket'

        if ($moduleInfo)
        {
            $version = $moduleInfo.Version.ToString()
            if ($moduleInfo.Author) { $author = $moduleInfo.Author }
            if ($moduleInfo.Description) { $description = $moduleInfo.Description }
            if ($moduleInfo.ProjectUri) { $projectUri = $moduleInfo.ProjectUri.ToString() }
        }

        $aboutContent = @(
            "[grey]Version:[/]     [white]$version[/]"
            "[grey]Author:[/]      [white]$author[/]"
            "[grey]Description:[/] [white]$description[/]"
            "[grey]Repository:[/]  [link=$projectUri]$projectUri[/]"
            ''
            '[grey]Built with[/] [cyan]PwshSpectreConsole[/] [grey]and[/] [cyan]Spectre.Console[/]'
        ) -join "`n"

        $menuColor = 'Cyan1'
        $aboutResult = $null

        $layout = New-SpectreLayout -Name 'root' -Rows @(
            (New-SpectreLayout -Name 'header' -MinimumSize 5 -Ratio 1 -Data 'empty')
            (New-SpectreLayout -Name 'content' -Ratio 10 -Data 'empty')
            (New-SpectreLayout -Name 'footer' -MinimumSize 3 -Ratio 1 -Data 'empty')
        )

        Invoke-SpectreLive -Data $layout -ScriptBlock {
            param (
                [Spectre.Console.LiveDisplayContext] $Context
            )

            while ($true)
            {
                $headerContent = "[bold $menuColor]About[/]`n[grey]Bucket - WIM Image Provisioning Tool[/]"
                $headerPanel = $headerContent |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded -Color $menuColor

                $contentPanel = $aboutContent |
                    Format-SpectrePanel -Expand -Border Rounded

                $footerContent = '[grey]Enter/Esc: Back[/]'
                $footerPanel = $footerContent |
                    Format-SpectreAligned -HorizontalAlignment Center -VerticalAlignment Middle |
                    Format-SpectrePanel -Expand -Border Rounded

                $layout['header'].Update($headerPanel) | Out-Null
                $layout['content'].Update($contentPanel) | Out-Null
                $layout['footer'].Update($footerPanel) | Out-Null

                $Context.Refresh()

                [Console]::TreatControlCAsInput = $true
                $keyInfo = [Console]::ReadKey($true)

                if ($keyInfo.Key -eq 'Escape')
                {
                    Set-Variable -Name 'aboutResult' -Value (New-BucketNavResult -Action Back) -Scope 1
                    return
                }

                if ($keyInfo.Key -eq 'C' -and $keyInfo.Modifiers -eq 'Control')
                {
                    Set-Variable -Name 'aboutResult' -Value (New-BucketNavResult -Action Back) -Scope 1
                    return
                }

                if ($keyInfo.Key -eq 'Enter')
                {
                    Set-Variable -Name 'aboutResult' -Value (New-BucketNavResult -Action Back) -Scope 1
                    return
                }
            }
        }

        return $aboutResult
    }
}
