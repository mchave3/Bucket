BeforeAll {
    $script:moduleName = 'Bucket'
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\UI\Read-BucketFilePath.ps1"
    . $privateFunctionPath
}

Describe 'Read-BucketFilePath' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Read-BucketFilePath -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Read-BucketFilePath
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should have optional Title parameter' {
            $command = Get-Command -Name Read-BucketFilePath
            $command.Parameters['Title'] | Should -Not -BeNullOrEmpty
        }

        It 'Should have optional Filter parameter' {
            $command = Get-Command -Name Read-BucketFilePath
            $command.Parameters['Filter'] | Should -Not -BeNullOrEmpty
        }

        It 'Should have optional InitialDirectory parameter' {
            $command = Get-Command -Name Read-BucketFilePath
            $command.Parameters['InitialDirectory'] | Should -Not -BeNullOrEmpty
        }
    }
}
