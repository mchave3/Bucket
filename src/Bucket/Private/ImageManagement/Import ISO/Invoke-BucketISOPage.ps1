<#
.SYNOPSIS
    Navigates to a specific page in the ISO import wizard

.DESCRIPTION
    Handles the navigation between pages in the ISO import wizard
    Updates the UI to reflect the current page and manages page initialization

.NOTES
    Name:        Invoke-BucketISOPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/16/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Invoke-BucketISOPage -PageTag "dataSourcePage"
#>
function Invoke-BucketISOPage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$PageDataContext
    )

    process {
        $script:ImportISODataContext.CurrentPage = $PageTag
        Update-BucketISONavigationUI -CurrentPage $PageTag
        Update-BucketISOButtonVisibility -CurrentPage $PageTag
        $initializeFunctionName = "Initialize-BucketISO_$($PageTag)"
        $navParams = @{ DataContext = $script:ImportISODataContext }
        if ($PageDataContext) {
            # Merge the DataContext page-specific properties with the global DataContext
            $properties = @{}
            foreach ($property in $script:ImportISODataContext.PSObject.Properties) { $properties[$property.Name] = $property.Value }
            foreach ($property in $PageDataContext.PSObject.Properties) { $properties[$property.Name] = $property.Value }
            $navParams.DataContext = [PSCustomObject]$properties
        }
        # Add the page dictionary to navigation parameters
        $navParams.PageDictionary = $script:ImportISOPages

        # Get XAML base path for ImportISO pages
        $xamlBasePath = Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement"
        $navParams.XamlBasePath = $xamlBasePath

        Invoke-BucketPage -PageTag $PageTag -RootFrame $WPF_MainWindow_ImportISO_MainFrame -InitFunction $initializeFunctionName -NavigationServiceParams $navParams
    }
}
