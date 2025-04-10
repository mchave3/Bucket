﻿<#
.SYNOPSIS
    Navigates to a specified page in the Bucket GUI application

.DESCRIPTION
    Handles navigation between pages in the Bucket WPF application by dynamically loading
    XAML files based on a page tag. The function looks up the page name in a dictionary,
    constructs the path to the appropriate XAML file, and loads it into the main application frame.
    If the XAML file is not found, it creates a fallback page with an error message.
    This function also updates navigation button styles to reflect the current page.

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
    Invoke-BucketGuiNav -PageTag "homePage"
    # Navigates to the Home page by loading its XAML file and updating the navigation UI
#>
function Invoke-BucketGuiNav {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$DataContext
    )

    process {
        Write-BucketLog -Data "Navigating to page: $PageTag" -Level Debug

        # Ensure the page tag exists in our dictionary
        if (-not $script:pages.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }

        # Get a reference to the frame
        $rootFrame = $WPF_MainWindow_RootFrame
        if (-not $rootFrame) {
            Write-BucketLog -Data "RootFrame UI element not found" -Level Error
            return
        }

        try {
            # Get the page name
            $pageName = $script:pages[$PageTag]
            $simplePageName = $pageName.Split('.')[-1]  # Extract just the page name part
            
            # Construct the path to the XAML file
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
                
                # Handle the DataContext for the page
                # If a custom DataContext is provided, we need to merge it with the global one
                if ($null -ne $DataContext) {
                    # Create a shallow copy of the global data context properties
                    $properties = @{}
                    
                    # Add global context properties first (if available)
                    if ($null -ne $script:globalDataContext) {
                        foreach ($property in $script:globalDataContext.PSObject.Properties) {
                            $properties[$property.Name] = $property.Value
                        }
                    }
                    
                    # Add/override with custom context properties
                    foreach ($property in $DataContext.PSObject.Properties) {
                        $properties[$property.Name] = $property.Value
                    }
                    
                    # Create new PSObject with combined properties
                    $mergedContext = New-Object PSObject -Property $properties
                    $page.DataContext = $mergedContext
                    
                    Write-BucketLog -Data "Created merged DataContext for page $simplePageName" -Level Debug
                }
                else {
                    # No custom context, just use global
                    $page.DataContext = $script:globalDataContext
                    Write-BucketLog -Data "Using global DataContext for page $simplePageName" -Level Debug
                }
                
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
                $descBlock.Text = "The XAML file for this page ($simplePageName.xaml) was not found."
                $descBlock.FontSize = 16
                $descBlock.TextWrapping = "Wrap"
                $stackPanel.Children.Add($descBlock)
                
                # Add the path of the file that was being searched for
                $pathBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $pathBlock.Text = "Path searched: $xamlFilePath"
                $pathBlock.FontSize = 12
                $pathBlock.Opacity = 0.7
                $pathBlock.TextWrapping = "Wrap"
                $pathBlock.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 0, 5, 0, 0
                $stackPanel.Children.Add($pathBlock)
                
                # Add the StackPanel to the Page
                $page.Content = $stackPanel
                $page.DataContext = $script:globalDataContext
                
                # Navigate to the page
                $rootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to fallback page for: $simplePageName" -Level Info
            }
            
            # Update the navigation button styles
            if (Get-Command -Name "Update-BucketNavBtnStyle" -ErrorAction SilentlyContinue) {
                Update-BucketNavBtnStyle -PageTag $PageTag
            }
            else {
                Write-BucketLog -Data "Update-BucketNavBtnStyle function not found" -Level Warning
            }
        }
        catch {
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
        }
    }
}