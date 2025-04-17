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
    Version:     1.0.0
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
                # Collect all buttons
                $allButtons = $ButtonMap.Values | Where-Object { $_ }

                # Improved strategy to find resource context
                $defaultStyle = $null
                $selectedStyle = $null

                # Hierarchical search for styles across multiple possible contexts
                $potentialContexts = @()

                # 1. Explicitly provided context
                if ($ResourceContext) {
                    $potentialContexts += $ResourceContext
                }

                # 2. Button templates
                if ($allButtons.Count -gt 0) {
                    $potentialContexts += $allButtons | ForEach-Object { $_.TemplatedParent }
                }

                # 3. Button parents (traverse up the WPF tree)
                if ($allButtons.Count -gt 0) {
                    $potentialContexts += $allButtons | ForEach-Object {
                        $parent = $_.Parent
                        while ($parent) {
                            $parent
                            $parent = $parent.Parent
                        }
                    }
                }

                # 4. Application itself (global level)
                if ([System.Windows.Application]::Current) {
                    $potentialContexts += [System.Windows.Application]::Current
                    $potentialContexts += [System.Windows.Application]::Current.MainWindow
                }

                # 5. Known scope variables that might contain MainWindow
                if (Get-Variable -Name WPF_MainWindow -ErrorAction SilentlyContinue) {
                    $potentialContexts += Get-Variable -Name WPF_MainWindow -ValueOnly -ErrorAction SilentlyContinue
                }

                # Clean up duplicates and null objects
                $potentialContexts = $potentialContexts | Where-Object { $_ -ne $null } | Select-Object -Unique

                # Look for styles in each context until found
                foreach ($context in $potentialContexts) {
                    if ($defaultStyle -and $selectedStyle) {
                        break  # Already found, stop searching
                    }

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

                # Plan B: If no styles found, try dynamic creation
                if (-not $defaultStyle -or -not $selectedStyle) {
                    Write-BucketLog -Data "Trying alternative style lookup method..." -Level Debug
                    try {
                        # Create basic default style as fallback
                        if (-not $defaultStyle) {
                            $defaultStyle = New-Object System.Windows.Style([System.Windows.Controls.Button])
                        }
                        if (-not $selectedStyle) {
                            # Create selected style based on default
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

                # Final verification
                if (-not $defaultStyle -or -not $selectedStyle) {
                    Write-BucketLog -Data "Navigation styles not found in any resource context, skipping style update" -Level Warning
                    return
                }
                # Reset all buttons to default style
                foreach ($btn in $allButtons) {
                    $btn.Style = $defaultStyle
                }
                # Set selected style for the current page
                if ($ButtonMap.ContainsKey($PageTag) -and $ButtonMap[$PageTag]) {
                    $ButtonMap[$PageTag].Style = $selectedStyle
                    Write-BucketLog -Data "Set selected style for: $PageTag" -Level Verbose
                }
                else {
                    Write-BucketLog -Data "Unknown page tag: $PageTag (no matching button)" -Level Warning
                }
                Write-BucketLog -Data "Navigation button style updated successfully" -Level Debug
            }
            catch {
                Write-BucketLog -Data "Failed to update navigation button style: $_" -Level Error
            }
        }
    }
}
