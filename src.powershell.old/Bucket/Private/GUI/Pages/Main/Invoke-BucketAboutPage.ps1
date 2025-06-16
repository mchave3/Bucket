<#
.SYNOPSIS
    Navigates to the About page in the Bucket application

.DESCRIPTION
    Navigates to the About page by calling the central navigation service with the appropriate page tag and initialization function.

.NOTES
    Name:        Invoke-BucketAboutPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/15/2025
    Version:     25.6.3.4
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket
#>
function Invoke-BucketAboutPage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "Navigating to About page" -Level Info
        Invoke-BucketPage -PageTag "aboutPage" -RootFrame $WPF_MainWindow_RootFrame -InitFunction "Initialize-BucketAboutPage" -NavigationServiceParams @{ DataContext = $script:globalDataContext }
    }
}
