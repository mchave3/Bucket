<#
.SYNOPSIS
    Initializes the Select Index page for the ISO import wizard.

.DESCRIPTION
    Sets up event handlers, populates the DataGrid with available WIM indices, and manages selection logic for exporting selected indices to the final WIM.

.NOTES
    Name:        Initialize-BucketISO_SelectIndexPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/22/2025
    Version:     25.6.11.2
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

    begin {
        # Import required namespace for UI refresh
        Add-Type -AssemblyName System.Windows.Forms
    }

    process {
        #region Initialization
        Write-BucketLog -Data "Initializing Select Index page" -Level Info
        #endregion

        #region DataContext & Navigation
        # Ensure SelectedIndices property exists in the data context
        if (-not (Get-Member -InputObject $script:ImportISO_DataContext -Name "SelectedIndices" -MemberType NoteProperty)) {
            $script:ImportISO_DataContext | Add-Member -MemberType NoteProperty -Name "SelectedIndices" -Value @()
            Write-BucketLog -Data "SelectedIndices property initialized in ImportISODataContext" -Level Debug
        }

        # Create a modified page loaded handler that shows waiting overlay immediately
        $pageLoadedHandler = {
            param($senderObj, $e)

            # Store page reference for event access
            $script:importISOCurrentPage = $senderObj
            Write-BucketLog -Data "Select index page loaded" -Level Info

            # Get required UI elements
            $dataGrid = $senderObj.FindName("ImportISO_SelectIndex_DataGrid")
            $summaryLabel = $senderObj.FindName("ImportISO_SelectIndex_SummaryLabel")
            $selectAllButton = $senderObj.FindName("ImportISO_SelectIndex_SelectAllButton")
            $deselectAllButton = $senderObj.FindName("ImportISO_SelectIndex_DeselectAllButton")
            $helpTextElement = $senderObj.FindName("ImportISO_SelectIndex_HelpText")

            #region Extract WIM Indices using Background Task
            $isoPath = $script:ImportISO_DataContext.ISOSourcePath

            # Execute WIM index extraction as background job
            Invoke-BucketBackgroundTask -ScriptBlock {
                Get-BucketWimIndex -IsoPath $using:isoPath
            } -TaskName "Extracting Windows editions" -OnSuccess {
                param($indices)
                Write-BucketLog -Data "OnSuccess callback started with $($indices.Count) indices" -Level Debug

                # Execute UI updates on the dispatcher thread
                $script:ImportISOCurrentPage.Dispatcher.Invoke([Action]{
                        try {
                            Write-BucketLog -Data "Updating UI on dispatcher thread" -Level Debug

                            # Store results in data context
                            $script:ImportISO_DataContext.AvailableIndices = $indices

                            # Get UI elements from current page
                            $currentPage = $script:ImportISOCurrentPage
                            $dataGridElement = $currentPage.FindName("ImportISO_SelectIndex_DataGrid")
                            $waitOverlayElement = $currentPage.FindName("ImportISO_SelectIndex_WaitOverlay")
                            $selectAllButtonElement = $currentPage.FindName("ImportISO_SelectIndex_SelectAllButton")
                            $helpTextElement = $currentPage.FindName("ImportISO_SelectIndex_HelpText")
                            $summaryLabelElement = $currentPage.FindName("ImportISO_SelectIndex_SummaryLabel")

                            Write-BucketLog -Data "Found UI elements - DataGrid:$($null -ne $dataGridElement) WaitOverlay:$($null -ne $waitOverlayElement)" -Level Debug

                            # Update UI elements
                            if ($dataGridElement) {
                                $dataGridElement.ItemsSource = $indices
                                $dataGridElement.Visibility = "Visible"
                                Write-BucketLog -Data "DataGrid updated with $($indices.Count) items" -Level Debug
                            }
                            if ($waitOverlayElement) {
                                $waitOverlayElement.Visibility = "Collapsed"
                                Write-BucketLog -Data "Wait overlay hidden" -Level Debug
                            }

                            # Show UI elements
                            if ($selectAllButtonElement) {
                                $selectAllButtonElement.Parent.Visibility = "Visible"
                            }
                            if ($summaryLabelElement) {
                                $summaryLabelElement.Visibility = "Visible"
                            }
                            if ($helpTextElement) {
                                $helpTextElement.Visibility = "Visible"
                            }

                            Write-BucketLog -Data "Populated DataGrid with $($indices.Count) WIM indices" -Level Info

                            # Update the summary text
                            if ($dataGridElement -and $summaryLabelElement) {
                                Update-BucketISOSelectIndexSummary -DataGrid $dataGridElement -SummaryLabel $summaryLabelElement
                            }
                        }
                        catch {
                            Write-BucketLog -Data "Error updating UI after background task: $_" -Level Error
                        }
                    })
            } -OnError {
                param($errorInfo)
                try {
                    Write-BucketLog -Data "Error extracting WIM indices: $($errorInfo.ErrorInfo)" -Level Error

                    # Get UI elements from current page
                    $currentPage = $script:ImportISOCurrentPage
                    $waitOverlayElement = $currentPage.FindName("ImportISO_SelectIndex_WaitOverlay")
                    $helpTextElement = $currentPage.FindName("ImportISO_SelectIndex_HelpText")
                    $dataGridElement = $currentPage.FindName("ImportISO_SelectIndex_DataGrid")
                    $selectAllButtonElement = $currentPage.FindName("ImportISO_SelectIndex_SelectAllButton")
                    $summaryLabelElement = $currentPage.FindName("ImportISO_SelectIndex_SummaryLabel")

                    # Hide overlay and show UI elements with error message
                    if ($waitOverlayElement) { $waitOverlayElement.Visibility = "Collapsed" }
                    if ($dataGridElement) { $dataGridElement.Visibility = "Visible" }
                    if ($selectAllButtonElement) { $selectAllButtonElement.Parent.Visibility = "Visible" }
                    if ($summaryLabelElement) { $summaryLabelElement.Visibility = "Visible" }
                    if ($helpTextElement) {
                        $helpTextElement.Visibility = "Visible"
                        $helpTextElement.Text = "Error extracting WIM indices. Please check the ISO file and try again."
                    }
                }
                catch {
                    Write-BucketLog -Data "Error handling background task failure: $_" -Level Error
                }
            }
            #endregion Extract WIM Indices

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
                        Write-BucketLog -Data "Select All button clicked" -Level Info
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
                        Write-BucketLog -Data "Deselect All button clicked" -Level Info
                        foreach ($item in $script:importISO_DataGrid.Items) {
                            $item.Include = $false
                        }
                        Update-BucketISOSelectIndexSummary -DataGrid $script:importISO_DataGrid -SummaryLabel $script:importISO_SummaryLabel
                    })
            }
            #endregion
        }

        $dataContext = [PSCustomObject]@{
            AvailableIndices = $script:ImportISO_DataContext.AvailableIndices
            SelectedIndices  = $script:ImportISO_DataContext.SelectedIndices
            PageLoaded       = $pageLoadedHandler
        }
        Write-BucketLog -Data "Data context for select index page created" -Level Debug

        Invoke-BucketNavigationService -PageTag "selectIndexPage" `
            -RootFrame $WPF_ImportISO_MainWindow_MainFrame `
            -XamlBasePath (Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement") `
            -PageDictionary $script:ImportISOPages `
            -DataContext $dataContext `
            -GlobalDataContext $script:ImportISO_DataContext
        Write-BucketLog -Data "Select index page navigation started" -Level Debug
        #endregion
    }
}
