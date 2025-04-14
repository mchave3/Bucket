<#
.SYNOPSIS
    Invokes the Bucket Select Image Page in the GUI interface

.DESCRIPTION
    This function displays the select image page of the Bucket application.
    It renders the UI layout for the select image page including navigation elements,
    statistics, and recent activities.

.NOTES
    Name:        Invoke-BucketSelectImagePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/11/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Invoke-BucketSelectImagePage
#>
function Invoke-BucketSelectImagePage {
    [CmdletBinding()]
    param(

    )    
    
    process {
        # Initialize image data arrays
        $images = @()
        $imageDetails = @()
        
        # Load existing images (if available)
        try {
            # TODO: Replace with actual image loading logic
            # This is just sample data for now
            $images = @(
                [PSCustomObject]@{
                    SelectImage_Images_IsSelected = $false
                    SelectImage_Images_Name       = "Windows 11 Enterprise"
                    SelectImage_Images_Path       = "C:\Images\Windows11.wim"
                    SelectImage_Images_Version    = "22H2"
                    SelectImage_Images_Size       = "4500"
                    SelectImage_Images_ImportDate = (Get-Date).AddDays(-5).ToString("yyyy-MM-dd")
                },
                [PSCustomObject]@{
                    SelectImage_Images_IsSelected = $false
                    SelectImage_Images_Name       = "Windows 10 Pro"
                    SelectImage_Images_Path       = "C:\Images\Windows10.wim"
                    SelectImage_Images_Version    = "21H2"
                    SelectImage_Images_Size       = "3800"
                    SelectImage_Images_ImportDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
                }
            )
            
            # Sample image details for the first image
            $imageDetails = @(
                [PSCustomObject]@{
                    SelectImage_Details_Index        = "1"
                    SelectImage_Details_Name         = "Windows 11 Enterprise"
                    SelectImage_Details_Architecture = "x64"
                    SelectImage_Details_Edition      = "Enterprise"
                    SelectImage_Details_Version      = "22H2 (10.0.22621)"
                    SelectImage_Details_Languages    = "en-US"
                    SelectImage_Details_Description  = "Windows 11 Enterprise Edition"
                    SelectImage_Details_Size         = "4500"
                }
            )
        }
        catch {
            Write-BucketLog -Data "Error loading image data: $_" -Level "Error"
        }
        
        # Create a scriptblock to set up event handlers after page is loaded
        $pageLoadedHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "SelectImagePage loaded, setting up handlers" -Level "Info"
            
            # Get the page itself
            $page = $sender
            
            # Get references to UI elements
            $importISOButton = $page.FindName("SelectImage_ImportISOButton")
            $importWIMButton = $page.FindName("SelectImage_ImportWIMButton")
            $refreshButton = $page.FindName("SelectImage_RefreshButton")
            $deleteButton = $page.FindName("SelectImage_DeleteButton")
            $selectAllCheckbox = $page.FindName("SelectImage_SelectAllCheckbox")
            $nextButton = $page.FindName("SelectImage_NextButton")
            $previousButton = $page.FindName("SelectImage_PreviousButton")
            $skipButton = $page.FindName("SelectImage_SkipButton")
            $summaryButton = $page.FindName("SelectImage_SummaryButton")
            $imagesDataGrid = $page.FindName("SelectImage_ImagesDataGrid")
            $detailsDataGrid = $page.FindName("SelectImage_DetailsDataGrid")
            
            # Set data sources directly
            if ($imagesDataGrid) {
                $imagesDataGrid.ItemsSource = $images
                $imagesDataGrid.Add_SelectionChanged({
                        param($sender, $e)
                        
                        $selectedItem = $sender.SelectedItem
                        if ($null -ne $selectedItem) {
                            Write-BucketLog -Data "Selected image: $($selectedItem.SelectImage_Images_Name)" -Level "Info"
                            # For now, just use the same image details
                            # In a real implementation, you would load actual details
                        }
                    })
            }
            
            if ($detailsDataGrid) {
                $detailsDataGrid.ItemsSource = $imageDetails
            }
            
            # Set up button event handlers
            if ($importISOButton) {
                $importISOButton.Add_Click({
                        param($sender, $e)
                        
                        Write-BucketLog -Data "Import ISO button clicked" -Level "Info"
                        # Show file dialog to select ISO file
                        $dialog = New-Object Microsoft.Win32.OpenFileDialog
                        $dialog.Filter = "ISO files (*.iso)|*.iso"
                        $dialog.Title = "Select ISO file"
                        
                        if ($dialog.ShowDialog()) {
                            $isoPath = $dialog.FileName
                            Write-BucketLog -Data "Selected ISO: $isoPath" -Level "Info"
                        }
                    })
            }
            
            if ($importWIMButton) {
                $importWIMButton.Add_Click({
                        param($sender, $e)
                        
                        Write-BucketLog -Data "Import WIM button clicked" -Level "Info"
                        # Show file dialog to select WIM file
                        $dialog = New-Object Microsoft.Win32.OpenFileDialog
                        $dialog.Filter = "WIM files (*.wim)|*.wim"
                        $dialog.Title = "Select WIM file"
                        
                        if ($dialog.ShowDialog()) {
                            $wimPath = $dialog.FileName
                            Write-BucketLog -Data "Selected WIM: $wimPath" -Level "Info"
                        }
                    })
            }
            
            if ($refreshButton) {
                $refreshButton.Add_Click({
                        param($sender, $e)
                        Write-BucketLog -Data "Refresh button clicked" -Level "Info"
                    })
            }
            
            if ($deleteButton) {
                $deleteButton.Add_Click({
                        param($sender, $e)
                        Write-BucketLog -Data "Delete button clicked" -Level "Info"
                    })
            }
            
            if ($selectAllCheckbox) {
                $selectAllCheckbox.Add_Checked({
                        param($sender, $e)
                        $isChecked = $sender.IsChecked
                        Write-BucketLog -Data "Select All checkbox changed to $isChecked" -Level "Info"
                        
                        if ($imagesDataGrid -and $imagesDataGrid.ItemsSource) {
                            foreach ($image in $imagesDataGrid.ItemsSource) {
                                $image.SelectImage_Images_IsSelected = $true
                            }
                            $imagesDataGrid.Items.Refresh()
                        }
                    })
                
                $selectAllCheckbox.Add_Unchecked({
                        param($sender, $e)
                        $isChecked = $sender.IsChecked
                        Write-BucketLog -Data "Select All checkbox changed to $isChecked" -Level "Info"
                        
                        if ($imagesDataGrid -and $imagesDataGrid.ItemsSource) {
                            foreach ($image in $imagesDataGrid.ItemsSource) {
                                $image.SelectImage_Images_IsSelected = $false
                            }
                            $imagesDataGrid.Items.Refresh()
                        }
                    })
            }
            
            if ($nextButton) {
                $nextButton.Add_Click({
                        param($sender, $e)
                        Write-BucketLog -Data "Next button clicked" -Level "Info"
                        # TODO: Add navigation logic
                    })
            }
            
            if ($previousButton) {
                $previousButton.Add_Click({
                        param($sender, $e)
                        Write-BucketLog -Data "Previous button clicked" -Level "Info"
                        # TODO: Add navigation logic
                    })
            }
            
            if ($skipButton) {
                $skipButton.Add_Click({
                        param($sender, $e)
                        Write-BucketLog -Data "Skip button clicked" -Level "Info"
                        # TODO: Add navigation logic
                    })
            }
            
            if ($summaryButton) {
                $summaryButton.Add_Click({
                        param($sender, $e)
                        Write-BucketLog -Data "Summary button clicked" -Level "Info"
                        # TODO: Add navigation logic
                    })
            }
            
            Write-BucketLog -Data "SelectImagePage event handlers configured successfully" -Level "Info"
        }
        
        # Create a custom data context
        $customDataContext = [PSCustomObject]@{
            # Image data for initial binding
            SelectImage_Images  = $images
            SelectImage_Details = $imageDetails
            
            # PageLoaded handler
            PageLoaded          = $pageLoadedHandler
        }
        
        # Navigate to the Select Image page with our custom data context
        Write-BucketLog -Data "Navigating to SelectImagePage with data context" -Level "Info"
        Invoke-BucketGuiNav -PageTag "selectImagePage" -DataContext $customDataContext
    }
}