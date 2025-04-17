<#
.SYNOPSIS
    Performs pre-flight checks and setup for the Bucket module.

.DESCRIPTION
    Initializes logging, creates required directory structure, and verifies that
    the environment is ready for Bucket operations.

.NOTES
    Name:        Invoke-BucketPreFlight.ps1
    Author:      Mickaël CHAVE
    Created:     04/03/2025
    Version:     1.0.0
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.EXAMPLE
    Invoke-BucketPreFlight
#>
function Invoke-BucketPreFlight {
    [CmdletBinding()]
    param()

    process {
        #region Logging Initialization
        try {
            if (-not $script:workingDirectory) {
                $script:workingDirectory = Join-Path -Path $env:ProgramData -ChildPath 'Bucket'
                Write-Verbose "[Pre-Flight] Working directory was not initialized, setting to: $script:workingDirectory"
            }
            if (-not (Test-Path -Path $script:workingDirectory)) {
                New-Item -Path $script:workingDirectory -ItemType Directory -Force -ErrorAction Stop | Out-Null
            }
            $logDirectory = Join-Path -Path $script:workingDirectory -ChildPath "Logs"
            $logFile = Join-Path -Path $logDirectory -ChildPath "Bucket.log"
            New-Item -Path $logDirectory -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null
            if (Test-Path -Path $logFile) {
                Remove-Item -Path $logFile -Force -ErrorAction SilentlyContinue | Out-Null
            }
            if (-not (Get-Module -Name PoShLog -ErrorAction SilentlyContinue)) {
                Import-Module -Name PoShLog -ErrorAction Stop
            }
            New-Logger |
                Set-MinimumLevel -Value Verbose |
                Add-SinkConsole |
                Add-SinkFile -Path $logFile |
                Start-Logger -ErrorAction Stop
        }
        catch {
            Write-Error "[Pre-Flight] Failed to initialize logger: $_" -ErrorAction Stop
            exit 1
        }
        #endregion

        #region Log Initialization Status
        Write-BucketLog -Data "[Pre-Flight] ========== BUCKET INITIALIZATION ==========" -Level Info
        Write-BucketLog -Data "[Pre-Flight] Logger initialized successfully" -Level Info
        Write-BucketLog -Data "[Pre-Flight] Working directory: $script:workingDirectory" -Level Debug
        #endregion

        #region Administrator Privileges
        Write-BucketLog -Data "[Pre-Flight] ---------- Administrator Check ----------" -Level Info
        try {
            $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
            $isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
            if (-not $isAdmin) {
                $errorMsg = "Bucket requires administrator privileges. Please restart PowerShell as Administrator."
                Write-BucketLog -Data "[Pre-Flight] $errorMsg" -Level Error
                #exit 1
            }
            Write-BucketLog -Data "[Pre-Flight] Administrator privileges verified" -Level Info
        }
        catch {
            Write-BucketLog -Data "[Pre-Flight] Error: $_" -Level Error
            #exit 1
        }
        #endregion

        #region Required Assemblies
        Write-BucketLog -Data "[Pre-Flight] ---------- Required Assemblies Check ----------" -Level Info
        try {
            [void][System.Reflection.Assembly]::LoadWithPartialName('presentationframework')
            [void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms')
            Write-BucketLog -Data "[Pre-Flight] Loaded basic system assemblies: presentationframework, System.Windows.Forms" -Level Info
        }
        catch {
            Write-BucketLog -Data "[Pre-Flight] Error during assembly verification: $_" -Level Error
        }
        #endregion

        #region Directory Structure
        Write-BucketLog -Data "[Pre-Flight] ---------- Directory Structure ----------" -Level Info
        $requiredFolders = @(
            @{Path = "Updates"; Description = "Updates"},
            @{Path = "Staging"; Description = "Staging"},
            @{Path = "Mount"; Description = "Mount"},
            @{Path = "CompletedWIMs"; Description = "CompletedWIMs"},
            @{Path = "ImportedWIMs"; Description = "ImportedWIMs"},
            @{Path = "Configs"; Description = "Configs"}
        )
        foreach ($folder in $requiredFolders) {
            try {
                $folderPath = Join-Path -Path $script:workingDirectory -ChildPath $folder.Path
                if (-not (Test-Path -Path $folderPath)) {
                    $result = New-Item -Path $folderPath -ItemType Directory -Force -ErrorAction Stop
                    if ($result) {
                        Write-BucketLog -Data "[Pre-Flight] Created: $($folder.Description) directory" -Level Info
                    }
                }
                else {
                    $testFile = Join-Path -Path $folderPath -ChildPath "test_permission_$([Guid]::NewGuid()).tmp"
                    try {
                        $null = New-Item -Path $testFile -ItemType File -ErrorAction Stop
                        Remove-Item -Path $testFile -Force -ErrorAction SilentlyContinue | Out-Null
                        Write-BucketLog -Data "[Pre-Flight] Verified: $($folder.Description) directory (exists)" -Level Debug
                    }
                    catch {
                        Write-BucketLog -Data "[Pre-Flight] Warning: $($folder.Description) directory may not be writeable" -Level Warning
                        Write-BucketLog -Data "[Pre-Flight] Permission error: $_" -Level Debug
                    }
                }
            }
            catch {
                $errorMsg = "Failed to verify directory: $($folder.Description). Error: $_"
                Write-BucketLog -Data "[Pre-Flight] $errorMsg" -Level Error
                exit 1
            }
        }
        #endregion

        #region WIMs.xml Structure
        Write-BucketLog -Data "[Pre-Flight] ---------- WIMs Structure ----------" -Level Info
        $wimsXmlPath = Join-Path -Path $script:workingDirectory -ChildPath "Configs\WIMs.xml"
        $defaultWimsXml = @"
<?xml version="1.0" encoding="utf-8"?>
<WIMs>
</WIMs>
"@
        $createWimsXml = $false
        if (-not (Test-Path -Path $wimsXmlPath)) {
            $createWimsXml = $true
            Write-BucketLog -Data "[Pre-Flight] WIMs.xml file not found, will create a new one." -Level Debug
        }
        else {
            try {
                [xml]$xmlTest = Get-Content -Path $wimsXmlPath -Raw
                if ($null -eq $xmlTest.WIMs) {
                    Write-BucketLog -Data "[Pre-Flight] WIMs.xml file is invalid, will recreate." -Level Warning
                    $createWimsXml = $true
                }
                else {
                    Write-BucketLog -Data "[Pre-Flight] Verified: WIMs.xml file (exists and valid)" -Level Debug
                }
            }
            catch {
                Write-BucketLog -Data "[Pre-Flight] WIMs.xml file is corrupted or unreadable: $_" -Level Warning
                $createWimsXml = $true
            }
        }
        if ($createWimsXml) {
            try {
                $defaultWimsXml | Out-File -FilePath $wimsXmlPath -Encoding UTF8 -Force -ErrorAction Stop
                Write-BucketLog -Data "[Pre-Flight] Created or repaired: WIMs.xml file" -Level Info
            }
            catch {
                Write-BucketLog -Data "[Pre-Flight] Failed to create or repair WIMs.xml file: $_" -Level Error
                exit 1
            }
        }
        #endregion

        #region Finalize Pre-Flight
        Write-BucketLog -Data "[Pre-Flight] ----------   ----------" -Level Info
        Write-BucketLog -Data "[Pre-Flight] Bucket is ready for use" -Level Info
        Write-BucketLog -Data "[Pre-Flight] ========== PRE-FLIGHT COMPLETE ==========" -Level Info
        #endregion
    }
}
