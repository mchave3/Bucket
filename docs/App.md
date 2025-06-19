# App Class Documentation

## Overview

The main application class for Bucket, responsible for application lifecycle management, dependency injection configuration, and global service access. This class serves as the entry point for the WinUI 3 application and manages core services like navigation, theming, and logging.

## Location

- **File**: `src/App.xaml.cs`
- **Namespace**: `Bucket`

## Class Definition

```csharp
public partial class App : Application
```

## Properties

### Current
```csharp
public new static App Current => (App)Application.Current;
```
Gets the current App instance with proper type casting.

### MainWindow
```csharp
public static Window MainWindow = Window.Current;
```
Gets the main application window reference.

### Hwnd
```csharp
public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
```
Gets the native window handle for Win32 interop scenarios.

### Services
```csharp
public IServiceProvider Services { get; }
```
Gets the dependency injection service provider.

### NavService
```csharp
public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
```
Gets the JSON-based navigation service instance.

### ThemeService
```csharp
public IThemeService ThemeService => GetService<IThemeService>();
```
Gets the theme management service instance.

## Methods

### GetService&lt;T&gt;
```csharp
public static T GetService<T>() where T : class
```
Generic method to retrieve registered services from the dependency injection container.

**Returns:** The requested service instance
**Throws:** `ArgumentException` if the service is not registered

### Constructor
```csharp
public App()
```
Initializes the application, configures services, and enables performance optimizations.

### ConfigureServices
```csharp
private static IServiceProvider ConfigureServices()
```
Configures the dependency injection container with all required services.

### OnLaunched
```csharp
protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
```
Handles application launch, initializes logging, and sets up the main window.

## Usage Examples

### Service Access
```csharp
// Get navigation service
var navService = App.GetService<IJsonNavigationService>();

// Get theme service
var themeService = App.Current.ThemeService;

// Get custom service
var customService = App.GetService<ICustomService>();
```

### Window Handle Access
```csharp
// Get native window handle for Win32 APIs
IntPtr hwnd = App.Hwnd;
```

## Features

### Dependency Injection
- **Service Container**: Centralized service registration and resolution
- **Lifetime Management**: Proper service lifecycle management
- **Type Safety**: Generic service access with compile-time type checking

### Performance Optimization
- **Multicore JIT**: Enables profile-guided optimization for faster startup
- **Profile Root**: Uses application directory for JIT profiles
- **Startup Profile**: Optimizes common startup code paths

### Application Lifecycle
- **Initialization**: Configures logging, services, and UI components
- **Window Management**: Creates and manages the main application window
- **Navigation Setup**: Initializes the navigation framework

## Dependencies

### External Dependencies
- **Microsoft.UI.Xaml**: Core WinUI 3 framework
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container
- **DevWinUI.Controls**: Navigation and theme services
- **WinRT.Interop**: Win32 interop capabilities

### Internal Dependencies
- [`Constants`](../Common/Constants.md): Application constants and paths
- [`LoggerSetup`](../Common/LoggerSetup.md): Logging configuration
- [`AppConfig`](../Common/AppConfig.md): Application configuration
- [`MainWindow`](./MainWindow.md): Main application window

## Related Files

- [`MainWindow.md`](./MainWindow.md): Main application window documentation
- [`Constants.md`](../Common/Constants.md): Application constants
- [`LoggerSetup.md`](../Common/LoggerSetup.md): Logging system setup
- [`Navigation-Bar-Management.md`](../Navigation-Bar-Management.md): Navigation system

## Best Practices

### Service Registration
- Register all services in `ConfigureServices()` method
- Use appropriate service lifetimes (Singleton, Scoped, Transient)
- Register interfaces rather than concrete types when possible

### Service Access
- Use `App.GetService<T>()` for service resolution
- Cache service references in ViewModels for performance
- Handle service registration errors gracefully

### Error Handling
- Always check service registration before use
- Use meaningful error messages for missing services
- Log service initialization errors

## Error Handling

### Common Error Scenarios
1. **Service Not Registered**: Service requested but not configured in DI container
2. **Window Initialization**: Issues creating or showing main window
3. **Navigation Setup**: Problems initializing navigation service

### Error Recovery
- Clear error messages for missing service registrations
- Graceful fallback when optional services are unavailable
- Comprehensive logging for troubleshooting

## Security Considerations

- **Service Isolation**: Services are properly encapsulated
- **Window Handle Access**: Controlled access to native window handle
- **Configuration Security**: Sensitive configuration handled securely

## Performance Notes

### Startup Optimization
- Multicore JIT compilation for faster application startup
- Lazy service initialization where appropriate
- Efficient service container configuration

### Memory Management
- Proper service lifetime management
- Resource disposal in application shutdown
- Efficient window handle management

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
