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
    Version:     25.6.3.4
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
        #region Navigation State & UI Update
        $script:ImportISO_DataContext.CurrentPage = $PageTag
        Update-BucketISONavigationUI -CurrentPage $PageTag
        Update-BucketISOButtonVisibility -CurrentPage $PageTag
        #endregion

        #region Navigation Parameters
        $InitFunction = "Initialize-BucketISO_$($PageTag)"
        $navParams = @{
            DataContext    = $script:ImportISO_DataContext
            PageDictionary = $script:ImportISOPages
            XamlBasePath   = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "GUI\ImageManagement"
        }
        if ($PageDataContext) {
            # Merge global and page-specific DataContext
            $properties = @{}
            foreach ($property in $script:ImportISO_DataContext.PSObject.Properties) { $properties[$property.Name] = $property.Value }
            foreach ($property in $PageDataContext.PSObject.Properties) { $properties[$property.Name] = $property.Value }
            $navParams.DataContext = [PSCustomObject]$properties
        }
        #endregion

        #region Invoke Navigation
        Write-BucketLog -Data "Navigating to page: $PageTag (Init: $InitFunction)" -Level Info
        Invoke-BucketPage -PageTag $PageTag -RootFrame $WPF_ImportISO_MainWindow_MainFrame -InitFunction $InitFunction -NavigationServiceParams $navParams
        #endregion
    }
}
