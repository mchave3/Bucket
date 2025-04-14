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

    )    process {
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
        
        # Define event handlers
        $importISOHandler = {
            param($sender, $e)
            
            # TODO: Implement ISO import logic
            Write-BucketLog -Data "Import ISO button clicked" -Level "Info"
            
            # Show file dialog to select ISO file
            $dialog = New-Object Microsoft.Win32.OpenFileDialog
            $dialog.Filter = "ISO files (*.iso)|*.iso"
            $dialog.Title = "Select ISO file"
            
            if ($dialog.ShowDialog()) {
                $isoPath = $dialog.FileName
                Write-BucketLog -Data "Selected ISO: $isoPath" -Level "Info"
                
                # Process the ISO file
                # TODO: Add actual ISO processing logic
            }
        }
        
        $importWIMHandler = {
            param($sender, $e)
            
            # TODO: Implement WIM import logic
            Write-BucketLog -Data "Import WIM button clicked" -Level "Info"
            
            # Show file dialog to select WIM file
            $dialog = New-Object Microsoft.Win32.OpenFileDialog
            $dialog.Filter = "WIM files (*.wim)|*.wim"
            $dialog.Title = "Select WIM file"
            
            if ($dialog.ShowDialog()) {
                $wimPath = $dialog.FileName
                Write-BucketLog -Data "Selected WIM: $wimPath" -Level "Info"
                
                # Process the WIM file
                # TODO: Add actual WIM processing logic
            }
        }
        
        $refreshHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "Refresh button clicked" -Level "Info"
            
            # TODO: Implement refresh logic
            # This should reload the images list
        }
        
        $deleteHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "Delete button clicked" -Level "Info"
            
            # TODO: Implement delete logic
            # This should delete selected images
        }
        
        $selectAllHandler = {
            param($sender, $e)
            
            $isChecked = $sender.IsChecked
            Write-BucketLog -Data "Select All checkbox changed to $isChecked" -Level "Info"
            
            # Update all image selection checkboxes
            foreach ($image in $DataContext.SelectImage_Images) {
                $image.SelectImage_Images_IsSelected = $isChecked
            }
        }
        
        $nextHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "Next button clicked" -Level "Info"
            # Navigate to the next page
            # TODO: Replace with actual navigation logic
        }
        
        $previousHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "Previous button clicked" -Level "Info"
            # Navigate to the previous page
            # TODO: Replace with actual navigation logic
        }
        
        $skipHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "Skip button clicked" -Level "Info"
            # Skip this step
            # TODO: Replace with actual skip logic
        }
        
        $summaryHandler = {
            param($sender, $e)
            
            Write-BucketLog -Data "Summary button clicked" -Level "Info"
            # Navigate to the summary page
            # TODO: Replace with actual navigation logic
        }
        
        $imageSelectionChangedHandler = {
            param($sender, $e)
            
            $selectedItem = $sender.SelectedItem
            if ($null -ne $selectedItem) {
                Write-BucketLog -Data "Selected image: $($selectedItem.SelectImage_Images_Name)" -Level "Info"
                
                # Update details grid based on selected image
                # TODO: Replace with actual image details loading logic
            }
        }
        
        # Create data context with all required properties
        $DataContext = [PSCustomObject]@{
            # Image data
            SelectImage_Images                          = $images
            SelectImage_Details                         = $imageDetails
            
            # Event handlers
            SelectImage_ImportISOButton_Click           = $importISOHandler
            SelectImage_ImportWIMButton_Click           = $importWIMHandler
            SelectImage_RefreshButton_Click             = $refreshHandler
            SelectImage_DeleteButton_Click              = $deleteHandler
            SelectImage_SelectAllCheckbox_Changed       = $selectAllHandler
            SelectImage_NextButton_Click                = $nextHandler
            SelectImage_PreviousButton_Click            = $previousHandler
            SelectImage_SkipButton_Click                = $skipHandler
            SelectImage_SummaryButton_Click             = $summaryHandler
            SelectImage_ImagesDataGrid_SelectionChanged = $imageSelectionChangedHandler
            
            # Helper properties
            SelectImage_HasSelectedImages               = $false
        }

        # Navigate to the Select Image page
        Invoke-BucketGuiNav -PageTag "selectImagePage" -DataContext $DataContext
    }
}
