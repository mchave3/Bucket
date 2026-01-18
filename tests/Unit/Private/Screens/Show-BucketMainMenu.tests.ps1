BeforeAll {
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketMainMenu.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"

    . $navResultPath
    . $privateFunctionPath

    # Minimal stubs for dependencies called by the screen
    if (-not (Get-Command -Name Show-BucketHeader -ErrorAction SilentlyContinue))
    {
        function Show-BucketHeader { }
    }
}

Describe 'Show-BucketMainMenu' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketMainMenu -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When menu returns unexpected Selection' {
        It 'Should pass through Selection (engine will handle as invalid)' {
            Set-Item -Path function:Read-BucketMenu -Value {
                @{ Action = 'Selection'; Selection = 'Anything' }
            }

            $result = Show-BucketMainMenu

            $result.Action | Should -Be 'Selection'
            $result.Selection | Should -Be 'Anything'
        }
    }

    Context 'When menu returns a normal navigation result' {
        It 'Should pass through Navigate' {
            Set-Item -Path function:Read-BucketMenu -Value {
                New-BucketNavResult -Action Navigate -Target 'Settings'
            }

            $result = Show-BucketMainMenu

            $result.Action | Should -Be 'Navigate'
            $result.Target | Should -Be 'Settings'
        }
    }
}
