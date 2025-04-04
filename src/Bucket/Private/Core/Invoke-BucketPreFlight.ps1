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
            # Ensure working directory variable is correctly initialized
            if (-not $script:workingDirectory) {
                $script:workingDirectory = Join-Path -Path $env:ProgramData -ChildPath 'Bucket'
                Write-Verbose "Working directory was not initialized, setting to: $script:workingDirectory"
            }
            
            # Ensure working directory structure exists
            if (-not (Test-Path -Path $script:workingDirectory)) {
                New-Item -Path $script:workingDirectory -ItemType Directory -Force -ErrorAction Stop | Out-Null
            }
            
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
            exit 1
        }
        #endregion Initialize Logging

        #region Log Initialization Status
        Write-BucketLog -Data "========== BUCKET INITIALIZATION ==========" -Level Info
        Write-BucketLog -Data "Logger initialized successfully" -Level Info
        Write-BucketLog -Data "Working directory: $script:workingDirectory" -Level Debug
        #endregion Log Initialization Status

        #region Check Administrator Privileges
        Write-BucketLog -Data "---------- Administrator Check ----------" -Level Info
        try {
            $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
            $isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
            
            if (-not $isAdmin) {
                $errorMsg = "Bucket requires administrator privileges. Please restart PowerShell as Administrator."
                Write-BucketLog -Data $errorMsg -Level Error
                #exit 1
            }
            
            Write-BucketLog -Data "Administrator privileges verified" -Level Info
        }
        catch {
            Write-BucketLog -Data "Error: $_" -Level Error
            #exit 1
        }
        #endregion Check Administrator Privileges

        #region Check Required Assemblies
        Write-BucketLog -Data "---------- Required Assemblies Check ----------" -Level Info
        try {
            # Get the module root directory
            $moduleRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
            $assembliesFolder = Join-Path -Path $moduleRoot -ChildPath "Assemblies"
            
            if (Test-Path -Path $assembliesFolder) {
                # Get all DLLs in the Assemblies folder and its subfolders
                $dllFiles = Get-ChildItem -Path $assembliesFolder -Filter "*.dll" -Recurse -ErrorAction Stop
                
                if ($dllFiles.Count -eq 0) {
                    Write-BucketLog -Data "Warning: No DLL files found in the Assemblies folder" -Level Warning
                } 
                else {
                    Write-BucketLog -Data "Found $($dllFiles.Count) DLL files in the Assemblies folder" -Level Info
                    
                    # Check each DLL
                    foreach ($dllFile in $dllFiles) {
                        $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($dllFile.Name)
                        $isLoaded = [System.AppDomain]::CurrentDomain.GetAssemblies() | 
                            Where-Object { $_.GetName().Name -eq $assemblyName }
                        
                        if ($isLoaded) {
                            Write-BucketLog -Data "Assembly '$assemblyName' is already loaded: v$($isLoaded.GetName().Version)" -Level Info
                        } 
                        else {
                            # Attempt to load the assembly
                            try {
                                Add-Type -Path $dllFile.FullName -ErrorAction Stop
                                Write-BucketLog -Data "Successfully loaded assembly: $assemblyName" -Level Info
                            } 
                            catch {
                                Write-BucketLog -Data "Error loading assembly $assemblyName`: $_" -Level Error
                            }
                        }
                    }
                }
            } 
            else {
                Write-BucketLog -Data "Warning: Assemblies folder not found at $assembliesFolder" -Level Warning
            }
            
            # Check if any assemblies from the manifest's RequiredAssemblies are not loaded
            $manifestPath = Join-Path -Path $moduleRoot -ChildPath "Bucket.psd1"
            if (Test-Path -Path $manifestPath) {
                $manifest = Import-PowerShellDataFile -Path $manifestPath -ErrorAction Stop
                
                if ($manifest.RequiredAssemblies -and $manifest.RequiredAssemblies.Count -gt 0) {
                    Write-BucketLog -Data "Verifying manifest required assemblies..." -Level Info
                    
                    foreach ($requiredAssembly in $manifest.RequiredAssemblies) {
                        $assemblyPath = Join-Path -Path $moduleRoot -ChildPath $requiredAssembly
                        $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($assemblyPath)
                        
                        $isLoaded = [System.AppDomain]::CurrentDomain.GetAssemblies() | 
                            Where-Object { $_.GetName().Name -eq $assemblyName }
                            
                        if (-not $isLoaded -and (Test-Path -Path $assemblyPath)) {
                            try {
                                Add-Type -Path $assemblyPath -ErrorAction Stop
                                Write-BucketLog -Data "Loaded required assembly from manifest: $assemblyName" -Level Info
                            } 
                            catch {
                                Write-BucketLog -Data "Error loading required assembly $assemblyName`: $_" -Level Error
                            }
                        }
                    }
                    
                    Write-BucketLog -Data "Manifest required assemblies verification complete" -Level Info
                }
                else {
                    Write-BucketLog -Data "No required assemblies specified in the manifest" -Level Debug
                }
            }
            else {
                Write-BucketLog -Data "Warning: Module manifest not found at $manifestPath" -Level Warning
            }
            
            Write-BucketLog -Data "Assembly verification complete" -Level Info
        } 
        catch {
            Write-BucketLog -Data "Error during assembly verification: $_" -Level Error
        }
        #endregion Check Required Assemblies

        #region Create Required Directories
        Write-BucketLog -Data "---------- Directory Structure ----------" -Level Info
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
                        Write-BucketLog -Data "Created: $($folder.Description) directory" -Level Info
                    }
                } 
                else {
                    # Verify we have write permissions to the existing folder
                    $testFile = Join-Path -Path $folderPath -ChildPath "test_permission_$([Guid]::NewGuid()).tmp"
                    try {
                        $null = New-Item -Path $testFile -ItemType File -ErrorAction Stop
                        Remove-Item -Path $testFile -Force -ErrorAction SilentlyContinue | Out-Null
                        Write-BucketLog -Data "Verified: $($folder.Description) directory (exists)" -Level Debug
                    }
                    catch {
                        Write-BucketLog -Data "Warning: $($folder.Description) directory may not be writeable" -Level Warning
                        Write-BucketLog -Data "Permission error: $_" -Level Debug
                    }
                }
            }
            catch {
                $errorMsg = "Failed to verify directory: $($folder.Description). Error: $_"
                Write-BucketLog -Data $errorMsg -Level Error
                exit 1
            }
        }
        #endregion Create Required Directories

        #region Finalize Pre-Flight
        Write-BucketLog -Data "Bucket is ready for use" -Level Info
        Write-BucketLog -Data "========== PRE-FLIGHT COMPLETE ==========" -Level Info
        #endregion Finalize Pre-Flight
    }
}
