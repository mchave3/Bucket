<#
.SYNOPSIS
    Brief description of the script purpose

.DESCRIPTION
    Detailed description of what the script/function does

.NOTES
    Name:        Import-BucketISO.ps1
    Author:      Mickaël CHAVE
    Created:     04/16/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Import-BucketISO
#>
function Import-BucketISO {
    [CmdletBinding()]
    param(

    )

    process {
        # Prevent multiple instances from being opened simultaneously
        if ($script:ImportISO_WindowOpen) {
            Write-BucketLog -Data "ISO import window already open, bringing to front" -Level Info
            return
        }

        # Set window state
        $script:ImportISO_WindowOpen = $true
        #region XAML
        # Load the XAML file for the GUI
        $xamlPath = Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement\MainWindow_ImportISO.xaml"

        Write-BucketLog -Data "Chemin du fichier XAML: $xamlPath" -Level Debug

        if (-not (Test-Path -Path $xamlPath)) {
            Write-BucketLog -Data "Could not find MainWindow_ImportISO.xaml at $xamlPath" -Level Error
            $script:ImportISO_WindowOpen = $false
            return
        }

        $inputXaml = Get-Content -Path $xamlPath -Raw

        # Clean up the XAML for PowerShell's XML parser
        $inputXaml = $inputXaml -replace 'xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"', ''
        $inputXaml = $inputXaml -replace 'xmlns:d="http://schemas.microsoft.com/expression/blend/2008"', ''
        $inputXaml = $inputXaml -replace 'mc:Ignorable="d"', ''
        $inputXaml = $inputXaml -replace 'x:Class="[^"]*"', ''

        # Create the XML document
        $xamlDoc = New-Object System.Xml.XmlDocument
        $xamlDoc.LoadXml($inputXaml)

        # Add namespaces for XPath queries
        $nsManager = New-Object System.Xml.XmlNamespaceManager($xamlDoc.NameTable)
        $nsManager.AddNamespace('x', 'http://schemas.microsoft.com/winfx/2006/xaml')

        Write-BucketLog -Data "XAML parsed successfully" -Level Verbose

        try {
            # Load XAML as a WPF object
            $reader = New-Object System.Xml.XmlNodeReader $xamlDoc
            $form = [Windows.Markup.XamlReader]::Load($reader)
            Write-BucketLog -Data "XAML loaded successfully" -Level Info
        }
        catch {
            Write-BucketLog -Data "Failed to load XAML: $_" -Level Error
            exit 1
        }

        # Load the XAML objects into variables
        $namedNodes = $xamlDoc.SelectNodes("//*[@x:Name]", $nsManager)

        if ($namedNodes -and $namedNodes.Count -gt 0) {
            Write-BucketLog -Data "Found $($namedNodes.Count) named elements in XAML" -Level Debug

            foreach ($node in $namedNodes) {
                $elementName = $node.GetAttribute('Name', 'http://schemas.microsoft.com/winfx/2006/xaml')
                Write-BucketLog -Data "Processing XAML element: $elementName" -Level Verbose

                try {
                    $element = $form.FindName($elementName)
                    if ($element) {
                        $varName = "WPF_$elementName"
                        Set-Variable -Name $varName -Value $element -Scope Script
                        Write-BucketLog -Data "Found UI element: $elementName -> $varName" -Level Debug
                    }
                    else {
                        Write-BucketLog -Data "UI element not found in form: $elementName" -Level Warning
                    }
                }
                catch {
                    Write-BucketLog -Data "Error processing UI element $elementName : $_" -Level Error
                }
            }
        }
        else {
            Write-BucketLog -Data "No named elements found in XAML" -Level Warning
        }
        #endregion XAML

        #region GUI Events
        # Create a hashtable to store page references for ISO import wizard
        $script:ImportISOPages = @{
            # Follow existing naming conventions for pages
            dataSourcePage  = "Bucket.GUI.ImageManagement.ImportISO_DataSourcePage"
            selectIndexPage = "Bucket.GUI.ImageManagement.ImportISO_SelectIndexPage"
            summaryPage     = "Bucket.GUI.ImageManagement.ImportISO_SummaryPage"
            progressPage    = "Bucket.GUI.ImageManagement.ImportISO_ProgressPage"
            completionPage  = "Bucket.GUI.ImageManagement.ImportISO_CompletionPage"
        }

        # Initialize the data context for the ISO import wizard
        # Define default paths
        $defaultImportedWimsPath = Join-Path -Path $script:workingDirectory -ChildPath "ImportedWIMs"

        $script:ImportISODataContext = [PSCustomObject]@{
            # Common properties
            BucketVersion          = $script:BucketVersion
            WorkingDirectory       = $script:workingDirectory
            MountDirectory         = Join-Path -Path $script:workingDirectory -ChildPath "Mount"
            CompletedWimsDirectory = Join-Path -Path $script:workingDirectory -ChildPath "CompletedWIMs"
            DefaultImportedWims    = $defaultImportedWimsPath

            # ISO import specific properties
            ISOSourcePath          = ""
            SelectedIndex          = ""
            OutputPath             = $defaultImportedWimsPath  # Initialize with default path when UseDefaultLocation is true
            UseDefaultLocation     = $true  # Default: use default location
            UseCustomLocation      = $false # Default: don't use custom location
            AvailableIndices       = @()
            CurrentPage            = "dataSourcePage"
            ProcessComplete        = $false
            ExtractedWimPath       = ""
        }

        # Configure navigation buttons
        # Previous button
        $WPF_MainWindow_ImportISO_PreviousButton.add_Click({
                # Get previous page based on current page
                $previousPage = switch ($script:ImportISODataContext.CurrentPage) {
                    "selectIndexPage" { "dataSourcePage" }
                    "summaryPage" { "selectIndexPage" }
                    "progressPage" { "summaryPage" }
                    "completionPage" { "progressPage" }
                    default { $null }
                }

                if ($previousPage) {
                    Invoke-BucketISOPage -PageTag $previousPage
                }
            })

        # Next button
        $WPF_MainWindow_ImportISO_NextButton.add_Click({
                # Validate current page before proceeding
                $canContinue = $true

                # Validation logic based on current page
                switch ($script:ImportISODataContext.CurrentPage) {
                    "dataSourcePage" {
                        if ([string]::IsNullOrWhiteSpace($script:ImportISODataContext.ISOSourcePath) -or
                            [string]::IsNullOrWhiteSpace($script:ImportISODataContext.OutputPath)) {
                            [System.Windows.MessageBox]::Show("Please select both an ISO file and output directory.", "Missing Information", "OK", "Warning")
                            $canContinue = $false
                        }
                    }
                    "selectIndexPage" {
                        if ([string]::IsNullOrWhiteSpace($script:ImportISODataContext.SelectedIndex)) {
                            [System.Windows.MessageBox]::Show("Please select a Windows edition.", "Missing Information", "OK", "Warning")
                            $canContinue = $false
                        }
                    }
                }

                if ($canContinue) {
                    # Get next page based on current page
                    $nextPage = switch ($script:ImportISODataContext.CurrentPage) {
                        "dataSourcePage" { "selectIndexPage" }
                        "selectIndexPage" { "summaryPage" }
                        "summaryPage" { "progressPage" }
                        "progressPage" { "completionPage" }
                        default { $null }
                    }

                    if ($nextPage) {
                        Invoke-BucketISOPage -PageTag $nextPage
                    }
                }
            })

        # Summary button - direct navigation to summary page if data is available
        $WPF_MainWindow_ImportISO_SummaryButton.add_Click({
                if (-not [string]::IsNullOrWhiteSpace($script:ImportISODataContext.ISOSourcePath) -and
                    -not [string]::IsNullOrWhiteSpace($script:ImportISODataContext.SelectedIndex)) {
                    Invoke-BucketISOPage -PageTag "summaryPage"
                }
                else {
                    [System.Windows.MessageBox]::Show("Please complete the previous steps before viewing the summary.", "Missing Information", "OK", "Warning")
                }
            })

        # Cancel button - close the window
        $WPF_MainWindow_ImportISO_CancelButton.add_Click({
                $form.Close()
            })

        # The navigation functions have been moved to separate files:
        # - Invoke-BucketISOPage.ps1: Handles navigation between pages
        # - Update-BucketISONavigationUI.ps1: Updates sidebar styling
        # - Update-BucketISOButtonVisibility.ps1: Controls navigation button visibility

        # Set the initial page when the form is loaded
        $form.add_Loaded({
                if ($WPF_MainWindow_ImportISO_MainFrame) {
                    Write-BucketLog -Data "Form loaded, initializing navigation system and navigating to data source page" -Level Info

                    # Navigate to the first page
                    Invoke-BucketISOPage -PageTag "dataSourcePage"
                }
            })
        #endregion GUI Events

        #region Start GUI
        $form.Add_Closed({
                $script:ImportISO_WindowOpen = $false
                Write-BucketLog -Data "ISO import window closed" -Level Info
            })

        $form.ShowDialog() | Out-Null
        #endregion Start GUI
    }
}
