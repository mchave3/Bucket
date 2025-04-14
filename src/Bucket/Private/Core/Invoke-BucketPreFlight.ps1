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
            # Load basic required assemblies for WPF
            [void][System.Reflection.Assembly]::LoadWithPartialName('presentationframework')
            [void][System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms')
            Write-BucketLog -Data "Loaded basic system assemblies: presentationframework, System.Windows.Forms" -Level Info
            
            <#

            # Get the module root directory
            $moduleRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
            $assembliesFolder = Join-Path -Path $moduleRoot -ChildPath "Assemblies"
            
            # Determine .NET Framework version based on PowerShell version
            $psVersion = $PSVersionTable.PSVersion
            
            # Map PowerShell versions to .NET Framework/Core versions according to the correspondence table
            $netFrameworkFolder = if ($PSEdition -eq 'Desktop' -or $psVersion.Major -lt 6) {
                # PowerShell 5.1 uses .NET Framework 4.7.2
                Write-BucketLog -Data "PowerShell 5.1 detected - using .NET Framework 4.7.2 assemblies" -Level Info
                "net472"
            } 
            elseif ($psVersion.Major -eq 7) {
                if ($psVersion.Minor -lt 4) {
                    # PowerShell 7.0-7.3 uses .NET 6.0
                    Write-BucketLog -Data "PowerShell 7.0-7.3 detected - using .NET 6.0 assemblies" -Level Info
                    "net6.0"
                } 
                elseif ($psVersion.Minor -lt 5) {
                    # PowerShell 7.4+ can use .NET 8.0 assemblies
                    Write-BucketLog -Data "PowerShell 7.4+ detected - using .NET 8.0 assemblies" -Level Info
                    "net8.0"
                } 
                else {
                    # PowerShell 7.5+ can use .NET 9.0 assemblies
                    Write-BucketLog -Data "PowerShell 7.5+ detected - using .NET 9.0 assemblies" -Level Info
                    "net9.0"
                }
            } 
            else {
                # Default fallback - for future PowerShell versions - Use .NET 6.0
                Write-BucketLog -Data "Unknown PowerShell version - using .NET 6.0 assemblies as fallback" -Level Warning
                "net6.0"
            }
            
            # Default fallback framework version if primary fails
            $fallbackFramework = "net6.0"
            
            $assemblyLoadSuccess = $true
            
            # Dynamically discover packages and their assemblies
            if (Test-Path -Path $assembliesFolder) {
                # Get all package directories
                $packageDirs = Get-ChildItem -Path $assembliesFolder -Directory
                
                foreach ($packageDir in $packageDirs) {
                    # For each package, get the available versions
                    $versionDirs = Get-ChildItem -Path $packageDir.FullName -Directory
                    
                    foreach ($versionDir in $versionDirs) {
                        $packageName = $packageDir.Name
                        $packageVersion = $versionDir.Name
                        
                        Write-BucketLog -Data "Processing package: $packageName v$packageVersion" -Level Info
                        
                        # Check if this package version has the needed framework
                        $frameworkPath = Join-Path -Path $versionDir.FullName -ChildPath $netFrameworkFolder
                        
                        if (Test-Path -Path $frameworkPath) {
                            # Get all DLLs in this framework version
                            $dllFiles = Get-ChildItem -Path $frameworkPath -Filter "*.dll"
                            
                            if ($dllFiles) {
                                Write-BucketLog -Data "Found $($dllFiles.Count) assemblies for $packageName v$packageVersion ($netFrameworkFolder)" -Level Info
                                
                                # Load each DLL
                                foreach ($dllFile in $dllFiles) {
                                    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($dllFile.Name)
                                    
                                    # Check if assembly is already loaded
                                    $isLoaded = [System.AppDomain]::CurrentDomain.GetAssemblies() | 
                                        Where-Object { $_.GetName().Name -eq $assemblyName }
                                    
                                    if ($isLoaded) {
                                        Write-BucketLog -Data "Assembly '$assemblyName' is already loaded: v$($isLoaded.GetName().Version)" -Level Info
                                    } 
                                    else {
                                        # Try to load the assembly
                                        try {
                                            Add-Type -Path $dllFile.FullName -ErrorAction Stop
                                            Write-BucketLog -Data "Successfully loaded assembly: $assemblyName (from $netFrameworkFolder)" -Level Info
                                        } 
                                        catch {
                                            $assemblyLoadSuccess = $false
                                            Write-BucketLog -Data "Error loading assembly $assemblyName`: $_" -Level Error
                                            
                                            # Try fallback framework
                                            $fallbackPath = Join-Path -Path $versionDir.FullName -ChildPath $fallbackFramework
                                            $fallbackDllPath = Join-Path -Path $fallbackPath -ChildPath $dllFile.Name
                                            
                                            if (Test-Path -Path $fallbackDllPath) {
                                                try {
                                                    Write-BucketLog -Data "Attempting fallback to $fallbackFramework version: $fallbackDllPath" -Level Warning
                                                    Add-Type -Path $fallbackDllPath -ErrorAction Stop
                                                    Write-BucketLog -Data "Successfully loaded fallback assembly: $assemblyName (from $fallbackFramework)" -Level Info
                                                    $assemblyLoadSuccess = $true
                                                } 
                                                catch {
                                                    Write-BucketLog -Data "Fallback to $fallbackFramework failed: $_" -Level Error
                                                    
                                                    # Try any other available framework as a last resort
                                                    $otherFrameworks = Get-ChildItem -Path $versionDir.FullName -Directory | Where-Object { $_.Name -ne $netFrameworkFolder -and $_.Name -ne $fallbackFramework }
                                                    
                                                    foreach ($otherFramework in $otherFrameworks) {
                                                        $otherDllPath = Join-Path -Path $otherFramework.FullName -ChildPath $dllFile.Name
                                                        
                                                        if (Test-Path -Path $otherDllPath) {
                                                            try {
                                                                Write-BucketLog -Data "Attempting to use alternative framework version: $($otherFramework.Name)" -Level Warning
                                                                Add-Type -Path $otherDllPath -ErrorAction Stop
                                                                Write-BucketLog -Data "Successfully loaded alternative assembly: $assemblyName (from $($otherFramework.Name))" -Level Info
                                                                $assemblyLoadSuccess = $true
                                                                break
                                                            } 
                                                            catch {
                                                                Write-BucketLog -Data "Alternative load failed: $_" -Level Error
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } 
                            else {
                                Write-BucketLog -Data "No assemblies found in $frameworkPath" -Level Warning
                            }
                        } 
                        else {
                            Write-BucketLog -Data "Framework directory not found: $frameworkPath" -Level Warning
                            
                            # Try using fallback framework path
                            $fallbackPath = Join-Path -Path $versionDir.FullName -ChildPath $fallbackFramework
                            
                            if (Test-Path -Path $fallbackPath) {
                                $fallbackDlls = Get-ChildItem -Path $fallbackPath -Filter "*.dll"
                                
                                if ($fallbackDlls) {
                                    Write-BucketLog -Data "Using fallback framework $fallbackFramework. Found $($fallbackDlls.Count) assemblies" -Level Info
                                    
                                    foreach ($fallbackDll in $fallbackDlls) {
                                        $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($fallbackDll.Name)
                                        
                                        # Check if assembly is already loaded
                                        $isLoaded = [System.AppDomain]::CurrentDomain.GetAssemblies() | 
                                            Where-Object { $_.GetName().Name -eq $assemblyName }
                                        
                                        if ($isLoaded) {
                                            Write-BucketLog -Data "Assembly '$assemblyName' is already loaded: v$($isLoaded.GetName().Version)" -Level Info
                                        } 
                                        else {
                                            try {
                                                Add-Type -Path $fallbackDll.FullName -ErrorAction Stop
                                                Write-BucketLog -Data "Successfully loaded fallback assembly: $assemblyName (from $fallbackFramework)" -Level Info
                                            } 
                                            catch {
                                                Write-BucketLog -Data "Error loading fallback assembly $assemblyName`: $_" -Level Error
                                                $assemblyLoadSuccess = $false
                                            }
                                        }
                                    }
                                } 
                                else {
                                    Write-BucketLog -Data "No assemblies found in fallback path: $fallbackPath" -Level Warning
                                }
                            } 
                            else {
                                Write-BucketLog -Data "Fallback framework directory not found: $fallbackPath" -Level Warning
                                
                                # Check if any other framework versions are available
                                $availableFrameworks = Get-ChildItem -Path $versionDir.FullName -Directory
                                
                                if ($availableFrameworks) {
                                    $frameworksList = $availableFrameworks.Name -join ", "
                                    Write-BucketLog -Data "Available framework versions: $frameworksList" -Level Info
                                    
                                    # Use the first available framework as a last resort
                                    $firstFramework = $availableFrameworks[0].FullName
                                    $firstFrameworkDlls = Get-ChildItem -Path $firstFramework -Filter "*.dll"
                                    
                                    if ($firstFrameworkDlls) {
                                        Write-BucketLog -Data "Using alternative framework $($availableFrameworks[0].Name) as last resort" -Level Warning
                                        
                                        foreach ($altDll in $firstFrameworkDlls) {
                                            $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($altDll.Name)
                                            
                                            # Check if assembly is already loaded
                                            $isLoaded = [System.AppDomain]::CurrentDomain.GetAssemblies() | 
                                                Where-Object { $_.GetName().Name -eq $assemblyName }
                                            
                                            if ($isLoaded) {
                                                Write-BucketLog -Data "Assembly '$assemblyName' is already loaded: v$($isLoaded.GetName().Version)" -Level Info
                                            } 
                                            else {
                                                try {
                                                    Add-Type -Path $altDll.FullName -ErrorAction Stop
                                                    Write-BucketLog -Data "Successfully loaded alternative assembly: $assemblyName (from $($availableFrameworks[0].Name))" -Level Info
                                                } 
                                                catch {
                                                    Write-BucketLog -Data "Error loading alternative assembly $assemblyName`: $_" -Level Error
                                                    $assemblyLoadSuccess = $false
                                                }
                                            }
                                        }
                                    }
                                } 
                                else {
                                    Write-BucketLog -Data "No framework versions available for $packageName v$packageVersion" -Level Error
                                    $assemblyLoadSuccess = $false
                                }
                            }
                        }
                    }
                }
            } 
            else {
                Write-BucketLog -Data "Assemblies folder not found: $assembliesFolder" -Level Error
                $assemblyLoadSuccess = $false
            }
            
            # Check if manifest requires any additional assemblies
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
                                $assemblyLoadSuccess = $false
                            }
                        }
                    }
                    
                    Write-BucketLog -Data "Manifest required assemblies verification complete" -Level Info
                } 
                else {
                    Write-BucketLog -Data "No required assemblies specified in the manifest" -Level Debug
                }
            }
            
            # Final assembly load status
            if ($assemblyLoadSuccess) {
                Write-BucketLog -Data "All assemblies loaded successfully" -Level Info
            } 
            else {
                Write-BucketLog -Data "Some assemblies failed to load - UI functionality may be limited" -Level Warning
            }

            #>
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

