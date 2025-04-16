<#
.SYNOPSIS
    Generic wizard navigation service for Bucket application

.DESCRIPTION
    Provides navigation functionality for wizard-style windows in the Bucket application.
    This service handles page loading, navigation between wizard pages, and maintaining navigation state.

.NOTES
    Name:        Invoke-BucketWizardNavigationService.ps1
    Author:      Mickaël CHAVE
    Created:     04/16/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    # Navigate to a page in a wizard
    Invoke-BucketWizardNavigationService -PageTag "dataSourcePage" -Frame $WPF_ImportISO_Frame -XamlBasePath "$PSScriptRoot\..\ImageManagement" -PageDictionary $isoWizardPages -ButtonUpdateFunction $null
#>
function Invoke-BucketWizardNavigationService {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $true)]
        [System.Windows.Controls.Frame]$Frame,

        [Parameter(Mandatory = $false)]
        [string]$XamlBasePath = "$PSScriptRoot\GUI",

        [Parameter(Mandatory = $false)]
        [hashtable]$PageDictionary,

        [Parameter(Mandatory = $false)]
        [scriptblock]$ButtonUpdateFunction,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$DataContext,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$GlobalDataContext
    )

    process {
        Write-BucketLog -Data "Wizard Navigation service: Navigating to page: $PageTag" -Level Debug

        # Use provided PageDictionary or fallback to script:pages
        $pageDictToUse = $PageDictionary
        if (-not $pageDictToUse) {
            $pageDictToUse = $script:pages
        }

        # Ensure the page tag exists in our dictionary
        if (-not $pageDictToUse.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }

        # Ensure frame exists
        if (-not $Frame) {
            Write-BucketLog -Data "Frame UI element not provided or null" -Level Error
            return
        }

        try {
            # Get the page name
            $pageName = $pageDictToUse[$PageTag]
            $simplePageName = $pageName.Split('.')[-1]  # Extract just the page name part

            # Construct the path to the XAML file
            $xamlFilePath = "$XamlBasePath\$simplePageName.xaml"

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
                    $globalContextToUse = $GlobalDataContext
                    if (-not $globalContextToUse) {
                        $globalContextToUse = $script:globalDataContext
                    }

                    if ($null -ne $globalContextToUse) {
                        foreach ($property in $globalContextToUse.PSObject.Properties) {
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
                    $page.DataContext = $globalContextToUse ?? $script:globalDataContext
                    Write-BucketLog -Data "Using global DataContext for page $simplePageName" -Level Debug
                }

                # Add the Loaded event handler if provided in DataContext
                if ($DataContext -and $DataContext.PSObject.Properties['PageLoaded']) {
                    $page.Add_Loaded($DataContext.PageLoaded)
                    Write-BucketLog -Data "Added PageLoaded event handler for $simplePageName" -Level Debug
                }

                # Navigate to the page
                $Frame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to XAML page: $simplePageName" -Level Info

                # Call button update function if provided
                if ($null -ne $ButtonUpdateFunction) {
                    & $ButtonUpdateFunction -PageTag $PageTag
                    Write-BucketLog -Data "Updated navigation buttons for page: $PageTag" -Level Debug
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

                # Set the data context
                $contextToUse = $GlobalDataContext ?? $script:globalDataContext
                if ($contextToUse) {
                    $page.DataContext = $contextToUse
                }

                # Navigate to the page
                $Frame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to fallback page for: $simplePageName" -Level Info
            }

            # Return the page object for reference
            return $page
        }
        catch {
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
            return $null
        }
    }
}
