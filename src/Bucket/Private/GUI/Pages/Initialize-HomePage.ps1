<#
.SYNOPSIS
    Initializes the Home Page for the Bucket application

.DESCRIPTION
    This function initializes the Home Page, sets up its data context,
    and handles all event bindings for UI elements on the page.

.NOTES
    Name:        Initialize-HomePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-HomePage
#>
function Initialize-HomePage {
    [CmdletBinding()]
    param()
    
    process {
        Write-BucketLog -Data "Initializing Home Page" -Level Info
        
        # Create the page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)
            
            Write-BucketLog -Data "Home Page loaded, setting up handlers" -Level Info
            
            # Get the page itself
            $page                = $senderObj
            
            # Get references to UI elements
            $btnSelectImage      = $page.FindName("BtnSelectImage")
            $btnAppManagement    = $page.FindName("BtnAppManagement")
            $btnDriverManagement = $page.FindName("BtnDriverManagement")
            $btnCustomization    = $page.FindName("BtnCustomization")
            $btnCompletedWIMs    = $page.FindName("BtnCompletedWIMs")
            $btnSettings         = $page.FindName("BtnSettings")
            $btnHelp             = $page.FindName("BtnHelp")
            
            # Set up button event handlers
            if ($btnSelectImage) {
                $btnSelectImage.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Select Image button clicked from Home Page" -Level Info
                        Invoke-BucketSelectImagePage
                    })
            }
            
            if ($btnAppManagement) {
                $btnAppManagement.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "App Management button clicked" -Level Info
                        # TODO: Navigate to App Management page when implemented
                    })
            }
            
            if ($btnDriverManagement) {
                $btnDriverManagement.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Driver Management button clicked" -Level Info
                        # TODO: Navigate to Driver Management page when implemented
                    })
            }
            
            if ($btnCustomization) {
                $btnCustomization.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Customization button clicked" -Level Info
                        # TODO: Navigate to Customization page when implemented
                    })
            }
            
            if ($btnCompletedWIMs) {
                $btnCompletedWIMs.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Completed WIMs button clicked" -Level Info
                        # TODO: Navigate to Completed WIMs page when implemented
                    })
            }
            
            if ($btnSettings) {
                $btnSettings.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Settings button clicked" -Level Info
                        # TODO: Navigate to Settings page when implemented
                    })
            }
            
            if ($btnHelp) {
                $btnHelp.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Help button clicked" -Level Info
                        # TODO: Navigate to Help page when implemented
                    })
            }
            
            Write-BucketLog -Data "Home Page event handlers configured successfully" -Level Info
        }
        
        # Create the data context for the page
        $dataContext = [PSCustomObject]@{
            # System information
            MountDirectory         = (Join-Path -Path $script:workingDirectory -ChildPath 'Mount')
            CompletedWIMsDirectory = (Join-Path -Path $script:workingDirectory -ChildPath 'CompletedWIMs')
            WorkingDirectory       = $script:workingDirectory
            
            # Disk space information
            DiskSpaceInfo          = "C: Drive - Available space: $([math]::Round($(Get-PSDrive -Name 'C').Free/1GB, 2)) GB / $([math]::Round(($(Get-PSDrive -Name 'C').Free + $(Get-PSDrive -Name 'C').Used)/1GB, 2)) GB"
            
            # Image status
            MountedImagesCount     = 0
            ImageMountStatus       = "No image mounted"
            CurrentImageInfo       = "Please select and mount a Windows image to begin customization."
            
            # Statistics
            PendingDriversCount    = 0
            InstalledDriversCount  = 0
            SelectedAppsCount      = 0
            
            # Event handler
            PageLoaded             = $pageLoadedHandler
        }
        
        # Add mount directory and completed WIMs directory to data context
        $dataContext | Add-Member -MemberType NoteProperty -Name "MountDirectory" -Value (Join-Path -Path $script:workingDirectory -ChildPath 'Mount') -Force
        $dataContext | Add-Member -MemberType NoteProperty -Name "CompletedWIMsDirectory" -Value (Join-Path -Path $script:workingDirectory -ChildPath 'CompletedWIMs') -Force
        
        # Navigate to the page
        Invoke-BucketNavigationService -PageTag "homePage" -DataContext $dataContext
    }
}

