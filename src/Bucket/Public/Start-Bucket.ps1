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
        $moduleRoot = Split-Path -Parent $PSScriptRoot
        $xamlPath = Join-Path -Path $moduleRoot -ChildPath "GUI\MainWindow.xaml"
        
        if (-not (Test-Path -Path $xamlPath)) {
            Write-BucketLog -Data "Could not find MainWindow.xaml at $xamlPath" -Level Error
            exit 1
        }
        
        $inputXaml = Get-Content -Path $xamlPath -Raw
        $inputXaml = $inputXaml -replace 'BucketVer', $script:BucketVersion

        [xml]$xaml = $inputXaml

        $reader = (New-Object System.Xml.XmlNodeReader $xaml)
        Write-BucketLog -Data "$reader" -Level Verbose
        Write-BucketLog -Data "$xaml" -Level Verbose
        try {
            $form = [Windows.Markup.XamlReader]::Load($reader)
        }
        catch {
            Write-BucketLog -Data "Failed to load XAML: $_" -Level Error
            exit 1
        }

        # Load the XAML objects into variables
        $xaml.SelectNodes('//*[@Name]') | ForEach-Object { "trying item $($_.Name)" | Out-Null
            try { Set-Variable -Name "WPF$($_.Name)" -Value $form.FindName($_.Name) -ErrorAction Stop }
            catch { throw }
        }
        #endregion XAML

        #region GUI Events
        # Create a hashtable to store page types
        $script:pages = @{
            homePage          = [type]"Bucket.GUI.HomePage"
            selectImagePage   = [type]"Bucket.GUI.SelectImagePage"
            customizationPage = [type]"Bucket.GUI.CustomizationPage"
            applicationsPage  = [type]"Bucket.GUI.ApplicationsPage"
            driversPage       = [type]"Bucket.GUI.DriversPage"
            configPage        = [type]"Bucket.GUI.ConfigPage"
            aboutPage         = [type]"Bucket.GUI.AboutPage"
        }

        # Create a data context for binding
        $script:dataContext = [PSCustomObject]@{
            BucketVersion          = $script:BucketVersion
            WorkingDirectory       = $script:workingDirectory
            MountDirectory         = (Join-Path -Path $script:workingDirectory -ChildPath 'Mount')
            CompletedWIMsDirectory = (Join-Path -Path $script:workingDirectory -ChildPath 'CompletedWIMs')
            DiskSpaceInfo          = "Espace disponible: $(Get-PSDrive -Name 'C').Free / $(Get-PSDrive -Name 'C').Used"
            MountedImagesCount     = 0
            ImageMountStatus       = "Aucune image montée"
            CurrentImageInfo       = "Veuillez sélectionner et monter une image Windows pour commencer la personnalisation."
            PendingDriversCount    = 0
            InstalledDriversCount  = 0
            SelectedAppsCount      = 0
        }

        # Initialize navigation events
        $WPFRootNavigation.add_Loaded({
                param($sender, $e)
                # Set the initial page to home page
                Invoke-BucketGuiNav -PageTag "homePage"
            })

        # Handle navigation item selection
        $WPFRootNavigation.add_ItemInvoked({
                param($sender, $e)
                Invoke-BucketGuiNav -PageTag $e.InvokedItem.Tag
            })

        # Handle menu items in the tray
        $WPFMenuShowWindow.add_Click({
                $form.Show()
                $form.WindowState = "Normal"
            })

        $WPFMenuAbout.add_Click({
                Invoke-BucketGuiNav -PageTag "aboutPage"
                $form.Show()
                $form.WindowState = "Normal"
            })

        $WPFMenuExit.add_Click({
                $form.Close()
            })

        # Set window title with version info
        $form.Title = "Bucket $($script:BucketVersion) - Windows Image Customizer"
        #endregion GUI Events

        #region Start GUI
        $form.ShowDialog() | Out-Null
        #endregion Start GUI
    }
}
