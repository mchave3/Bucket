BeforeAll {
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Screens\Show-BucketStartProvisioning.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"

    . $navResultPath
    . $privateFunctionPath

    if (-not (Get-Command -Name Write-SpectreHost -ErrorAction SilentlyContinue))
    {
        function Write-SpectreHost { param([Parameter(ValueFromRemainingArguments = $true)] $Args) }
    }
}

Describe 'Show-BucketStartProvisioning' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketStartProvisioning -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When menu returns Selection (placeholder path)' {
        It 'Should return Refresh' {
            Set-Item -Path function:Read-BucketMenu -Value {
                @{ Action = 'Selection'; Selection = 'Select Image' }
            }

            $result = Show-BucketStartProvisioning

            $result.Action | Should -Be 'Refresh'
        }
    }

    Context 'When menu returns Back' {
        It 'Should pass through Back' {
            Set-Item -Path function:Read-BucketMenu -Value {
                New-BucketNavResult -Action Back
            }

            $result = Show-BucketStartProvisioning

            $result.Action | Should -Be 'Back'
        }
    }
}
