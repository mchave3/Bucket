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
            # Ensure Write-SpectreHost exists (PwshSpectreConsole) and capture all calls.
            $script:spectreCalls = @()
            Set-Item -Path function:Write-SpectreHost -Value {
                param([Parameter(ValueFromRemainingArguments = $true)] $Args)
                $script:spectreCalls += ,($Args -join ' ')
            }

            Mock -CommandName Write-BucketLog -MockWith { }
            Mock -CommandName Copy-Item -MockWith { }
            Mock -CommandName Save-BucketMetadata -MockWith { }

            # Initialize state
            Initialize-BucketState
        }

        It 'Should return false when file is not a .wim file' {
            Mock -CommandName Read-BucketFilePath -MockWith { 'C:\test.iso' }

            $result = Import-BucketWim -Path 'C:\test.iso'
            $result | Should -BeFalse
        }

        It 'Should not emit unescaped markup when rendering index list' {
            # Ensure we reach index rendering:
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName Get-BucketWimMetadata -MockWith {
                param([Parameter(Mandatory = $true)] [string] $ImagePath)

                return [pscustomobject]@{
                    Id         = 'id[1]'
                    FileName   = 'install[1].wim'
                    IndexCount = 2
                    Indexes    = @(
                        [pscustomobject]@{ ImageIndex = 1; ImageName = 'Windows [Pro]'; Architecture = 'x64'; ImageSize = 1GB },
                        [pscustomobject]@{ ImageIndex = 2; ImageName = 'Windows [Home]'; Architecture = 'x64'; ImageSize = 2GB }
                    )
                }
            }
            Mock -CommandName Get-BucketImportedImages -MockWith { @() }
            Mock -CommandName Invoke-SpectreCommandWithStatus -MockWith {
                param(
                    [Parameter(Mandatory = $true)]
                    [string] $Title,
                    [Parameter(Mandatory = $true)]
                    [scriptblock] $ScriptBlock
                )

                return & $ScriptBlock
            }

            { Import-BucketWim -Path 'C:\Images\install[1].wim' } | Should -Not -Throw

            # Assert using a call history wrapper (more reliable than Pester's Should -Invoke for this case).
            $script:spectreCalls | Should -Not -BeNullOrEmpty
            ($script:spectreCalls -join "`n") | Should -Match '\[grey\]\(1\)\[/\]'
            ($script:spectreCalls -join "`n") | Should -Not -Match '\[grey\]\[1\]\[/\]'
        }
    }
}
