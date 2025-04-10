﻿<#
.SYNOPSIS
    Updates the navigation button styles

.DESCRIPTION
    This function updates the navigation button styles to visually indicate which page is currently selected.

.NOTES
    Name:        Update-BucketNavBtnStyle.ps1
    Author:      Mickaël CHAVE
    Created:     04/06/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Update-BucketNavBtnStyle --PageTag "homePage"
#>
function Update-BucketNavBtnStyle {
    [CmdletBinding(SupportsShouldProcess=$true)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag
    )
    
    process {
        try {
            Write-BucketLog -Data "Updating navigation button styles for: $PageTag" -Level Debug
            
            # Reference to all navigation buttons
            $navButtons = @(
                $WPF_MainWindow_NavHome,
                $WPF_MainWindow_NavSelectImage,
                $WPF_MainWindow_NavAbout
            )
            
            # Try to get styles from form resources
            $defaultStyle = $form.FindResource("MenuButtonStyle")
            $selectedStyle = $form.FindResource("SelectedMenuButtonStyle")
            
            # If styles are not found, log warning and exit
            if (-not $defaultStyle -or -not $selectedStyle) {
                Write-BucketLog -Data "Navigation styles not found in resources" -Level Warning
                return
            }
            
            # Reset all navigation buttons to default style
            if ($PSCmdlet.ShouldProcess("Navigation buttons", "Reset to default style")) {
                foreach ($button in $navButtons) {
                    if ($button) {
                        $button.Style = $defaultStyle
                    }
                }
            }
            
            # Set the selected button style based on the page tag
            switch ($PageTag) {
                "homePage" { 
                    if ($WPF_MainWindow_NavHome -and $PSCmdlet.ShouldProcess("NavHome button", "Set selected style")) { 
                        $WPF_MainWindow_NavHome.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavHome" -Level Verbose
                    }
                }
                "selectImagePage" { 
                    if ($WPF_MainWindow_NavSelectImage -and $PSCmdlet.ShouldProcess("NavSelectImage button", "Set selected style")) { 
                        $WPF_MainWindow_NavSelectImage.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavSelectImage" -Level Verbose
                    }
                }
                "aboutPage" { 
                    if ($WPF_MainWindow_NavAbout -and $PSCmdlet.ShouldProcess("NavAbout button", "Set selected style")) { 
                        $WPF_MainWindow_NavAbout.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavAbout" -Level Verbose
                    }
                }
                default {
                    Write-BucketLog -Data "Unknown page tag: $PageTag" -Level Warning
                }
            }
            
            Write-BucketLog -Data "Navigation button styles updated successfully" -Level Debug
        }
        catch {
            Write-BucketLog -Data "Failed to update navigation button styles: $_" -Level Error
        }
    }
}
