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
    Example of how to use this script/function
#>
function Invoke-BucketPreFlight {
    [CmdletBinding()]
    param(

    )

    process {
        #region Initialize Logging
        try {
            # Ensure working directory structure exists
            $logDirectory = Join-Path -Path $script:workingDirectory -ChildPath "Logs"
            $logFile = Join-Path -Path $logDirectory -ChildPath "Bucket.log"
            
            # Create directories in one step with -Force (creates all parent directories)
            New-Item -Path $logDirectory -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null
            
            # Remove log file if it exists (no need for the else branch)
            if (Test-Path -Path $logFile) {
                Remove-Item -Path $logFile -Force -ErrorAction SilentlyContinue | Out-Null
            }

            # Check if PoShLog module is loaded
            if (-not (Get-Module -Name PoShLog -ErrorAction SilentlyContinue)) {
                # Import the PoShLog module if not already loaded
                Import-Module -Name PoShLog -ErrorAction Stop
            }
            
            # Initialize logger
            New-Logger | 
                Set-MinimumLevel -Value Verbose | 
                Add-SinkConsole | 
                Add-SinkFile -Path $logFile | 
                Start-Logger -ErrorAction Stop
        }
        catch {
            Write-Error "Failed to initialize logger: $_" -ErrorAction Stop
        }
        #endregion Initialize Logging

        #region Log Initialization Status
        Write-BucketLog -Data "Logger initialized successfully." -Level Info
        Write-BucketLog -Data "Working directory: $script:workingDirectory" -Level Debug
        Write-BucketLog -Data "Log file: $logFile" -Level Debug
        Write-BucketLog -Data "Log directory: $logDirectory" -Level Debug
        Write-BucketLog -Data "Logger started successfully." -Level Info
        Write-BucketLog -Data "Starting pre-flight checks..." -Level Info
        #endregion Log Initialization Status

        #region Create Required Directories
        # Create and verify required directories
        $requiredFolders = @(
            @{Path = "Updates"; Description = "Updates"},
            @{Path = "Staging"; Description = "Staging"},
            @{Path = "Mount"; Description = "Mount"},
            @{Path = "CompletedWIMs"; Description = "CompletedWIMs"},
            @{Path = "Configs"; Description = "Configs"}
        )

        foreach ($folder in $requiredFolders) {
            try {
                $folderPath = Join-Path -Path $script:workingDirectory -ChildPath $folder.Path
                
                if (-not (Test-Path -Path $folderPath)) {
                    $result = New-Item -Path $folderPath -ItemType Directory -Force -ErrorAction Stop
                    if ($result) {
                        Write-BucketLog -Data "Created folder: $($folder.Description) at $folderPath" -Level Info
                    }
                } 
                else {
                    # Verify we have write permissions to the existing folder
                    $testFile = Join-Path -Path $folderPath -ChildPath "test_permission_$([Guid]::NewGuid()).tmp"
                    try {
                        $null = New-Item -Path $testFile -ItemType File -ErrorAction Stop
                        Remove-Item -Path $testFile -Force -ErrorAction SilentlyContinue | Out-Null
                        Write-BucketLog -Data "Folder already exists: $($folder.Description) at $folderPath" -Level Debug
                    }
                    catch {
                        Write-BucketLog -Data "Warning: Folder exists but may not be writeable: $($folder.Description) at $folderPath" -Level Warning
                        Write-BucketLog -Data "Permission error: $_" -Level Debug
                    }
                }
            }
            catch {
                $errorMsg = "Failed to create or verify directory: $($folder.Description) at $folderPath. Error: $_"
                Write-BucketLog -Data $errorMsg -Level Error
                throw $errorMsg
            }
        }
        #endregion Create Required Directories

        #region Finalize Pre-Flight
        Write-BucketLog -Data "Bucket pre-flight checks completed." -Level Info
        Write-BucketLog -Data "Bucket is ready for use." -Level Info
        #endregion Finalize Pre-Flight

    }

}
