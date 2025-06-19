# PreFlightService Class Documentation

## Overview
The `PreFlightService` class is responsible for performing comprehensive system checks at application startup to ensure all prerequisites are met for Windows image management operations. It validates system capabilities, creates necessary directory structures, and verifies that Bucket can operate correctly.

## Location
- **File**: `src/Common/PreFlightService.cs`
- **Namespace**: `Bucket.Common`

## Class Definition
```csharp
public class PreFlightService
```

## Purpose
The pre-flight service ensures that:
- The application has necessary administrator privileges
- Required directory structure exists in ProgramData
- System tools (DISM, PowerShell) are available
- Sufficient disk space is available
- Windows version is compatible
- Required services are running
- Configuration files are properly initialized

## Pre-Flight Check Categories

### 🔴 Critical Checks (Block startup if failed)

#### 1. Administrator Privileges
- **Purpose**: Ensures the application runs with elevated privileges
- **Required**: Yes - Windows image management requires admin rights
- **Failure Action**: Application terminates with error dialog

#### 2. Directory Structure
- **Purpose**: Creates ProgramData directory structure
- **Directories Created**:
  - `C:\ProgramData\Bucket\` (Working Directory)
  - `C:\ProgramData\Bucket\Updates\` (Windows Updates)
  - `C:\ProgramData\Bucket\Staging\` (Temporary staging area)
  - `C:\ProgramData\Bucket\Mount\` (WIM file mount points)
  - `C:\ProgramData\Bucket\CompletedWIMs\` (Finished images)
  - `C:\ProgramData\Bucket\ImportedWIMs\` (Imported images)
  - `C:\ProgramData\Bucket\Configs\` (Configuration files)
  - `C:\ProgramData\Bucket\Logs\` (Application logs)

#### 3. Write Permissions
- **Purpose**: Verifies write access to all working directories
- **Method**: Creates temporary test files in each directory
- **Cleanup**: Removes test files after verification

#### 4. Disk Space
- **Purpose**: Ensures sufficient space for Windows image operations
- **Minimum Required**: 15 GB free space on C: drive
- **Rationale**: WIM files and updates can be several GB in size

#### 5. System Tools
- **Purpose**: Verifies required external tools are available
- **Tools Checked**:
  - `DISM.exe` (Critical - required for WIM operations)
  - `PowerShell.exe` (Recommended - for advanced operations)

### 🟡 Recommended Checks (Warnings only)

#### 6. Windows Version
- **Purpose**: Ensures compatibility with Windows imaging APIs
- **Minimum Build**: 17763 (Windows 10 version 1809)
- **Action**: Warning dialog if older version detected

#### 7. Windows Services
- **Purpose**: Verifies services needed for update downloads
- **Services Checked**:
  - Windows Update Service (`wuauserv`)
  - Background Intelligent Transfer Service (`BITS`)
- **Action**: Warnings logged if services not running

#### 8. Network Connectivity
- **Purpose**: Tests connectivity to Microsoft Update servers
- **Test**: HTTP GET to `https://update.microsoft.com`
- **Timeout**: 10 seconds
- **Action**: Warning if connectivity fails

### 🔧 Configuration Setup (Auto-repair)

#### 9. Configuration Files
- **Purpose**: Creates or repairs configuration files
- **Files Managed**:
  - `WIMs.xml` - Windows image registry
- **Validation**: XML structure verification
- **Recovery**: Corrupted files are backed up and recreated

## Usage

### Service Registration
```csharp
// In App.xaml.cs ConfigureServices()
services.AddSingleton<PreFlightService>();
```

### Execution at Startup
```csharp
// In App.xaml.cs InitializeApp()
var preFlightService = GetService<PreFlightService>();
var result = await preFlightService.RunPreFlightChecksAsync();

if (!result.AllCriticalChecksPassed)
{
    // Show error dialog and exit application
    Application.Current.Exit();
}
```

## Result Handling

### PreFlightResult Properties
- `AllCriticalChecksPassed` - Overall success status
- `FailedCriticalChecks` - List of failed critical checks
- `FailedRecommendedChecks` - List of failed recommended checks
- `ErrorMessages` - Detailed error descriptions
- `WarningMessages` - Warning descriptions

### Error Handling Flow
1. **Critical Failures**: Application shows error dialog and exits
2. **Warnings**: Non-blocking warning dialog, application continues
3. **Configuration Issues**: Auto-repair attempted, warnings logged

## Configuration Integration

### AppConfig Properties
The service updates configuration tracking properties:
- `PreFlightCompleted` - Whether checks passed
- `LastPreFlightCheck` - Timestamp of last check

### Skip Options
Debug/testing skip options available:
- `SkipAdminCheck` - Bypass administrator privilege check
- `SkipDiskSpaceCheck` - Bypass disk space verification

## Logging Integration

### Log Levels Used
- **Information**: Successful checks and progress
- **Warning**: Non-critical issues, service problems
- **Error**: Critical failures that prevent startup
- **Debug**: Detailed diagnostic information
- **Fatal**: Application termination conditions

### Log Structure
```
========== BUCKET PRE-FLIGHT CHECKS ==========
---------- Critical System Checks ----------
Administrator privileges verified
Created: Working Directory at C:\ProgramData\Bucket
...
========== PRE-FLIGHT COMPLETE - SUCCESS ==========
```

## Directory Structure Created

```
C:\ProgramData\Bucket\
├── Updates\                 # Windows update packages
├── Staging\                 # Temporary work area
├── Mount\                   # WIM mount points
├── CompletedWIMs\          # Finished image files
├── ImportedWIMs\           # Imported source images
├── Configs\                # Configuration files
│   └── WIMs.xml            # Image registry
└── Logs\                   # Application logs
    └── Bucket.log          # Primary log file (with daily rotation)
```

## Error Recovery

### Automatic Recovery
- **Corrupted Config**: Backup and recreate with defaults
- **Missing Directories**: Create with proper permissions
- **Invalid XML**: Repair with valid structure

### Manual Recovery
- **Admin Rights**: User must restart as administrator
- **Disk Space**: User must free up disk space
- **Missing Tools**: User must install Windows SDK/ADK

## Best Practices

### Implementation
1. **Early Execution**: Run before any other application initialization
2. **Graceful Degradation**: Continue with warnings where possible
3. **User Communication**: Clear error messages and resolution steps
4. **Logging**: Comprehensive logging for troubleshooting

### Maintenance
1. **Regular Updates**: Update minimum requirements as needed
2. **Test Coverage**: Verify all check scenarios
3. **Performance**: Keep checks fast to minimize startup delay
4. **Backwards Compatibility**: Handle configuration migrations

## Related Files

- [`PreFlightResult.cs`](../Models/PreFlightResult.md) - Result data model
- [`Constants.cs`](./Constants.md) - Directory and requirement constants
- [`AppConfig.cs`](./AppConfig.md) - Configuration persistence
- [`App.xaml.cs`](../App.md) - Application startup integration

## Security Considerations

- **Elevation Prompt**: Admin privilege check triggers UAC
- **Directory Permissions**: ProgramData location ensures proper access
- **File Validation**: XML structure validation prevents injection
- **Error Information**: Avoid exposing sensitive system details

## Performance Notes

- **Parallel Execution**: Independent checks run concurrently where possible
- **Timeout Handling**: Network checks have reasonable timeouts
- **Cached Results**: Configuration migration status is cached
- **Minimal I/O**: Efficient file system operations

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
