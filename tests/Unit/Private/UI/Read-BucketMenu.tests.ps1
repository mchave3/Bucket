BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Read-BucketMenu.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"
    . $navResultPath
    . $privateFunctionPath
}

Describe 'Read-BucketMenu' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Read-BucketMenu -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have required parameters' {
            $cmd = Get-Command -Name Read-BucketMenu
            $cmd.Parameters.ContainsKey('Title') | Should -Be $true
            $cmd.Parameters.ContainsKey('Choices') | Should -Be $true
        }
    }
}
