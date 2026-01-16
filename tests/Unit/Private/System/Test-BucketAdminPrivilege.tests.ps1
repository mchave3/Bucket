BeforeAll {
    $script:moduleName = 'Bucket'

    # For testing private functions, we need to dot-source them directly
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\System\Test-BucketAdminPrivilege.ps1"
    . $privateFunctionPath
}

Describe 'Test-BucketAdminPrivilege' {
    Context 'When checking administrative privileges' {
        It 'Should return a boolean value' {
            $result = Test-BucketAdminPrivilege

            $result | Should -BeOfType [bool]
        }

        It 'Should not throw any exceptions' {
            { Test-BucketAdminPrivilege } | Should -Not -Throw
        }
    }
}
