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
        #region Navigation State & UI Update
        $script:ImportISODataContext.CurrentPage = $PageTag
        Update-BucketISONavigationUI -CurrentPage $PageTag
        Update-BucketISOButtonVisibility -CurrentPage $PageTag
        #endregion

        #region DataContext Preparation
        $InitFunction = "Initialize-BucketISO_$($PageTag)"
        $navParams = @{ DataContext = $script:ImportISODataContext }
        if ($PageDataContext) {
            # Merge page-specific properties with the global DataContext
            $properties = @{}
            foreach ($property in $script:ImportISODataContext.PSObject.Properties) { $properties[$property.Name] = $property.Value }
            foreach ($property in $PageDataContext.PSObject.Properties) { $properties[$property.Name] = $property.Value }
            $navParams.DataContext = [PSCustomObject]$properties
        }        $navParams.PageDictionary = $script:ImportISOPages

        $navParams.XamlBasePath = Join-Path -Path (Split-Path $PSScriptRoot -Parent) -ChildPath "GUI\ImageManagement"
        #endregion

        #region Page Invocation
        $xamlFileName = $script:ImportISOPages[$PageTag] + ".xaml"
        $xamlPath = Join-Path $navParams.XamlBasePath $xamlFileName
        Write-BucketLog -Data "[ISO Import] XAML file path used: $xamlPath" -Level Debug
        Write-BucketLog -Data "[ISO Import] XAML file exists: $(Test-Path $xamlPath)" -Level Debug
        if (Test-Path $xamlPath) {
            try {
                $xamlContent = Get-Content $xamlPath -Raw
                [xml]$xamlDoc = $xamlContent
                Write-BucketLog -Data "[ISO Import] XAML root node: $($xamlDoc.DocumentElement.Name)" -Level Debug
            }
            catch {
                Write-BucketLog -Data "[ISO Import] Could not parse XAML for root node: $_" -Level Warning
            }
        }
        Invoke-BucketPage -PageTag $PageTag -RootFrame $WPF_MainWindow_ImportISO_MainFrame -InitFunction $InitFunction -NavigationServiceParams $navParams
        #endregion
    }
}
