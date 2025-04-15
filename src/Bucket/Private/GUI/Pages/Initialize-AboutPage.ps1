<#
.SYNOPSIS
    Initializes the About Page for the Bucket application

.DESCRIPTION
    This function initializes the About Page, sets up its data context,
    and handles all event bindings for UI elements on the page.

.NOTES
    Name:        Initialize-AboutPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-AboutPage
#>
function Initialize-AboutPage {
    [CmdletBinding()]
    param()

    process {
        Write-BucketLog -Data "Initializing About Page" -Level Info

        # Create the page loaded event handler
        $pageLoadedHandler = {
            param($senderObj, $e)

            Write-BucketLog -Data "About Page loaded, setting up handlers" -Level Info

            # Get references to UI elements directly from the sender object
            # Décommentez cette ligne lorsque vous ajouterez des éléments spécifiques à la page
            # $page = $senderObj

            # Get references to UI elements - add any specific About page elements here
            # For example:
            # $btnGitHub = $page.FindName("AboutPage_GitHubButton")

            # Set up button event handlers
            # For example:
            # if ($btnGitHub) {
            #     $btnGitHub.Add_Click({
            #         param($sender, $e)
            #         Start-Process "https://github.com/mchave3/Bucket"
            #     })
            # }

            Write-BucketLog -Data "About Page event handlers configured successfully" -Level Info
        }

        # Get application information
        $appVersion = "1.0.0" # Replace with actual version retrieval
        $buildDate = Get-Date -Format "yyyy-MM-dd"
        $author = "Mickaël CHAVE"

        # Create the data context for the page
        $dataContext = [PSCustomObject]@{
            # Application information
            AppVersion        = $appVersion
            BuildDate         = $buildDate
            Author            = $author
            Repository        = "https://github.com/mchave3/Bucket"
            License           = "MIT License"

            # System information
            PowerShellVersion = $PSVersionTable.PSVersion.ToString()
            OSVersion         = [System.Environment]::OSVersion.VersionString

            # Event handler
            PageLoaded        = $pageLoadedHandler
        }

        # Navigate to the About page with our custom data context
        Invoke-BucketNavigationService -PageTag "aboutPage" -DataContext $dataContext
    }
}
