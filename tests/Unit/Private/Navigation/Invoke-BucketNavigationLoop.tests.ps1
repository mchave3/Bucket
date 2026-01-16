BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Invoke-BucketNavigationLoop.ps1"
    $navResultPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"
    $initNavPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Initialize-BucketNavigation.ps1"
    $logPath = "$PSScriptRoot\..\..\..\..\source\Private\Core\Write-BucketLog.ps1"
    . $navResultPath
    . $initNavPath
    . $logPath
    . $privateFunctionPath
}

Describe 'Invoke-BucketNavigationLoop' {
    Context 'When navigation is initialized' {
        BeforeAll {
            Initialize-BucketNavigation
        }

        It 'Should be a valid function' {
            Get-Command -Name Invoke-BucketNavigationLoop -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When handling array results (pipeline pollution defense)' {
        It 'Should extract valid navigation result from array with Action property' {
            # Simulate what happens when a screen returns multiple objects
            $pollutedResult = @(
                'SomeRenderableObject'
                @{ Action = 'Back' }
            )

            # The last element with Action property should be extracted
            $extracted = $pollutedResult | Where-Object {
                ($_ -is [hashtable] -and $_.ContainsKey('Action')) -or
                ($null -ne $_.PSObject -and $null -ne $_.PSObject.Properties['Action'])
            } | Select-Object -Last 1

            $extracted | Should -Not -BeNullOrEmpty
            $extracted.Action | Should -Be 'Back'
        }

        It 'Should return null when array has no valid navigation result' {
            $invalidArray = @('Object1', 'Object2', 123)

            $extracted = $invalidArray | Where-Object {
                ($_ -is [hashtable] -and $_.ContainsKey('Action')) -or
                ($null -ne $_.PSObject -and $null -ne $_.PSObject.Properties['Action'])
            } | Select-Object -Last 1

            $extracted | Should -BeNullOrEmpty
        }
    }
}
