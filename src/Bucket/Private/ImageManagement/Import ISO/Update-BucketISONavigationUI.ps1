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
        #region Navigation UI Styling
        # Define default and active styles for sidebar navigation
        $defaultStyle = @{ FontWeight = "Normal"; Foreground = "#555555" }
        $activeStyle = @{ FontWeight = "Bold"; Foreground = "#0078D7" }

        # Find sidebar panel and update styles
        Write-BucketLog -Data "[ISO Import] Updating navigation UI for page: $CurrentPage" -Level Debug
        $stackPanel = $form.FindName("MainWindow_ImportISO_SidebarPanel")
        if ($stackPanel) {
            Write-BucketLog -Data "[ISO Import] Found sidebar panel with $($stackPanel.Children.Count) children" -Level Debug
            $targetText = switch ($CurrentPage) {
                "dataSourcePage" { "Data Source" }
                "selectIndexPage" { "Select index" }
                "summaryPage" { "Summary" }
                "progressPage" { "Progress" }
                "completionPage" { "Completion" }
                default { $null }
            }
            Write-BucketLog -Data "[ISO Import] Looking for TextBlock with text: '$targetText'" -Level Debug
            if ($targetText -and $PSCmdlet.ShouldProcess("Navigation UI", "Update sidebar styles to highlight '$targetText'")) {
                # Reset all sidebar elements to default style
                foreach ($element in $stackPanel.Children) {
                    if ($element -is [System.Windows.Controls.TextBlock]) {
                        Write-BucketLog -Data "[ISO Import] Found TextBlock with text: '$($element.Text)'" -Level Verbose
                        $element.FontWeight = $defaultStyle.FontWeight
                        $element.Foreground = New-Object System.Windows.Media.SolidColorBrush ([System.Windows.Media.ColorConverter]::ConvertFromString($defaultStyle.Foreground))
                    }
                }
                # Apply active style to the current page
                $found = $false
                foreach ($element in $stackPanel.Children) {
                    if ($element -is [System.Windows.Controls.TextBlock] -and $element.Text -eq $targetText) {
                        $found = $true
                        Write-BucketLog -Data "[ISO Import] Applied active style to TextBlock with text: '$targetText'" -Level Debug
                        $element.FontWeight = $activeStyle.FontWeight
                        $element.Foreground = New-Object System.Windows.Media.SolidColorBrush ([System.Windows.Media.ColorConverter]::ConvertFromString($activeStyle.Foreground))
                        break
                    }
                }
                if (-not $found) {
                    Write-BucketLog -Data "[ISO Import] No TextBlock found with text: '$targetText' for page: $CurrentPage" -Level Warning
                }
            }
        }
        else {
            Write-BucketLog -Data "[ISO Import] Could not find MainWindow_ImportISO_SidebarPanel" -Level Warning
        }
        #endregion
    }
}
