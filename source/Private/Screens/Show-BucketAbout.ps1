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

        Show-BucketSubmenuHeader -Title 'About' -Subtitle 'Bucket - WIM Image Provisioning Tool'

        # Display about information using Spectre panel
        $aboutContent = @(
            "[grey]Version:[/]     [white]$version[/]"
            "[grey]Author:[/]      [white]$author[/]"
            "[grey]Description:[/] [white]$description[/]"
            "[grey]Repository:[/]  [link=$projectUri]$projectUri[/]"
            ''
            '[grey]Built with[/] [cyan]PwshSpectreConsole[/] [grey]and[/] [cyan]Spectre.Console[/]'
        ) -join "`n"

        $aboutContent | Format-SpectrePanel -Expand -Border Rounded | Out-SpectreHost
        Write-Host ''

        # Simple menu with just Back option
        $choices = @()
        $result = Read-BucketMenu -Title 'About' -Choices $choices -ShowBack $true -ShowExit $false

        return $result
    }
}
