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
    Version:     25.6.3.4
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
        #region Navigation Service Entry
        # Log the navigation request for traceability
        Write-BucketLog -Data "Navigating to page: $PageTag" -Level Debug
        #endregion

        #region Page Dictionary Resolution
        # Select the page dictionary (custom or script-scope)
        $pages = $PageDictionary
        if (-not $pages) {
            $pages = $script:pages
        }
        #endregion

        #region PageTag & Frame Validation
        # Validate page existence and frame
        if (-not $pages.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }
        if (-not $RootFrame) {
            Write-BucketLog -Data "RootFrame UI element not provided or is null" -Level Error
            return
        }
        #endregion

        try {
            #region XAML Path & Loading
            # Build XAML file path and clean up design-time attributes
            $pageName = $pages[$PageTag]
            $simplePageName = $pageName.Split('.')[-1]
            $basePath = $XamlBasePath
            if (-not $basePath) {
                $basePath = Join-Path -Path $PSScriptRoot -ChildPath "GUI"
            }
            $xamlFilePath = "$basePath\$simplePageName.xaml"
            Write-BucketLog -Data "Looking for XAML file: $xamlFilePath" -Level Debug
            #endregion

            if (Test-Path -Path $xamlFilePath) {
                #region XAML Loading
                # Load and parse the XAML file
                Write-BucketLog -Data "Loading XAML file: $xamlFilePath" -Level Debug
                $xamlContent = Get-Content -Path $xamlFilePath -Raw
                $xamlContent = $xamlContent -replace 'xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"', ''
                $xamlContent = $xamlContent -replace 'xmlns:d="http://schemas.microsoft.com/expression/blend/2008"', ''
                $xamlContent = $xamlContent -replace 'mc:Ignorable="d"', ''
                $xamlContent = $xamlContent -replace 'x:Class="[^"]*"', ''
                $xamlDoc = New-Object System.Xml.XmlDocument
                $xamlDoc.LoadXml($xamlContent)
                $reader = New-Object System.Xml.XmlNodeReader $xamlDoc
                $page = [Windows.Markup.XamlReader]::Load($reader)
                #endregion

                #region DataContext Handling
                # Merge DataContext with global context if provided
                if ($null -ne $DataContext) {
                    $properties = @{}
                    $globalContext = $GlobalDataContext
                    if ($null -eq $globalContext) {
                        $globalContext = $script:globalDataContext
                    }
                    if ($null -ne $globalContext) {
                        foreach ($property in $globalContext.PSObject.Properties) {
                            $properties[$property.Name] = $property.Value
                        }
                    }
                    foreach ($property in $DataContext.PSObject.Properties) {
                        $properties[$property.Name] = $property.Value
                    }
                    $mergedContext = New-Object PSObject -Property $properties
                    $page.DataContext = $mergedContext
                    Write-BucketLog -Data "Created merged DataContext for page $simplePageName" -Level Debug
                }
                else {
                    $globalContext = $GlobalDataContext
                    if ($null -eq $globalContext) {
                        $globalContext = $script:globalDataContext
                    }
                    $page.DataContext = $globalContext
                    Write-BucketLog -Data "Using global DataContext for page $simplePageName" -Level Debug
                }
                #endregion

                #region PageLoaded Event
                # Attach PageLoaded event handler if present
                if ($DataContext -and $DataContext.PSObject.Properties['PageLoaded']) {
                    $page.Add_Loaded($DataContext.PageLoaded)
                    Write-BucketLog -Data "Added PageLoaded event handler for $simplePageName" -Level Debug
                }
                #endregion

                #region Navigation
                # Navigate to the loaded page
                $RootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to XAML page: $simplePageName" -Level Info
                #endregion

                #region Navigation Completion Callback
                # Call navigation completion callback if provided
                if ($OnNavigationComplete) {
                    & $OnNavigationComplete -Page $page -PageTag $PageTag -PageName $simplePageName
                }
                #endregion

                #region Navigation Button Style
                # Optionally update navigation button styles if available
                $mainNavButtons = @{
                    "homePage"        = $WPF_MainWindow_NavHome
                    "selectImagePage" = $WPF_MainWindow_NavSelectImage
                    "aboutPage"       = $WPF_MainWindow_NavAbout
                }
                if (Get-Command 'Update-BucketNavigationStyle' -ErrorAction SilentlyContinue) {
                    Update-BucketNavigationStyle -PageTag $PageTag -ButtonMap $mainNavButtons -ResourceContext $WPF_MainWindow
                }
                #endregion
            }
            else {
                #region Fallback Page
                # Show a simple error page if XAML is missing
                Write-BucketLog -Data "[Navigation] XAML file not found: $xamlFilePath - Creating fallback page" -Level Warning
                $page = New-Object -TypeName System.Windows.Controls.Page
                $stackPanel = New-Object -TypeName System.Windows.Controls.StackPanel
                $stackPanel.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 20
                $titleBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $titleBlock.Text = $simplePageName
                $titleBlock.FontSize = 24
                $titleBlock.FontWeight = "Bold"
                $titleBlock.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 0, 0, 0, 20
                $stackPanel.Children.Add($titleBlock)
                $descBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $descBlock.Text = "The XAML file for this page ($simplePageName.xaml) was not found."
                $descBlock.FontSize = 16
                $descBlock.TextWrapping = "Wrap"
                $stackPanel.Children.Add($descBlock)
                $pathBlock = New-Object -TypeName System.Windows.Controls.TextBlock
                $pathBlock.Text = "Path searched: $xamlFilePath"
                $pathBlock.FontSize = 12
                $pathBlock.Opacity = 0.7
                $pathBlock.TextWrapping = "Wrap"
                $pathBlock.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 0, 5, 0, 0
                $stackPanel.Children.Add($pathBlock)
                $page.Content = $stackPanel
                $page.DataContext = $script:globalDataContext
                $RootFrame.Navigate($page)
                Write-BucketLog -Data "Successfully navigated to fallback page for: $simplePageName" -Level Info
                if ($OnNavigationComplete) {
                    & $OnNavigationComplete -Page $page -PageTag $PageTag -PageName $simplePageName
                }
                #endregion
            }
        }
        catch {
            #region Error Handling
            # Log any navigation errors
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
            #endregion
        }
    }
}
