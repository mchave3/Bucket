# LoggerSetup Class Documentation

## Overview

The `LoggerSetup` class provides centralized logging configuration and access using Serilog for the Bucket application. It implements structured logging with multiple sinks and automatic configuration based on application settings.

**Key Features:**
- Global logger access via static property
- Automatic configuration based on developer mode
- File logging with daily rotation
- Structured logging with application metadata
- Exception handling with fallback configuration

## Location

- **File**: `src/Common/LoggerSetup.cs`
- **Namespace**: `Bucket.Common`

## Class Definition

```csharp
/// <summary>
/// Provides global logging configuration and access using Serilog
/// </summary>
public static partial class LoggerSetup
```

## Properties

### Logger
```csharp
public static ILogger Logger { get; private set; }
```
Global logger instance accessible throughout the application via global using statement.

## Methods

### ConfigureLogger()
```csharp
public static void ConfigureLogger()
```
Configures the global logger with appropriate sinks and settings based on application configuration.

**Features:**
- Automatic log level determination (Debug in developer mode, Information otherwise)
- File logging with daily rotation (retains 7 days)
- Debug output for development
- Structured logging with application metadata
- Exception handling with fallback configuration

### CloseLogger()
```csharp
public static void CloseLogger()
```
Properly closes and flushes the logger to ensure all log entries are written.

## Dependencies

- **Serilog**: Primary logging framework
- **Constants**: For log directory and file paths
- **ProcessInfoHelper**: For application version information

## Properties

### Logger

```csharp
public static ILogger Logger { get; private set; }
```

- **Type**: `ILogger` (Serilog interface)
- **Access**: Public getter, Private setter
- **Purpose**: Main logger instance used throughout the application
- **Lifetime**: Static singleton, created once during application startup

## Methods

### ConfigureLogger()

```csharp
public static void ConfigureLogger()
```

- **Access**: Public, Static
- **Purpose**: Initializes and configures the application logger
- **Returns**: `void`
- **Side Effects**:
  - Creates log directory if it doesn't exist
  - Initializes the static `Logger` property

#### Configuration Details

The method performs the following setup:

1. **Directory Creation**: Ensures log directory exists
2. **Logger Configuration**: Sets up Serilog with multiple outputs
3. **Property Enrichment**: Adds application version to all log entries
4. **Output Sinks**: Configures file and debug console output

#### Logger Configuration

```csharp
Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
    .WriteTo.File(Constants.LogFilePath, rollingInterval: RollingInterval.Day)
    .WriteTo.Debug()
    .CreateLogger();
```

**Enrichment**:
- Adds `Version` property to all log entries
- Uses `ProcessInfoHelper.Version` for current application version

**File Output**:
- **Path**: Defined by `Constants.LogFilePath`
- **Rolling**: Daily log file rotation (`RollingInterval.Day`)
- **Format**: Default Serilog text format

**Debug Output**:
- Writes to debug console (visible in Visual Studio Output window)
- Useful for development and debugging

## Usage Examples

### Basic Logging

```csharp
// Configure logger at application startup
LoggerSetup.ConfigureLogger();

// Use logger throughout application
LoggerSetup.Logger.Information("Application started");
LoggerSetup.Logger.Warning("Configuration file not found, using defaults");
LoggerSetup.Logger.Error("Failed to connect to service: {Error}", exception.Message);
```

### Global Access

Thanks to `global using static Bucket.Common.LoggerSetup;` in `GlobalUsings.cs`:

```csharp
// Direct access without class prefix
Logger.Information("Direct logger access");
Logger.Debug("Debug message: {Value}", someValue);
```

### Structured Logging

```csharp
// Log with structured data
Logger.Information("User {UserId} performed {Action} at {Timestamp}",
    userId, "login", DateTime.Now);

// Log exceptions with context
try
{
    // Some operation
}
catch (Exception ex)
{
    Logger.Error(ex, "Operation failed for user {UserId}", userId);
}
```

## Log Levels

Serilog supports the following log levels (in order of severity):

1. **Verbose**: Extremely detailed information
2. **Debug**: Detailed information for diagnosing problems
3. **Information**: General information about application flow
4. **Warning**: Potentially harmful situations
5. **Error**: Error events that allow application to continue
6. **Fatal**: Critical errors that may cause application termination

### Usage Examples by Level

```csharp
Logger.Verbose("Detailed trace information");
Logger.Debug("Variable value: {Value}", variable);
Logger.Information("User logged in successfully");
Logger.Warning("Deprecated method called");
Logger.Error("Database connection failed");
Logger.Fatal("Critical system failure");
```

## File Output Features

### Daily Rolling

- **New File Daily**: Creates a new log file each day
- **File Naming**: Automatic timestamp suffixes
- **Example**:
  - `Log20250617.txt` (June 17, 2025)
  - `Log20250618.txt` (June 18, 2025)

### File Location

```
%APPDATA%\[ProductName][Version]\Log\
├── Log20250617.txt
├── Log20250618.txt
└── Log20250619.txt
```

## Configuration Best Practices

### Application Startup

```csharp
// In App.xaml.cs or Program.cs
public partial class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configure logging first
        LoggerSetup.ConfigureLogger();

        // Log application start
        Logger.Information("Application launched with version {Version}",
            ProcessInfoHelper.Version);

        // Continue with application initialization
    }
}
```

### Error Handling

```csharp
public static void ConfigureLoggerSafe()
{
    try
    {
        ConfigureLogger();
        Logger.Information("Logging configured successfully");
    }
    catch (Exception ex)
    {
        // Fallback logging mechanism
        System.Diagnostics.Debug.WriteLine($"Failed to configure logger: {ex.Message}");
    }
}
```

## Advanced Configuration

### Custom Log Format

```csharp
Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
    .WriteTo.File(Constants.LogFilePath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Version} {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    .CreateLogger();
```

### Multiple Output Sinks

```csharp
Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
    .WriteTo.File(Constants.LogFilePath, rollingInterval: RollingInterval.Day)
    .WriteTo.File(Path.Combine(Constants.LogDirectoryPath, "errors.txt"),
        restrictedToMinimumLevel: LogEventLevel.Error)
    .WriteTo.Debug()
    .WriteTo.Console()
    .CreateLogger();
```

## Performance Considerations

### Asynchronous Logging

For high-throughput applications:

```csharp
Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Version", ProcessInfoHelper.Version)
    .WriteTo.Async(a => a.File(Constants.LogFilePath, rollingInterval: RollingInterval.Day))
    .WriteTo.Debug()
    .CreateLogger();
```

### Conditional Compilation

```csharp
#if DEBUG
    .WriteTo.Debug()
    .WriteTo.Console()
#endif
```

## Error Scenarios

### Directory Creation Failure

```csharp
public static void ConfigureLogger()
{
    try
    {
        if (!Directory.Exists(Constants.LogDirectoryPath))
        {
            Directory.CreateDirectory(Constants.LogDirectoryPath);
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        // Handle permission issues
        throw new InvalidOperationException("Cannot create log directory", ex);
    }

    // Logger configuration...
}
```

### Disk Space Issues

- Serilog handles disk space issues gracefully
- Rolling files help manage disk usage
- Consider implementing log file cleanup policies

## Related Files

- [`Constants.cs`](./Constants.md) - Provides log directory and file paths
- [`AppHelper.cs`](./AppHelper.md) - May use logger for configuration events
- [`GlobalUsings.cs`](./GlobalUsings.md) - Enables global access to logger

## Security Considerations

- **Log Content**: Avoid logging sensitive information (passwords, tokens)
- **File Permissions**: Log files inherit user directory permissions
- **Log Rotation**: Prevents unbounded disk usage

## Testing Considerations

### Unit Testing

```csharp
[Test]
public void LoggerSetup_ConfigureLogger_CreatesLogDirectory()
{
    // Arrange
    var testLogPath = Path.Combine(Path.GetTempPath(), "TestLogs");

    // Act
    LoggerSetup.ConfigureLogger();

    // Assert
    Assert.That(Directory.Exists(Constants.LogDirectoryPath), Is.True);
    Assert.That(LoggerSetup.Logger, Is.Not.Null);
}
```

### Integration Testing

```csharp
[Test]
public void Logger_WriteToFile_CreatesLogFile()
{
    // Arrange
    LoggerSetup.ConfigureLogger();

    // Act
    LoggerSetup.Logger.Information("Test message");

    // Assert
    Assert.That(File.Exists(Constants.LogFilePath), Is.True);
}
```
