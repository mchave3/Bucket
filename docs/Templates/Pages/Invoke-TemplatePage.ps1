# TEMPLATE: Replace PageName with your actual page name (e.g., UserSettings)
function Invoke-PageNamePage {
    process {
        Write-BucketLog -Data "Invoking PageNamePage" -Level "Info"
        
        # Check if the initialization function exists
        if (Get-Command -Name "Initialize-PageNamePage" -ErrorAction SilentlyContinue) {
            # Call the initialization function
            Initialize-PageNamePage
        }
        else {
            # Fallback if the initialization function is not available
            Write-BucketLog -Data "Initialize-PageNamePage not found, using direct navigation instead" -Level "Warning"
            Invoke-BucketNavigationService -PageName "PageNamePage"
        }
    }
}
