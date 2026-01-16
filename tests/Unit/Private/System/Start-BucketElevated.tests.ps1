BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\System\Start-BucketElevated.ps1"
    . $privateFunctionPath
}

Describe 'Start-BucketElevated' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Start-BucketElevated -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
