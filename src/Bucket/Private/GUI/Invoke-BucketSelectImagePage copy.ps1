<#
.SYNOPSIS
    Invokes the Bucket Select Image Page in the GUI interface

.DESCRIPTION
    This function displays the select image page of the Bucket application.
    It renders the UI layout for the select image page including navigation elements,
    statistics, and recent activities.

.NOTES
    Name:        Invoke-BucketSelectImagePage.ps1
    Author:      Mickaël CHAVE
    Created:     04/11/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Invoke-BucketSelectImagePage
#>
function Invoke-BucketSelectImagePage {
    [CmdletBinding()]
    param(

    )

    process {
        $DataContext = [PSCustomObject]@{
            MountDirectory         = (Join-Path -Path $script:workingDirectory -ChildPath 'Mount')
            CompletedWIMsDirectory = (Join-Path -Path $script:workingDirectory -ChildPath 'CompletedWIMs')
            DiskSpaceInfo          = "C: Drive - Available space: $([math]::Round($(Get-PSDrive -Name 'C').Free/1GB, 2)) GB / $([math]::Round(($(Get-PSDrive -Name 'C').Free + $(Get-PSDrive -Name 'C').Used)/1GB, 2)) GB"
            MountedImagesCount     = 0
            ImageMountStatus       = "No image mounted"
            CurrentImageInfo       = "Please select and mount a Windows image to begin customization."
            PendingDriversCount    = 0
            InstalledDriversCount  = 0
            SelectedAppsCount      = 0
        }

        Invoke-BucketGuiNav -PageTag "selectImagePage" -DataContext $DataContext
    }
}
