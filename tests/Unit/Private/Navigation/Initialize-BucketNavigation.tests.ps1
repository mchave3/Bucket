BeforeAll {
    $script:moduleName = 'Bucket'

    Import-Module -Name $script:moduleName -Force -ErrorAction SilentlyContinue
}

AfterAll {
    Get-Module -Name $script:moduleName -All | Remove-Module -Force -ErrorAction SilentlyContinue
}

Describe 'Initialize-BucketNavigation' {
    Context 'When initializing navigation' {
        It 'Should be a valid function in the module' {
            InModuleScope -ModuleName $script:moduleName {
                Get-Command -Name Initialize-BucketNavigation -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
            }
        }

        It 'Should not throw when called' {
            InModuleScope -ModuleName $script:moduleName {
                { Initialize-BucketNavigation } | Should -Not -Throw
            }
        }

        It 'Should create the ScreenRegistry with expected screens' {
            InModuleScope -ModuleName $script:moduleName {
                Initialize-BucketNavigation

                $script:ScreenRegistry | Should -Not -BeNullOrEmpty
                $script:ScreenRegistry | Should -BeOfType [hashtable]
                $script:ScreenRegistry.ContainsKey('MainMenu') | Should -Be $true
                $script:ScreenRegistry.ContainsKey('ImageManagement') | Should -Be $true
            }
        }
    }
}
