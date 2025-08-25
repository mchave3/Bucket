# Navigation Bar Management in Bucket Application

## Overview

The Bucket application implements a sophisticated navigation system using WinUI 3 and the DevWinUI library. The navigation architecture is built around a custom `JsonNavigationService` that provides a flexible, JSON-driven approach to managing navigation and breadcrumb functionality.

## Architecture Components

### 1. Core Navigation Components

#### Main Window Structure

The main window (`MainWindow.xaml`) contains the primary navigation structure:

```xml
<NavigationView x:Name="NavView"
                Grid.Row="1"
                IsBackButtonVisible="Collapsed"
                IsPaneToggleButtonVisible="False">
    <NavigationView.Header>
        <dev:BreadcrumbNavigator x:Name="BreadCrumbNav" />
    </NavigationView.Header>
    <Frame x:Name="NavFrame" />
</NavigationView>
```

#### Key Elements

- **NavigationView**: Primary navigation container
- **BreadcrumbNavigator**: Header navigation for showing current location
- **Frame**: Content frame for page rendering

### 2. Navigation Service (JsonNavigationService)

The application uses `IJsonNavigationService` which is implemented as `JsonNavigationService`. This service is registered as a singleton in the dependency injection container.

#### Service Registration

```csharp
// In App.xaml.cs - ConfigureServices()
services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
```

#### Service Initialization

The navigation service is initialized in `MainWindow.xaml.cs` constructor:

```csharp
var navService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
if (navService != null)
{
    navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
        .ConfigureDefaultPage(typeof(HomeLandingPage))
        .ConfigureSettingsPage(typeof(SettingsPage))
        .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
        .ConfigureTitleBar(AppTitleBar)
        .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
}
```

### 3. Page Mapping System

#### Navigation Page Mappings

The application uses auto-generated page mappings through T4 templates:

**File**: `T4Templates/NavigationPageMappings.cs` (auto-generated)

```csharp
public partial class NavigationPageMappings
{
    public static Dictionary<string, Type> PageDictionary { get; } = new Dictionary<string, Type>
    {
        {"Bucket.Views.HomeLandingPage", typeof(Bucket.Views.HomeLandingPage)},
    };
}
```

#### Breadcrumb Page Mappings

Configuration for breadcrumb behavior:

**File**: `T4Templates/BreadcrumbPageMappings.cs` (auto-generated)

```csharp
public partial class BreadcrumbPageMappings
{
    public static Dictionary<Type, BreadcrumbPageConfig> PageDictionary = new()
    {
        {typeof(Bucket.Views.SettingsPage), new BreadcrumbPageConfig { PageTitle = null, IsHeaderVisible = true, ClearNavigation = false}},
        {typeof(Bucket.Views.AboutUsSettingPage), new BreadcrumbPageConfig { PageTitle = null, IsHeaderVisible = true, ClearNavigation = false}},
        // ... other page configurations
    };
}
```

### 4. JSON-Driven Navigation Configuration

#### Navigation Menu Data

**File**: `Assets/NavViewMenu/AppData.json`

```json
{
  "$schema": "https://raw.githubusercontent.com/ghost1372/DevWinUI/refs/heads/main/tools/AppData.Schema.json",
  "Groups": [
    {
      "UniqueId": "Bucket",
      "Title": "Bucket",
      "ImagePath": "ms-appx:///Assets/AppIcon.png",
      "IsSpecialSection": false,
      "ShowItemsWithoutGroup": true,
      "IsExpanded": false,
      "Items": [
        {
          "UniqueId": "Bucket.Views.HomeLandingPage",
          "Title": "Bucket",
          "Subtitle": "Bucket",
          "ImagePath": "ms-appx:///Assets/AppIcon.png",
          "HideItem": true
        }
      ]
    }
  ]
}
```

## Navigation Implementation Patterns

### 1. Command-Based Navigation

The application uses command-based navigation through the DevWinUI framework:

```xml
<!-- In SettingsPage.xaml -->
<dev:SettingsCard Description="Change your app Settings"
                  Header="General"
                  HeaderIcon="{dev:BitmapIcon Source=Assets/Fluent/General.png}"
                  IsClickEnabled="True"
                  Command="{x:Bind local:App.Current.NavService.NavigateToCommand}"
                  CommandParameter="{dev:NavigationParameter PageType=views:GeneralSettingPage, BreadCrumbHeader='General'}" />
```

### 2. Service Access Pattern

Navigation service is accessed through the application singleton:

```csharp
// In App.xaml.cs
public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
```

### 3. Page Structure

Each page follows a consistent structure:

```csharp
// Example: GeneralSettingPage.xaml.cs
public sealed partial class GeneralSettingPage : Page
{
    public GeneralSettingViewModel ViewModel { get; }

    public GeneralSettingPage()
    {
        ViewModel = App.GetService<GeneralSettingViewModel>();
        this.InitializeComponent();
    }
}
```

## Features

### 1. Breadcrumb Navigation

- Automatic breadcrumb generation based on navigation path
- Configurable visibility and behavior per page
- Integration with page titles and headers

### 2. Auto-Suggest Search

- Title bar integration with search functionality
- Connected to navigation frame for search-based navigation

```csharp
// In MainWindow.xaml.cs
private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
{
    AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
}

private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
{
    AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
}
```

### 3. Dynamic Page Generation

- T4 templates automatically generate page mappings
- JSON schema validation for navigation configuration
- Support for both visible and hidden navigation items

### 4. Theme Integration

- Theme toggle button in title bar
- Theme service integration with navigation

```csharp
private void ThemeButton_Click(object sender, RoutedEventArgs e)
{
    ThemeService.ChangeThemeWithoutSave(App.MainWindow);
}
```

## Configuration Options

### Title Bar Configuration

- Extended content into title bar
- Custom height preferences
- Back button and pane toggle button control

```csharp
ExtendsContentIntoTitleBar = true;
SetTitleBar(AppTitleBar);
AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
```

### Window Constraints

```csharp
((OverlappedPresenter)AppWindow.Presenter).PreferredMinimumWidth = 800;
((OverlappedPresenter)AppWindow.Presenter).PreferredMinimumHeight = 600;
```

## Dependencies

### External Libraries

- **DevWinUI** (v8.3.0): Core UI framework
- **DevWinUI.Controls** (v8.3.0): Extended controls
- **DevWinUI.ContextMenu** (v8.3.0): Context menu support

### Internal Services

- `IJsonNavigationService`: Core navigation functionality
- `IThemeService`: Theme management
- `MainViewModel`: Main window view model
- Various setting view models for different pages

## Legacy Navigation System

The project also contains a legacy PowerShell-based navigation system located in `src.powershell.old/`, which includes:

- `Invoke-BucketNavigationService.ps1`: Generic navigation service
- `Invoke-BucketPage.ps1`: Page invocation utilities
- XAML-based page loading with fallback mechanisms

This legacy system demonstrates the evolution from PowerShell-based WPF navigation to the current WinUI 3 implementation.

## Best Practices

1. **Service Registration**: Always register navigation services as singletons
2. **Page Mappings**: Use T4 templates for automatic generation of page mappings
3. **Breadcrumb Configuration**: Configure breadcrumb behavior per page type
4. **Command Pattern**: Use command-based navigation for consistency
5. **JSON Configuration**: Leverage JSON files for flexible navigation menu configuration
6. **DevWinUI Integration**: Utilize DevWinUI controls for consistent UI experience

## Future Considerations

- The navigation system is built to be extensible through JSON configuration
- T4 templates allow for automated page discovery and mapping
- The service-based architecture supports easy testing and maintenance
- Schema validation ensures navigation configuration integrity

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
