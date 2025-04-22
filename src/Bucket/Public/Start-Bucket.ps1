<#
.SYNOPSIS
    Start the Bucket GUI application.

.DESCRIPTION
    Initializes and launches the Bucket GUI application, performing necessary pre-flight checks and loading the user interface.

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
    Start-Bucket
#>
function Start-Bucket {
    #Requires -Version 5.1
    #Requires -Modules PoShLog

    [CmdletBinding(SupportsShouldProcess)]
    param()

    begin {
        Clear-Host
    }

    process {
        #region Pre-flight & Version
        if ($PSCmdlet.ShouldProcess("Starting Bucket", "Initialize Bucket module")) {
            # Run pre-flight checks and setup
            Invoke-BucketPreFlight
        }
        # Get the current version of the Bucket module
        Get-BucketVersion
        #endregion

        #region XAML Loading
        # Load the XAML file for the GUI
        $xamlPath = Join-Path -Path $PSScriptRoot -ChildPath "GUI\MainWindow.xaml"
        if (-not (Test-Path -Path $xamlPath)) {
            Write-BucketLog -Data "[Bucket] Could not find MainWindow.xaml at $xamlPath" -Level Error
            exit 1
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
        Write-BucketLog -Data "[Bucket] XAML parsed successfully" -Level Verbose
        try {
            $reader = New-Object System.Xml.XmlNodeReader $xamlDoc
            $form = [Windows.Markup.XamlReader]::Load($reader)
            Write-BucketLog -Data "[Bucket] XAML loaded successfully" -Level Info
        }
        catch {
            Write-BucketLog -Data "[Bucket] Failed to load XAML: $_" -Level Error
            exit 1
        }
        # Load the XAML objects into variables
        $namedNodes = $xamlDoc.SelectNodes("//*[@x:Name]", $nsManager)
        if ($namedNodes -and $namedNodes.Count -gt 0) {
            Write-BucketLog -Data "[Bucket] Found $($namedNodes.Count) named elements in XAML" -Level Debug
            foreach ($node in $namedNodes) {
                $elementName = $node.GetAttribute('Name', 'http://schemas.microsoft.com/winfx/2006/xaml')
                Write-BucketLog -Data "[Bucket] Processing XAML element: $elementName" -Level Verbose
                try {
                    $element = $form.FindName($elementName)
                    if ($element) {
                        $varName = "WPF_$elementName"
                        Set-Variable -Name $varName -Value $element -Scope Script
                        Write-BucketLog -Data "[Bucket] Found UI element: $elementName -> $varName" -Level Debug
                    }
                    else {
                        Write-BucketLog -Data "[Bucket] UI element not found in form: $elementName" -Level Warning
                    }
                }
                catch {
                    Write-BucketLog -Data "[Bucket] Error processing UI element $elementName : $_" -Level Error
                }
            }
        }
        else {
            Write-BucketLog -Data "[Bucket] No named elements found in XAML" -Level Warning
        }
        #endregion

        #region GUI Events & Navigation
        # Store page references
        $script:pages = @{
            homePage        = "Bucket.GUI.HomePage"
            selectImagePage = "Bucket.GUI.SelectImagePage"
            aboutPage       = "Bucket.GUI.AboutPage"
        }
        # Initialize the global data context
        $script:globalDataContext  = [PSCustomObject]@{
            BucketVersion          = $script:BucketVersion
            WorkingDirectory       = $script:workingDirectory
            MountDirectory         = Join-Path -Path $WorkingDirectory -ChildPath "Mount"
            CompletedWimsDirectory = Join-Path -Path $WorkingDirectory -ChildPath "CompletedWIMs"
        }
        # Navigation for main UI elements
        $WPF_MainWindow_NavHome.add_Click({ Invoke-BucketHomePage })
        $WPF_MainWindow_NavSelectImage.add_Click({ Invoke-BucketSelectImagePage })
        $WPF_MainWindow_NavAbout.add_Click({ Invoke-BucketAboutPage })
        # Set window title with version info
        $form.Title = "Bucket $($script:BucketVersion) - Windows Image Customizer"
        # Set the initial page to home page when the form is loaded
        $form.add_Loaded({
                if ($WPF_MainWindow_RootFrame) {
                    Write-BucketLog -Data "[Bucket] Form loaded, initializing navigation system and navigating to home page" -Level Info
                    Invoke-BucketHomePage
                }
            })
        #endregion

        #region Start GUI
        $form.ShowDialog() | Out-Null
        #endregion
    }
}
