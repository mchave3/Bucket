BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketStartProvisioning.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketStartProvisioning' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketStartProvisioning -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
