<#
.SYNOPSIS
    Updates the visibility of navigation buttons in the Bucket ISO import wizard

.DESCRIPTION
    Controls which buttons are visible on each page of the Bucket ISO import wizard
    Enables/disables buttons based on the current step in the process

.NOTES
    Name:        Update-BucketISOButtonVisibility.ps1
    Author:      Mickau00ebl CHAVE
    Created:     04/16/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Update-BucketISOButtonVisibility -CurrentPage "dataSourcePage"
#>
function Update-BucketISOButtonVisibility {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CurrentPage
    )

    process {
        if ($PSCmdlet.ShouldProcess("Navigation buttons", "Update visibility for '$CurrentPage' page")) {
            # Default visibility for buttons
            $previousVisible = $true
            $nextVisible = $true
            $summaryVisible = $true
            $cancelVisible = $true

            # Adjust visibility based on current page
            switch ($CurrentPage) {
                "dataSourcePage" {
                    $previousVisible = $false
                    $summaryVisible = $false
                }
                "progressPage" {
                    $previousVisible = $false
                    $nextVisible = $false
                    $summaryVisible = $false
                }
                "completionPage" {
                    $previousVisible = $false
                    $nextVisible = $false
                    # Rename Cancel button to "Close" on completion page
                    $WPF_MainWindow_ImportISO_CancelButton.Content = "Close"
                }
                default {
                    # Reset Cancel button text for other pages
                    $WPF_MainWindow_ImportISO_CancelButton.Content = "Cancel"
                }
            }

            # Apply visibility settings
            $WPF_MainWindow_ImportISO_PreviousButton.Visibility = if ($previousVisible) { "Visible" } else { "Collapsed" }
            $WPF_MainWindow_ImportISO_NextButton.Visibility = if ($nextVisible) { "Visible" } else { "Collapsed" }
            $WPF_MainWindow_ImportISO_SummaryButton.Visibility = if ($summaryVisible) { "Visible" } else { "Collapsed" }
            $WPF_MainWindow_ImportISO_CancelButton.Visibility = if ($cancelVisible) { "Visible" } else { "Collapsed" }
        }
    }
}
