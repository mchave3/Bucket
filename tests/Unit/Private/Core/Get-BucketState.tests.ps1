BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Get-BucketState.ps1"
    $initStatePath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Initialize-BucketState.ps1"
    . $initStatePath
    . $privateFunctionPath
}

Describe 'Get-BucketState' {
    Context 'When state is initialized' {
        BeforeAll {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{}' }
            Initialize-BucketState
        }

        It 'Should return the BucketState object' {
            $state = Get-BucketState
            $state | Should -Not -BeNullOrEmpty
        }
    }
}
