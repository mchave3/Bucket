BeforeAll {
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketImportWim.ps1"
    . $privateFunctionPath

    # Minimal stub if Spectre isn't present in test session
    if (-not (Get-Command -Name Write-SpectreHost -ErrorAction SilentlyContinue))
    {
        function Write-SpectreHost { param([Parameter(ValueFromRemainingArguments = $true)] $Args) }
    }
}

Describe 'Show-BucketImportWim' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketImportWim -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context 'UI footer behavior' {
        It 'Should provide a return hint after completion' {
            $content = Get-Content -Path $privateFunctionPath -Raw
            $content | Should -Match 'ReadyToExit'
            $content | Should -Match 'Enter/Esc: Return'
        }

        It 'Should keep a working footer while running' {
            $content = Get-Content -Path $privateFunctionPath -Raw
            $content | Should -Match 'Working\.\.\. please wait'
        }
    }
}
