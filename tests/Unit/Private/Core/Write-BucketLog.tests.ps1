BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Write-BucketLog.ps1"
    . $privateFunctionPath
}

Describe 'Write-BucketLog' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Write-BucketLog -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have Message and Level parameters' {
            $cmd = Get-Command -Name Write-BucketLog
            $cmd.Parameters.ContainsKey('Message') | Should -Be $true
            $cmd.Parameters.ContainsKey('Level') | Should -Be $true
        }
    }
}
