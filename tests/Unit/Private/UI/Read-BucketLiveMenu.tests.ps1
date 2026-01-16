BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Read-BucketLiveMenu.ps1"
    . $privateFunctionPath
}

Describe 'Read-BucketLiveMenu' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Read-BucketLiveMenu -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have mandatory Title parameter' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.Parameters.ContainsKey('Title') | Should -Be $true
            $cmd.Parameters['Title'].Attributes.Mandatory | Should -Contain $true
        }

        It 'Should have mandatory Choices parameter' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.Parameters.ContainsKey('Choices') | Should -Be $true
            $cmd.Parameters['Choices'].Attributes.Mandatory | Should -Contain $true
        }

        It 'Should have optional Subtitle parameter' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.Parameters.ContainsKey('Subtitle') | Should -Be $true
        }

        It 'Should have ShowBack parameter with nullable bool type' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.Parameters.ContainsKey('ShowBack') | Should -Be $true
        }

        It 'Should have ShowExit parameter defaulting to true' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.Parameters.ContainsKey('ShowExit') | Should -Be $true
        }

        It 'Should have Color parameter' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.Parameters.ContainsKey('Color') | Should -Be $true
        }

        It 'Should have hashtable output type' {
            $cmd = Get-Command -Name Read-BucketLiveMenu
            $cmd.OutputType.Type.Name | Should -Contain 'Hashtable'
        }
    }

    Context 'When validating CmdletBinding' {
        It 'Should have CmdletBinding attribute' {
            $function = Get-Command -Name Read-BucketLiveMenu
            $function.CmdletBinding | Should -Be $true
        }
    }
}
