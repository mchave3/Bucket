BeforeAll {
    $script:moduleName = 'Bucket'

    # For testing private functions, we need to dot-source them directly
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Initialize-BucketState.ps1"
    $getStatePath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Get-BucketState.ps1"
    . $privateFunctionPath
    . $getStatePath
}

AfterAll {
    # Clean up script-scoped variables
    Remove-Variable -Name BucketState -Scope Script -ErrorAction SilentlyContinue
}

Describe 'Initialize-BucketState' {
    BeforeEach {
        # Reset state before each test
        Remove-Variable -Name BucketState -Scope Script -ErrorAction SilentlyContinue
    }

    Context 'When initializing state' {
        It 'Should create the BucketState object' {
            # Mock file system operations to avoid creating actual directories
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{}' }

            Initialize-BucketState

            $script:BucketState | Should -Not -BeNullOrEmpty
        }

        It 'Should initialize Paths with correct structure' {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{}' }

            Initialize-BucketState

            $script:BucketState.Paths | Should -Not -BeNullOrEmpty
            $script:BucketState.Paths.Root | Should -Match 'Bucket$'
            $script:BucketState.Paths.Logs | Should -Match 'Logs$'
            $script:BucketState.Paths.Mount | Should -Match 'Mount$'
            $script:BucketState.Paths.Images | Should -Match 'Images$'
            $script:BucketState.Paths.Updates | Should -Match 'Updates$'
        }

        It 'Should initialize Session with StartTime' {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{}' }

            Initialize-BucketState

            $script:BucketState.Session | Should -Not -BeNullOrEmpty
            $script:BucketState.Session.StartTime | Should -BeOfType [DateTime]
        }
    }
}

Describe 'Get-BucketState' {
    Context 'When state is not initialized' {
        BeforeEach {
            Remove-Variable -Name BucketState -Scope Script -ErrorAction SilentlyContinue
        }

        It 'Should throw when state has not been initialized' {
            { Get-BucketState } | Should -Throw '*not been initialized*'
        }
    }

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
            $state | Should -BeOfType [PSCustomObject]
        }

        It 'Should return the same object as script:BucketState' {
            $state = Get-BucketState

            $state | Should -Be $script:BucketState
        }
    }
}
