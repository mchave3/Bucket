<#
.SYNOPSIS
    Updates the summary label and DataContext for the Select Index page in the ISO import wizard.
.DESCRIPTION
    Updates the summary label to reflect the number of selected editions and synchronizes the selected indices in the DataContext.
.NOTES
    Name:        Update-BucketISOSelectIndexSummary.ps1
    Author:      Mickaël CHAVE
    Created:     04/22/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License
.LINK
    https://github.com/mchave3/Bucket
.EXAMPLE
    Update-BucketISOSelectIndexSummary -DataGrid $dataGrid -SummaryLabel $summaryLabel
#>
function Update-BucketISOSelectIndexSummary {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [System.Windows.Controls.DataGrid]$DataGrid,
        [Parameter(Mandatory = $true)]
        [System.Windows.Controls.TextBlock]$SummaryLabel
    )
    process {
        $selected = @($DataGrid.ItemsSource | Where-Object { $_.Include })
        $count = $selected.Count
        $SummaryLabel.Text = if ($count -eq 0) {
            "All editions will be included in the final WIM."
        }
        else {
            "$count edition(s) selected."
        }
        $script:ImportISO_DataContext.SelectedIndices = $selected
        Write-BucketLog -Data "[ISO Import] Selected indices: $($selected | ForEach-Object { $_.Index })" -Level Debug
    }
}
