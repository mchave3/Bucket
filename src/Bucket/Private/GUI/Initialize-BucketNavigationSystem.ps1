<#
.SYNOPSIS
    Initializes the Bucket navigation system

.DESCRIPTION
    This function initializes the navigation system for the Bucket application.
    It loads all necessary navigation components and sets up the page handlers.

.NOTES
    Name:        Initialize-BucketNavigationSystem.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Initialize-BucketNavigationSystem
#>
function Initialize-BucketNavigationSystem {
    [CmdletBinding()]
    param()
    
    process {
        Write-BucketLog -Data "Initializing Bucket navigation system" -Level Info
        
        # Import the navigation module components
        $navigationPath = Join-Path -Path $PSScriptRoot -ChildPath "Navigation"
        $pagesPath = Join-Path -Path $PSScriptRoot -ChildPath "Pages"
        
        # Import navigation service
        $navigationServicePath = Join-Path -Path $PSScriptRoot -ChildPath "Navigation\Invoke-BucketNavigationService.ps1"
        if (Test-Path -Path $navigationServicePath) {
            . $navigationServicePath
            Write-BucketLog -Data "Navigation service imported successfully" -Level Info
        }
        else {
            Write-BucketLog -Data "Navigation service not found at: $navigationServicePath" -Level Warning
        }
        
        # Import page navigation functions
        $pageNavigationPath = Join-Path -Path $PSScriptRoot -ChildPath "Navigation\Invoke-BucketPageNavigation.ps1"
        if (Test-Path -Path $pageNavigationPath) {
            . $pageNavigationPath
            Write-BucketLog -Data "Page navigation functions imported successfully" -Level Info
        }
        else {
            Write-BucketLog -Data "Page navigation functions not found at: $pageNavigationPath" -Level Warning
        }
        
        # Import navigation button styles
        $navButtonStylesPath = Join-Path -Path $navigationPath -ChildPath "Update-BucketNavButtonStyle.ps1"
        if (Test-Path -Path $navButtonStylesPath) {
            . $navButtonStylesPath
            Write-BucketLog -Data "Navigation button styles imported successfully" -Level Debug
        }
        else {
            Write-BucketLog -Data "Navigation button styles not found at: $navButtonStylesPath" -Level Warning
        }
        
        # Ensure we have the correct version from Get-BucketVersion
        if (-not $script:BucketVersion -or $script:BucketVersion -eq "Version not found") {
            # Make sure Get-BucketVersion has been called
            if (Get-Command -Name "Get-BucketVersion" -ErrorAction SilentlyContinue) {
                Get-BucketVersion
                Write-BucketLog -Data "Updated BucketVersion to: $script:BucketVersion" -Level Debug
            }
            else {
                Write-BucketLog -Data "Get-BucketVersion function not found, version may be incorrect" -Level Warning
            }
        }
        
        # Import page initializers
        $pageFiles = @(
            "Initialize-HomePage.ps1",
            "Initialize-SelectImagePage.ps1",
            "Initialize-AboutPage.ps1"
        )
        
        foreach ($pageFile in $pageFiles) {
            $pagePath = Join-Path -Path $pagesPath -ChildPath $pageFile
            if (Test-Path -Path $pagePath) {
                . $pagePath
                Write-BucketLog -Data "Page initializer imported: $pageFile" -Level Debug
            }
            else {
                Write-BucketLog -Data "Page initializer not found at: $pagePath" -Level Warning
            }
        }
        
        Write-BucketLog -Data "Bucket navigation system initialized successfully" -Level Info
    }
}

