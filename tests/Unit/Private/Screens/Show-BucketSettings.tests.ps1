BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketSettings.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketSettings' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketSettings -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
