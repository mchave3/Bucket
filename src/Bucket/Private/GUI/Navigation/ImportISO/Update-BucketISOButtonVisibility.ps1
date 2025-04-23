<#
.SYNOPSIS
    Updates the visibility of navigation buttons in the Bucket ISO import wizard

.DESCRIPTION
    Controls which buttons are visible on each page of the Bucket ISO import wizard
    Enables/disables buttons based on the current step in the process

.NOTES
    Name:        Update-BucketISOButtonVisibility.ps1
    Author:      Mickaël CHAVE
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
        #region Button Visibility Logic
        if ($PSCmdlet.ShouldProcess("Navigation buttons", "Update visibility for '$CurrentPage' page")) {
            # Set default visibility for all navigation buttons
            $previousVisible = $true
            $nextVisible = $true
            $summaryVisible = $true
            $cancelVisible = $true

            # Adjust visibility based on the current page
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
                    $WPF_ImportISO_MainWindow_CancelButton.Content = "Close"
                }
                default {
                    $WPF_ImportISO_MainWindow_CancelButton.Content = "Cancel"
                }
            }

            # Apply visibility settings to UI
            $WPF_ImportISO_MainWindow_PreviousButton.Visibility = if ($previousVisible) { "Visible" } else { "Collapsed" }
            $WPF_ImportISO_MainWindow_NextButton.Visibility = if ($nextVisible) { "Visible" } else { "Collapsed" }
            $WPF_ImportISO_MainWindow_SummaryButton.Visibility = if ($summaryVisible) { "Visible" } else { "Collapsed" }
            $WPF_ImportISO_MainWindow_CancelButton.Visibility = if ($cancelVisible) { "Visible" } else { "Collapsed" }
        }
        #endregion
    }
}
