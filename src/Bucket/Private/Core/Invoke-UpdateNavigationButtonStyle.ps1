<#
.SYNOPSIS
    Brief description of the script purpose

.DESCRIPTION
    Detailed description of what the script/function does

.NOTES
    Name:        ScriptName.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Example of how to use this script/function
#>
function Invoke-UpdateNavigationButtonStyle {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$selectedTag
    )
    
    process {
        # Reset all navigation buttons to default style
        $navButtons = @(
            $WPFNavHome,
            $WPFNavSelectImage,
            $WPFNavCustomization,
            $WPFNavApplications,
            $WPFNavDrivers,
            $WPFNavSettings,
            $WPFNavAbout
        )
        
        $defaultStyle = $form.FindResource("MenuButtonStyle")
        $selectedStyle = $form.FindResource("SelectedMenuButtonStyle")
        
        foreach ($button in $navButtons) {
            if ($button) {
                $button.Style = $defaultStyle
            }
        }
        
        # Set the selected button style
        switch ($selectedTag) {
            "homePage" { if ($WPFNavHome) { $WPFNavHome.Style = $selectedStyle } }
            "selectImagePage" { if ($WPFNavSelectImage) { $WPFNavSelectImage.Style = $selectedStyle } }
            "customizationPage" { if ($WPFNavCustomization) { $WPFNavCustomization.Style = $selectedStyle } }
            "applicationsPage" { if ($WPFNavApplications) { $WPFNavApplications.Style = $selectedStyle } }
            "driversPage" { if ($WPFNavDrivers) { $WPFNavDrivers.Style = $selectedStyle } }
            "configPage" { if ($WPFNavSettings) { $WPFNavSettings.Style = $selectedStyle } }
            "aboutPage" { if ($WPFNavAbout) { $WPFNavAbout.Style = $selectedStyle } }
        }
    }
}
