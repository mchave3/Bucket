<#
.SYNOPSIS
    Brief description of the script purpose

.DESCRIPTION
    Detailed description of what the script/function does

.NOTES
    Name:        Start-Bucket.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Example of how to use this script/function
#>
function Start-Bucket {

    #Requires -Version 5.1
    #Requires -Modules PoShLog

    [CmdletBinding(SupportsShouldProcess)]
    param(

    )

    begin {
        Clear-Host
    }

    process {
        if ($PSCmdlet.ShouldProcess("Starting Bucket", "Initialize Bucket module")) {
            # Start the Bucket module Pre-flight checks and setup
            Invoke-BucketPreFlight
        }

        # Get the current version of the Bucket module
        Get-BucketVersion

        #region XAML
        # Load the XAML file for the GUI
        #$moduleRoot = Split-Path -Parent $PSScriptRoot
        $xamlPath = "$PSScriptRoot\GUI\MainWindow.xaml"
        
        if (-not (Test-Path -Path $xamlPath)) {
            Write-BucketLog -Data "Could not find MainWindow.xaml at $xamlPath" -Level Error
            exit 1
        }
        
        $inputXaml = Get-Content -Path $xamlPath -Raw
        $inputXaml = $inputXaml -replace 'BucketVer', $script:BucketVersion
        
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
        # Create a hashtable to store page references
        $script:pages = @{
            homePage        = "Bucket.GUI.HomePage" 
            selectImagePage = "Bucket.GUI.SelectImagePage"
            aboutPage       = "Bucket.GUI.AboutPage"
        }

        # Global data context for the GUI
        $script:globalDataContext = [PSCustomObject]@{
            BucketVersion    = $script:BucketVersion
            WorkingDirectory = $script:workingDirectory
        }

        # Navigation function to handle page changes
        $WPF_MainWindow_NavHome.add_Click({
                Invoke-BucketHomePage
            })

        # Handle navigation for other UI elements
        $WPF_MainWindow_NavSelectImage.add_Click({
                Invoke-BucketSelectImagePage
            })
        
        $WPF_MainWindow_NavAbout.add_Click({
                Invoke-BucketAboutPage
            })

        # Set window title with version info
        $form.Title = "Bucket $($script:BucketVersion) - Windows Image Customizer"

        # Set the initial page to home page when the form is loaded
        $form.add_Loaded({
                if ($WPF_MainWindow_RootFrame) {
                    Write-BucketLog -Data "Form loaded, navigating to home page" -Level Info
                    Invoke-BucketHomePage
                }
            })
        #endregion GUI Events

        #region Start GUI
        $form.ShowDialog() | Out-Null
        #endregion Start GUI
    }
}
