<#
.SYNOPSIS
    Defines the working directory path for Bucket operations.

.DESCRIPTION
    This script contains the private variable definition for the working directory path
    used throughout the Bucket application. The working directory is a crucial
    path where temporary files, logs, and processing operations occur during the Windows
    image manipulation process.

.NOTES
    Name:        workingDirectory.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket
#>

$script:workingDirectory = $env:ProgramData + '\Bucket'

