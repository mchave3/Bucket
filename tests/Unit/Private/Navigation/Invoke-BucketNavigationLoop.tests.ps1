BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Invoke-BucketNavigationLoop.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"
    $initNavPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Initialize-BucketNavigation.ps1"
    $logPath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Write-BucketLog.ps1"
    . $navResultPath
    . $initNavPath
    . $logPath
    . $privateFunctionPath
}

Describe 'Invoke-BucketNavigationLoop' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Invoke-BucketNavigationLoop -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When a screen returns an invalid result' {
        BeforeEach {
            # Minimal navigation state
            $script:NavigationStack = [System.Collections.Generic.Stack[PSCustomObject]]::new()
            $script:ScreenRegistry = @{ 'MainMenu' = 'Show-BucketMainMenu' }

            Set-Item -Path function:Write-BucketLog -Value { }

            Mock -CommandName Clear-Host
            Mock -CommandName Write-Warning
            Mock -CommandName Write-Verbose
        }

        It 'Should not throw and should go back' {
            function Show-BucketMainMenu { 'not-a-nav-result' }

            { Invoke-BucketNavigationLoop } | Should -Not -Throw
            $script:NavigationStack.Count | Should -Be 0
            Should -Invoke Clear-Host -Times 1
        }
    }

    Context 'When navigating to another screen' {
        BeforeEach {
            $script:NavigationStack = [System.Collections.Generic.Stack[PSCustomObject]]::new()
            $script:ScreenRegistry = @{
                'MainMenu'  = 'Show-BucketMainMenu'
                'Settings'  = 'Show-BucketSettings'
            }

            Set-Item -Path function:Write-BucketLog -Value { }

            Mock -CommandName Clear-Host
            Mock -CommandName Write-Warning
            Mock -CommandName Write-Verbose

            $script:mainMenuCalls = 0
            $script:settingsCalls = 0

            function Show-BucketMainMenu {
                $script:mainMenuCalls++
                return New-BucketNavResult -Action Navigate -Target 'Settings'
            }

            function Show-BucketSettings {
                $script:settingsCalls++
                return New-BucketNavResult -Action Exit
            }
        }

        It 'Should call both screens and exit' {
            { Invoke-BucketNavigationLoop } | Should -Not -Throw

            $script:mainMenuCalls | Should -Be 1
            $script:settingsCalls | Should -Be 1
            $script:NavigationStack.Count | Should -Be 0
            Should -Invoke Clear-Host -Times 2
        }
    }

    Context 'When a screen returns Back' {
        BeforeEach {
            $script:NavigationStack = [System.Collections.Generic.Stack[PSCustomObject]]::new()
            $script:ScreenRegistry = @{ 'MainMenu' = 'Show-BucketMainMenu' }

            Set-Item -Path function:Write-BucketLog -Value { }

            Mock -CommandName Clear-Host
            Mock -CommandName Write-Warning
            Mock -CommandName Write-Verbose

            function Show-BucketMainMenu {
                return New-BucketNavResult -Action Back
            }
        }

        It 'Should pop and exit without throwing' {
            { Invoke-BucketNavigationLoop } | Should -Not -Throw
            $script:NavigationStack.Count | Should -Be 0
            Should -Invoke Clear-Host -Times 1
        }
    }
}
