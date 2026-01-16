BeforeAll {
    $script:moduleName = 'Bucket'

    # For testing private functions, we need to dot-source them directly
    # since they won't be exported from the module
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\New-BucketNavResult.ps1"
    . $privateFunctionPath
}

Describe 'New-BucketNavResult' {
    Context 'When creating a Navigate action' {
        It 'Should return a valid navigation result with target' {
            $result = New-BucketNavResult -Action Navigate -Target 'ImageManagement'

            $result | Should -BeOfType [hashtable]
            $result.Action | Should -Be 'Navigate'
            $result.Target | Should -Be 'ImageManagement'
            $result.Arguments | Should -BeOfType [hashtable]
        }

        It 'Should throw when Target is not provided for Navigate action' {
            { New-BucketNavResult -Action Navigate } | Should -Throw
        }

        It 'Should include Arguments when provided' {
            $args = @{ ImageId = 123 }
            $result = New-BucketNavResult -Action Navigate -Target 'ViewImage' -Arguments $args

            $result.Arguments.ImageId | Should -Be 123
        }
    }

    Context 'When creating a Back action' {
        It 'Should return a valid Back navigation result' {
            $result = New-BucketNavResult -Action Back

            $result.Action | Should -Be 'Back'
        }

        It 'Should not require Target for Back action' {
            { New-BucketNavResult -Action Back } | Should -Not -Throw
        }
    }

    Context 'When creating an Exit action' {
        It 'Should return a valid Exit navigation result' {
            $result = New-BucketNavResult -Action Exit

            $result.Action | Should -Be 'Exit'
        }
    }

    Context 'When creating a Refresh action' {
        It 'Should return a valid Refresh navigation result' {
            $result = New-BucketNavResult -Action Refresh

            $result.Action | Should -Be 'Refresh'
        }
    }

    Context 'When providing invalid action' {
        It 'Should throw for invalid Action value' {
            { New-BucketNavResult -Action 'InvalidAction' } | Should -Throw
        }
    }
}
