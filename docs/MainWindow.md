# MainWindow Class Documentation

## Overview

The main application window for Bucket, serving as the primary UI container and navigation host. This window manages the overall application layout, title bar customization, navigation framework, and provides the foundation for all user interactions within the application.

## Location

- **File**: `src/MainWindow.xaml.cs`
- **Namespace**: `Bucket.Views`

## Class Definition

```csharp
public sealed partial class MainWindow : Window
```

## Properties

### ViewModel
```csharp
public MainViewModel ViewModel { get; }
```
Gets the main view model associated with this window, providing data and commands for the main window's functionality.

## Methods

### Constructor
```csharp
public MainWindow()
```
Initializes the main window, configures the navigation system, sets up title bar customization, and establishes minimum window dimensions.

## Usage Examples

### Window Initialization
```csharp
// Window is typically created by the App class
var mainWindow = new MainWindow();
mainWindow.Activate();
```

### Accessing ViewModel
```csharp
// Access main window's ViewModel
var viewModel = mainWindow.ViewModel;
```

## Features

### Navigation Framework
- **JSON-based Navigation**: Uses configuration file for navigation structure
- **Default Page**: Configures HomeLandingPage as the startup page
- **Settings Integration**: Automatic settings page configuration
- **Breadcrumb Navigation**: Provides hierarchical navigation breadcrumbs

### Window Customization
- **Extended Title Bar**: Content extends into the title bar area
- **Custom Title Bar**: Custom title bar element for modern appearance
- **Minimum Dimensions**: Enforces minimum window size (800x600)
- **Tall Title Bar**: Uses preferred tall title bar for better usability

### UI Components
- **Navigation View**: Main navigation container (`NavView`)
- **Content Frame**: Primary content display area (`NavFrame`)
- **Title Bar**: Custom title bar element (`AppTitleBar`)
- **Breadcrumb Bar**: Navigation breadcrumb display (`BreadCrumbNav`)

## Dependencies

### External Dependencies
- **Microsoft.UI.Windowing**: Window management and customization
- **DevWinUI.Controls**: Navigation and UI controls

### Internal Dependencies
- [`MainViewModel`](../ViewModels/MainViewModel.md): Main window view model
- [`HomeLandingPage`](./HomeLandingPage.md): Default startup page
- [`SettingsPage`](./SettingsPage.md): Settings page
- **NavigationPageMappings**: Generated T4 template for page mappings
- **BreadcrumbPageMappings**: Generated T4 template for breadcrumb mappings

## Related Files

- [`App.md`](../App.md): Application entry point and service configuration
- [`MainViewModel.md`](../ViewModels/MainViewModel.md): Main window view model
- [`Navigation-Bar-Management.md`](../Navigation-Bar-Management.md): Navigation system documentation
- **AppData.json**: Navigation configuration file

## Best Practices

### Window Configuration
- Set appropriate minimum window dimensions for usability
- Configure title bar properly for modern Windows applications
- Use proper presenter configuration for window behavior

### Navigation Setup
- Initialize navigation service with proper configuration
- Configure default and settings pages appropriately
- Use JSON configuration for flexible navigation structure

### Service Integration
- Retrieve services through dependency injection
- Handle service initialization errors gracefully
- Maintain proper service lifecycle

## Error Handling

### Common Error Scenarios
1. **Navigation Service Initialization**: Service not available or misconfigured
2. **Page Registration**: Pages not properly registered in navigation mappings
3. **Window Configuration**: Issues with window presenter or title bar setup

### Error Recovery
- Check service availability before initialization
- Provide fallback navigation if JSON configuration fails
- Handle window configuration errors gracefully

## Security Considerations

- **Service Access**: Secure access to application services
- **Navigation Security**: Controlled navigation between pages
- **Window Integrity**: Proper window state management

## Performance Notes

### Initialization Optimization
- Lazy loading of navigation configuration
- Efficient service resolution
- Minimal window startup overhead

### Memory Management
- Proper disposal of navigation resources
- Efficient ViewModel lifecycle management
- Resource cleanup on window close

### UI Performance
- Hardware-accelerated rendering
- Efficient navigation transitions
- Optimized title bar rendering

## Navigation Configuration

### JSON Structure
The navigation is configured through `Assets/NavViewMenu/AppData.json`:
```json
{
  "Groups": [
    {
      "UniqueId": "Bucket",
      "Title": "Bucket",
      "Items": [
        {
          "UniqueId": "Bucket.Views.ImageManagementPage",
          "Title": "Image Management"
        }
      ]
    }
  ]
}
```

### Page Mappings
Navigation uses T4-generated mappings for type-safe page resolution:
- **NavigationPageMappings**: Maps page identifiers to types
- **BreadcrumbPageMappings**: Maps breadcrumb navigation structure

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
