BeforeAll {
    $script:moduleName = 'Bucket'

    # Source the required dependencies
    $corePath = "$PSScriptRoot\..\..\..\..\source\Private\Core"

    . "$corePath\Get-BucketState.ps1"
    . "$corePath\Initialize-BucketState.ps1"
    . "$corePath\Save-BucketMetadata.ps1"
}

Describe 'Save-BucketMetadata' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Save-BucketMetadata -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Save-BucketMetadata
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should have optional Metadata parameter' {
            $command = Get-Command -Name Save-BucketMetadata
            $command.Parameters['Metadata'] | Should -Not -BeNullOrEmpty
        }

        It 'Should have optional Path parameter' {
            $command = Get-Command -Name Save-BucketMetadata
            $command.Parameters['Path'] | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When saving metadata with default values' {
        BeforeAll {
            # Mock all external dependencies
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }
            Mock -CommandName Set-Content -MockWith { }

            # Initialize state
            Initialize-BucketState
        }

        It 'Should call Set-Content to save the file' {
            Save-BucketMetadata
            Should -Invoke -CommandName Set-Content -Times 1 -Exactly
        }

        It 'Should use UTF8 encoding' {
            Save-BucketMetadata
            Should -Invoke -CommandName Set-Content -ParameterFilter {
                $Encoding -eq [System.Text.Encoding]::UTF8 -or $Encoding -eq 'UTF8' -or "$Encoding" -match 'UTF8|utf8'
            }
        }
    }

    Context 'When saving custom metadata' {
        BeforeAll {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }
            Mock -CommandName Set-Content -MockWith { }

            Initialize-BucketState
        }

        It 'Should accept custom metadata object' {
            $customMetadata = @{
                Images = @(
                    @{
                        Id       = 'custom-id'
                        FileName = 'custom.wim'
                    }
                )
            }

            { Save-BucketMetadata -Metadata $customMetadata } | Should -Not -Throw
            Should -Invoke -CommandName Set-Content -Times 1
        }

        It 'Should accept custom path' {
            $customPath = 'C:\custom\path\metadata.json'

            { Save-BucketMetadata -Path $customPath } | Should -Not -Throw
            Should -Invoke -CommandName Set-Content -ParameterFilter {
                $Path -eq $customPath
            }
        }
    }

    Context 'When parent directory does not exist' {
        BeforeAll {
            # First call returns false (directory doesn't exist), subsequent calls return true
            $script:testPathCallCount = 0
            Mock -CommandName Test-Path -MockWith {
                $script:testPathCallCount++
                if ($PathType -eq 'Container')
                {
                    return $false
                }
                return $true
            }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }
            Mock -CommandName Set-Content -MockWith { }

            Initialize-BucketState
        }

        It 'Should create the parent directory if it does not exist' {
            Save-BucketMetadata

            Should -Invoke -CommandName New-Item -ParameterFilter {
                $ItemType -eq 'Directory'
            } -Times 1
        }
    }
}
