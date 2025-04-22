# filepath: e:\Bucket\src\Bucket\Private\ImageManagement\Import ISO\Initialize-BucketISO_SelectIndexPage.ps1
<#
.SYNOPSIS
    Initializes the Select Index page for the ISO import wizard.
.DESCRIPTION
    Sets up event handlers, populates the DataGrid with available WIM indices, and manages selection logic for exporting selected indices to the final WIM.
.NOTES
    Name:        Initialize-BucketISO_SelectIndexPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/22/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License
.LINK
    https://github.com/mchave3/Bucket
.EXAMPLE
    Initialize-BucketISO_SelectIndexPage
#>
function Initialize-BucketISO_SelectIndexPage {
    [CmdletBinding()]
    param()

    process {
        #region Initialization
        Write-BucketLog -Data "[ISO Import] Initializing Select Index page" -Level Info
        #endregion

        #region Page Logic
        $pageLoadedHandler = {
            param($senderObj, $e)

            # Store page reference for event access
            $script:importISOCurrentPage = $senderObj
            Write-BucketLog -Data "[ISO Import] Select index page loaded" -Level Info

            # Retrieve UI controls
            $dataGrid = $senderObj.FindName("ImportISO_SelectIndex_DataGrid")
            $summaryLabel = $senderObj.FindName("ImportISO_SelectIndex_SummaryLabel")
            $waitOverlay = $senderObj.FindName("ImportISO_SelectIndex_WaitOverlay")
            $helpText = $senderObj.FindName("ImportISO_SelectIndex_HelpText")
            $selectAllButton = $senderObj.FindName("ImportISO_SelectIndex_SelectAllButton")
            $deselectAllButton = $senderObj.FindName("ImportISO_SelectIndex_DeselectAllButton")

            # Log control discovery results
            Write-BucketLog -Data "[ISO Import] Control 'ImportISO_SelectIndex_DataGrid' found: $($null -ne $dataGrid)" -Level Debug
            Write-BucketLog -Data "[ISO Import] Control 'ImportISO_SelectIndex_SummaryLabel' found: $($null -ne $summaryLabel)" -Level Debug
            Write-BucketLog -Data "[ISO Import] Control 'ImportISO_SelectIndex_WaitOverlay' found: $($null -ne $waitOverlay)" -Level Debug
            Write-BucketLog -Data "[ISO Import] Control 'ImportISO_SelectIndex_HelpText' found: $($null -ne $helpText)" -Level Debug
            Write-BucketLog -Data "[ISO Import] Control 'ImportISO_SelectIndex_SelectAllButton' found: $($null -ne $selectAllButton)" -Level Debug
            Write-BucketLog -Data "[ISO Import] Control 'ImportISO_SelectIndex_DeselectAllButton' found: $($null -ne $deselectAllButton)" -Level Debug

            #region Show Waiting Overlay and Disable Navigation
            if ($waitOverlay) { $waitOverlay.Visibility = "Visible" }
            if ($dataGrid) { $dataGrid.Visibility = "Collapsed" }
            if ($selectAllButton) { $selectAllButton.IsEnabled = $false }
            if ($deselectAllButton) { $deselectAllButton.IsEnabled = $false }
            if ($summaryLabel) { $summaryLabel.Text = "" }
            if ($helpText) { $helpText.Text = "Loading Windows editions from ISO..." }
            #endregion

            #region Extract WIM Indices
            try {
                $isoPath = $script:ImportISODataContext.ISOSourcePath
                $indices = Get-BucketWimIndex -IsoPath $isoPath
                $script:ImportISODataContext.AvailableIndices = $indices

                if ($dataGrid) {
                    $dataGrid.ItemsSource = $indices
                    $dataGrid.IsEnabled = $true
                    $dataGrid.Visibility = "Visible"
                }

                if ($waitOverlay) { $waitOverlay.Visibility = "Collapsed" }
                if ($selectAllButton) { $selectAllButton.IsEnabled = $true }
                if ($deselectAllButton) { $deselectAllButton.IsEnabled = $true }
                if ($helpText) { $helpText.Text = "If no edition is selected, all editions will be included in the final WIM." }

                Write-BucketLog -Data "[ISO Import] Populated DataGrid with $($indices.Count) WIM indices" -Level Debug

                # Update the summary text
                if ($dataGrid -and $summaryLabel) {
                    Update-BucketISOSelectIndexSummary -DataGrid $dataGrid -SummaryLabel $summaryLabel
                }
            }
            catch {
                Write-BucketLog -Data "[ISO Import] Error extracting WIM indices: $_" -Level Error
                if ($waitOverlay) { $waitOverlay.Visibility = "Collapsed" }
                if ($helpText) { $helpText.Text = "Error extracting WIM indices: $_" }
            }
            #endregion

            #region Event Handlers
            # Create script:level variables for event handlers to access
            $script:importISO_DataGrid = $dataGrid
            $script:importISO_SummaryLabel = $summaryLabel

            # Setup DataGrid selection changed event
            if ($dataGrid) {
                $dataGrid.Add_SelectionChanged({
                        param($senderObj, $e)
                        Update-BucketISOSelectIndexSummary -DataGrid $script:importISO_DataGrid -SummaryLabel $script:importISO_SummaryLabel
                    })
            }

            # Setup Select All button click event
            if ($selectAllButton) {
                $selectAllButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[ISO Import] Select All button clicked" -Level Info
                        foreach ($item in $script:importISO_DataGrid.Items) {
                            $item.Include = $true
                        }
                        Update-BucketISOSelectIndexSummary -DataGrid $script:importISO_DataGrid -SummaryLabel $script:importISO_SummaryLabel
                    })
            }

            # Setup Deselect All button click event
            if ($deselectAllButton) {
                $deselectAllButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[ISO Import] Deselect All button clicked" -Level Info
                        foreach ($item in $script:importISO_DataGrid.Items) {
                            $item.Include = $false
                        }
                        Update-BucketISOSelectIndexSummary -DataGrid $script:importISO_DataGrid -SummaryLabel $script:importISO_SummaryLabel
                    })
            }
            #endregion
        }
        #endregion

        #region DataContext & Navigation
        # Ensure SelectedIndices property exists in the data context
        if (-not (Get-Member -InputObject $script:ImportISODataContext -Name "SelectedIndices" -MemberType NoteProperty)) {
            $script:ImportISODataContext | Add-Member -MemberType NoteProperty -Name "SelectedIndices" -Value @()
            Write-BucketLog -Data "[ISO Import] SelectedIndices property initialized in ImportISODataContext" -Level Debug
        }

        $dataContext = [PSCustomObject]@{
            AvailableIndices = $script:ImportISODataContext.AvailableIndices
            SelectedIndices  = $script:ImportISODataContext.SelectedIndices
            PageLoaded       = $pageLoadedHandler
        }
        Write-BucketLog -Data "[ISO Import] Data context for select index page created" -Level Debug

        Invoke-BucketNavigationService -PageTag "selectIndexPage" `
            -RootFrame $WPF_MainWindow_ImportISO_MainFrame `
            -XamlBasePath (Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement") `
            -PageDictionary $script:ImportISOPages `
            -DataContext $dataContext `
            -GlobalDataContext $script:ImportISODataContext
        Write-BucketLog -Data "[ISO Import] Select index page navigation started" -Level Debug
        #endregion
    }
}
