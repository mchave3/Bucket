<#[
.SYNOPSIS
    Initializes the Home Page for the Bucket application

.DESCRIPTION
    Initializes the Home Page, sets up its data context, and handles all event bindings for UI elements on the page.

.NOTES
    Name:        Initialize-BucketHomePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-BucketHomePage
#>
function Initialize-BucketHomePage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "[Home] Initializing Home Page" -Level Info

        #region Data Initialization
        # Initialize system information
        $script:homePage_AppVersion = $script:BucketVersion
        $script:homePage_MountDirectory = Join-Path -Path $script:workingDirectory -ChildPath 'Mount'
        $script:homePage_CompletedWIMsDirectory = Join-Path -Path $script:workingDirectory -ChildPath 'CompletedWIMs'

        # Initialize disk space information (refresh at load time)
        try {
            $cDrive = Get-PSDrive -Name 'C' -ErrorAction SilentlyContinue
            $availableGB = [math]::Round($cDrive.Free/1GB, 2)
            $totalGB = [math]::Round(($cDrive.Free + $cDrive.Used)/1GB, 2)
            $script:homePage_DiskSpaceInfo = "C: Drive - Available space: $availableGB GB / $totalGB GB"
        }
        catch {
            Write-BucketLog -Data "[Home] Error getting disk space information: $_" -Level Warning
            $script:homePage_DiskSpaceInfo = "Disk space information unavailable"
        }

        # Initialize image status
        $script:homePage_MountedImagesCount = 0
        $script:homePage_ImageMountStatus = "No image mounted"
        $script:homePage_CurrentImageInfo = "Please select and mount a Windows image to begin customization."

        # Initialize statistics
        $script:homePage_PendingDriversCount = 0
        $script:homePage_InstalledDriversCount = 0
        $script:homePage_SelectedAppsCount = 0
        #endregion Data Initialization

        #region Event Handlers
        # Create the page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)

            Write-BucketLog -Data "[Home] HomePage loaded, setting up handlers" -Level Info

            #region UI Element References
            $page = $senderObj
            $selectImageButton = $page.FindName("Home_SelectImageButton")
            $appManagementButton = $page.FindName("Home_AppManagementButton")
            $driverManagementButton = $page.FindName("Home_DriverManagementButton")
            $customizationButton = $page.FindName("Home_CustomizationButton")
            $completedWIMsButton = $page.FindName("Home_CompletedWIMsButton")
            $settingsButton = $page.FindName("Home_SettingsButton")
            $helpButton = $page.FindName("Home_HelpButton")
            #endregion UI Element References

            #region Button Event Handlers
            if ($selectImageButton) {
                $selectImageButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] Select Image button clicked" -Level Info
                        Invoke-BucketSelectImagePage
                    })
            }
            if ($appManagementButton) {
                $appManagementButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] App Management button clicked" -Level Info
                        # TODO: Navigate to App Management page when implemented
                    })
            }
            if ($driverManagementButton) {
                $driverManagementButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] Driver Management button clicked" -Level Info
                        # TODO: Navigate to Driver Management page when implemented
                    })
            }
            if ($customizationButton) {
                $customizationButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] Customization button clicked" -Level Info
                        # TODO: Navigate to Customization page when implemented
                    })
            }
            if ($completedWIMsButton) {
                $completedWIMsButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] Completed WIMs button clicked" -Level Info
                        # TODO: Navigate to Completed WIMs page when implemented
                    })
            }
            if ($settingsButton) {
                $settingsButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] Settings button clicked" -Level Info
                        # TODO: Navigate to Settings page when implemented
                    })
            }
            if ($helpButton) {
                $helpButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "[Home] Help button clicked" -Level Info
                        # TODO: Navigate to Help page when implemented
                    })
            }
            #endregion Button Event Handlers

            Write-BucketLog -Data "[Home] HomePage event handlers configured successfully" -Level Info
        }
        #endregion Event Handlers

        #region Data Context Creation
        $dataContext = [PSCustomObject]@{
            MountDirectory         = $script:homePage_MountDirectory
            CompletedWIMsDirectory = $script:homePage_CompletedWIMsDirectory
            WorkingDirectory       = $script:workingDirectory
            AppVersion             = $script:homePage_AppVersion
            DiskSpaceInfo          = $script:homePage_DiskSpaceInfo
            MountedImagesCount     = $script:homePage_MountedImagesCount
            ImageMountStatus       = $script:homePage_ImageMountStatus
            CurrentImageInfo       = $script:homePage_CurrentImageInfo
            PendingDriversCount    = $script:homePage_PendingDriversCount
            InstalledDriversCount  = $script:homePage_InstalledDriversCount
            SelectedAppsCount      = $script:homePage_SelectedAppsCount
            PageLoaded             = $pageLoadedHandler
        }
        Invoke-BucketNavigationService -PageTag "homePage" -RootFrame $WPF_MainWindow_RootFrame -DataContext $dataContext
        #endregion Data Context Creation
    }
}
