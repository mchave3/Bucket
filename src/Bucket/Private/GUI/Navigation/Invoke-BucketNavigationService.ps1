<#
.SYNOPSIS
    Generic navigation service for Bucket application

.DESCRIPTION
    Provides centralized navigation functionality for the Bucket application.
    This service handles page loading, navigation between pages, and maintaining navigation state.
    The service is designed to work with any window and frame configuration.

.NOTES
    Name:        Invoke-BucketNavigationService.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.1.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    # For MainWindow navigation
    Invoke-BucketNavigationService -PageTag "homePage" -RootFrame $WPF_MainWindow_RootFrame

.EXAMPLE
    # For ISO import wizard navigation
    Invoke-BucketNavigationService -PageTag "importStep1" -RootFrame $WPF_ImportWizard_RootFrame -XamlBasePath "$PSScriptRoot\ISO\Wizard"
#>
function Invoke-BucketNavigationService {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $true)]
        [System.Windows.Controls.Frame]$RootFrame,

        [Parameter(Mandatory = $false)]
        [string]$XamlBasePath,

        [Parameter(Mandatory = $false)]
        [hashtable]$PageDictionary,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$DataContext,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$GlobalDataContext,

        [Parameter(Mandatory = $false)]
        [scriptblock]$OnNavigationComplete
    )

    process {
        Write-BucketLog -Data "Navigation service: Navigating to page: $PageTag" -Level Debug

        # Determine which page dictionary to use
        $pages = $PageDictionary
        if (-not $pages) {
            # Fall back to global script-level pages dictionary if available
            $pages = $script:pages
        }

        # Ensure the page tag exists in our dictionary
        if (-not $pages.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }

        # Verify that we have a valid frame
        if (-not $RootFrame) {
            Write-BucketLog -Data "RootFrame UI element not provided or is null" -Level Error
            return
        }

        try {
            # Get the page name
            $pageName = $pages[$PageTag]
            $simplePageName = $pageName.Split('.')[-1]  # Extract just the page name part

            # Determine base path for XAML files
            $basePath = $XamlBasePath
            if (-not $basePath) {
                # Default to standard GUI path if not specified
                $basePath = "$PSScriptRoot\GUI"
            }

            # Construct the path to the XAML file
            $xamlFilePath = "$basePath\$simplePageName.xaml"

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

                    # Determine which global context to use
                    $globalContext = $GlobalDataContext
                    if ($null -eq $globalContext) {
                        # Fall back to script level global context if available
                        $globalContext = $script:globalDataContext
                    }

                    # Add global context properties first (if available)
                    if ($null -ne $globalContext) {
                        foreach ($property in $globalContext.PSObject.Properties) {
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
                    # No custom context, use the provided global or fall back to script level
                    $globalContext = $GlobalDataContext
                    if ($null -eq $globalContext) {
                        # Fall back to script level global context if available
                        $globalContext = $script:globalDataContext
                    }
                    $page.DataContext = $globalContext
                    Write-BucketLog -Data "Using global DataContext for page $simplePageName" -Level Debug
                }

                # Add the Loaded event handler if provided in DataContext
                if ($DataContext -and $DataContext.PSObject.Properties['PageLoaded']) {
                    $page.Add_Loaded($DataContext.PageLoaded)
                    Write-BucketLog -Data "Added PageLoaded event handler for $simplePageName" -Level Debug
                }

                # Navigate to the page
                $RootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to XAML page: $simplePageName" -Level Info

                # Execute OnNavigationComplete if provided
                if ($OnNavigationComplete) {
                    & $OnNavigationComplete -Page $page -PageTag $PageTag -PageName $simplePageName
                }
                # Fall back to the default navigation button style update if available
                elseif (Get-Command 'Update-BucketNavButtonStyle' -ErrorAction SilentlyContinue) {
                    Update-BucketNavButtonStyle -PageTag $PageTag
                }
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
                $RootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to fallback page for: $simplePageName" -Level Info

                # Execute OnNavigationComplete if provided
                if ($OnNavigationComplete) {
                    & $OnNavigationComplete -Page $page -PageTag $PageTag -PageName $simplePageName
                }
            }
        }
        catch {
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
        }
    }
}
