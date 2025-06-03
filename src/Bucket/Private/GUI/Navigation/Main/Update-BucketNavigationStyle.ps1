<#
.SYNOPSIS
    Updates the visual style of navigation buttons, highlighting the button corresponding to the current page.

.DESCRIPTION
    This function updates the visual style of navigation buttons, highlighting the button corresponding to the current page.
    It is generic and can be used for any navigation bar by providing the mapping between page tags and button objects.

.NOTES
    Name:        Update-BucketNavigationStyle.ps1
    Author:      Mickaël CHAVE
    Created:     04/14/2025
    Version:     25.6.3.4
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Update-BucketNavigationStyle -PageTag "homePage" -ButtonMap $mainNavButtons -ResourceContext $WPF_MainWindow
#>

function Update-BucketNavigationStyle {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$PageTag,

        [Parameter(Mandatory = $true)]
        [hashtable]$ButtonMap,

        [Parameter(Mandatory = $false)]
        $ResourceContext = $null,

        [Parameter(Mandatory = $false)]
        [string]$DefaultStyleKey = 'MenuButtonStyle',

        [Parameter(Mandatory = $false)]
        [string]$SelectedStyleKey = 'SelectedMenuButtonStyle'
    )
    process {
        if ($PSCmdlet.ShouldProcess("Navigation button for page $PageTag", "Update style")) {
            try {
                Write-BucketLog -Data "Updating navigation button style for: $PageTag" -Level Debug

                #region Collect Buttons
                # Gather all valid button objects from the provided map
                $allButtons = $ButtonMap.Values | Where-Object { $_ }
                #endregion

                #region Resource Context Resolution
                # Build a list of potential resource contexts for style lookup
                $defaultStyle = $null
                $selectedStyle = $null
                $potentialContexts = @()
                if ($ResourceContext) {
                    $potentialContexts += $ResourceContext
                }
                if ($allButtons.Count -gt 0) {
                    $potentialContexts += $allButtons | ForEach-Object { $_.TemplatedParent }
                }
                if ($allButtons.Count -gt 0) {
                    $potentialContexts += $allButtons | ForEach-Object {
                        $parent = $_.Parent
                        while ($parent) {
                            $parent
                            $parent = $parent.Parent
                        }
                    }
                }
                if ([System.Windows.Application]::Current) {
                    $potentialContexts += [System.Windows.Application]::Current
                    $potentialContexts += [System.Windows.Application]::Current.MainWindow
                }
                if (Get-Variable -Name WPF_MainWindow -ErrorAction SilentlyContinue) {
                    $potentialContexts += Get-Variable -Name WPF_MainWindow -ValueOnly -ErrorAction SilentlyContinue
                }
                # Remove duplicates and nulls
                $potentialContexts = $potentialContexts | Where-Object { $_ -ne $null } | Select-Object -Unique
                #endregion

                #region Style Lookup
                # Attempt to find the required styles in the available contexts
                foreach ($context in $potentialContexts) {
                    if ($defaultStyle -and $selectedStyle) { break }
                    try {
                        if (-not $defaultStyle) {
                            $defaultStyle = $context.FindResource($DefaultStyleKey)
                        }
                        if (-not $selectedStyle) {
                            $selectedStyle = $context.FindResource($SelectedStyleKey)
                        }
                    }
                    catch {
                        Write-BucketLog -Data "Failed to find styles in context: $_" -Level Warning
                    }
                }
                #endregion

                #region Fallback Style Creation
                # If styles are not found, create basic fallback styles for buttons
                if (-not $defaultStyle -or -not $selectedStyle) {
                    Write-BucketLog -Data "Trying alternative style lookup method..." -Level Debug
                    try {
                        if (-not $defaultStyle) {
                            $defaultStyle = New-Object System.Windows.Style([System.Windows.Controls.Button])
                        }
                        if (-not $selectedStyle) {
                            $selectedStyle = New-Object System.Windows.Style([System.Windows.Controls.Button])
                            $selectedStyle.BasedOn = $defaultStyle
                            $bgSetter = New-Object System.Windows.Setter([System.Windows.Controls.Control]::BackgroundProperty, [System.Windows.Media.Brushes]::LightBlue)
                            $selectedStyle.Setters.Add($bgSetter)
                        }
                    }
                    catch {
                        Write-BucketLog -Data "Failed to create fallback styles: $_" -Level Warning
                    }
                }
                #endregion

                #region Style Application
                # Abort if styles are still missing
                if (-not $defaultStyle -or -not $selectedStyle) {
                    Write-BucketLog -Data "Navigation styles not found in any resource context, skipping style update" -Level Warning
                    return
                }
                # Apply default style to all buttons
                foreach ($btn in $allButtons) {
                    $btn.Style = $defaultStyle
                }
                # Apply selected style to the button for the current page
                if ($ButtonMap.ContainsKey($PageTag) -and $ButtonMap[$PageTag]) {
                    $ButtonMap[$PageTag].Style = $selectedStyle
                    Write-BucketLog -Data "Set selected style for: $PageTag" -Level Verbose
                }
                Write-BucketLog -Data "Navigation button style updated successfully" -Level Debug
                #endregion
            }
            catch {
                #region Error Handling
                Write-BucketLog -Data "Failed to update navigation button style: $_" -Level Error
                #endregion
            }
        }
    }
}
