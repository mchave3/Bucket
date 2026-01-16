BeforeAll {
    $script:moduleName = 'Bucket'

    # Source the required dependencies
    $corePath = "$PSScriptRoot\..\..\..\..\source\Private\Core"
    $wimPath = "$PSScriptRoot\..\..\..\..\source\Private\WIM"
    $navPath = "$PSScriptRoot\..\..\..\..\source\Private\Navigation"
    $uiPath = "$PSScriptRoot\..\..\..\..\source\Private\UI"

    . "$corePath\Get-BucketState.ps1"
    . "$corePath\Initialize-BucketState.ps1"
    . "$wimPath\Get-BucketImportedImages.ps1"
    . "$navPath\New-BucketNavResult.ps1"
    . "$uiPath\Show-BucketImageViewer.ps1"
}

Describe 'Show-BucketImageViewer' {
    Context 'When function is loaded' {
        It 'Should be a valid function' {
            Get-Command -Name Show-BucketImageViewer -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        }

        It 'Should have CmdletBinding attribute' {
            $command = Get-Command -Name Show-BucketImageViewer
            $command.CmdletBinding | Should -BeTrue
        }

        It 'Should return hashtable output type' {
            $command = Get-Command -Name Show-BucketImageViewer
            $outputType = $command.OutputType
            $outputType.Type.Name | Should -Contain 'Hashtable'
        }
    }

    Context 'When no images are imported' {
        BeforeAll {
            # Mock all external dependencies
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Get-Content -MockWith { '{"Images": []}' }
            Mock -CommandName Write-SpectreHost -MockWith { }
            Mock -CommandName Read-SpectreConfirm -MockWith { $true }

            # Initialize state with no images
            Initialize-BucketState
        }

        It 'Should return Back action when no images exist' {
            $result = Show-BucketImageViewer
            $result | Should -Not -BeNullOrEmpty
            $result.Action | Should -Be 'Back'
        }

        It 'Should display a message about no images' {
            Show-BucketImageViewer
            Should -Invoke -CommandName Write-SpectreHost -ParameterFilter {
                $Message -like '*No images imported*'
            } -Times 1
        }
    }

    Context 'When images are imported' {
        BeforeAll {
            Mock -CommandName Test-Path -MockWith { $true }
            Mock -CommandName New-Item -MockWith { }
            Mock -CommandName Write-SpectreHost -MockWith { }

            # Initialize state with test images
            $testMetadata = @'
{
    "Images": [
        {
            "Id": "test-guid-001",
            "FileName": "windows11.wim",
            "FileSize": 5000000000,
            "IndexCount": 2,
            "Indexes": [
                {
                    "ImageIndex": 1,
                    "ImageName": "Windows 11 Pro",
                    "ImageDescription": "Windows 11 Professional",
                    "ImageSize": 3000000000,
                    "Architecture": "x64",
                    "Version": "10.0.22621",
                    "EditionId": "Professional",
                    "InstallationType": "Client",
                    "Languages": ["en-US"]
                },
                {
                    "ImageIndex": 2,
                    "ImageName": "Windows 11 Enterprise",
                    "ImageDescription": "Windows 11 Enterprise",
                    "ImageSize": 3500000000,
                    "Architecture": "x64",
                    "Version": "10.0.22621",
                    "EditionId": "Enterprise",
                    "InstallationType": "Client",
                    "Languages": ["en-US"]
                }
            ]
        }
    ]
}
'@
            Mock -CommandName Get-Content -MockWith { $testMetadata }
            Initialize-BucketState
        }

        It 'Should have images available for display' {
            $images = Get-BucketImportedImages
            $images.Count | Should -Be 1
            $images[0].FileName | Should -Be 'windows11.wim'
        }

        It 'Should have multiple indexes in the test image' {
            $images = Get-BucketImportedImages
            $images[0].IndexCount | Should -Be 2
        }
    }
}
