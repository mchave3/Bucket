<#
.SYNOPSIS
    Function to navigate to the About page in the Bucket application

.DESCRIPTION
    This function navigates to the About page in the Bucket application.
    It calls the central navigation service with the appropriate page tag.

.NOTES
    Name:        Invoke-BucketAboutPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/15/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket
#>

function Invoke-BucketAboutPage {
    [CmdletBinding()]
    param(

    )

    process {
        Write-BucketLog -Data "Navigating to About Page" -Level Info

        # Check if the initialization function exists
        if (Get-Command -Name "Initialize-AboutPage" -ErrorAction SilentlyContinue) {
            # Call the initialization function that will set up data and events
            Initialize-AboutPage
        }
        else {
            # Fallback if the initialization function doesn't exist
            Write-BucketLog -Data "Initialize-AboutPage not found, using basic navigation" -Level Warning
            Invoke-BucketNavigationService -PageTag "aboutPage" -DataContext $script:globalDataContext
        }
    }
}
