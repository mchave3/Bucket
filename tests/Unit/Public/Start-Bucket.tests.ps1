BeforeAll {
    $script:moduleName = 'Bucket'

    Import-Module -Name $script:moduleName -ErrorAction SilentlyContinue
}

AfterAll {
    Get-Module -Name $script:moduleName -All | Remove-Module -Force -ErrorAction SilentlyContinue
}

Describe 'Start-Bucket' {
    Context 'When the function exists' {
        It 'Should be a valid command' {
            $command = Get-Command -Name Start-Bucket -ErrorAction SilentlyContinue

            $command | Should -Not -BeNullOrEmpty
            $command.CommandType | Should -Be 'Function'
        }

        It 'Should have the Elevated parameter' {
            $command = Get-Command -Name Start-Bucket

            $command.Parameters.ContainsKey('Elevated') | Should -Be $true
            $command.Parameters['Elevated'].ParameterType | Should -Be ([switch])
        }
    }

    Context 'When validating help content' {
        It 'Should have a synopsis' {
            $help = Get-Help -Name Start-Bucket

            $help.Synopsis | Should -Not -BeNullOrEmpty
        }

        It 'Should have a description' {
            $help = Get-Help -Name Start-Bucket

            $help.Description | Should -Not -BeNullOrEmpty
        }

        It 'Should have at least one example' {
            $help = Get-Help -Name Start-Bucket

            $help.Examples | Should -Not -BeNullOrEmpty
        }
    }
}
