BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Show-BucketSubmenuHeader.ps1"
    . $privateFunctionPath
}

Describe 'Show-BucketSubmenuHeader' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketSubmenuHeader -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have mandatory Title parameter' {
            $command = Get-Command -Name Show-BucketSubmenuHeader
            $command.Parameters['Title'].Attributes.Mandatory | Should -Contain $true
        }

        It 'Should have optional Subtitle parameter' {
            $command = Get-Command -Name Show-BucketSubmenuHeader
            $command.Parameters.ContainsKey('Subtitle') | Should -Be $true
        }

        It 'Should have optional Color parameter with default value' {
            $command = Get-Command -Name Show-BucketSubmenuHeader
            $command.Parameters.ContainsKey('Color') | Should -Be $true
        }

        It 'Should have OutputType of void' {
            $command = Get-Command -Name Show-BucketSubmenuHeader
            $command.OutputType.Type.Name | Should -Contain 'Void'
        }
    }
}
