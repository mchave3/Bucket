BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Read-BucketMenu.ps1"
    $liveMenuPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Read-BucketLiveMenu.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"
    . $navResultPath
    . $liveMenuPath
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

        It 'Should have optional Subtitle parameter' {
            $cmd = Get-Command -Name Read-BucketMenu
            $cmd.Parameters.ContainsKey('Subtitle') | Should -Be $true
        }

        It 'Should have NavigationMap parameter' {
            $cmd = Get-Command -Name Read-BucketMenu
            $cmd.Parameters.ContainsKey('NavigationMap') | Should -Be $true
        }

        It 'Should have ShowBack and ShowExit parameters' {
            $cmd = Get-Command -Name Read-BucketMenu
            $cmd.Parameters.ContainsKey('ShowBack') | Should -Be $true
            $cmd.Parameters.ContainsKey('ShowExit') | Should -Be $true
        }

        It 'Should have Color parameter with default value' {
            $cmd = Get-Command -Name Read-BucketMenu
            $cmd.Parameters.ContainsKey('Color') | Should -Be $true
        }
    }
}
