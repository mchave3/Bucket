BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Show-BucketHeader.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketHeader' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketHeader -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have Color parameter with default value' {
            $cmd = Get-Command -Name Show-BucketHeader
            $cmd.Parameters.ContainsKey('Color') | Should -Be $true
        }
    }
}
