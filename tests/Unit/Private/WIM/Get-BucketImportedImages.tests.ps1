BeforeAll {
    $script:moduleName = 'Bucket'

    # Source the required dependencies
    $corePath = "$PSScriptRoot\..\..\..\..\source\Private\Core"
    $wimPath = "$PSScriptRoot\..\..\..\..\source\Private\WIM"

    . "$corePath\Get-BucketState.ps1"
    . "$corePath\Initialize-BucketState.ps1"
    . "$wimPath\Get-BucketImportedImages.ps1"
}

Describe 'Get-BucketImportedImages' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Get-BucketImportedImages -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Get-BucketImportedImages
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should have an optional Id parameter' {
            $command = Get-Command -Name Get-BucketImportedImages
            $command.Parameters['Id'] | Should -Not -BeNullOrEmpty
        }
    }

    Context 'When no images are imported' {
        BeforeAll {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }

            Initialize-BucketState
        }

        It 'Should return an empty array' {
            $images = Get-BucketImportedImages
            $images | Should -BeNullOrEmpty
        }
    }

    Context 'When images are imported' {
        BeforeAll {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }

            $testMetadata = @'
{
    "Images": [
        {
            "Id": "guid-001",
            "FileName": "image1.wim",
            "FileSize": 1000000000,
            "IndexCount": 1
        },
        {
            "Id": "guid-002",
            "FileName": "image2.wim",
            "FileSize": 2000000000,
            "IndexCount": 2
        }
    ]
}
'@
            Mock -CommandName Get-Content -MockWith { $testMetadata }
            Initialize-BucketState
        }

        It 'Should return all images when no Id is specified' {
            $images = Get-BucketImportedImages
            $images.Count | Should -Be 2
        }

        It 'Should return specific image when Id is specified' {
            $image = Get-BucketImportedImages -Id 'guid-001'
            $image | Should -Not -BeNullOrEmpty
            $image.FileName | Should -Be 'image1.wim'
        }

        It 'Should return null when Id is not found' {
            $image = Get-BucketImportedImages -Id 'nonexistent'
            $image | Should -BeNullOrEmpty
        }
    }
}
