<#
.SYNOPSIS
    Initializes the About Page for the Bucket application

.DESCRIPTION
    This function initializes the About Page, sets up its data context,
    and handles all event bindings for UI elements on the page.

.NOTES
    Name:        Initialize-BucketAboutPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-BucketAboutPage
#>
function Initialize-BucketAboutPage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "Initializing About Page" -Level Info

        #region Data Initialization
        # Get application information
        $script:aboutPage_AppVersion = $script:BucketVersion # Use the global version
        $script:aboutPage_BuildDate = Get-Date -Format "yyyy-MM-dd"
        $script:aboutPage_Author = "Mickaël CHAVE"
        $script:aboutPage_Repository = "https://github.com/mchave3/Bucket"
        $script:aboutPage_License = "MIT License"

        # Get system information
        $script:aboutPage_PowerShellVersion = $PSVersionTable.PSVersion.ToString()
        $script:aboutPage_OSVersion = [System.Environment]::OSVersion.VersionString
        $script:aboutPage_DotNetVersion = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription

        # Get additional module information
        $script:aboutPage_Modules = @()
        try {
            # Try to get info about dependencies
            $script:aboutPage_Modules = @(
                [PSCustomObject]@{
                    About_Modules_Name    = "PoShLog"
                    About_Modules_Version = (Get-Module -Name "PoShLog" -ErrorAction SilentlyContinue).Version.ToString()
                    About_Modules_Status  = (Get-Module -Name "PoShLog" -ErrorAction SilentlyContinue) ? "Loaded" : "Not Loaded"
                }
            )
        }
        catch {
            Write-BucketLog -Data "Error getting module information: $_" -Level "Warning"
        }
        #endregion Data Initialization

        #region Event Handlers
        # Create the page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)

            Write-BucketLog -Data "AboutPage loaded, setting up handlers" -Level "Info"

            #region UI Element References
            # Get the page itself
            $page = $senderObj

            # Get references to UI elements
            $githubButton = $page.FindName("About_GitHubButton")
            $licenseButton = $page.FindName("About_LicenseButton")
            $issueButton = $page.FindName("About_ReportIssueButton")
            $checkUpdateButton = $page.FindName("About_CheckUpdateButton")
            $githubProfileLink = $page.FindName("About_GitHubProfileLink")
            $modulesDataGrid = $page.FindName("About_ModulesDataGrid")
            #endregion UI Element References

            #region DataGrid Setup
            # Set up modules DataGrid if it exists
            if ($modulesDataGrid) {
                # Access the script-scoped variables for modules
                $modulesToUse = $script:aboutPage_Modules
                Write-BucketLog -Data "Setting up ModulesDataGrid with $($modulesToUse.Count) items" -Level Info

                try {
                    # Create a list of type System.Collections.IList for better compatibility
                    $modulesList = New-Object System.Collections.ArrayList
                    foreach ($module in $modulesToUse) {
                        [void]$modulesList.Add($module)
                    }

                    # Explicitly set the data source
                    $modulesDataGrid.ItemsSource = $null  # First clear the current source
                    $modulesDataGrid.ItemsSource = $modulesList

                    # Force display refresh
                    $modulesDataGrid.UpdateLayout()
                    $modulesDataGrid.Items.Refresh()

                    Write-BucketLog -Data "ModulesDataGrid ItemsSource successfully set ($($modulesList.Count) items)" -Level Info
                }
                catch {
                    Write-BucketLog -Data "Error setting ItemsSource for ModulesDataGrid: $_" -Level Error
                }
            }
            #endregion DataGrid Setup

            #region Button Event Handlers
            # Set up GitHub button event
            if ($githubButton) {
                $githubButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "GitHub button clicked" -Level "Info"
                        Start-Process "https://github.com/mchave3/Bucket"
                    })
            }

            # Set up License button event
            if ($licenseButton) {
                $licenseButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "License button clicked" -Level "Info"
                        Start-Process "https://github.com/mchave3/Bucket/blob/main/LICENSE"
                    })
            }

            # Set up Report Issue button event
            if ($issueButton) {
                $issueButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Report Issue button clicked" -Level "Info"
                        Start-Process "https://github.com/mchave3/Bucket/issues/new"
                    })
            }

            # Set up Check for Updates button event
            if ($checkUpdateButton) {
                $checkUpdateButton.Add_Click({
                        param($senderObj, $e)

                        Write-BucketLog -Data "Check for Updates button clicked" -Level "Info"

                        # Call update check function if it exists
                        if (Get-Command -Name "Test-BucketUpdate" -ErrorAction SilentlyContinue) {
                            try {
                                $updateResult = Test-BucketUpdate

                                if ($updateResult.UpdateAvailable) {
                                    [System.Windows.MessageBox]::Show(
                                        "A new version ($($updateResult.LatestVersion)) is available. Your current version is $($script:aboutPage_AppVersion).\n\nPlease visit the GitHub repository to download the latest version.",
                                        "Update Available",
                                        [System.Windows.MessageBoxButton]::OK,
                                        [System.Windows.MessageBoxImage]::Information
                                    )
                                }
                                else {
                                    [System.Windows.MessageBox]::Show(
                                        "You are running the latest version ($($script:aboutPage_AppVersion)).",
                                        "No Updates Available",
                                        [System.Windows.MessageBoxButton]::OK,
                                        [System.Windows.MessageBoxImage]::Information
                                    )
                                }
                            }
                            catch {
                                Write-BucketLog -Data "Error checking for updates: $_" -Level "Error"
                                [System.Windows.MessageBox]::Show(
                                    "An error occurred while checking for updates: $($_.Exception.Message)",
                                    "Update Check Error",
                                    [System.Windows.MessageBoxButton]::OK,
                                    [System.Windows.MessageBoxImage]::Error
                                )
                            }
                        }
                        else {
                            [System.Windows.MessageBox]::Show(
                                "Update check functionality is not implemented.",
                                "Not Implemented",
                                [System.Windows.MessageBoxButton]::OK,
                                [System.Windows.MessageBoxImage]::Information
                            )
                        }
                    })
            }

            # Set up GitHub Profile link event
            if ($githubProfileLink) {
                $githubProfileLink.Add_MouseLeftButtonDown({
                        param($senderObj, $e)

                        Write-BucketLog -Data "GitHub Profile link clicked" -Level "Info"

                        # Mark the event as handled
                        $e.Handled = $true

                        # Open the GitHub profile URL in the default browser
                        Start-Process "https://github.com/mchave3"
                    })
            }
            #endregion Button Event Handlers

            Write-BucketLog -Data "AboutPage event handlers configured successfully" -Level "Info"
        }
        #endregion Event Handlers

        #region Data Context Creation
        # Create the data context for the page with necessary data and handlers
        $dataContext = [PSCustomObject]@{
            # Application information
            AppVersion        = $script:aboutPage_AppVersion
            BuildDate         = $script:aboutPage_BuildDate
            Author            = $script:aboutPage_Author
            Repository        = $script:aboutPage_Repository
            License           = $script:aboutPage_License

            # System information
            PowerShellVersion = $script:aboutPage_PowerShellVersion
            OSVersion         = $script:aboutPage_OSVersion
            DotNetVersion     = $script:aboutPage_DotNetVersion

            # Module information for DataGrid
            ModulesInfo       = $script:aboutPage_Modules

            # Event handler
            PageLoaded        = $pageLoadedHandler
        }

        # Navigate to the About page with our custom data context
        Invoke-BucketNavigationService -PageTag "aboutPage" -DataContext $dataContext
        #endregion Data Context Creation
    }
}
