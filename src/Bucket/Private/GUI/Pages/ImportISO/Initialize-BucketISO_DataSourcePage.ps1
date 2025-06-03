<#
.SYNOPSIS
    Initializes the Data Source Page for Bucket ISO import

.DESCRIPTION
    Sets up the UI elements and event handlers for the Data Source Page
    in the Bucket ISO import wizard. This includes handling file selection,
    output path configuration, and radio button state management.
    The page is designed to allow users to select an ISO file and specify
    the output directory for extracted WIM files.

.NOTES
    Name:        Initialize-BucketISO_DataSourcePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/16/2025
    Version:     25.6.3.4
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-BucketISO_DataSourcePage
#>
function Initialize-BucketISO_DataSourcePage {
    [CmdletBinding()]
    param()

    process {
        #region Initialization
        Write-BucketLog -Data "Initializing data source page" -Level Info
        #endregion

        #region Page Logic
        $pageLoadedHandler = {
            param($senderObj, $e)

            # Store page reference for event access
            $script:ImportISOCurrentPage = $senderObj
            Write-BucketLog -Data "Data source page loaded" -Level Info

            # Retrieve UI controls
            $browseISOButton = $senderObj.FindName("ImportISO_DataSource_BrowseISOButton")
            $browseOutputButton = $senderObj.FindName("ImportISO_DataSource_BrowseOutputButton")
            $useDefaultLocation = $senderObj.FindName("ImportISO_DataSource_UseDefaultLocation")
            $useCustomLocation = $senderObj.FindName("ImportISO_DataSource_UseCustomLocation")
            $isoPathTextBox = $senderObj.FindName("ImportISO_DataSource_ISOPathTextBox")
            $outputPathTextBox = $senderObj.FindName("ImportISO_DataSource_OutputPathTextBox")

            # Set initial values for textboxes and output path
            if ($isoPathTextBox -and -not [string]::IsNullOrWhiteSpace($script:ImportISO_DataContext.ISOSourcePath)) {
                $isoPathTextBox.Text = $script:ImportISO_DataContext.ISOSourcePath
            }
            if ($outputPathTextBox) {
                $outputPathTextBox.Text = $script:ImportISO_DataContext.OutputPath
            }

            # Set radio button states (default: use default location)
            if ($useDefaultLocation) { $useDefaultLocation.IsChecked = $true }
            if ($useCustomLocation) { $useCustomLocation.IsChecked = $false }
            $script:ImportISO_DataContext.UseCustomLocation = $false

            # ISO file browse button event
            if ($browseISOButton) {
                $browseISOButton.Add_Click({
                        $openFileDialog = New-Object Microsoft.Win32.OpenFileDialog
                        $openFileDialog.Filter = "ISO Files (*.iso)|*.iso|All Files (*.*)|*.*"
                        $openFileDialog.Title = "Select an ISO file"
                        if ($openFileDialog.ShowDialog()) {
                            $script:ImportISO_DataContext.ISOSourcePath = $openFileDialog.FileName
                            $textBox = $script:importISOCurrentPage.FindName("ImportISO_DataSource_ISOPathTextBox")
                            if ($textBox) { $textBox.Text = $openFileDialog.FileName }
                            Write-BucketLog -Data "ISO file selected: $($openFileDialog.FileName)" -Level Info
                        }
                    })
            }

            # Output directory browse button event
            if ($browseOutputButton) {
                $browseOutputButton.Add_Click({
                        $folderBrowserDialog = New-Object System.Windows.Forms.FolderBrowserDialog
                        $folderBrowserDialog.Description = "Select output directory for extracted WIM"
                        if ($folderBrowserDialog.ShowDialog() -eq 'OK') {
                            $script:ImportISO_DataContext.OutputPath = $folderBrowserDialog.SelectedPath
                            $textBox = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                            if ($textBox) { $textBox.Text = $folderBrowserDialog.SelectedPath }
                            Write-BucketLog -Data "Custom output directory selected: $($folderBrowserDialog.SelectedPath)" -Level Info
                        }
                    })
            }            # Radio button event: use default location
            if ($useDefaultLocation) {
                $useDefaultLocation.Add_Checked({
                        $script:ImportISO_DataContext.UseCustomLocation = $false
                        # Set the default path (ImportedWIMs in the working directory)
                        $defaultPath = Join-Path -Path $script:workingDirectory -ChildPath "ImportedWIMs"
                        $script:ImportISO_DataContext.OutputPath = $defaultPath

                        # Update UI
                        $browseBtn = $script:importISOCurrentPage.FindName("ImportISO_DataSource_BrowseOutputButton")
                        if ($browseBtn) { $browseBtn.IsEnabled = $false }
                        $outputTxt = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                        if ($outputTxt) {
                            $outputTxt.IsEnabled = $false
                            $outputTxt.Text = $defaultPath
                        }
                        Write-BucketLog -Data "Using default output directory: $defaultPath" -Level Info
                    })
            }
            # Radio button event: use custom location
            if ($useCustomLocation) {
                $useCustomLocation.Add_Checked({
                        $script:ImportISO_DataContext.UseCustomLocation = $true
                        $browseBtn = $script:importISOCurrentPage.FindName("ImportISO_DataSource_BrowseOutputButton")
                        if ($browseBtn) { $browseBtn.IsEnabled = $true }
                        $outputTxt = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                        if ($outputTxt) { $outputTxt.IsEnabled = $true }
                        Write-BucketLog -Data "Using custom output directory" -Level Info
                    })
            }

            Write-BucketLog -Data "Data source page event handlers configured" -Level Debug
        }
        #endregion

        #region DataContext & Navigation
        $dataContext = [PSCustomObject]@{
            ISOSourcePath       = $script:ImportISO_DataContext.ISOSourcePath
            OutputPath          = $script:ImportISO_DataContext.OutputPath
            UseCustomLocation   = $false
            DefaultLocationText = "Use default Bucket directory: $($script:ImportISO_DataContext.OutputPath)"
            PageLoaded          = $pageLoadedHandler
        }
        Write-BucketLog -Data "Data context for data source page created" -Level Debug

        Invoke-BucketNavigationService -PageTag "dataSourcePage" `
            -RootFrame $WPF_ImportISO_MainWindow_MainFrame `
            -XamlBasePath (Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement") `
            -PageDictionary $script:ImportISOPages `
            -DataContext $dataContext `
            -GlobalDataContext $script:ImportISO_DataContext
        Write-BucketLog -Data "Data source page navigation started" -Level Debug
        #endregion
    }
}
