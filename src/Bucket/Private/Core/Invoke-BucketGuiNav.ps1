<#
.SYNOPSIS
    Brief description of the script purpose

.DESCRIPTION
    Detailed description of what the script/function does

.NOTES
    Name:        Invoke-BucketGuiNav.ps1
    Author:      Mickaël CHAVE
    Created:     04/05/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Example of how to use this script/function
#>
function Invoke-BucketGuiNav {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag
    )

    process {
        Write-BucketLog -Data "Navigating to page: $PageTag" -Level Debug

        # Ensure the page tag exists in our dictionary
        if (-not $script:pages.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }

        # Get a reference to the frame
        $rootFrame = $WPF_RootFrame
        if (-not $rootFrame) {
            Write-BucketLog -Data "RootFrame UI element not found" -Level Error
            return
        }

        try {
            # Get the page name
            $pageName = $script:pages[$PageTag]
            $simplePageName = $pageName.Split('.')[-1]  # Extract just the page name part
            
            # Construct the path to the XAML file
            #$moduleRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
            $xamlFilePath = "$PSScriptRoot\GUI\$simplePageName.xaml"
            
            Write-BucketLog -Data "Looking for XAML file: $xamlFilePath" -Level Debug
            
            # Check if the XAML file exists
            if (Test-Path -Path $xamlFilePath) {
                Write-BucketLog -Data "Loading XAML file: $xamlFilePath" -Level Debug
                
                # Load the XAML content
                $xamlContent = Get-Content -Path $xamlFilePath -Raw
                
                # Clean up the XAML for PowerShell's XML parser
                $xamlContent = $xamlContent -replace 'xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"', ''
                $xamlContent = $xamlContent -replace 'xmlns:d="http://schemas.microsoft.com/expression/blend/2008"', ''
                $xamlContent = $xamlContent -replace 'mc:Ignorable="d"', ''
                $xamlContent = $xamlContent -replace 'x:Class="[^"]*"', ''
                
                # Create the XML document
                $xamlDoc = New-Object System.Xml.XmlDocument
                $xamlDoc.LoadXml($xamlContent)
                
                # Create the page object from XAML
                $reader = New-Object System.Xml.XmlNodeReader $xamlDoc
                $page = [Windows.Markup.XamlReader]::Load($reader)
                
                # Add the data context to the page
                $page.DataContext = $script:dataContext
                
                # Navigate to the page
                $rootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to XAML page: $simplePageName" -Level Info
            }
            else {
                Write-BucketLog -Data "XAML file not found: $xamlFilePath - Creating fallback page" -Level Warning
                
                # Create a fallback page
                $page = New-Object -TypeName System.Windows.Controls.Page
                
                # Create a StackPanel for page content
                $stackPanel = New-Object -TypeName System.Windows.Controls.StackPanel
                $stackPanel.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 20
                
                # Create a TextBlock with the page name
                $titleBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $titleBlock.Text = $simplePageName
                $titleBlock.FontSize = 24
                $titleBlock.FontWeight = "Bold"
                $titleBlock.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 0, 0, 0, 20
                $stackPanel.Children.Add($titleBlock)
                
                # Add description text
                $descBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $descBlock.Text = "Le fichier XAML pour cette page ($simplePageName.xaml) n'a pas été trouvé."
                $descBlock.FontSize = 16
                $descBlock.TextWrapping = "Wrap"
                $stackPanel.Children.Add($descBlock)
                
                # Ajouter le chemin du fichier qui était recherché
                $pathBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $pathBlock.Text = "Chemin recherché: $xamlFilePath"
                $pathBlock.FontSize = 12
                $pathBlock.Opacity = 0.7
                $pathBlock.TextWrapping = "Wrap"
                $pathBlock.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 0, 5, 0, 0
                $stackPanel.Children.Add($pathBlock)
                
                # Add the StackPanel to the Page
                $page.Content = $stackPanel
                $page.DataContext = $script:dataContext
                
                # Navigate to the page
                $rootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to fallback page for: $simplePageName" -Level Info
            }
            
            # Update the navigation button styles
            if (Get-Command -Name "Invoke-UpdateNavigationButtonStyle" -ErrorAction SilentlyContinue) {
                Invoke-UpdateNavigationButtonStyle -selectedTag $PageTag
            }
            else {
                Write-BucketLog -Data "Invoke-UpdateNavigationButtonStyle function not found" -Level Warning
            }
        }
        catch {
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
        }
    }
}