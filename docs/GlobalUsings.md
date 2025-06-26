# GlobalUsings.cs Documentation

## Overview

Global using directives file that defines commonly used namespaces throughout the Bucket application. This file eliminates the need to repeatedly specify `using` statements in individual source files, improving code readability and reducing boilerplate code.

## Location

- **File**: `src/GlobalUsings.cs`
- **Namespace**: Global scope (no specific namespace)

## Purpose

This file uses C# 10's global using feature to automatically import frequently used namespaces across all source files in the project. This reduces code repetition and ensures consistent namespace availability throughout the application.

## Global Using Statements

### Bucket Application Namespaces
```csharp
global using Bucket.Common;
global using Bucket.Services;
global using Bucket.Services.WindowsImage;
global using Bucket.ViewModels;
global using Bucket.Views;
```
**Purpose**: Provides access to all core application namespaces including common utilities, services, ViewModels, and Views.

### MVVM Framework
```csharp
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
```
**Purpose**: Enables MVVM pattern implementation with ObservableObject, ObservableProperty, and RelayCommand attributes.

### UI Framework
```csharp
global using Microsoft.UI;
global using Microsoft.UI.Xaml;
global using Microsoft.UI.Xaml.Controls;
global using Microsoft.UI.Xaml.Navigation;
```
**Purpose**: Provides access to WinUI 3 framework components including controls, navigation, and XAML infrastructure.

### Dependency Injection
```csharp
global using Microsoft.Extensions.DependencyInjection;
```
**Purpose**: Enables dependency injection container functionality throughout the application.

### Third-Party Libraries
```csharp
global using DevWinUI;
```
**Purpose**: Provides access to DevWinUI library components and controls.

### Static Helper Imports
```csharp
global using static Bucket.Common.AppHelper;
global using static Bucket.Common.LoggerSetup;
```
**Purpose**: Imports static members from helper classes, allowing direct use of helper methods and logger functionality without qualification.

## Benefits

### Code Simplification
- **Reduced Boilerplate**: Eliminates repetitive using statements
- **Cleaner Files**: Source files are more focused on actual functionality
- **Consistency**: Ensures uniform namespace availability across the project

### Maintenance
- **Centralized Management**: Single location to manage common imports
- **Easy Updates**: Changes to common namespaces require updates in only one file
- **Dependency Tracking**: Clear view of project's external dependencies

### Developer Experience
- **IntelliSense**: Full access to imported types without explicit using statements
- **Code Completion**: Enhanced development experience with readily available APIs
- **Reduced Errors**: Less chance of missing using statements

## Usage Examples

With global usings in place, source files can directly use imported types:

```csharp
// Without global usings, this would require multiple using statements
public partial class ExampleViewModel : ObservableObject
{
    [ObservableProperty]
    private string title;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        Logger.Information("Loading data...");
        // Direct access to helper methods
        var config = GetAppConfig();
    }
}
```

### Page Implementation
```csharp
// Clean page implementation without using statements
public sealed partial class ExamplePage : Page
{
    public ExampleViewModel ViewModel { get; }

    public ExamplePage()
    {
        ViewModel = App.GetService<ExampleViewModel>();
        this.InitializeComponent();
    }
}
```

## Best Practices

### Inclusion Criteria
Only include namespaces that are:
- Used in a significant number of files (>50% of source files)
- Core to the application architecture
- Unlikely to change frequently

### Avoid Including
- Specialized namespaces used in few files
- Conflicting namespace names
- Experimental or unstable APIs

## Impact on Project

### Positive Effects
- **Cleaner Codebase**: Reduced visual clutter in source files
- **Faster Development**: Less time spent on using statement management
- **Consistency**: Uniform access to core functionality

### Considerations
- **Implicit Dependencies**: Makes namespace dependencies less obvious
- **IDE Features**: Some development tools may require configuration adjustments
- **Learning Curve**: New developers need to understand global using concept

## Related Files

- All C# source files in the project benefit from these global usings
- [`AppHelper.cs`](./Common/AppHelper.md) - Static methods imported globally
- [`LoggerSetup.cs`](./Common/LoggerSetup.md) - Logger functionality imported globally

## Maintenance Guidelines

- Review periodically to ensure all included namespaces are still widely used
- Remove namespaces that become unused across the project
- Add new namespaces when they become widely adopted (>50% usage)
- Monitor for namespace conflicts when adding new global usings

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
