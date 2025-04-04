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

        Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase, System.Windows.Forms, System.Drawing 

        [void][System.Reflection.Assembly]::LoadWithPartialName('presentationframework')
        [void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms')

        # load the MahApps.Metro assembly
        $assemblyPath = Join-Path -Path (Split-Path -Parent $PSScriptRoot) -ChildPath "Assemblies"
        $AllDlls = Get-ChildItem -Path $assemblyPath -Recurse -Filter *.dll
        Write-BucketLog -Data "Looking for assemblies in: $assemblyPath" -Level Verbose
        Write-BucketLog -Data "$PSScriptRoot" -Level Verbose
        if ($AllDlls.Count -eq 0) {
            Write-BucketLog -Data "$PSScriptRoot\Assemblies does not contain any DLLs" -Level Error
            exit 1
        }
        foreach ($dll in $AllDlls) {
            try {
                [void][System.Reflection.Assembly]::LoadFrom($dll.FullName)
                Write-BucketLog -Data "Loaded assembly: $($dll.FullName)" -Level Verbose
            } 
            catch {
                Write-BucketLog -Data "Failed to load assembly: $($dll.FullName) - $_" -Level Error
            }
        }

        [xml]$xaml = $inputXaml

        $reader = (New-Object System.Xml.XmlNodeReader $xaml)
        write-bucketlog -Data "$reader" -Level Verbose
        write-bucketlog -Data "$xaml" -Level Verbose
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

        #region Start GUI
        $form.ShowDialog() | Out-Null
        #endregion Start GUI
    }
}
