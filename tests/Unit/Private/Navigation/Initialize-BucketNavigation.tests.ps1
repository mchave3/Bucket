BeforeAll {
    $privateFunctionPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation\Initialize-BucketNavigation.ps1"
    . $privateFunctionPath
}

Describe 'Initialize-BucketNavigation' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Initialize-BucketNavigation -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When initializing navigation with registered screens present' {
        BeforeEach {
            # Minimal stubs so registry validation can succeed.
            function Show-BucketMainMenu { }
            function Show-BucketStartProvisioning { }
            function Show-BucketImageManagement { }
            function Show-BucketImportWim { }
            function Show-BucketUpdateManagement { }
            function Show-BucketSettings { }
            function Show-BucketAbout { }
        }

        It 'Should not throw when called' {
            { Initialize-BucketNavigation } | Should -Not -Throw
        }

        It 'Should create the ScreenRegistry with expected screens' {
            Initialize-BucketNavigation

            $script:ScreenRegistry | Should -Not -BeNullOrEmpty
            $script:ScreenRegistry | Should -BeOfType [hashtable]
            $script:ScreenRegistry.ContainsKey('MainMenu') | Should -Be $true
            $script:ScreenRegistry.ContainsKey('ImageManagement') | Should -Be $true
        }

        It 'Should map each registered screen to an existing function' {
            Initialize-BucketNavigation

            foreach ($screen in $script:ScreenRegistry.GetEnumerator())
            {
                Get-Command -Name $screen.Value -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
            }
        }
    }

    Context 'When a registered screen function is missing' {
        BeforeEach {
            function Show-BucketMainMenu { }
            function Show-BucketStartProvisioning { }
            function Show-BucketImageManagement { }
            function Show-BucketUpdateManagement { }
            function Show-BucketSettings { }

            # Intentionally omit Show-BucketAbout.
            if (Test-Path -Path function:Show-BucketAbout)
            {
                Remove-Item -Path function:Show-BucketAbout
            }
        }

        It 'Should throw a clear error' {
            { Initialize-BucketNavigation } | Should -Throw -ErrorId '*'
        }
    }
}
