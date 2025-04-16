<#
.SYNOPSIS
    Updates the navigation UI elements to highlight the current step in ISO wizard

.DESCRIPTION
    Applies visual styling to the sidebar elements to indicate the current step
    in the Bucket ISO import process

.NOTES
    Name:        Update-BucketISONavigationUI.ps1
    Author:      Mickau00ebl CHAVE
    Created:     04/16/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Update-BucketISONavigationUI -CurrentPage "dataSourcePage"
#>
function Update-BucketISONavigationUI {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CurrentPage
    )

    process {
        # Default style for all steps
        $defaultStyle = @{
            FontWeight = "Normal"
            Foreground = "#555555"
        }

        # Style for the current step
        $activeStyle = @{
            FontWeight = "Bold"
            Foreground = "#0078D7"
        }

        # Find all sidebar TextBlock elements and update their styles
        $stackPanel = $form.FindName("MainWindow_ImportISO_SidebarPanel")
        if ($stackPanel) {
            $targetText = switch ($CurrentPage) {
                "dataSourcePage" { "Data Source" }
                "selectIndexPage" { "Select index" }
                "summaryPage" { "Summary" }
                "progressPage" { "Progress" }
                "completionPage" { "Completion" }
                default { $null }
            }

            if ($targetText -and $PSCmdlet.ShouldProcess("Navigation UI", "Update sidebar styles to highlight '$targetText'")) {
                # Reset all elements to default style
                foreach ($element in $stackPanel.Children) {
                    if ($element -is [System.Windows.Controls.TextBlock]) {
                        $element.FontWeight = $defaultStyle.FontWeight
                        $element.Foreground = New-Object System.Windows.Media.SolidColorBrush ([System.Windows.Media.ColorConverter]::ConvertFromString($defaultStyle.Foreground))
                    }
                }

                # Apply active style to the current page text
                foreach ($element in $stackPanel.Children) {
                    if ($element -is [System.Windows.Controls.TextBlock] -and $element.Text -eq $targetText) {
                        $element.FontWeight = $activeStyle.FontWeight
                        $element.Foreground = New-Object System.Windows.Media.SolidColorBrush ([System.Windows.Media.ColorConverter]::ConvertFromString($activeStyle.Foreground))
                        break
                    }
                }
            }
        }
    }
}
