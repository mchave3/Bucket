# Navigation Menu Management - Bucket Application

## 📋 Overview

The Bucket application uses an MVVM architecture with dependency injection to manage the navigation menu. The system is based on the WinUI 3 `NavigationView` control and several coordinated services that ensure smooth and maintainable navigation.

## 🏗️ General Architecture

- The navigation menu is built using a `NavigationView` control in XAML.
- The `ShellViewModel` connects the UI to the navigation logic and keeps track of the current state.
- Navigation logic is handled by services (`NavigationService`, `NavigationViewService`, `PageService`).
- The flow is: User interacts with the NavigationView → ShellViewModel updates → Services handle navigation and page changes.

### Main Components

1. **NavigationView** - Menu user interface
2. **ShellViewModel** - Coordination and data binding
3. **NavigationService** - Logic for page navigation
4. **NavigationViewService** - Handles interactions with the NavigationView
5. **PageService** - Registry of ViewModel ↔ Page associations
6. **NavigationHelper** - Helper for attached properties
7. **NavigationViewHeaderBehavior** - Dynamic header management


## 🔧 Component Details

### 1. NavigationView (ShellPage.xaml)

The main control located in `Views/ShellPage.xaml`:

```xaml
<NavigationView x:Name="NavigationViewControl"
                IsBackButtonVisible="Visible"
                IsBackEnabled="{x:Bind ViewModel.IsBackEnabled, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.Selected, Mode=OneWay}"
                IsSettingsVisible="True">
    <NavigationView.MenuItems>
        <NavigationViewItem x:Uid="Shell_Home"
                           helpers:NavigationHelper.NavigateTo="Bucket.ViewModels.HomeViewModel">
            <NavigationViewItem.Icon>
                <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" Glyph="&#xe7c3;"/>
            </NavigationViewItem.Icon>
        </NavigationViewItem>
        <NavigationViewItem x:Uid="Shell_WindowsImage"
                           helpers:NavigationHelper.NavigateTo="Bucket.ViewModels.WindowsImageViewModel">
            <NavigationViewItem.Icon>
                <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" Glyph="&#xe7c3;"/>
            </NavigationViewItem.Icon>
        </NavigationViewItem>
    </NavigationView.MenuItems>
    <Grid Margin="{StaticResource NavigationViewPageContentMargin}">
        <Frame x:Name="NavigationFrame" />
    </Grid>
</NavigationView>
```


**Key properties:**

- `IsBackEnabled`: Bound to the ViewModel to enable/disable the back button
- `SelectedItem`: Synchronized with the selected item
- `NavigationHelper.NavigateTo`: Attached property linking each item to a ViewModel


### 2. ShellViewModel (ViewModels/ShellViewModel.cs)

Coordinates all navigation services:

```csharp
public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
```


### 3. NavigationService (Services/NavigationService.cs)

Handles actual navigation between pages:

```csharp
public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private object? _lastParameterUsed;
    private Frame? _frame;

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        if (_frame != null && (_frame.Content?.GetType() != pageType ||
            (parameter != null && !parameter.Equals(_lastParameterUsed))))
        {
            _frame.Tag = clearNavigation;
            var vmBeforeNavigation = _frame.GetPageViewModel();
            var navigated = _frame.Navigate(pageType, parameter);

            if (navigated)
            {
                _lastParameterUsed = parameter;
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }

        return false;
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            var vmBeforeNavigation = _frame.GetPageViewModel();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
            return true;
        }
        return false;
    }
}
```


**Features:**

- Navigation by page key (string)
- Support for navigation parameters
- Navigation history management
- `INavigationAware` interface to notify ViewModels


### 4. NavigationViewService (Services/NavigationViewService.cs)

Handles interactions with the NavigationView:

```csharp
public class NavigationViewService : INavigationViewService
{
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private NavigationView? _navigationView;

    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.BackRequested += OnBackRequested;
        _navigationView.ItemInvoked += OnItemInvoked;
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;
            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                _navigationService.NavigateTo(pageKey);
            }
        }
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        => _navigationService.GoBack();
}
```


### 5. PageService (Services/PageService.cs)

Registry of ViewModel ↔ Page associations:

```csharp
public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<HomeViewModel, HomePage>();
        Configure<WindowsImageViewModel, WindowsImagePage>();
        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}");
            }
        }
        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            _pages.Add(key, typeof(V));
        }
    }
}
```


### 6. NavigationHelper (Helpers/NavigationHelper.cs)

Attached property to link NavigationViewItems to ViewModels:

```csharp
public class NavigationHelper
{
    public static string GetNavigateTo(NavigationViewItem item)
        => (string)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(NavigationViewItem item, string value)
        => item.SetValue(NavigateToProperty, value);

    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(string),
            typeof(NavigationHelper), new PropertyMetadata(null));
}
```


**Usage in XAML:**

```xaml
<NavigationViewItem helpers:NavigationHelper.NavigateTo="Bucket.ViewModels.HomeViewModel">
```


### 7. NavigationViewHeaderBehavior (Behaviors/NavigationViewHeaderBehavior.cs)

Dynamically manages NavigationView headers:

```csharp
public class NavigationViewHeaderBehavior : Behavior<NavigationView>
{
    private Page? _currentPage;

    protected override void OnAttached()
    {
        var navigationService = App.GetService<INavigationService>();
        navigationService.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame && frame.Content is Page page)
        {
            _currentPage = page;
            UpdateHeader();
            UpdateHeaderTemplate();
        }
    }

    private void UpdateHeader()
    {
        if (_currentPage != null)
        {
            var headerMode = GetHeaderMode(_currentPage);
            if (headerMode == NavigationViewHeaderMode.Never)
            {
                AssociatedObject.Header = null;
                AssociatedObject.AlwaysShowHeader = false;
            }
            else
            {
                var headerFromPage = GetHeaderContext(_currentPage);
                AssociatedObject.Header = headerFromPage ?? DefaultHeader;
                AssociatedObject.AlwaysShowHeader = headerMode == NavigationViewHeaderMode.Always;
            }
        }
    }
}
```


**Available header modes:**

- `Always`: Header always visible
- `Never`: Header never visible
- `Minimal`: Header visible depending on context


## 🔄 Navigation Flow

### Scenario: Clicking a menu item

```text
1. User clicks a NavigationViewItem
   ↓
2. NavigationViewService.OnItemInvoked() is triggered
   ↓
3. The NavigateTo property is retrieved via NavigationHelper
   ↓
4. NavigationService.NavigateTo(pageKey) is called
   ↓
5. PageService.GetPageType(pageKey) resolves the page type
   ↓
6. Frame.Navigate() navigates to the page
   ↓
7. NavigationService.OnNavigated() notifies INavigationAware ViewModels
   ↓
8. ShellViewModel.OnNavigated() updates IsBackEnabled and Selected
   ↓
9. UI is updated automatically via data binding
```

### INavigationAware Interface

ViewModels can implement this interface to be notified of navigation events:

```csharp
public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
    void OnNavigatedFrom();
}
```


## ➕ Adding a New Menu Item

### Step-by-step

1. **Create the ViewModel and Page**

   ```csharp
   // ViewModels/MyNewViewModel.cs
   public partial class MyNewViewModel : ObservableRecipient
   {
       // ViewModel logic
   }

   // Views/MyNewPage.xaml.cs
   public sealed partial class MyNewPage : Page
   {
       public MyNewViewModel ViewModel { get; }

       public MyNewPage(MyNewViewModel viewModel)
       {
           ViewModel = viewModel;
           InitializeComponent();
       }
   }
   ```

2. **Register in PageService**

   ```csharp
   // Services/PageService.cs - in the constructor
   Configure<MyNewViewModel, MyNewPage>();
   ```

3. **Register in dependency injection**

   ```csharp
   // App.xaml.cs - in ConfigureServices()
   services.AddTransient<MyNewViewModel>();
   services.AddTransient<MyNewPage>();
   ```

4. **Add to ShellPage.xaml**

   ```xaml
   <NavigationViewItem x:Uid="Shell_MyItem"
                      helpers:NavigationHelper.NavigateTo="Bucket.ViewModels.MyNewViewModel">
       <NavigationViewItem.Icon>
           <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" Glyph="&#xe7c3;"/>
       </NavigationViewItem.Icon>
   </NavigationViewItem>
   ```

5. **Add localization resources**

   ```xml
   <!-- Strings/en-us/Resources.resw -->
   <data name="Shell_MyItem.Content" xml:space="preserve">
       <value>My New Item</value>
   </data>
   ```

### Full Example

To add a "Reports" page:

1. **ViewModel:**

   ```csharp
   public partial class ReportsViewModel : ObservableRecipient, INavigationAware
   {
       public void OnNavigatedTo(object parameter) { }
       public void OnNavigatedFrom() { }
   }
   ```

2. **Page:**

   ```xaml
   <Page x:Class="Bucket.Views.ReportsPage">
       <Grid>
           <TextBlock Text="Reports Page" />
       </Grid>
   </Page>
   ```

3. **Configuration:**

   ```csharp
   // PageService
   Configure<ReportsViewModel, ReportsPage>();

   // App.xaml.cs
   services.AddTransient<ReportsViewModel>();
   services.AddTransient<ReportsPage>();
   ```

4. **XAML:**

   ```xaml
   <NavigationViewItem x:Uid="Shell_Reports"
                      helpers:NavigationHelper.NavigateTo="Bucket.ViewModels.ReportsViewModel">
       <NavigationViewItem.Icon>
           <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" Glyph="&#xe9f9;"/>
       </NavigationViewItem.Icon>
   </NavigationViewItem>
   ```

## 🎨 Header Customization

### Per-page configuration

Add attached properties in your XAML pages:

```xaml
<Page x:Class="Bucket.Views.MyPage"
      behaviors:NavigationViewHeaderBehavior.HeaderMode="Always"
      behaviors:NavigationViewHeaderBehavior.HeaderContext="Custom Title">
```

### Custom header template

```xaml
<Page behaviors:NavigationViewHeaderBehavior.HeaderTemplate="{StaticResource MyTemplate}">
```

## 🔧 Configuration and Dependency Injection

All services are configured in `App.xaml.cs`:

```csharp
private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // Services de navigation
    services.AddSingleton<INavigationService, NavigationService>();
    services.AddSingleton<INavigationViewService, NavigationViewService>();
    services.AddSingleton<IPageService, PageService>();

    // ViewModels
    services.AddTransient<ShellViewModel>();
    services.AddTransient<HomeViewModel>();
    services.AddTransient<WindowsImageViewModel>();
    services.AddTransient<SettingsViewModel>();

    // Pages
    services.AddTransient<ShellPage>();
    services.AddTransient<HomePage>();
    services.AddTransient<WindowsImagePage>();
    services.AddTransient<SettingsPage>();
}
```


## 📝 Best Practices

1. **Consistent naming**: Use consistent names for ViewModel/Page (e.g., `HomeViewModel` ↔ `HomePage`)
2. **Localization**: Always use `x:Uid` for NavigationViewItem texts
3. **Icons**: Use Segoe Fluent Icons for consistency
4. **INavigationAware**: Implement this interface when your ViewModels need to react to navigation
5. **Singleton services**: Navigation services should be singletons to maintain state
6. **Thread safety**: PageService uses locks for thread safety


## 🐛 Troubleshooting

### Common errors

1. **"Page not found"**: Check that `Configure<VM, V>()` is called in PageService
2. **Navigation not working**: Check the `NavigateTo` property in XAML
3. **Incorrect header**: Check NavigationViewHeaderBehavior and its attached properties
4. **Incorrect selection**: Check that the ViewModel is properly registered in dependency injection

### Debugging

Add logs in navigation methods to trace the flow:

```csharp
public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
{
    System.Diagnostics.Debug.WriteLine($"Navigating to: {pageKey}");
    // ... reste du code
}
```

---

Documentation generated June 16, 2025 for the Bucket application
