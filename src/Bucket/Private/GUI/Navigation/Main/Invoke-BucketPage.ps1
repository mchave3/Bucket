<#
.SYNOPSIS
    Utility to handle the common pattern of initializing or navigating to a Bucket page.
.DESCRIPTION
    This function checks for an Initialize-Bucket[Page]Page function and calls it if present. Otherwise, it falls back to Invoke-BucketNavigationService with the provided parameters.
    This avoids code duplication in all Invoke-Bucket[Page]Page functions.
.NOTES
    Name:        Invoke-BucketPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/17/2025
    Version:     25.6.3.4
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License
.LINK
    https://github.com/mchave3/Bucket
.EXAMPLE
    Invoke-BucketPage -PageTag "homePage" -RootFrame $WPF_MainWindow_RootFrame -InitFunction "Initialize-BucketHomePage" -NavigationServiceParams @{ DataContext = $script:globalDataContext }
#>

function Invoke-BucketPage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $true)]
        [System.Windows.Controls.Frame]$RootFrame,

        [Parameter(Mandatory = $true)]
        [string]$InitFunction,

        [Parameter(Mandatory = $false)]
        [hashtable]$NavigationServiceParams = @{}
    )

    process {
        # Log navigation attempt
        Write-BucketLog -Data "Invoke-BucketPage: Navigating to $PageTag via $InitFunction" -Level Debug

        #region Main Logic
        # Check if the initialization function exists, otherwise fallback to navigation service
        if (Get-Command -Name $InitFunction -ErrorAction SilentlyContinue) {
            # Call the initialization function for the page
            & $InitFunction
        }
        else {
            Write-BucketLog -Data "$InitFunction not found, using navigation service fallback" -Level Warning
            $params = @{
                PageTag   = $PageTag
                RootFrame = $RootFrame
            }
            if ($NavigationServiceParams) {
                $params += $NavigationServiceParams
            }
            Invoke-BucketNavigationService @params
        }
        #endregion Main Logic
    }
}
