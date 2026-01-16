BeforeAll {
    $script:moduleName = 'Bucket'

    # Source the required dependencies
    $corePath = "$PSScriptRoot\..\..\..\..\source\Private\Core"
    $wimPath = "$PSScriptRoot\..\..\..\..\source\Private\WIM"

    . "$corePath\Get-BucketState.ps1"
    . "$corePath\Initialize-BucketState.ps1"
    . "$corePath\Save-BucketMetadata.ps1"
    . "$corePath\Write-BucketLog.ps1"
    . "$wimPath\Get-BucketImportedImages.ps1"
    . "$wimPath\Remove-BucketWim.ps1"
}

Describe 'Remove-BucketWim' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Remove-BucketWim -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Remove-BucketWim
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should support ShouldProcess' {
            $command = Get-Command -Name Remove-BucketWim
            $attribute = $command.ScriptBlock.Attributes | Where-Object { $_ -is [System.Management.Automation.CmdletBindingAttribute] }
            $attribute.SupportsShouldProcess | Should -BeTrue
        }

        It 'Should have a mandatory Id parameter' {
            $command = Get-Command -Name Remove-BucketWim
            $command.Parameters['Id'] | Should -Not -BeNullOrEmpty
            $mandatoryAttr = $command.Parameters['Id'].Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] }
            $mandatoryAttr.Mandatory | Should -BeTrue
        }

        It 'Should have a Force switch parameter' {
            $command = Get-Command -Name Remove-BucketWim
            $command.Parameters['Force'] | Should -Not -BeNullOrEmpty
            $command.Parameters['Force'].ParameterType.Name | Should -Be 'SwitchParameter'
        }

        It 'Should return bool output type' {
            $command = Get-Command -Name Remove-BucketWim
            $outputType = $command.OutputType
            $outputType.Type.Name | Should -Contain 'Boolean'
        }
    }

    Context 'When removing an image that does not exist' {
        BeforeAll {
            # Mock all external dependencies
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }
            Mock -CommandName Write-SpectreHost -MockWith { }
            Mock -CommandName Write-BucketLog -MockWith { }

            # Initialize state with empty images
            Initialize-BucketState
        }

        It 'Should return false when image ID is not found' {
            $result = Remove-BucketWim -Id 'nonexistent-guid' -Force
            $result | Should -BeFalse
        }
    }
}
