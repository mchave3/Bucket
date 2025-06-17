# Logging System Documentation - Bucket Application

## Overview

The Bucket application implements a comprehensive logging system using **Serilog** for .NET and **PoShLog** for PowerShell components. The logging system provides centralized error tracking, debugging capabilities, and operational monitoring with support for multiple output targets and severity levels.

## Architecture Components

### 1. .NET Logging System (Serilog)

#### Core Implementation

**File**: `src/Common/LoggerSetup.cs`

```csharp
public static partial class LoggerSetup
{
    public static ILogger Logger { get; private set; }

    public static void ConfigureLogger()
    {
        if (!Directory.Exists(Constants.LogDirectoryPath))
        {
            Directory.CreateDirectory(Constants.LogDirectoryPath);
        }

        Logger = new LoggerConfiguration()
            .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
            .WriteTo.File(Constants.LogFilePath, rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .CreateLogger();
    }
}
```

#### Global Access

The logger is globally accessible through the `GlobalUsings.cs` file:

```csharp
global using static Bucket.Common.LoggerSetup;
```

This allows any part of the application to use `Logger?.Error()`, `Logger?.Info()`, etc. directly.

### 2. Configuration and Paths

#### Directory Structure

**File**: `src/Common/Constants.cs`

```csharp
public static partial class Constants
{
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductNameAndVersion);
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");
}
```

#### Default Log Locations

- **Root Directory**: `%AppData%/Bucket/{Version}/`
- **Log Directory**: `%AppData%/Bucket/{Version}/Log/`
- **Log File**: `%AppData%/Bucket/{Version}/Log/Log.txt`

### 3. Conditional Logging (Developer Mode)

#### Configuration Settings

**File**: `src/Common/AppConfig.cs`

```csharp
[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.0.0.0")]
    public Version Version { get; set; } = new Version(1, 0, 0, 0);

    private bool useDeveloperMode { get; set; }
    private string lastUpdateCheck { get; set; }
}
```

#### Developer Mode Activation

**File**: `src/App.xaml.cs`

```csharp
private async void InitializeApp()
{
    // Context menu setup...

    if (Settings.UseDeveloperMode)
    {
        ConfigureLogger();
    }

    UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
}
```

#### UI Configuration

**File**: `src/Views/Settings/GeneralSettingPage.xaml`

```xml
<dev:SettingsExpander Description="By activating this option, if an error or crash occurs, its information will be saved in a file called Log{YYYYMMDD}.txt"
                      Header="Developer Mode (Restart Required)"
                      HeaderIcon="{dev:BitmapIcon Source=Assets/Fluent/DevMode.png}">
    <ToggleSwitch IsOn="{x:Bind common:AppHelper.Settings.UseDeveloperMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
    <dev:SettingsExpander.ItemsHeader>
        <HyperlinkButton HorizontalAlignment="Stretch"
                         HorizontalContentAlignment="Left"
                         Click="NavigateToLogPath_Click"
                         Content="{x:Bind common:Constants.LogDirectoryPath}" />
    </dev:SettingsExpander.ItemsHeader>
</dev:SettingsExpander>
```

#### Log Path Navigation

**File**: `src/Views/Settings/GeneralSettingPage.xaml.cs`

```csharp
private async void NavigateToLogPath_Click(object sender, RoutedEventArgs e)
{
    string folderPath = (sender as HyperlinkButton).Content.ToString();
    if (Directory.Exists(folderPath))
    {
        Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
        await Windows.System.Launcher.LaunchFolderAsync(folder);
    }
}
```

## Serilog Configuration Features

### 1. Output Targets

#### File Logging
- **Rolling Interval**: Daily rotation (`RollingInterval.Day`)
- **File Path**: `%AppData%/Bucket/{Version}/Log/Log.txt`
- **Format**: Plain text with timestamps

#### Debug Output
- **Target**: Visual Studio Debug Output Window
- **Usage**: Development and debugging scenarios

### 2. Log Enrichment

#### Version Information
```csharp
.Enrich.WithProperty("Version", ProcessInfoHelper.Version)
```

All log entries are enriched with the current application version for better tracking and debugging.

### 3. Log File Management

#### Automatic Directory Creation
```csharp
if (!Directory.Exists(Constants.LogDirectoryPath))
{
    Directory.CreateDirectory(Constants.LogDirectoryPath);
}
```

The system automatically creates the log directory structure if it doesn't exist.

#### Daily File Rotation
- New log file created each day
- File naming pattern: `Log{YYYYMMDD}.txt`
- Automatic cleanup of old log files (based on Serilog configuration)

## Usage Patterns

### 1. Exception Logging

#### Unhandled Exceptions
```csharp
UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
```

### 2. Conditional Logging

#### Safe Logging Calls
```csharp
Logger?.Info("Application started successfully");
Logger?.Error(exception, "Failed to process request");
Logger?.Debug("Processing item {ItemId}", itemId);
```

The null-conditional operator (`?.`) ensures logging calls don't fail when the logger is not initialized.

### 3. Structured Logging

#### Message Templates
```csharp
Logger?.Information("User {UserId} performed {Action} at {Timestamp}", userId, action, DateTime.Now);
```

Serilog supports structured logging with message templates for better log analysis.

## Legacy PowerShell Logging

### 1. PoShLog Integration

The application also includes a legacy PowerShell logging system using PoShLog:

#### PowerShell Logger Configuration
```powershell
New-Logger |
    Set-MinimumLevel -Value Verbose |
    Add-SinkConsole |
    Add-SinkFile -Path $logFile |
    Start-Logger -ErrorAction Stop
```

#### Custom Logging Function

**File**: `src.powershell.old/Bucket/Private/Core/Write-BucketLog.ps1`

```powershell
function Write-BucketLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Data,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Verbose', 'Debug', 'Info', 'Warning', 'Error', 'Fatal')]
        [string]$Level = 'Info'
    )

    # Automatic caller detection and formatting
    # Output: [FunctionName] Message content
}
```

### 2. Log Levels and Usage

#### PowerShell Log Levels
- **Verbose**: Implementation details (`"Creating temporary variables"`)
- **Debug**: Important values (`"Working path: X"`)
- **Info**: Main steps (`"Initialization complete"`)
- **Warning**: Suboptimal situations (`"Folder already exists, using existing folder"`)
- **Error**: Operation failures (`"Unable to create folder"`)
- **Fatal**: Critical conditions (`"Incompatible configuration detected"`)

## Dependencies

### NuGet Packages

**File**: `src/Bucket.csproj`

```xml
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Debug" Version="3.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
```

### PowerShell Modules
- **PoShLog**: PowerShell logging framework for legacy components

## Configuration Best Practices

### 1. Development vs Production

#### Development Mode Benefits
- **Detailed Logging**: All operations are logged for debugging
- **File Persistence**: Logs are saved to disk for analysis
- **Error Tracking**: Unhandled exceptions are automatically captured

#### Production Mode
- **Minimal Overhead**: Logging is disabled to improve performance
- **Clean Operation**: No log files are created unless explicitly enabled

### 2. User Control

#### Settings Integration
- **UI Toggle**: Users can enable/disable developer mode through settings
- **Restart Required**: Changes require application restart to take effect
- **Path Access**: Direct access to log directory through settings UI

### 3. Error Handling

#### Graceful Degradation
```csharp
Logger?.Error(exception, "Operation failed");
```

Using null-conditional operators ensures the application continues to function even if logging fails.

## Monitoring and Maintenance

### 1. Log File Management

#### Automatic Cleanup
- Daily rotation prevents large single files
- Serilog handles file management automatically
- Old files are retained based on configuration

#### Manual Access
- Settings page provides direct link to log directory
- Users can easily access logs for troubleshooting
- Logs are stored in standard AppData location

### 2. Performance Considerations

#### Conditional Activation
- Logging only active when developer mode is enabled
- Minimal performance impact in production
- Debug output available during development

#### Structured Format
- Machine-readable log format
- Easy parsing for analysis tools
- Consistent message templates

## Troubleshooting

### 1. Common Issues

#### Logging Not Working
1. Check if Developer Mode is enabled in settings
2. Verify application has write permissions to AppData
3. Restart application after enabling Developer Mode

#### Log Files Not Created
1. Ensure log directory exists and is writable
2. Check disk space availability
3. Verify Serilog configuration is correct

### 2. Log Analysis

#### Log Location
Navigate to: `%AppData%/Bucket/{Version}/Log/`

#### File Format
- **Text Format**: Human-readable log entries
- **Timestamped**: Each entry includes precise timestamp
- **Version Tagged**: All entries include application version
- **Structured**: Consistent format for parsing

## Future Enhancements

### Potential Improvements
1. **Remote Logging**: Send logs to central server for analysis
2. **Log Levels**: Add configuration for different log levels
3. **Retention Policy**: Configurable log file retention
4. **Performance Metrics**: Add performance counters to logs
5. **User Context**: Include user actions in log context

### Integration Opportunities
1. **Analytics**: Integration with application analytics
2. **Crash Reporting**: Automatic crash report generation
3. **Telemetry**: Optional telemetry data collection
4. **Support Tools**: Integration with support ticket systems
