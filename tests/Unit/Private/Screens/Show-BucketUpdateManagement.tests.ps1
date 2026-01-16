BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketUpdateManagement.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketUpdateManagement' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketUpdateManagement -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
