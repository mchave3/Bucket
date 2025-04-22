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
        #region Page Initialization
        Write-BucketLog -Data "[ISO Import] Initializing Select Index page" -Level Info

        # Create a page loaded handler that will be attached to the page's Loaded event
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
            # Setup DataGrid selection changed event
            if ($dataGrid) {
                $dataGrid.Add_SelectionChanged({
                        param($senderObj, $e)
                        Update-BucketISOSelectIndexSummary -DataGrid $dataGrid -SummaryLabel $summaryLabel
                    })
            }

            # Setup Select All button click event
            if ($selectAllButton) {
                $selectAllButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[ISO Import] Select All button clicked" -Level Info
                        foreach ($item in $dataGrid.Items) {
                            $item.Include = $true
                        }
                        Update-BucketISOSelectIndexSummary -DataGrid $dataGrid -SummaryLabel $summaryLabel
                    })
            }

            # Setup Deselect All button click event
            if ($deselectAllButton) {
                $deselectAllButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[ISO Import] Deselect All button clicked" -Level Info
                        foreach ($item in $dataGrid.Items) {
                            $item.Include = $false
                        }
                        Update-BucketISOSelectIndexSummary -DataGrid $dataGrid -SummaryLabel $summaryLabel
                    })
            }
            #endregion
        }

        # Attach the page loaded handler to the page
        try {
            if ($WPF_MainWindow_ImportISO_MainFrame) {
                $currentContent = $WPF_MainWindow_ImportISO_MainFrame.Content

                if ($currentContent) {
                    Write-BucketLog -Data "[ISO Import] Found current page content, attaching Loaded handler" -Level Debug

                    try {
                        # Try to add the event handler using Add_Loaded method directly
                        $currentContent.Add_Loaded($pageLoadedHandler)
                        Write-BucketLog -Data "[ISO Import] Successfully attached Loaded handler using Add_Loaded method" -Level Debug

                        # If the page is already loaded, invoke the handler directly
                        if ($currentContent.IsLoaded) {
                            Write-BucketLog -Data "[ISO Import] Page already loaded, invoking handler directly" -Level Debug
                            $pageLoadedHandler.Invoke($currentContent, $null)
                        }
                    }
                    catch {
                        Write-BucketLog -Data "[ISO Import] Failed to attach Loaded handler, falling back to direct invocation: $_" -Level Warning
                        # If we can't attach the event handler, invoke it directly
                        $pageLoadedHandler.Invoke($currentContent, $null)
                    }
                }
                else {
                    Write-BucketLog -Data "[ISO Import] No current content in frame, waiting for ContentRendered event" -Level Warning

                    # Add handler for when content is rendered
                    try {
                        $WPF_MainWindow_ImportISO_MainFrame.Add_ContentRendered({
                                param($sender, $e)
                                $content = $sender.Content
                                if ($content) {
                                    Write-BucketLog -Data "[ISO Import] Frame ContentRendered, attaching Loaded handler" -Level Debug
                                    try {
                                        $content.Add_Loaded($pageLoadedHandler)
                                    }
                                    catch {
                                        Write-BucketLog -Data "[ISO Import] Failed to attach Loaded handler to content: $_" -Level Warning
                                        # Still try to invoke the handler directly
                                        $pageLoadedHandler.Invoke($content, $null)
                                    }
                                }
                            })
                        Write-BucketLog -Data "[ISO Import] Successfully attached ContentRendered handler to frame" -Level Debug
                    }
                    catch {
                        Write-BucketLog -Data "[ISO Import] Failed to attach ContentRendered handler to frame: $_" -Level Error
                    }
                }
            }
            else {
                Write-BucketLog -Data "[ISO Import] Main frame not found, cannot attach event handler" -Level Error
            }
        }
        catch {
            Write-BucketLog -Data "[ISO Import] Error attaching page loaded handler: $_" -Level Error
        }
        #endregion Page Initialization
    }
}
