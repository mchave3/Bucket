<#
.SYNOPSIS
    Function to navigate to the Select Image page in the Bucket application

.DESCRIPTION
    This function navigates to the select image page in the Bucket application.
    It calls the central navigation service with the appropriate page tag.

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
        Invoke-BucketPage -PageTag "selectImagePage" -RootFrame $WPF_MainWindow_RootFrame -InitFunction "Initialize-BucketSelectImagePage" -NavigationServiceParams @{ DataContext = $script:globalDataContext }
    }
}
