BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketDeleteImageMenu.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketDeleteImageMenu' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketDeleteImageMenu -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Show-BucketDeleteImageMenu
            $command.CmdletBinding | Should -BeTrue
        }
    }
}
