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
    param(

    )

    process {
        Write-BucketLog -Data "Navigating to Select Image Page" -Level Info

        # Check if the initialization function exists
        if (Get-Command -Name "Initialize-BucketSelectImagePage" -ErrorAction SilentlyContinue) {
            # Call the initialization function that will set up data and events
            Initialize-BucketSelectImagePage
        }
        else {
            # Fallback if the initialization function doesn't exist
            Write-BucketLog -Data "Initialize-BucketSelectImagePage not found, using basic navigation" -Level Warning
            Invoke-BucketNavigationService -PageTag "selectImagePage" -DataContext $script:globalDataContext
        }
    }
}
