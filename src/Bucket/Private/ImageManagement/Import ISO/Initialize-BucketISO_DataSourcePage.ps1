﻿<#
.SYNOPSIS
    Initializes the Data Source Page for Bucket ISO import

.DESCRIPTION
    Sets up the data context and event handlers for the Bucket ISO import data source page

.NOTES
    Name:        Initialize-BucketISO_DataSourcePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/16/2025
    Version:     1.0.0
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
        Write-BucketLog -Data "Initializing ISO import data source page" -Level Info

        #region Page Setup
        # Get the default ImportedWIMs directory
        $defaultOutputPath = Join-Path -Path $script:workingDirectory -ChildPath "ImportedWIMs"
        $script:ImportISODataContext.DefaultImportedWims = $defaultOutputPath

        # Store page reference globally so event handlers can access it

        # Page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)

            # Store page reference for access in events
            $script:importISOCurrentPage = $senderObj

            Write-BucketLog -Data "ISO import data source page loaded" -Level Info

            # Find UI controls
            $browseISOButton = $senderObj.FindName("ImportISO_DataSource_BrowseISOButton")
            $browseOutputButton = $senderObj.FindName("ImportISO_DataSource_BrowseOutputButton")
            $useDefaultLocation = $senderObj.FindName("ImportISO_DataSource_UseDefaultLocation")
            $useCustomLocation = $senderObj.FindName("ImportISO_DataSource_UseCustomLocation")
            $isoPathTextBox = $senderObj.FindName("ImportISO_DataSource_ISOPathTextBox")
            $outputPathTextBox = $senderObj.FindName("ImportISO_DataSource_OutputPathTextBox")

            # Set initial values
            if ($isoPathTextBox -and -not [string]::IsNullOrWhiteSpace($script:ImportISODataContext.ISOSourcePath)) {
                $isoPathTextBox.Text = $script:ImportISODataContext.ISOSourcePath
            }

            # Initialize default path if not set
            if (-not $script:ImportISODataContext.OutputPath) {
                $script:ImportISODataContext.OutputPath = $defaultOutputPath
            }

            if ($outputPathTextBox) {
                $outputPathTextBox.Text = $script:ImportISODataContext.OutputPath
            }

            # Set radio button states - Always select default location first
            if ($useDefaultLocation) {
                $useDefaultLocation.IsChecked = $true
            }

            if ($useCustomLocation) {
                $useCustomLocation.IsChecked = $false
            }

            # Now update the output path based on the selected radio button
            $script:ImportISODataContext.UseCustomLocation = $false

            # Ensure output path is set to default path
            $script:ImportISODataContext.OutputPath = $defaultOutputPath

            # Set up event handlers for buttons
            if ($browseISOButton) {
                $browseISOButton.Add_Click({
                        $openFileDialog = New-Object Microsoft.Win32.OpenFileDialog
                        $openFileDialog.Filter = "ISO Files (*.iso)|*.iso|All Files (*.*)|*.*"
                        $openFileDialog.Title = "Select an ISO file"

                        if ($openFileDialog.ShowDialog()) {
                            $script:ImportISODataContext.ISOSourcePath = $openFileDialog.FileName

                            # Access the TextBox through the stored page reference
                            $textBox = $script:importISOCurrentPage.FindName("ImportISO_DataSource_ISOPathTextBox")
                            if ($textBox) {
                                $textBox.Text = $openFileDialog.FileName
                            }

                            Write-BucketLog -Data "Selected ISO file: $($openFileDialog.FileName)" -Level Info
                        }
                    })
            }

            if ($browseOutputButton) {
                $browseOutputButton.Add_Click({
                        $folderBrowserDialog = New-Object System.Windows.Forms.FolderBrowserDialog
                        $folderBrowserDialog.Description = "Select output directory for extracted WIM"

                        if ($folderBrowserDialog.ShowDialog() -eq 'OK') {
                            $script:ImportISODataContext.OutputPath = $folderBrowserDialog.SelectedPath

                            # Access the TextBox through the stored page reference
                            $textBox = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                            if ($textBox) {
                                $textBox.Text = $folderBrowserDialog.SelectedPath
                            }

                            Write-BucketLog -Data "Selected custom output directory: $($folderBrowserDialog.SelectedPath)" -Level Info
                        }
                    })
            }

            # Radio button change events
            if ($useDefaultLocation) {
                $useDefaultLocation.Add_Checked({
                        $script:ImportISODataContext.UseCustomLocation = $false
                        $script:ImportISODataContext.OutputPath = $defaultOutputPath

                        # Update the browse button state directly (disable it)
                        $browseBtn = $script:importISOCurrentPage.FindName("ImportISO_DataSource_BrowseOutputButton")
                        if ($browseBtn) {
                            $browseBtn.IsEnabled = $false
                        }

                        # Also update the output textbox state (disable it)
                        $outputTxt = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                        if ($outputTxt) {
                            $outputTxt.IsEnabled = $false
                        }

                        Write-BucketLog -Data "Using default output directory $defaultOutputPath" -Level Info
                    })
            }

            if ($useCustomLocation) {
                $useCustomLocation.Add_Checked({
                        $script:ImportISODataContext.UseCustomLocation = $true

                        # Update the browse button state directly
                        $browseBtn = $script:importISOCurrentPage.FindName("ImportISO_DataSource_BrowseOutputButton")
                        if ($browseBtn) {
                            $browseBtn.IsEnabled = $true
                        }

                        # Also update the output textbox state
                        $outputTxt = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                        if ($outputTxt) {
                            $outputTxt.IsEnabled = $true
                        }

                        Write-BucketLog -Data "Using custom output directory" -Level Info
                    })
            }

            Write-BucketLog -Data "ISO import data source page event handlers configured" -Level Info
        }

        # Create the data context for the page with necessary data and handlers
        $dataContext = [PSCustomObject]@{
            # Data properties (bound to UI) - these should match what the XAML expects
            ISOSourcePath       = $script:ImportISODataContext.ISOSourcePath
            OutputPath          = $script:ImportISODataContext.OutputPath
            UseCustomLocation   = $false # Default to not using custom location
            DefaultLocationText = "Use default Bucket directory: $defaultOutputPath"

            # Event handler for page loaded event
            PageLoaded          = $pageLoadedHandler
        }

        # Log the data context creation for debugging
        Write-BucketLog -Data "Created ISO import data source page context" -Level Info

        # Use the generic navigation service - the Next button is already handled in Import-BucketISO.ps1
        Invoke-BucketNavigationService -PageTag "dataSourcePage" `
            -RootFrame $WPF_MainWindow_ImportISO_MainFrame `
            -XamlBasePath (Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement") `
            -PageDictionary $script:ImportISOPages `
            -DataContext $dataContext `
            -GlobalDataContext $script:ImportISODataContext

        Write-BucketLog -Data "Data source page navigation initiated" -Level Debug
    }
}
