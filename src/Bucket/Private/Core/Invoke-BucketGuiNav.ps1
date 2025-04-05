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

        # Ensure the page tag exists in our dictionary
        if (-not $script:pages.ContainsKey($PageTag)) {
            Write-BucketLog -Data "Page $PageTag not found in page dictionary." -Level Error
            return
        }

        # Get a reference to the frame
        $rootFrame = $WPFRootFrame
        if (-not $rootFrame) {
            Write-BucketLog -Data "RootFrame UI element not found" -Level Error
            return
        }

        # Create the page
        try {
            # Create page name for display
            $pageName = $script:pages[$PageTag]
            Write-BucketLog -Data "Creating page for: $pageName" -Level Debug
            
            # Create a simple page
            $page = New-Object -TypeName System.Windows.Controls.Page
            
            # Create a StackPanel for page content
            $stackPanel = New-Object -TypeName System.Windows.Controls.StackPanel
            $stackPanel.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 20
            
            # Create a TextBlock with the page name
            $titleBlock = New-Object -TypeName System.Windows.Controls.TextBlock
            $titleBlock.Text = $pageName.Split('.')[-1]  # Extract just the page name part
            $titleBlock.FontSize = 24
            $titleBlock.FontWeight = "Bold"
            $titleBlock.Margin = New-Object -TypeName System.Windows.Thickness -ArgumentList 0, 0, 0, 20
            $stackPanel.Children.Add($titleBlock)
            
            # Add description text
            $descBlock = New-Object -TypeName System.Windows.Controls.TextBlock
            $descBlock.Text = "Cette page est en cours de développement."
            $descBlock.FontSize = 16
            $descBlock.TextWrapping = "Wrap"
            $stackPanel.Children.Add($descBlock)
            
            # Add the StackPanel to the Page
            $page.Content = $stackPanel
            $page.DataContext = $script:dataContext

            # Navigate to the page
            $rootFrame.Navigate($page)
            Write-BucketLog -Data "Successfully navigated to: $pageName" -Level Info
            
            # Update the navigation button styles
            Invoke-UpdateNavigationButtonStyle -selectedTag $PageTag
        }
        catch {
            Write-BucketLog -Data "Failed to navigate to page $PageTag : $_" -Level Error
        }
    }
}