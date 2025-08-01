# GitHub Copilot Instructions - Bucket Project

## Project Overview

**Bucket** is a modern Windows application developed with WinUI 3 and .NET 9, following the MVVM (Model-View-ViewModel) pattern.

**Purpose**: Bucket is a Windows image management tool designed to provision updates in Windows images, automatically download updates according to WIM files, and manage Windows deployment scenarios.

### Core Technologies
- **Framework**: .NET 9 (net9.0-windows10.0.26100.0)
- **UI Framework**: WinUI 3 (UseWinUI)
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Service Architecture**: Specialized services with dependency injection following SRP
- **Target Platform**: Windows 10/11 (10.0.17763.0+)
- **Supported Architectures**: x86, x64, ARM64

## Project Structure

```
src/
├── Views/           # XAML pages and controls
├── ViewModels/      # MVVM ViewModels
├── Models/          # Data models
├── Services/        # Business logic services
│   ├── WindowsImage/    # Specialized Windows image services
│   │   ├── IWindowsImageFileService.cs
│   │   ├── WindowsImageFileService.cs
│   │   ├── IWindowsImageMetadataService.cs
│   │   ├── WindowsImageMetadataService.cs
│   │   ├── IWindowsImagePowerShellService.cs
│   │   ├── WindowsImagePowerShellService.cs
│   │   ├── IWindowsImageIsoService.cs
│   │   └── WindowsImageIsoService.cs
│   └── WindowsImageService.cs  # Main coordinator service
├── Common/          # Common utility classes
├── Themes/          # Theme resources and styles
├── T4Templates/     # T4 templates for code generation
└── Assets/          # Resources (icons, images)
```

## Development Conventions

### Naming
- **Classes**: PascalCase (e.g., `MainViewModel`, `SettingsPage`)
- **Methods/Properties**: PascalCase (e.g., `LoadData()`, `IsVisible`)
- **Local variables**: camelCase (e.g., `userData`, `configPath`)
- **Constants**: PascalCase (e.g., `DefaultTimeout`)

### MVVM Architecture
- Use `ObservableObject` from CommunityToolkit.Mvvm for ViewModels
- Views should only contain UI logic
- All business logic should be in ViewModels or Services
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands

### File Structure
- **Views**: `PageName.xaml` + `PageName.xaml.cs`
- **ViewModels**: `PageNameViewModel.cs`
- **Services**: `ServiceName.cs` in appropriate folder

### Global Usings and Imports
- Avoid redundant using statements (use GlobalUsings.cs)
- Prefer global access patterns where available
- Access common utilities directly without namespace prefix

## Patterns and Best Practices

### ViewModels
- Inherit from `ObservableObject` from CommunityToolkit.Mvvm
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands
- Follow async/await pattern for long-running operations

### Resource Management
- Use resource files in `Themes/`
- Prefer `StaticResource` over `DynamicResource` when possible
- Organize styles by functionality

### Configuration and Logging
- Use `AppConfig.cs` for configuration
- Use `LoggerSetup.cs` for logging configuration
- Refer to documentation in `docs/` for details

### Service Architecture (Specialized Services with Dependency Injection)
- **Follow Single Responsibility Principle (SRP)**: Each service should have one clear responsibility
- **Use Dependency Injection**: Services should be registered in `App.xaml.cs` and injected via constructor
- **Create Interfaces**: All services should implement interfaces for testability and loose coupling
- **Organize Services**: Place specialized services in appropriate subfolders (e.g., `Services/WindowsImage/`)
- **Service Examples**:
  - `IWindowsImageFileService`: File operations and path management
  - `IWindowsImageMetadataService`: JSON metadata persistence and management
  - `IWindowsImagePowerShellService`: PowerShell execution and parsing
  - `IWindowsImageIsoService`: ISO mounting and extraction operations
- **Service Registration Pattern**:
  ```csharp
  // In App.xaml.cs ConfigureServices()
  services.AddSingleton<ISpecializedService, SpecializedService>();
  services.AddTransient<CoordinatorService>(); // Uses injected specialized services
  ```
- **Service Consumption Pattern**:
  ```csharp
  public class CoordinatorService
  {
      private readonly ISpecializedService _specializedService;

      public CoordinatorService(ISpecializedService specializedService)
      {
          _specializedService = specializedService ?? throw new ArgumentNullException(nameof(specializedService));
      }
  }
  ```

## Project Specifics

### Navigation
- Navigation system is documented in `docs/Navigation-Bar-Management.md`
- Use T4 templates to generate navigation mappings

### Themes and Styling
- Support for dark/light themes
- Use Fluent Design System
- Assets organized by category in `Assets/`

### Configuration
- `AppConfig.cs`: Global application configuration
- `Constants.cs`: Shared constants
- `AppHelper.cs`: Utility methods

## Business Domain

### Core Functionality
Bucket specializes in Windows image management with the following key features:

- **Image Provisioning**: Manage and provision updates in Windows images
- **Automatic Updates**: Download updates automatically based on WIM file specifications
- **WIM Management**: Handle Windows Imaging Format files for deployment scenarios
- **Update Integration**: Seamlessly integrate Windows updates into existing images

### Technical Context
- Work with Windows Imaging APIs and tools
- Handle large file operations (WIM files, update packages)
- Manage Windows Update services and catalogs
- Support offline and online image servicing
- Integration with Windows deployment tools (PowerShell, etc.)

### Data Models Expected
- Image metadata and specifications
- Update packages and dependencies
- Download progress and status tracking
- Configuration profiles for different deployment scenarios

## Instructions for Copilot

When generating code for this project:

1. **Follow MVVM architecture**: Clearly separate UI logic from business logic
2. **Use CommunityToolkit.Mvvm**: Prefer `[ObservableProperty]` and `[RelayCommand]` attributes
3. **Follow naming conventions**: Use the established naming patterns in the project
4. **Leverage MCP Servers**: Don't hesitate to use available Model Context Protocol (MCP) servers to enhance development capabilities, gather additional context, or access external resources that could benefit the project
5. **Use Specialized Services with Dependency Injection**:
   - Create focused services following Single Responsibility Principle (SRP)
   - Always create interfaces for services to enable testing and loose coupling
   - Register services in `App.xaml.cs` and inject via constructor parameters
   - Organize specialized services in appropriate subfolders (e.g., `Services/WindowsImage/`)
   - Avoid monolithic services - split complex services into specialized components
5. **Integrate with existing structure**: Use appropriate folders
6. **Document complex code**: Add XML comments for public APIs
7. **Handle errors**: Include appropriate error handling with logging
8. **Respect existing patterns**: Observe how other classes are structured
9. **Consider async operations**: Most Windows imaging operations are long-running
10. **Handle large files**: WIM files and updates can be several GB in size
11. **Progress reporting**: Always include progress tracking for long operations

12. **Language**: All code and comments must be written in English.
13. **Documentation Awareness**: When analyzing code for context, always check if related documentation already exists in the `docs/` folder and use it to complement your understanding of the code.
14. **Documentation Language**: All generated documentation (Markdown files) must be written in English.

### Service Architecture Guidelines

When creating or refactoring services:
- **Single Responsibility**: Each service should handle one specific domain or concern
- **Interface-Based Design**: Always create interfaces for services to enable dependency injection and testing
- **Constructor Injection**: Use constructor injection for dependencies, validate parameters with `ArgumentNullException`
- **Service Lifetime Management**: Choose appropriate service lifetimes (Singleton, Transient, Scoped)
- **Service Organization**: Place specialized services in domain-specific subfolders
- **Error Handling**: Each service should handle its domain-specific errors appropriately
- **Logging**: Include comprehensive logging for service operations and errors
- **Documentation**: Document each service with clear responsibilities and usage examples

### Domain-Specific Guidelines

When working with Windows imaging functionality:
- Use async/await patterns for all file and network operations
- Implement proper cancellation tokens for long-running tasks
- Include progress reporting with `IProgress<T>` interface
- Handle Windows-specific exceptions and error codes
- Consider memory usage when working with large WIM files
- Implement proper logging for troubleshooting deployment issues

### Windows Image Services Guidelines
When working with Windows image services, follow the established specialized service pattern:
- **IWindowsImageFileService**: Use for file operations, copying, naming, and path management
- **IWindowsImageMetadataService**: Use for JSON persistence and metadata CRUD operations
- **IWindowsImagePowerShellService**: Use for PowerShell execution and Windows image analysis
- **IWindowsImageIsoService**: Use for ISO mounting, extraction, and related operations
- **WindowsImageService**: Main coordinator service that orchestrates the specialized services
- Always inject required services via constructor and validate dependencies
- Prefer delegation to specialized services over implementing operations directly in coordinator services

### Security & Permissions Guidelines
- Handle administrator permissions requirements
- Validate file paths to prevent directory traversal
- Check disk space before large operations

### Error Handling Standards
- Handle Windows-specific errors (access denied, disk space, etc.)
- Implement proper retry logic for network operations
- Graceful degradation when operations fail

## Logging Standards

### Logger Usage
- Use `LoggerSetup.Logger` for all logging operations
- Access globally via `Logger` (thanks to global using statement)
- Configure logging once at application startup using `LoggerSetup.ConfigureLogger()`

### Log Levels and Usage
- **Logger.Verbose()**: Extremely detailed trace information for debugging
- **Logger.Debug()**: Detailed information for diagnosing problems during development
- **Logger.Information()**: General application flow and important events
- **Logger.Warning()**: Potentially harmful situations that don't stop execution
- **Logger.Error()**: Error events that allow application to continue running
- **Logger.Fatal()**: Critical errors that may cause application termination

### Structured Logging Best Practices
- Use named parameters for structured logging: `Logger.Information("User {UserId} started operation {Operation}", userId, operationName)`
- Include relevant context in log messages
- Log method entry/exit for critical operations
- Always log exceptions with full context: `Logger.Error(ex, "Operation failed for {Context}", context)`

### Required Logging Points
- Application startup and shutdown
- Configuration loading and changes
- File operations (especially large files)
- Network operations and downloads
- Error conditions and recovery actions
- Performance-critical operations with timing information
- User-initiated actions in ViewModels

## Documentation Standards

### Automatic Documentation Generation

For every class created or significantly modified, generate corresponding documentation in the `docs/` folder following this structure:

#### File Naming Convention
- **File Name**: `{ClassName}.md` (exact match with class name)
- **Location**: Mirror the source code structure in `docs/`
- **Examples**:
  - `src/Common/AppConfig.cs` → `docs/Common/AppConfig.md`
  - `src/ViewModels/MainViewModel.cs` → `docs/ViewModels/MainViewModel.md`
  - `src/Views/Settings/SettingsPage.xaml.cs` → `docs/Views/Settings/SettingsPage.md`

#### Mandatory Documentation Structure

```markdown
# [ClassName] Class Documentation

## Overview
Brief description of the class purpose and functionality.

## Location
- **File**: `src/[folder]/[ClassName].cs`
- **Namespace**: `Bucket.[Namespace]`

## Class Definition
```csharp
[access modifier] [partial] class [ClassName] [: base class, interfaces]
```

## [Context-Specific Sections]
Choose relevant sections based on class type:
- **Properties** (for data classes, ViewModels)
- **Methods** (for service classes, utilities)
- **Constants** (for constant classes)
- **Commands** (for ViewModels)
- **Events** (if applicable)

## Usage Examples
Practical code examples showing how to use the class.

## Features
Key features and capabilities of the class.

## Dependencies
External dependencies and related classes.

## Related Files
Links to related documentation using relative paths:
- [`RelatedClass.md`](./RelatedClass.md)

## Best Practices
Guidelines for using the class effectively.

## Error Handling
Common error scenarios and handling approaches.

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
```

#### Section Guidelines

1. **Always Include**: Overview, Location, Class Definition, Usage Examples, Dependencies, Related Files
2. **Include When Relevant**: Features, Best Practices, Error Handling, Security Considerations, Performance Notes
3. **Use Consistent Formatting**:
   - Code blocks with proper syntax highlighting
   - Property/method signatures in code format
   - Clear section headers
   - Descriptive examples

#### Documentation Quality Standards

- **Comprehensive**: Cover all public members and key functionality
- **Practical Examples**: Include real-world usage scenarios
- **Cross-References**: Link to related documentation files
- **Code Samples**: Provide working code examples
- **Consistent Style**: Follow the established template structure

## Code Editing Standards

### File Editing Best Practices

When using code editing tools (insert_edit_into_file, replace_string_in_file):

- **Proper Line Breaks**: Always ensure proper line breaks between existing code and new code
- **No Inline Additions**: Never add new code with tabs or spaces directly after existing code on the same line
- **Clean Indentation**: Each new line of code should start on its own line with proper indentation
- **Preserve Formatting**: Maintain consistent indentation and formatting with the existing codebase

**Example of INCORRECT formatting**:
```csharp
existing code line		new code line  // WRONG: tabs/spaces on same line
```

**Example of CORRECT formatting**:
```csharp
existing code line
new code line  // CORRECT: proper line break and indentation
```

## Documentation

Refer to files in `docs/` for more details:
- `AppConfig.md`: Application configuration
- `AppHelper.md`: Available utilities
- `Constants.md`: Project constants
- `LoggerSetup.md`: Logging configuration
- `Logging-System.md`: Logging system
- `Navigation-Bar-Management.md`: Navigation management