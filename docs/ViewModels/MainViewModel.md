# MainViewModel Class Documentation

## Overview

The `MainViewModel` class serves as the primary view model for the main window of the Bucket application. It demonstrates the logging system functionality and provides a foundation for main application operations following MVVM architecture patterns.

## Location

- **File**: `src/ViewModels/MainViewModel.cs`
- **Namespace**: `Bucket.ViewModels`

## Class Definition

```csharp
public partial class MainViewModel : ObservableObject
```

## Commands

### TestLogging

```csharp
[RelayCommand]
private void TestLogging()
```

- **Purpose**: Demonstrates comprehensive logging system functionality
- **Access**: Private (exposed through RelayCommand)
- **Functionality**:
  - Tests all log levels (Debug, Information, Warning, Error)
  - Demonstrates structured logging with parameters
  - Shows domain-specific logging for Windows imaging operations
  - Includes performance measurement logging
  - Tests error handling and exception logging

## Usage Examples

### Basic Implementation

```csharp
public partial class MyPage : Page
{
    public MainViewModel ViewModel { get; }

    public MyPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
    }
}
```

### Command Binding in XAML

```xml
<Button Content="Test Logging"
        Command="{x:Bind ViewModel.TestLoggingCommand}" />
```

## Features

### Logging Demonstration

The `TestLogging` command provides comprehensive examples of:

1. **Log Level Testing**:
   - Debug messages (developer mode only)
   - Information for general application flow
   - Warning messages for attention-worthy events
   - Error handling with full exception context

2. **Structured Logging**:
   - User interaction tracking with timestamps
   - Windows imaging domain-specific logging patterns
   - Progress tracking for long-running operations

3. **Performance Monitoring**:
   - Execution time measurement using `Stopwatch`
   - Performance metrics logging for optimization

4. **Error Handling**:
   - Exception logging with full context
   - Structured error information for debugging

### MVVM Compliance

- Inherits from `ObservableObject` (CommunityToolkit.Mvvm)
- Uses `[RelayCommand]` for command implementation
- Follows partial class pattern for MVVM source generators
- Demonstrates proper logging integration in ViewModels

## Dependencies

### Core Dependencies

- **CommunityToolkit.Mvvm**: MVVM infrastructure and source generators
- **LoggerSetup**: Global logging access via `Logger` property

### Related Classes

- **LoggerSetup**: Provides global `Logger` instance
- **AppHelper**: Configuration management for developer mode

## Advanced Usage Examples

### Logging Best Practices Demonstration

```csharp
// Information logging with context
Logger.Information("User action: {ActionName} triggered by {User}",
    "TestLogging", Environment.UserName);

// Domain-specific structured logging
Logger.Information("WIM Analysis: Found {ImageCount} images in {FilePath}",
    imageCount, wimFilePath);

// Performance measurement
var stopwatch = Stopwatch.StartNew();
// ... operation ...
stopwatch.Stop();
Logger.Information("Performance: {Operation} completed in {ElapsedMs}ms",
    operationName, stopwatch.ElapsedMilliseconds);

// Exception handling
try
{
    // ... risky operation ...
}
catch (Exception ex)
{
    Logger.Error(ex, "Operation failed during {Context}", contextInfo);
}
```

## Related Files

- [`LoggerSetup.md`](../Common/LoggerSetup.md) - Logging system configuration
- [`Logging-System.md`](../Logging-System.md) - Complete logging documentation
- [`AppHelper.md`](../Common/AppHelper.md) - Application configuration helpers

## Best Practices

### ViewModel Design

1. **Initialization Logging**: Log ViewModel creation for debugging
2. **Command Implementation**: Use RelayCommand for consistent command patterns
3. **Structured Logging**: Include relevant context in all log messages
4. **Error Handling**: Always log exceptions with full context

### Logging Integration

1. **Use Global Logger**: Access `Logger` directly without injection
2. **Structured Messages**: Include parameters for machine-readable logs
3. **Performance Tracking**: Measure and log operation timing
4. **Context Information**: Include user and environment details

### Testing and Debugging

1. **Developer Mode**: Leverage developer mode for enhanced debugging
2. **Log Level Awareness**: Understand when messages will/won't appear
3. **Exception Simulation**: Test error handling paths regularly

## Error Handling

### Common Scenarios

- **Logger Access**: Logger is globally available and null-safe
- **Command Execution**: RelayCommand handles exceptions automatically
- **Thread Safety**: Serilog logger is thread-safe for concurrent access

### Error Recovery

- **Logging Failures**: Logger configuration includes fallback mechanisms
- **Command Errors**: RelayCommand provides automatic error boundaries
- **State Management**: ObservableObject handles property change notifications

## Performance Considerations

- **Logging Overhead**: Minimal performance impact in production mode
- **Developer Mode**: Enhanced logging in developer mode for debugging
- **Async Operations**: Consider async patterns for long-running operations
- **Memory Usage**: Structured logging parameters are efficiently handled

## Security Considerations

- **User Information**: Be cautious about logging sensitive user data
- **File Paths**: Log paths may contain user information
- **Exception Details**: Full exception details may expose system information

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
