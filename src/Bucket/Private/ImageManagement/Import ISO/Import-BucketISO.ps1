﻿<#
.SYNOPSIS
    Opens the Windows ISO import wizard to extract and import Windows installation media

.DESCRIPTION
    This function creates and manages a wizard-style interface that guides the user through
    selecting a Windows ISO file, choosing which Windows edition to extract, and importing
    it into Bucket for customization. The wizard prevents multiple instances from being
    opened simultaneously and handles the complete workflow from ISO selection to extraction.

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
    # Opens the Windows ISO import wizard
#>
function Import-BucketISO {
    [CmdletBinding()]
    param()

    process {
        #region Instance Check & XAML Loading
        # Prevent multiple instances from being opened simultaneously
        if ($script:ImportISO_WindowOpen) {
            Write-BucketLog -Data "[ISO Import] Window already open, bringing to front" -Level Info
            return
        }
        $script:ImportISO_WindowOpen = $true

        # Load the XAML file for the GUI
        $xamlPath = Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement\MainWindow_ImportISO.xaml"
        Write-BucketLog -Data "[ISO Import] XAML file path: $xamlPath" -Level Debug

        if (-not (Test-Path -Path $xamlPath)) {
            Write-BucketLog -Data "[ISO Import] Could not find MainWindow_ImportISO.xaml at $xamlPath" -Level Error
            $script:ImportISO_WindowOpen = $false
            return
        }

        $inputXaml = Get-Content -Path $xamlPath -Raw
        # Clean up the XAML for PowerShell's XML parser
        $inputXaml = $inputXaml -replace 'xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"', ''
        $inputXaml = $inputXaml -replace 'xmlns:d="http://schemas.microsoft.com/expression/blend/2008"', ''
        $inputXaml = $inputXaml -replace 'mc:Ignorable="d"', ''
        $inputXaml = $inputXaml -replace 'x:Class="[^"]*"', ''

        $xamlDoc = New-Object System.Xml.XmlDocument
        $xamlDoc.LoadXml($inputXaml)
        $nsManager = New-Object System.Xml.XmlNamespaceManager($xamlDoc.NameTable)
        $nsManager.AddNamespace('x', 'http://schemas.microsoft.com/winfx/2006/xaml')
        Write-BucketLog -Data "[ISO Import] XAML parsed successfully" -Level Verbose

        try {
            $reader = New-Object System.Xml.XmlNodeReader $xamlDoc
            $form = [Windows.Markup.XamlReader]::Load($reader)
            Write-BucketLog -Data "[ISO Import] XAML loaded successfully" -Level Info
        }
        catch {
            Write-BucketLog -Data "[ISO Import] Failed to load XAML: $_" -Level Error
            exit 1
        }

        # Load the XAML objects into variables
        $namedNodes = $xamlDoc.SelectNodes("//*[@x:Name]", $nsManager)
        if ($namedNodes -and $namedNodes.Count -gt 0) {
            Write-BucketLog -Data "[ISO Import] Found $($namedNodes.Count) named elements in XAML" -Level Debug
            foreach ($node in $namedNodes) {
                $elementName = $node.GetAttribute('Name', 'http://schemas.microsoft.com/winfx/2006/xaml')
                Write-BucketLog -Data "[ISO Import] Processing XAML element: $elementName" -Level Verbose
                try {
                    $element = $form.FindName($elementName)
                    if ($element) {
                        $varName = "WPF_$elementName"
                        Set-Variable -Name $varName -Value $element -Scope Script
                        Write-BucketLog -Data "[ISO Import] Found UI element: $elementName -> $varName" -Level Debug
                    }
                    else {
                        Write-BucketLog -Data "[ISO Import] UI element not found in form: $elementName" -Level Warning
                    }
                }
                catch {
                    Write-BucketLog -Data "[ISO Import] Error processing UI element $elementName : $_" -Level Error
                }
            }
        }
        else {
            Write-BucketLog -Data "[ISO Import] No named elements found in XAML" -Level Warning
        }
        #endregion

        #region DataContext & Navigation Setup
        # Store page references for ISO import wizard
        $script:ImportISOPages = @{
            dataSourcePage  = "Bucket.GUI.ImageManagement.ImportISO_DataSourcePage"
            selectIndexPage = "Bucket.GUI.ImageManagement.ImportISO_SelectIndexPage"
            summaryPage     = "Bucket.GUI.ImageManagement.ImportISO_SummaryPage"
            progressPage    = "Bucket.GUI.ImageManagement.ImportISO_ProgressPage"
            completionPage  = "Bucket.GUI.ImageManagement.ImportISO_CompletionPage"
        }

        # Initialize the data context for the ISO import wizard
        $defaultImportedWimsPath = Join-Path -Path $script:workingDirectory -ChildPath "ImportedWIMs"
        $script:ImportISODataContext = [PSCustomObject]@{
            BucketVersion          = $script:BucketVersion
            WorkingDirectory       = $script:workingDirectory
            MountDirectory         = Join-Path -Path $script:workingDirectory -ChildPath "Mount"
            CompletedWimsDirectory = Join-Path -Path $script:workingDirectory -ChildPath "CompletedWIMs"
            DefaultImportedWims    = $defaultImportedWimsPath
            ISOSourcePath          = ""
            SelectedIndex          = ""
            OutputPath             = $defaultImportedWimsPath
            UseDefaultLocation     = $true
            UseCustomLocation      = $false
            DefaultLocationText    = "Use default location: $defaultImportedWimsPath"
            AvailableIndices       = @()
            CurrentPage            = "dataSourcePage"
            ProcessComplete        = $false
            ExtractedWimPath       = ""
        }
        #endregion

        #region GUI Events
        # Previous button
        $WPF_MainWindow_ImportISO_PreviousButton.add_Click({
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
                $canContinue = $true
                switch ($script:ImportISODataContext.CurrentPage) {
                    "dataSourcePage" {
                        if ($script:importISOCurrentPage) {
                            $isoPathTextBox = $script:importISOCurrentPage.FindName("ImportISO_DataSource_ISOPathTextBox")
                            $outputPathTextBox = $script:importISOCurrentPage.FindName("ImportISO_DataSource_OutputPathTextBox")
                            if ($isoPathTextBox -and -not [string]::IsNullOrWhiteSpace($isoPathTextBox.Text)) {
                                $script:ImportISODataContext.ISOSourcePath = $isoPathTextBox.Text
                                Write-BucketLog -Data "[ISO Import] Synchronized ISO path from UI: $($isoPathTextBox.Text)" -Level Debug
                            }
                            if ($script:ImportISODataContext.UseCustomLocation) {
                                if ($outputPathTextBox -and -not [string]::IsNullOrWhiteSpace($outputPathTextBox.Text)) {
                                    $script:ImportISODataContext.OutputPath = $outputPathTextBox.Text
                                    Write-BucketLog -Data "[ISO Import] Synchronized custom output path from UI: $($outputPathTextBox.Text)" -Level Debug
                                }
                            }
                            else {
                                $defaultPath = Join-Path -Path $script:workingDirectory -ChildPath "ImportedWIMs"
                                $script:ImportISODataContext.OutputPath = $defaultPath
                                Write-BucketLog -Data "[ISO Import] Using default output path: $defaultPath" -Level Debug
                            }
                        }
                        if ([string]::IsNullOrWhiteSpace($script:ImportISODataContext.ISOSourcePath) -or
                            [string]::IsNullOrWhiteSpace($script:ImportISODataContext.OutputPath)) {
                            [System.Windows.MessageBox]::Show("Please select both an ISO file and output directory.", "Missing Information", "OK", "Warning")
                            $canContinue = $false
                            Write-BucketLog -Data "[ISO Import] Validation failed - ISO path: '$($script:ImportISODataContext.ISOSourcePath)', Output path: '$($script:ImportISODataContext.OutputPath)'" -Level Warning
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

        # Summary button
        $WPF_MainWindow_ImportISO_SummaryButton.add_Click({
                if (-not [string]::IsNullOrWhiteSpace($script:ImportISODataContext.ISOSourcePath) -and
                    -not [string]::IsNullOrWhiteSpace($script:ImportISODataContext.SelectedIndex)) {
                    Invoke-BucketISOPage -PageTag "summaryPage"
                }
                else {
                    [System.Windows.MessageBox]::Show("Please complete the previous steps before viewing the summary.", "Missing Information", "OK", "Warning")
                }
            })

        # Cancel button
        $WPF_MainWindow_ImportISO_CancelButton.add_Click({
                $form.Close()
            })

        # Set the initial page when the form is loaded
        $form.add_Loaded({
                if ($WPF_MainWindow_ImportISO_MainFrame) {
                    Write-BucketLog -Data "[ISO Import] Form loaded, initializing navigation system and navigating to data source page" -Level Info
                    Invoke-BucketISOPage -PageTag "dataSourcePage"
                }
            })
        #endregion

        #region Start GUI
        $form.Add_Closed({
                $script:ImportISO_WindowOpen = $false
                Write-BucketLog -Data "[ISO Import] Window closed" -Level Info
            })
        $form.ShowDialog() | Out-Null
        #endregion
    }
}
