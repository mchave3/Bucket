﻿<#
.SYNOPSIS
    Functions to navigate between pages in the Bucket application

.DESCRIPTION
    This file contains functions to navigate between different pages in the Bucket application.
    Each function calls the central navigation service with the appropriate page tag.

.NOTES
    Name:        Invoke-BucketPageNavigation.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
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
        Write-BucketLog -Data "Navigating to Home Page" -Level Info
        Invoke-BucketNavigationService -PageTag "homePage" -DataContext $script:globalDataContext
    }
}

function Invoke-BucketSelectImagePage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "Navigating to Select Image Page" -Level Info
        Invoke-BucketNavigationService -PageTag "selectImagePage" -DataContext $script:globalDataContext
    }
}

function Invoke-BucketAboutPage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "Navigating to About Page" -Level Info
        Invoke-BucketNavigationService -PageTag "aboutPage" -DataContext $script:globalDataContext
    }
}

