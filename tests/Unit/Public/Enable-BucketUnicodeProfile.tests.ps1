BeforeAll {
    $script:moduleName = 'Bucket'

    Import-Module -Name $script:moduleName -ErrorAction SilentlyContinue
}

AfterAll {
    Get-Module -Name $script:moduleName -All | Remove-Module -Force -ErrorAction SilentlyContinue
}

Describe 'Enable-BucketUnicodeProfile' {
    Context 'When the function exists' {
        It 'Should be a valid command' {
            $command = Get-Command -Name Enable-BucketUnicodeProfile -ErrorAction SilentlyContinue

            $command | Should -Not -BeNullOrEmpty
            $command.CommandType | Should -Be 'Function'
        }

        It 'Should have the ProfilePath parameter' {
            $command = Get-Command -Name Enable-BucketUnicodeProfile

            $command.Parameters.ContainsKey('ProfilePath') | Should -Be $true
            $command.Parameters['ProfilePath'].ParameterType | Should -Be ([string])
        }

        It 'Should have the Force parameter' {
            $command = Get-Command -Name Enable-BucketUnicodeProfile

            $command.Parameters.ContainsKey('Force') | Should -Be $true
            $command.Parameters['Force'].ParameterType | Should -Be ([switch])
        }

        It 'Should have the PassThru parameter' {
            $command = Get-Command -Name Enable-BucketUnicodeProfile

            $command.Parameters.ContainsKey('PassThru') | Should -Be $true
            $command.Parameters['PassThru'].ParameterType | Should -Be ([switch])
        }
    }

    Context 'When applying profile updates' {
        It 'Should insert the encoding line as the first line when missing' {
            $profilePath = Join-Path -Path $TestDrive -ChildPath 'Microsoft.PowerShell_profile.ps1'
            Set-Content -Path $profilePath -Value "Write-Output 'hi'" -Encoding utf8

            Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force

            $firstLine = Get-Content -Path $profilePath -TotalCount 1
            $firstLine | Should -Be '$OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()'
        }

        It 'Should move the encoding line to the first line when present later' {
            $profilePath = Join-Path -Path $TestDrive -ChildPath 'Microsoft.PowerShell_profile.ps1'
            $content = @(
                "Write-Output 'before'",
                '$OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()',
                "Write-Output 'after'"
            ) -join "`r`n"

            Set-Content -Path $profilePath -Value $content -Encoding utf8

            Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force

            $lines = Get-Content -Path $profilePath
            $lines[0] | Should -Be '$OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()'
            (@($lines | Where-Object { $_ -eq '$OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()' }).Count) | Should -Be 1
        }

        It 'Should be idempotent' {
            $profilePath = Join-Path -Path $TestDrive -ChildPath 'Microsoft.PowerShell_profile.ps1'
            Set-Content -Path $profilePath -Value "Write-Output 'hi'" -Encoding utf8

            Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force
            $first = Get-Content -Path $profilePath -Raw

            Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force
            $second = Get-Content -Path $profilePath -Raw

            $second | Should -Be $first
        }

        It 'Should honor -WhatIf and not modify the file' {
            $profilePath = Join-Path -Path $TestDrive -ChildPath 'Microsoft.PowerShell_profile.ps1'
            Set-Content -Path $profilePath -Value "Write-Output 'hi'" -Encoding utf8
            $before = Get-Content -Path $profilePath -Raw

            Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force -WhatIf
            $after = Get-Content -Path $profilePath -Raw

            $after | Should -Be $before
        }

        It 'Should create the profile file when missing' {
            $profilePath = Join-Path -Path $TestDrive -ChildPath 'Microsoft.PowerShell_profile.ps1'

            Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force

            Test-Path -Path $profilePath -PathType Leaf | Should -Be $true
            (Get-Content -Path $profilePath -TotalCount 1) | Should -Be '$OutputEncoding = [Console]::InputEncoding = [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()'
        }

        It 'Should return a status object with -PassThru' {
            $profilePath = Join-Path -Path $TestDrive -ChildPath 'Microsoft.PowerShell_profile.ps1'

            $result = Enable-BucketUnicodeProfile -ProfilePath $profilePath -Force -PassThru

            $result | Should -Not -BeNullOrEmpty
            $result.ProfilePath | Should -Be $profilePath
            $result.PSObject.Properties.Name | Should -Contain 'Changed'
            $result.PSObject.Properties.Name | Should -Contain 'Action'
        }
    }
}
