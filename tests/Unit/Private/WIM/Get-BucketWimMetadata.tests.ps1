BeforeAll {
    $script:moduleName = 'Bucket'

    # Source the required dependencies
    $wimPath = "$PSScriptRoot\..\..\..\..\source\Private\WIM"

    . "$wimPath\Get-BucketWimMetadata.ps1"
}

Describe 'Get-BucketWimMetadata' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Get-BucketWimMetadata -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Get-BucketWimMetadata
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should have a mandatory ImagePath parameter' {
            $command = Get-Command -Name Get-BucketWimMetadata
            $command.Parameters['ImagePath'] | Should -Not -BeNullOrEmpty
            $mandatoryAttr = $command.Parameters['ImagePath'].Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] }
            $mandatoryAttr.Mandatory | Should -BeTrue
        }

        It 'Should have ValidateScript on ImagePath parameter' {
            $command = Get-Command -Name Get-BucketWimMetadata
            $validateAttr = $command.Parameters['ImagePath'].Attributes | Where-Object { $_ -is [System.Management.Automation.ValidateScriptAttribute] }
            $validateAttr | Should -Not -BeNullOrEmpty
        }
    }
}
