BeforeAll {
    $script:moduleName = 'Bucket'

    # Source the required dependencies
    $corePath = "$PSScriptRoot\..\..\..\..\source\Private\Core"
    $wimPath = "$PSScriptRoot\..\..\..\..\source\Private\WIM"
    $uiPath = "$PSScriptRoot\..\..\..\..\source\Private\UI"

    . "$corePath\Get-BucketState.ps1"
    . "$corePath\Initialize-BucketState.ps1"
    . "$corePath\Save-BucketMetadata.ps1"
    . "$corePath\Write-BucketLog.ps1"
    . "$uiPath\Read-BucketFilePath.ps1"
    . "$wimPath\Get-BucketImportedImages.ps1"
    . "$wimPath\Get-BucketWimMetadata.ps1"
    . "$wimPath\Import-BucketWim.ps1"
}

Describe 'Import-BucketWim' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Import-BucketWim -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Import-BucketWim
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should have an optional Path parameter' {
            $command = Get-Command -Name Import-BucketWim
            $command.Parameters['Path'] | Should -Not -BeNullOrEmpty
            $command.Parameters['Path'].Attributes.Mandatory | Should -Not -Contain $true
        }

        It 'Should return bool output type' {
            $command = Get-Command -Name Import-BucketWim
            $outputType = $command.OutputType
            $outputType.Type.Name | Should -Contain 'Boolean'
        }
    }

    Context 'When importing a WIM file' {
        BeforeAll {
            # Mock all external dependencies
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }
            Mock -CommandName Write-SpectreHost -MockWith { }
            Mock -CommandName Write-BucketLog -MockWith { }

            # Initialize state
            Initialize-BucketState
        }

        It 'Should return false when file is not a .wim file' {
            Mock -CommandName Read-BucketFilePath -MockWith { 'C:\test.iso' }

            $result = Import-BucketWim -Path 'C:\test.iso'
            $result | Should -BeFalse
        }
    }
}
