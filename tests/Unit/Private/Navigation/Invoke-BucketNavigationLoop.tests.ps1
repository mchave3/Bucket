BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Invoke-BucketNavigationLoop.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"
    $initNavPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Initialize-BucketNavigation.ps1"
    . $navResultPath
    . $initNavPath
    . $privateFunctionPath
}

Describe 'Invoke-BucketNavigationLoop' {
    Context 'When navigation is initialized' {
        BeforeAll {
            Initialize-BucketNavigation
        }

        It 'Should be a valid function' {
            Get-Command -Name Invoke-BucketNavigationLoop -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
