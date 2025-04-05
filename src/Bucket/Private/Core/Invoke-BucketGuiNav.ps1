<#
.SYNOPSIS
    Brief description of the script purpose

.DESCRIPTION
    Detailed description of what the script/function does

.NOTES
    Name:        Invoke-BucketGuiNav.ps1
    Author:      Mickaël CHAVE
    Created:     04/05/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Example of how to use this script/function
#>
function Invoke-BucketGuiNav {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag
    )

    process {
        Write-BucketLog -Data "Navigating to page: $PageTag" -Level Debug

        # Ensure the page type exists in our dictionary
        if (-not $script:pages.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }

        # Create the page if it doesn't exist
        try {
            $page = New-Object -TypeName $script:pages[$PageTag]
            $page.DataContext = $script:dataContext

            # Navigate to the page
            $WPFRootFrame.Navigate($page)
        }
        catch {
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
        }
    }
}
