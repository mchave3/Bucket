<#
.SYNOPSIS
    Navigates to the Home page in the Bucket application

.DESCRIPTION
    Navigates to the Home page by calling the central navigation service with the appropriate page tag and initialization function.

.NOTES
    Name:        Invoke-BucketHomePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/15/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket
#>
function Invoke-BucketHomePage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "[Home] Navigating to Home page" -Level Info
        Invoke-BucketPage -PageTag "homePage" -RootFrame $WPF_MainWindow_RootFrame -InitFunction "Initialize-BucketHomePage" -NavigationServiceParams @{ DataContext = $script:globalDataContext }
    }
}
