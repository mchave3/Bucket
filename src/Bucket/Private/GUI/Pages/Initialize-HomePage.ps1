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

        #region Data Initialization
        # Initialize system information
        $script:homePage_AppVersion = $script:BucketVersion # Pour le binding AppVersion dans le XAML
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
            Write-BucketLog -Data "Error getting disk space information: $_" -Level "Warning"
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

            Write-BucketLog -Data "HomePage loaded, setting up handlers" -Level "Info"

            #region UI Element References
            # Get the page itself
            $page = $senderObj

            # Get references to UI elements
            $selectImageButton = $page.FindName("Home_SelectImageButton")
            $appManagementButton = $page.FindName("Home_AppManagementButton")
            $driverManagementButton = $page.FindName("Home_DriverManagementButton")
            $customizationButton = $page.FindName("Home_CustomizationButton")
            $completedWIMsButton = $page.FindName("Home_CompletedWIMsButton")
            $settingsButton = $page.FindName("Home_SettingsButton")
            $helpButton = $page.FindName("Home_HelpButton")
            #endregion UI Element References

            #region Button Event Handlers
            # Set up Select Image button event
            if ($selectImageButton) {
                $selectImageButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Select Image button clicked from Home Page" -Level "Info"
                        Invoke-BucketSelectImagePage
                    })
            }

            # Set up App Management button event
            if ($appManagementButton) {
                $appManagementButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "App Management button clicked" -Level "Info"
                        # TODO: Navigate to App Management page when implemented
                    })
            }

            # Set up Driver Management button event
            if ($driverManagementButton) {
                $driverManagementButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Driver Management button clicked" -Level "Info"
                        # TODO: Navigate to Driver Management page when implemented
                    })
            }

            # Set up Customization button event
            if ($customizationButton) {
                $customizationButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Customization button clicked" -Level "Info"
                        # TODO: Navigate to Customization page when implemented
                    })
            }

            # Set up Completed WIMs button event
            if ($completedWIMsButton) {
                $completedWIMsButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Completed WIMs button clicked" -Level "Info"
                        # TODO: Navigate to Completed WIMs page when implemented
                    })
            }

            # Set up Settings button event
            if ($settingsButton) {
                $settingsButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Settings button clicked" -Level "Info"
                        # TODO: Navigate to Settings page when implemented
                    })
            }

            # Set up Help button event
            if ($helpButton) {
                $helpButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Help button clicked" -Level "Info"
                        # TODO: Navigate to Help page when implemented
                    })
            }
            #endregion Button Event Handlers

            Write-BucketLog -Data "HomePage event handlers configured successfully" -Level "Info"
        }
        #endregion Event Handlers

        #region Data Context Creation
        # Create the data context for the page with necessary data and handlers
        $dataContext = [PSCustomObject]@{
            # System information
            MountDirectory         = $script:homePage_MountDirectory
            CompletedWIMsDirectory = $script:homePage_CompletedWIMsDirectory
            WorkingDirectory       = $script:workingDirectory
            AppVersion             = $script:homePage_AppVersion

            # Disk space information
            DiskSpaceInfo          = $script:homePage_DiskSpaceInfo

            # Image status
            MountedImagesCount     = $script:homePage_MountedImagesCount
            ImageMountStatus       = $script:homePage_ImageMountStatus
            CurrentImageInfo       = $script:homePage_CurrentImageInfo

            # Statistics
            PendingDriversCount    = $script:homePage_PendingDriversCount
            InstalledDriversCount  = $script:homePage_InstalledDriversCount
            SelectedAppsCount      = $script:homePage_SelectedAppsCount

            # Event handler
            PageLoaded             = $pageLoadedHandler
        }

        # Navigate to the Home page with our custom data context
        Invoke-BucketNavigationService -PageTag "homePage" -DataContext $dataContext
        #endregion Data Context Creation
    }
}
