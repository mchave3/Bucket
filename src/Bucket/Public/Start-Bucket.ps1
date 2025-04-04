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
        $inputXaml = Get-Content -Path "$PSScriptRoot\GUI\BucketGUI.xaml" -Raw

        $inputXaml = $inputXaml -replace 'mc:Ignorable="d"', '' -replace 'x:N', 'N' -replace '^<Win.*', '<Window' -replace 'BucketVer', $script:BucketVersion
        [void][System.Reflection.Assembly]::LoadWithPartialName('presentationframework')
        [void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms')
        [xml]$xaml = $inputXaml

        $reader = (New-Object System.Xml.XmlNodeReader $xaml)
        try {
        $form = [Windows.Markup.XamlReader]::Load($reader)
        } catch {
        Write-Warning @"
Unable to parse XML, with error: $($Error[0])
Ensure that there are NO SelectionChanged or TextChanged properties in your textboxes
(PowerShell cannot process them)
"@
            throw
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
