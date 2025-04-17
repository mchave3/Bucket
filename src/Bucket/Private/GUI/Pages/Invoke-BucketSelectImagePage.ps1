<#
.SYNOPSIS
    Navigates to the Select Image page in the Bucket application

.DESCRIPTION
    Navigates to the Select Image page by calling the central navigation service with the appropriate page tag and initialization function.

.NOTES
    Name:        Invoke-BucketSelectImagePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/15/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket
#>
function Invoke-BucketSelectImagePage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "[SelectImage] Navigating to Select Image page" -Level Info
        Invoke-BucketPage -PageTag "selectImagePage" -RootFrame $WPF_MainWindow_RootFrame -InitFunction "Initialize-BucketSelectImagePage" -NavigationServiceParams @{ DataContext = $script:globalDataContext }
    }
}
