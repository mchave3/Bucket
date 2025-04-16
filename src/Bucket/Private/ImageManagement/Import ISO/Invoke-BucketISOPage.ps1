<#
.SYNOPSIS
    Navigates to a specific page in the ISO import wizard

.DESCRIPTION
    Handles the navigation between pages in the ISO import wizard
    Updates the UI to reflect the current page and manages page initialization

.NOTES
    Name:        Invoke-BucketISOPage.ps1
    Author:      Mickaël CHAVE
    Created:     04/16/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Invoke-BucketISOPage -PageTag "dataSourcePage"
#>
function Invoke-BucketISOPage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $false)]
        [PSCustomObject]$PageDataContext
    )

    process {
        # Update the current page in the data context
        $script:ImportISODataContext.CurrentPage = $PageTag

        # Update the UI to highlight the current step
        Update-BucketISONavigationUI -CurrentPage $PageTag

        # Update button visibility/state based on current page
        Update-BucketISOButtonVisibility -CurrentPage $PageTag

        # Check if we have an Initialize function for this page
        $initializeFunctionName = "Initialize-BucketISO_$($PageTag)"

        if (Get-Command -Name $initializeFunctionName -ErrorAction SilentlyContinue) {
            # Call the initialization function that will set up data and events
            & $initializeFunctionName
        }
        else {
            # Fallback if the initialization function doesn't exist
            Write-BucketLog -Data "$initializeFunctionName not found, using basic navigation" -Level Warning

            # Combine page-specific context with global if provided
            $dataContext = $script:ImportISODataContext
            if ($PageDataContext) {
                # Create a shallow copy of the global context
                $properties = @{}
                foreach ($property in $script:ImportISODataContext.PSObject.Properties) {
                    $properties[$property.Name] = $property.Value
                }

                # Add page-specific properties
                foreach ($property in $PageDataContext.PSObject.Properties) {
                    $properties[$property.Name] = $property.Value
                }

                $dataContext = New-Object PSObject -Property $properties
            }

            # Use the generic navigation service
            $xamlBasePath = Join-Path -Path $PSScriptRoot -ChildPath "GUI\ImageManagement"
            Write-BucketLog -Data "Using XAML base path: $xamlBasePath" -Level "Debug"

            Invoke-BucketNavigationService -PageTag $PageTag `
                -RootFrame $WPF_MainWindow_ImportISO_MainFrame `
                -XamlBasePath $xamlBasePath `
                -PageDictionary $script:ImportISOPages `
                -DataContext $dataContext `
                -GlobalDataContext $script:ImportISODataContext
        }
    }
}
