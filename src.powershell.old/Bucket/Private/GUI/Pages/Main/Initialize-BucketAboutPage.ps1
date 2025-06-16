<#[
.SYNOPSIS
    Initializes the About Page for the Bucket application

.DESCRIPTION
    Initializes the About Page, sets up its data context, and handles event bindings for UI elements.

.NOTES
    Name:        Initialize-BucketAboutPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     25.6.3.4
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
        $script:aboutPage_AppVersion = $script:BucketVersion
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
            $poShLogModule = Get-Module -Name "PoShLog" -ErrorAction SilentlyContinue
            $script:aboutPage_Modules = @(
                [PSCustomObject]@{
                    About_Modules_Name    = "PoShLog"
                    About_Modules_Version = if ($poShLogModule) { $poShLogModule.Version.ToString() } else { "" }
                    About_Modules_Status  = if ($poShLogModule) { "Loaded" } else { "Not Loaded" }
                }
            )
        }
        catch {
            Write-BucketLog -Data "Error getting module information: $_" -Level Warning
        }
        #endregion Data Initialization

        #region Event Handlers
        $pageLoadedHandler = {
            param($senderObj, $e)

            Write-BucketLog -Data "AboutPage loaded, setting up handlers" -Level Info

            #region UI Element References
            $page = $senderObj
            $githubButton = $page.FindName("About_GitHubButton")
            $licenseButton = $page.FindName("About_LicenseButton")
            $issueButton = $page.FindName("About_ReportIssueButton")
            $checkUpdateButton = $page.FindName("About_CheckUpdateButton")
            $githubProfileLink = $page.FindName("About_GitHubProfileLink")
            $modulesDataGrid = $page.FindName("About_ModulesDataGrid")
            #endregion UI Element References

            #region DataGrid Setup
            if ($modulesDataGrid) {
                $modulesToUse = $script:aboutPage_Modules
                Write-BucketLog -Data "Setting up ModulesDataGrid with $($modulesToUse.Count) items" -Level Info
                try {
                    $modulesList = New-Object System.Collections.ArrayList
                    foreach ($module in $modulesToUse) {
                        [void]$modulesList.Add($module)
                    }
                    $modulesDataGrid.ItemsSource = $null
                    $modulesDataGrid.ItemsSource = $modulesList
                    $modulesDataGrid.UpdateLayout()
                    $modulesDataGrid.Items.Refresh()
                    Write-BucketLog -Data "ModulesDataGrid ItemsSource set ($($modulesList.Count) items)" -Level Info
                }
                catch {
                    Write-BucketLog -Data "Error setting ItemsSource for ModulesDataGrid: $_" -Level Error
                }
            }
            #endregion DataGrid Setup

            #region Button Event Handlers
            if ($githubButton) {
                $githubButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "GitHub button clicked" -Level Info
                        Start-Process "https://github.com/mchave3/Bucket"
                    })
            }
            if ($licenseButton) {
                $licenseButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "License button clicked" -Level Info
                        Start-Process "https://github.com/mchave3/Bucket/blob/main/LICENSE"
                    })
            }
            if ($issueButton) {
                $issueButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Report Issue button clicked" -Level Info
                        Start-Process "https://github.com/mchave3/Bucket/issues/new"
                    })
            }
            if ($checkUpdateButton) {
                $checkUpdateButton.Add_Click({
                        param($senderObj, $e)
                        Write-BucketLog -Data "Check for Updates button clicked" -Level Info
                        if (Get-Command -Name "Test-BucketUpdate" -ErrorAction SilentlyContinue) {
                            try {
                                $updateResult = Test-BucketUpdate
                                if ($updateResult.UpdateAvailable) {
                                    [System.Windows.MessageBox]::Show(
                                        "A new version ($($updateResult.LatestVersion)) is available. Your current version is $($script:aboutPage_AppVersion).`n`nPlease visit the GitHub repository to download the latest version.",
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
                                Write-BucketLog -Data "Error checking for updates: $_" -Level Error
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
            if ($githubProfileLink) {
                $githubProfileLink.Add_MouseLeftButtonDown({
                        param($senderObj, $e)
                        Write-BucketLog -Data "GitHub Profile link clicked" -Level Info
                        $e.Handled = $true
                        Start-Process "https://github.com/mchave3"
                    })
            }
            #endregion Button Event Handlers

            Write-BucketLog -Data "AboutPage event handlers configured successfully" -Level Info
        }
        #endregion Event Handlers

        #region Data Context Creation
        $dataContext = [PSCustomObject]@{
            AppVersion        = $script:aboutPage_AppVersion
            BuildDate         = $script:aboutPage_BuildDate
            Author            = $script:aboutPage_Author
            Repository        = $script:aboutPage_Repository
            License           = $script:aboutPage_License
            PowerShellVersion = $script:aboutPage_PowerShellVersion
            OSVersion         = $script:aboutPage_OSVersion
            DotNetVersion     = $script:aboutPage_DotNetVersion
            ModulesInfo       = $script:aboutPage_Modules
            PageLoaded        = $pageLoadedHandler
        }
        Invoke-BucketNavigationService -PageTag "aboutPage" -RootFrame $WPF_MainWindow_RootFrame -DataContext $dataContext
        #endregion Data Context Creation
    }
}
