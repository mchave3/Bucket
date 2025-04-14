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
        
        #region Data Initialization
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
                #region Sample Data
                # Sample data for development/testing
                $images = @(
                    [PSCustomObject]@{
                        SelectImage_Images_IsSelected = $false
                        SelectImage_Images_Name       = "Windows 11 Consumer Editions"
                        SelectImage_Images_Path       = "C:\Images\Windows11Consumer.wim"
                        SelectImage_Images_Version    = "22H2"
                        SelectImage_Images_Size       = "4800"
                        SelectImage_Images_ImportDate = (Get-Date).AddDays(-5).ToString("yyyy-MM-dd")
                        SelectImage_Images_IndexCount = 4  # Number of editions/indexes in this WIM
                    },
                    [PSCustomObject]@{
                        SelectImage_Images_IsSelected = $false
                        SelectImage_Images_Name       = "Windows 11 Business Editions"
                        SelectImage_Images_Path       = "C:\Images\Windows11Business.wim"
                        SelectImage_Images_Version    = "22H2"
                        SelectImage_Images_Size       = "5200"
                        SelectImage_Images_ImportDate = (Get-Date).AddDays(-3).ToString("yyyy-MM-dd")
                        SelectImage_Images_IndexCount = 3  # Number of editions/indexes in this WIM
                    },
                    [PSCustomObject]@{
                        SelectImage_Images_IsSelected = $false
                        SelectImage_Images_Name       = "Windows 10 Consumer Editions"
                        SelectImage_Images_Path       = "C:\Images\Windows10Consumer.wim"
                        SelectImage_Images_Version    = "21H2"
                        SelectImage_Images_Size       = "4200"
                        SelectImage_Images_ImportDate = (Get-Date).AddDays(-15).ToString("yyyy-MM-dd")
                        SelectImage_Images_IndexCount = 4  # Number of editions/indexes in this WIM
                    },
                    [PSCustomObject]@{
                        SelectImage_Images_IsSelected = $false
                        SelectImage_Images_Name       = "Windows 10 Business Editions"
                        SelectImage_Images_Path       = "C:\Images\Windows10Business.wim"
                        SelectImage_Images_Version    = "21H2"
                        SelectImage_Images_Size       = "4600"
                        SelectImage_Images_ImportDate = (Get-Date).AddDays(-12).ToString("yyyy-MM-dd")
                        SelectImage_Images_IndexCount = 3  # Number of editions/indexes in this WIM
                    },
                    [PSCustomObject]@{
                        SelectImage_Images_IsSelected = $false
                        SelectImage_Images_Name       = "Windows 10 Enterprise LTSC"
                        SelectImage_Images_Path       = "C:\Images\Windows10LTSC.wim"
                        SelectImage_Images_Version    = "2021"
                        SelectImage_Images_Size       = "3800"
                        SelectImage_Images_ImportDate = (Get-Date).AddDays(-20).ToString("yyyy-MM-dd")
                        SelectImage_Images_IndexCount = 2  # Number of editions/indexes in this WIM
                    },
                    [PSCustomObject]@{
                        SelectImage_Images_IsSelected = $false
                        SelectImage_Images_Name       = "Windows Server 2022"
                        SelectImage_Images_Path       = "C:\Images\WindowsServer2022.wim"
                        SelectImage_Images_Version    = "21H2"
                        SelectImage_Images_Size       = "5500"
                        SelectImage_Images_ImportDate = (Get-Date).AddDays(-2).ToString("yyyy-MM-dd")
                        SelectImage_Images_IndexCount = 4  # Number of editions/indexes in this WIM
                    }
                )
            }
            
            # Sample image details for the first image
            if (Get-Command -Name "Get-BucketImageDetails" -ErrorAction SilentlyContinue) {
                $imageDetails = Get-BucketImageDetails -ImagePath $images[0].SelectImage_Images_Path
            }
            else {
                # Create a hashtable to store details for each WIM file and make it accessible in script scope
                $script:allWimDetails = @{}
                
                # Windows 11 Consumer Editions (4 indexes)
                $script:allWimDetails["C:\Images\Windows11Consumer.wim"] = @(
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "1"
                        SelectImage_Details_Name         = "Windows 11 Home"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Home"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Home Edition"
                        SelectImage_Details_Size         = "3900"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "2"
                        SelectImage_Details_Name         = "Windows 11 Home N"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Home N"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Home N Edition"
                        SelectImage_Details_Size         = "3800"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "3"
                        SelectImage_Details_Name         = "Windows 11 Pro"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Pro"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Pro Edition"
                        SelectImage_Details_Size         = "4100"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "4"
                        SelectImage_Details_Name         = "Windows 11 Pro N"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Pro N"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Pro N Edition"
                        SelectImage_Details_Size         = "4000"
                    }
                )
                
                # Windows 11 Business Editions (3 indexes)
                $script:allWimDetails["C:\Images\Windows11Business.wim"] = @(
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "1"
                        SelectImage_Details_Name         = "Windows 11 Enterprise"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Enterprise"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Enterprise Edition"
                        SelectImage_Details_Size         = "4500"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "2"
                        SelectImage_Details_Name         = "Windows 11 Enterprise N"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Enterprise N"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Enterprise N Edition"
                        SelectImage_Details_Size         = "4400"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "3"
                        SelectImage_Details_Name         = "Windows 11 Education"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Education"
                        SelectImage_Details_Version      = "22H2 (10.0.22621)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 11 Education Edition"
                        SelectImage_Details_Size         = "4300"
                    }
                )
                
                # Windows 10 Consumer Editions (4 indexes)
                $script:allWimDetails["C:\Images\Windows10Consumer.wim"] = @(
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "1"
                        SelectImage_Details_Name         = "Windows 10 Home"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Home"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Home Edition"
                        SelectImage_Details_Size         = "3500"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "2"
                        SelectImage_Details_Name         = "Windows 10 Home N"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Home N"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Home N Edition"
                        SelectImage_Details_Size         = "3400"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "3"
                        SelectImage_Details_Name         = "Windows 10 Pro"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Pro"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Pro Edition"
                        SelectImage_Details_Size         = "3700"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "4"
                        SelectImage_Details_Name         = "Windows 10 Pro N"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Pro N"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Pro N Edition"
                        SelectImage_Details_Size         = "3600"
                    }
                )
                
                # Windows 10 Business Editions (3 indexes)
                $script:allWimDetails["C:\Images\Windows10Business.wim"] = @(
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "1"
                        SelectImage_Details_Name         = "Windows 10 Enterprise"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Enterprise"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Enterprise Edition"
                        SelectImage_Details_Size         = "3800"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "2"
                        SelectImage_Details_Name         = "Windows 10 Enterprise N"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Enterprise N"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Enterprise N Edition"
                        SelectImage_Details_Size         = "3700"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "3"
                        SelectImage_Details_Name         = "Windows 10 Education"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Education"
                        SelectImage_Details_Version      = "21H2 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Education Edition"
                        SelectImage_Details_Size         = "3750"
                    }
                )
                
                # Windows 10 LTSC (2 indexes)
                $script:allWimDetails["C:\Images\Windows10LTSC.wim"] = @(
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "1"
                        SelectImage_Details_Name         = "Windows 10 Enterprise LTSC"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Enterprise LTSC"
                        SelectImage_Details_Version      = "2021 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 Enterprise LTSC 2021 Edition"
                        SelectImage_Details_Size         = "3800"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "2"
                        SelectImage_Details_Name         = "Windows 10 IoT Enterprise LTSC"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "IoT Enterprise LTSC"
                        SelectImage_Details_Version      = "2021 (10.0.19044)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows 10 IoT Enterprise LTSC 2021 Edition"
                        SelectImage_Details_Size         = "3600"
                    }
                )
                
                # Windows Server 2022 (4 indexes)
                $script:allWimDetails["C:\Images\WindowsServer2022.wim"] = @(
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "1"
                        SelectImage_Details_Name         = "Windows Server 2022 Standard"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Standard"
                        SelectImage_Details_Version      = "21H2 (10.0.20348)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows Server 2022 Standard Edition"
                        SelectImage_Details_Size         = "5100"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "2"
                        SelectImage_Details_Name         = "Windows Server 2022 Standard (Desktop Experience)"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Standard Desktop"
                        SelectImage_Details_Version      = "21H2 (10.0.20348)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows Server 2022 Standard with Desktop Experience"
                        SelectImage_Details_Size         = "8200"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "3"
                        SelectImage_Details_Name         = "Windows Server 2022 Datacenter"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Datacenter"
                        SelectImage_Details_Version      = "21H2 (10.0.20348)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows Server 2022 Datacenter Edition"
                        SelectImage_Details_Size         = "5300"
                    },
                    [PSCustomObject]@{
                        SelectImage_Details_Index        = "4"
                        SelectImage_Details_Name         = "Windows Server 2022 Datacenter (Desktop Experience)"
                        SelectImage_Details_Architecture = "x64"
                        SelectImage_Details_Edition      = "Datacenter Desktop"
                        SelectImage_Details_Version      = "21H2 (10.0.20348)"
                        SelectImage_Details_Languages    = "en-US"
                        SelectImage_Details_Description  = "Windows Server 2022 Datacenter with Desktop Experience"
                        SelectImage_Details_Size         = "8400"
                    }
                )
                
                # Use the details for the first WIM file as the initial details data
                $imageDetails = $script:allWimDetails[$images[0].SelectImage_Images_Path]
                #endregion Sample Data
            }
        }
        catch {
            Write-BucketLog -Data "Error loading image data: $_" -Level "Error"
        }
        
        # Store images and details in script scope so they're accessible in the event handler
        $script:selectImagePageImages = $images
        $script:selectImagePageDetails = $imageDetails

        #region Event Handlers
        # Create the page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)
            
            Write-BucketLog -Data "SelectImagePage loaded, setting up handlers" -Level "Info"
            
            #region DataGrid Setup
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
            
            # Check if DataGrids are properly found
            if ($null -eq $imagesDataGrid) {
                Write-BucketLog -Data "CRITICAL ERROR: ImagesDataGrid not found in page" -Level Error
            } 
            else {
                Write-BucketLog -Data "ImagesDataGrid successfully found: $($imagesDataGrid.GetType().FullName)" -Level Info
            }
            
            if ($null -eq $detailsDataGrid) {
                Write-BucketLog -Data "CRITICAL ERROR: DetailsDataGrid not found in page" -Level Error
            } 
            else {
                Write-BucketLog -Data "DetailsDataGrid successfully found: $($detailsDataGrid.GetType().FullName)" -Level Info
            }
            
            # Set data sources for DataGrids
            if ($imagesDataGrid) {
                # Access the script-scoped variables for images
                $imagesToUse = $script:selectImagePageImages
                Write-BucketLog -Data "Setting up ImagesDataGrid with $($imagesToUse.Count) items" -Level Info
                
                try {
                    # Create a list of type System.Collections.IList for better compatibility
                    $imagesList = New-Object System.Collections.ArrayList
                    foreach ($image in $imagesToUse) {
                        [void]$imagesList.Add($image)
                    }
                    
                    # Explicitly set the data source
                    $imagesDataGrid.ItemsSource = $null  # First clear the current source
                    $imagesDataGrid.ItemsSource = $imagesList
                    
                    # Force display refresh
                    $imagesDataGrid.UpdateLayout()
                    $imagesDataGrid.Items.Refresh()
                    
                    # Display the first row
                    if ($imagesList.Count -gt 0) {
                        $imagesDataGrid.SelectedIndex = 0
                    }
                    
                    Write-BucketLog -Data "ImagesDataGrid ItemsSource successfully set ($($imagesList.Count) items)" -Level Info
                }
                catch {
                    Write-BucketLog -Data "Error setting ItemsSource for ImagesDataGrid: $_" -Level Error
                }
                
                # Store DataGrids in script scope so they can be accessed in event handlers
                $script:imagesDataGridRef = $imagesDataGrid
                $script:detailsDataGridRef = $detailsDataGrid
                
                #region Image Details Update Function
                # Dedicated function to update image details - for better troubleshooting
                $script:UpdateImageDetails = {
                    param($selectedItem)
                    
                    if ($null -eq $selectedItem) {
                        Write-BucketLog -Data "No item selected" -Level "Warning"
                        return
                    }
                    
                    Write-BucketLog -Data "Updating details for image: $($selectedItem.SelectImage_Images_Name)" -Level "Info"
                    
                    # Get the correct image path
                    $imagePath = $selectedItem.SelectImage_Images_Path
                    
                    # Try to get the details
                    try {
                        if (Get-Command -Name "Get-BucketImageDetails" -ErrorAction SilentlyContinue) {
                            # If the actual function exists, use it
                            $details = Get-BucketImageDetails -ImagePath $imagePath
                        }
                        else {
                            # Otherwise use our mock data
                            Write-BucketLog -Data "Using mock data for $imagePath" -Level "Debug"
                            
                            # Make sure the hashtable exists and contains the key
                            if (-not $script:allWimDetails) {
                                Write-BucketLog -Data "allWimDetails variable not found" -Level "Error"
                                return
                            }
                            
                            if (-not $script:allWimDetails.ContainsKey($imagePath)) {
                                Write-BucketLog -Data "No details found for path: $imagePath" -Level "Warning"
                                return
                            }
                            
                            $details = $script:allWimDetails[$imagePath]
                            Write-BucketLog -Data "Found $($details.Count) indexes for $imagePath" -Level "Info"
                        }
                        
                        # Get reference to the details grid
                        $detailsGrid = $script:detailsDataGridRef
                        if ($null -eq $detailsGrid) {
                            Write-BucketLog -Data "Details DataGrid reference is null" -Level "Error"
                            return
                        }
                        
                        # Create a new list for the details
                        $detailsList = New-Object System.Collections.ArrayList
                        
                        # Add the details to the list
                        foreach ($detail in $details) {
                            [void]$detailsList.Add($detail)
                        }
                        
                        # Update the UI
                        $detailsGrid.Dispatcher.Invoke([Action]{
                                $detailsGrid.ItemsSource = $null
                                $detailsGrid.ItemsSource = $detailsList
                                $detailsGrid.UpdateLayout()
                                $detailsGrid.Items.Refresh()
                                
                                # Select the first item if available
                                if ($detailsList.Count -gt 0) {
                                    $detailsGrid.SelectedIndex = 0
                                }
                            })
                        
                        Write-BucketLog -Data "Successfully updated details grid with $($detailsList.Count) indexes" -Level "Info"
                    }
                    catch {
                        Write-BucketLog -Data "Error updating image details: $_" -Level "Error"
                    }
                }
                #endregion Image Details Update Function
                
                # Handle selection changed event
                $imagesDataGrid.Add_SelectionChanged({
                        param($senderObj, $e)
                        
                        # Get the selected item
                        $selectedItem = $senderObj.SelectedItem
                        Write-BucketLog -Data "Selection changed event triggered" -Level "Debug"
                        
                        if ($null -ne $selectedItem) {
                            Write-BucketLog -Data "Selected image: $($selectedItem.SelectImage_Images_Name)" -Level "Info"
                            
                            # Call our update function
                            & $script:UpdateImageDetails $selectedItem
                        }
                    })
            }
            
            #region Detail DataGrid Setup
            if ($detailsDataGrid) {
                # Access the script-scoped variables for image details
                $detailsToUse = $script:selectImagePageDetails
                Write-BucketLog -Data "Setting up DetailsDataGrid with $($detailsToUse.Count) items" -Level Info
                
                try {
                    # Create a list of type System.Collections.IList for better compatibility
                    $detailsList = New-Object System.Collections.ArrayList
                    foreach ($detail in $detailsToUse) {
                        [void]$detailsList.Add($detail)
                    }
                    
                    # Explicitly set the data source
                    $detailsDataGrid.ItemsSource = $null  # First clear the current source
                    $detailsDataGrid.ItemsSource = $detailsList
                    
                    # Force display refresh
                    $detailsDataGrid.UpdateLayout()
                    $detailsDataGrid.Items.Refresh()
                    
                    # Display the first row
                    if ($detailsList.Count -gt 0) {
                        $detailsDataGrid.SelectedIndex = 0
                    }
                    
                    Write-BucketLog -Data "DetailsDataGrid ItemsSource successfully set ($($detailsList.Count) items)" -Level Info
                }
                catch {
                    Write-BucketLog -Data "Error setting ItemsSource for DetailsDataGrid: $_" -Level Error
                }
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
            
            #region Button Event Handlers
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
        #endregion Event Handlers
        
        #region Data Context Creation
        # Create the data context for the page with necessary data and handlers
        $dataContext = [PSCustomObject]@{
            # Image data for initial binding (make it accessible in the page context)
            SelectImage_Images  = $images
            SelectImage_Details = $imageDetails
            
            # Event handler for page loaded event
            PageLoaded          = $pageLoadedHandler
        }
        
        # Log the data context creation for debugging
        Write-BucketLog -Data "Created SelectImagePage data context with $($images.Count) images and $($imageDetails.Count) detail items" -Level Info
        
        # Navigate to the Select Image page with our custom data context
        Invoke-BucketNavigationService -PageTag "selectImagePage" -DataContext $dataContext
        #endregion Data Context Creation
    }
}
