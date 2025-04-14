<#
.SYNOPSIS
    Initializes the Select Image Page for the Bucket application

.DESCRIPTION
    This function initializes the Select Image Page, sets up its data context,
    and handles all event bindings for UI elements on the page including
    DataGrid functionality and button actions.

.NOTES
    Name:        Initialize-SelectImagePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-SelectImagePage
#>
function Initialize-SelectImagePage {
    [CmdletBinding()]
    param()
    
    process {
        Write-BucketLog -Data "Initializing Select Image Page" -Level Info
        
        # Initialize image data arrays
        $images = @()
        $imageDetails = @()
        
        # Load existing images (if available)
        try {
            # Check if we have a function to get images
            if (Get-Command -Name "Get-BucketImages" -ErrorAction SilentlyContinue) {
                $images = Get-BucketImages
            }
            else {
                # Sample data for development/testing
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
            }
            
            # Sample image details for the first image
            if (Get-Command -Name "Get-BucketImageDetails" -ErrorAction SilentlyContinue) {
                $imageDetails = Get-BucketImageDetails -ImagePath $images[0].SelectImage_Images_Path
            }
            else {
                # Sample data for development/testing
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
        }
        catch {
            Write-BucketLog -Data "Error loading image data: $_" -Level "Error"
        }
        
        # Create the page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)
            
            Write-BucketLog -Data "SelectImagePage loaded, setting up handlers" -Level "Info"
            
            # Get the page itself
            $page = $senderObj
            
            # Get references to UI elements
            $importISOButton   = $page.FindName("SelectImage_ImportISOButton")
            $importWIMButton   = $page.FindName("SelectImage_ImportWIMButton")
            $refreshButton     = $page.FindName("SelectImage_RefreshButton")
            $deleteButton      = $page.FindName("SelectImage_DeleteButton")
            $selectAllCheckbox = $page.FindName("SelectImage_SelectAllCheckbox")
            $nextButton        = $page.FindName("SelectImage_NextButton")
            $previousButton    = $page.FindName("SelectImage_PreviousButton")
            $skipButton        = $page.FindName("SelectImage_SkipButton")
            $summaryButton     = $page.FindName("SelectImage_SummaryButton")
            $imagesDataGrid    = $page.FindName("SelectImage_ImagesDataGrid")
            $detailsDataGrid   = $page.FindName("SelectImage_DetailsDataGrid")
            
            # Set data sources for DataGrids
            if ($imagesDataGrid) {
                # Create an ObservableCollection for better data binding
                $observableImages = New-Object System.Collections.ObjectModel.ObservableCollection[PSObject]
                foreach ($image in $images) {
                    $observableImages.Add($image)
                }
                
                $imagesDataGrid.ItemsSource = $observableImages
                
                # Handle selection changed event
                $imagesDataGrid.Add_SelectionChanged({
                        param($senderObj, $e)
                        
                        $selectedItem = $senderObj.SelectedItem
                        if ($null -ne $selectedItem) {
                            Write-BucketLog -Data "Selected image: $($selectedItem.SelectImage_Images_Name)" -Level "Info"
                            
                            # Update details DataGrid with information for the selected image
                            if ($detailsDataGrid -and (Get-Command -Name "Get-BucketImageDetails" -ErrorAction SilentlyContinue)) {
                                try {
                                    $details = Get-BucketImageDetails -ImagePath $selectedItem.SelectImage_Images_Path
                                    $detailsDataGrid.ItemsSource = $details
                                }
                                catch {
                                    Write-BucketLog -Data "Error loading image details: $_" -Level "Error"
                                }
                            }
                        }
                    })
            }
            
            if ($detailsDataGrid) {
                # Create an ObservableCollection for better data binding
                $observableDetails = New-Object System.Collections.ObjectModel.ObservableCollection[PSObject]
                foreach ($detail in $imageDetails) {
                    $observableDetails.Add($detail)
                }
                
                $detailsDataGrid.ItemsSource = $observableDetails
            }
            
            # Set up button event handlers
            if ($importISOButton) {
                $importISOButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Import ISO button clicked" -Level "Info"
                        
                        # Show file dialog to select ISO file
                        $dialog = New-Object Microsoft.Win32.OpenFileDialog
                        $dialog.Filter = "ISO files (*.iso)|*.iso"
                        $dialog.Title = "Select ISO file"
                        
                        if ($dialog.ShowDialog()) {
                            $isoPath = $dialog.FileName
                            Write-BucketLog -Data "Selected ISO: $isoPath" -Level "Info"
                            
                            # Process the ISO file
                            if (Get-Command -Name "Import-BucketISO" -ErrorAction SilentlyContinue) {
                                try {
                                    $result = Import-BucketISO -Path $isoPath
                                    if ($result) {
                                        # Refresh the DataGrid
                                        if (Get-Command -Name "Get-BucketImages" -ErrorAction SilentlyContinue) {
                                            $newImages = Get-BucketImages
                                            $imagesDataGrid.ItemsSource = $newImages
                                        }
                                    }
                                }
                                catch {
                                    Write-BucketLog -Data "Error importing ISO: $_" -Level "Error"
                                }
                            }
                        }
                    })
            }
            
            if ($importWIMButton) {
                $importWIMButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Import WIM button clicked" -Level "Info"
                        
                        # Show file dialog to select WIM file
                        $dialog = New-Object Microsoft.Win32.OpenFileDialog
                        $dialog.Filter = "WIM files (*.wim)|*.wim"
                        $dialog.Title = "Select WIM file"
                        
                        if ($dialog.ShowDialog()) {
                            $wimPath = $dialog.FileName
                            Write-BucketLog -Data "Selected WIM: $wimPath" -Level "Info"
                            
                            # Process the WIM file
                            if (Get-Command -Name "Import-BucketWIM" -ErrorAction SilentlyContinue) {
                                try {
                                    $result = Import-BucketWIM -Path $wimPath
                                    if ($result) {
                                        # Refresh the DataGrid
                                        if (Get-Command -Name "Get-BucketImages" -ErrorAction SilentlyContinue) {
                                            $newImages = Get-BucketImages
                                            $imagesDataGrid.ItemsSource = $newImages
                                        }
                                    }
                                }
                                catch {
                                    Write-BucketLog -Data "Error importing WIM: $_" -Level "Error"
                                }
                            }
                        }
                    })
            }
            
            if ($refreshButton) {
                $refreshButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Refresh button clicked" -Level "Info"
                        
                        # Refresh the images list
                        if (Get-Command -Name "Get-BucketImages" -ErrorAction SilentlyContinue) {
                            try {
                                $newImages = Get-BucketImages
                                
                                # Create an ObservableCollection for better data binding
                                $observableImages = New-Object System.Collections.ObjectModel.ObservableCollection[PSObject]
                                foreach ($image in $newImages) {
                                    $observableImages.Add($image)
                                }
                                
                                $imagesDataGrid.ItemsSource = $observableImages
                            }
                            catch {
                                Write-BucketLog -Data "Error refreshing images: $_" -Level "Error"
                            }
                        }
                    })
            }
            
            if ($deleteButton) {
                $deleteButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Delete button clicked" -Level "Info"
                        
                        # Get selected images
                        $selectedImages = $imagesDataGrid.ItemsSource | Where-Object { $_.SelectImage_Images_IsSelected -eq $true }
                        
                        if ($selectedImages.Count -gt 0) {
                            $result = [System.Windows.MessageBox]::Show(
                                "Are you sure you want to delete $($selectedImages.Count) selected image(s)?",
                                "Confirm Delete",
                                [System.Windows.MessageBoxButton]::YesNo,
                                [System.Windows.MessageBoxImage]::Warning
                            )
                            
                            if ($result -eq [System.Windows.MessageBoxResult]::Yes) {
                                # Delete the selected images
                                if (Get-Command -Name "Remove-BucketImage" -ErrorAction SilentlyContinue) {
                                    try {
                                        foreach ($image in $selectedImages) {
                                            Remove-BucketImage -Path $image.SelectImage_Images_Path
                                        }
                                        
                                        # Refresh the DataGrid
                                        if (Get-Command -Name "Get-BucketImages" -ErrorAction SilentlyContinue) {
                                            $newImages = Get-BucketImages
                                            $imagesDataGrid.ItemsSource = $newImages
                                        }
                                    }
                                    catch {
                                        Write-BucketLog -Data "Error deleting images: $_" -Level "Error"
                                    }
                                }
                            }
                        }
                        else {
                            [System.Windows.MessageBox]::Show(
                                "No images selected for deletion.",
                                "No Selection",
                                [System.Windows.MessageBoxButton]::OK,
                                [System.Windows.MessageBoxImage]::Information
                            )
                        }
                    })
            }
            
            if ($selectAllCheckbox) {
                $selectAllCheckbox.Add_Checked({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Select All checkbox checked" -Level "Info"
                        
                        if ($imagesDataGrid -and $imagesDataGrid.ItemsSource) {
                            foreach ($image in $imagesDataGrid.ItemsSource) {
                                $image.SelectImage_Images_IsSelected = $true
                            }
                            $imagesDataGrid.Items.Refresh()
                        }
                    })
                
                $selectAllCheckbox.Add_Unchecked({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Select All checkbox unchecked" -Level "Info"
                        
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
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Next button clicked" -Level "Info"
                        
                        # Check if an image is selected
                        $selectedImage = $imagesDataGrid.SelectedItem
                        if ($selectedImage) {
                            # Store the selected image information in the global context
                            $script:selectedImage = $selectedImage
                            
                            # Navigate to the next page
                            # TODO: Replace with actual next page when implemented
                            Invoke-BucketHomePage
                        }
                        else {
                            [System.Windows.MessageBox]::Show(
                                "Please select an image to continue.",
                                "No Selection",
                                [System.Windows.MessageBoxButton]::OK,
                                [System.Windows.MessageBoxImage]::Information
                            )
                        }
                    })
            }
            
            if ($previousButton) {
                $previousButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Previous button clicked" -Level "Info"
                        
                        # Navigate to the previous page
                        Invoke-BucketHomePage
                    })
            }
            
            if ($skipButton) {
                $skipButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Skip button clicked" -Level "Info"
                        
                        # Navigate to the next page without selecting an image
                        # TODO: Replace with actual next page when implemented
                        Invoke-BucketHomePage
                    })
            }
            
            if ($summaryButton) {
                $summaryButton.Add_Click({
                        param($senderObj, $e)
                        
                        Write-BucketLog -Data "Summary button clicked" -Level "Info"
                        
                        # Navigate to the summary page
                        # TODO: Replace with actual summary page when implemented
                        Invoke-BucketHomePage
                    })
            }
            
            Write-BucketLog -Data "SelectImagePage event handlers configured successfully" -Level "Info"
        }
        
        # Create the data context for the page
        $dataContext = [PSCustomObject]@{
            # Image data for initial binding
            SelectImage_Images  = $images
            SelectImage_Details = $imageDetails
            
            # Event handler
            PageLoaded          = $pageLoadedHandler
        }
        
        # Navigate to the Select Image page with our custom data context
        Invoke-BucketNavigationService -PageTag "selectImagePage" -DataContext $dataContext
    }
}
