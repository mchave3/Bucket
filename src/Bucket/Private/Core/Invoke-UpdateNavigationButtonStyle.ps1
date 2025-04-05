<#
.SYNOPSIS
    Met à jour les styles des boutons de navigation

.DESCRIPTION
    Cette fonction met à jour les styles des boutons de navigation pour indiquer visuellement quelle page est actuellement sélectionnée.

.NOTES
    Name:        Invoke-UpdateNavigationButtonStyle.ps1
    Author:      Mickaël CHAVE
    Created:     04/06/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Invoke-UpdateNavigationButtonStyle -selectedTag "homePage"
#>
function Invoke-UpdateNavigationButtonStyle {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$selectedTag
    )
    
    process {
        try {
            Write-BucketLog -Data "Updating navigation button styles for: $selectedTag" -Level Debug
            
            # Reference to all navigation buttons
            $navButtons = @(
                $WPFNavHome,
                $WPFNavSelectImage,
                $WPFNavCustomization,
                $WPFNavApplications,
                $WPFNavDrivers,
                $WPFNavSettings,
                $WPFNavAbout
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
            foreach ($button in $navButtons) {
                if ($button) {
                    $button.Style = $defaultStyle
                }
            }
            
            # Set the selected button style based on the page tag
            switch ($selectedTag) {
                "homePage" { 
                    if ($WPFNavHome) { 
                        $WPFNavHome.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavHome" -Level Verbose
                    }
                }
                "selectImagePage" { 
                    if ($WPFNavSelectImage) { 
                        $WPFNavSelectImage.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavSelectImage" -Level Verbose
                    }
                }
                "customizationPage" { 
                    if ($WPFNavCustomization) { 
                        $WPFNavCustomization.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavCustomization" -Level Verbose
                    }
                }
                "applicationsPage" { 
                    if ($WPFNavApplications) { 
                        $WPFNavApplications.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavApplications" -Level Verbose
                    }
                }
                "driversPage" { 
                    if ($WPFNavDrivers) { 
                        $WPFNavDrivers.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavDrivers" -Level Verbose
                    }
                }
                "configPage" { 
                    if ($WPFNavSettings) { 
                        $WPFNavSettings.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavSettings" -Level Verbose
                    }
                }
                "aboutPage" { 
                    if ($WPFNavAbout) { 
                        $WPFNavAbout.Style = $selectedStyle 
                        Write-BucketLog -Data "Set selected style for: NavAbout" -Level Verbose
                    }
                }
                default {
                    Write-BucketLog -Data "Unknown page tag: $selectedTag" -Level Warning
                }
            }
            
            Write-BucketLog -Data "Navigation button styles updated successfully" -Level Debug
        }
        catch {
            Write-BucketLog -Data "Failed to update navigation button styles: $_" -Level Error
        }
    }
}
