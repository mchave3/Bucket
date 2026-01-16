BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Initialize-BucketLogging.ps1"
    $initStatePath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Initialize-BucketState.ps1"
    $getStatePath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Get-BucketState.ps1"
    . $initStatePath
    . $getStatePath
    . $privateFunctionPath
}

Describe 'Initialize-BucketLogging' {
    Context 'When initializing logging' {
        BeforeAll {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{}' }
            Mock -CommandName Add-Content -MockWith { }
            Initialize-BucketState
        }

        It 'Should not throw when initializing' {
            { Initialize-BucketLogging } | Should -Not -Throw
        }
    }
}
