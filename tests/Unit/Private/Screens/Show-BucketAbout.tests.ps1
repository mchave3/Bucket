BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketAbout.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketAbout' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketAbout -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }
}
