<#
.SYNOPSIS
    Function to navigate to the Home page in the Bucket application

.DESCRIPTION
    This function navigates to the Home page in the Bucket application.
    It calls the central navigation service with the appropriate page tag.

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
    param(

    )

    process {
        Write-BucketLog -Data "Navigating to Home Page" -Level Info
        Invoke-BucketNavigationService -PageTag "homePage" -DataContext $script:globalDataContext
    }
}
