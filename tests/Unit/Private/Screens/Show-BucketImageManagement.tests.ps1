BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketImageManagement.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketImageManagement' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketImageManagement -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
