BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketMainMenu.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketMainMenu' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketMainMenu -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
