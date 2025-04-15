<#
.SYNOPSIS
    Updates the navigation button style based on the current page

.DESCRIPTION
    This function updates the navigation button style to visually indicate
    which page is currently selected in the navigation menu.

.NOTES
    Name:        Update-BucketNavButtonStyle.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Update-BucketNavButtonStyle -PageTag "homePage"
#>
function Update-BucketNavButtonStyle {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag
    )

    process {
        if ($PSCmdlet.ShouldProcess("Navigation button for page $PageTag", "Update style")) {
            try {
                Write-BucketLog -Data "Updating navigation button style for: $PageTag" -Level Debug

                # Reference to all navigation buttons
                $navButtons = @(
                    $WPF_MainWindow_NavHome,
                    $WPF_MainWindow_NavSelectImage,
                    $WPF_MainWindow_NavAbout
                )

                # Try to get styles from form resources
                $defaultStyle = $form.FindResource("MenuButtonStyle")
                $selectedStyle = $form.FindResource("SelectedMenuButtonStyle")

                # If styles are not found, try to get them from the main window
                if ((-not $defaultStyle -or -not $selectedStyle) -and $WPF_MainWindow) {
                    $defaultStyle = $WPF_MainWindow.FindResource("MenuButtonStyle")
                    $selectedStyle = $WPF_MainWindow.FindResource("SelectedMenuButtonStyle")
                }

                # If styles are still not found, log warning and exit
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
                switch ($PageTag) {
                    "homePage" {
                        if ($WPF_MainWindow_NavHome) {
                            $WPF_MainWindow_NavHome.Style = $selectedStyle
                            Write-BucketLog -Data "Set selected style for: NavHome" -Level Verbose
                        }
                    }
                    "selectImagePage" {
                        if ($WPF_MainWindow_NavSelectImage) {
                            $WPF_MainWindow_NavSelectImage.Style = $selectedStyle
                            Write-BucketLog -Data "Set selected style for: NavSelectImage" -Level Verbose
                        }
                    }
                    "aboutPage" {
                        if ($WPF_MainWindow_NavAbout) {
                            $WPF_MainWindow_NavAbout.Style = $selectedStyle
                            Write-BucketLog -Data "Set selected style for: NavAbout" -Level Verbose
                        }
                    }
                    default {
                        Write-BucketLog -Data "Unknown page tag: $PageTag" -Level Warning
                    }
                }

                Write-BucketLog -Data "Navigation button style updated successfully" -Level Debug
            }
            catch {
                Write-BucketLog -Data "Failed to update navigation button style: $_" -Level Error
            }
        }
    }
}
