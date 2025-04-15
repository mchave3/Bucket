# Bucket Page Creation Guide

This document describes the steps and best practices for creating new pages in the Bucket application using the provided templates.

## Page Structure

Each page in Bucket consists of three main files:

1. **XAML** - Defines the user interface of the page (`PageName.xaml`)
2. **Initialization** - Initializes data and event handlers (`Initialize-PageNamePage.ps1`)
3. **Invocation** - Handles navigation to the page (`Invoke-PageNamePage.ps1`)

## Available Templates

The following templates are available in this directory:

- `TemplatePage.xaml` - XAML template for the user interface
- `Initialize-TemplatePage.ps1` - Template for the initialization script
- `Invoke-TemplatePage.ps1` - Template for the invocation script

## Steps to Create a New Page

### 1. Create Base Files

1. Copy the templates to the appropriate locations:
   - `TemplatePage.xaml` to `e:\Bucket\src\Bucket\GUI\<PageName>Page.xaml`
   - `Initialize-TemplatePage.ps1` to `e:\Bucket\src\Bucket\Private\GUI\Pages\Initialize-<PageName>Page.ps1`
   - `Invoke-TemplatePage.ps1` to `e:\Bucket\src\Bucket\Private\GUI\Pages\Invoke-<PageName>Page.ps1`

2. In each file, replace the template variables with your specific values:
   - `${PageName}` with your page name (e.g., "UserSettings")
   - `${PageTitle}` with the title displayed in the header (e.g., "User Settings")
   - `${pageNameLower}` with the lowercase page name (e.g., "userSettings")

### 2. Customize XAML

1. Modify the grid structure according to your needs.
2. Add the necessary controls to your page.
3. **Important**: Name all controls with the `PageName_` prefix followed by the control name and type (e.g., `UserSettings_SaveButton`).

### 3. Customize Initialization

1. Add script variables needed to store data (with the `$script:pageNameLower_` prefix).
2. Create event handlers for all interactive controls.
3. Define DataContext properties needed for XAML bindings.

### 4. Add the Page to the Navigation System

1. Add your page to the export table in `src\Bucket\Private\GUI\Services\Invoke-BucketNavigationService.ps1`:

```powershell
$pageList = @{
    "HomePage" = [uri]"/Bucket;component/GUI/HomePage.xaml"
    "AboutPage" = [uri]"/Bucket;component/GUI/AboutPage.xaml"
    "SelectImagePage" = [uri]"/Bucket;component/GUI/SelectImagePage.xaml"
    "YourPageName" = [uri]"/Bucket;component/GUI/YourPageNamePage.xaml"  # Add your page here
}
```

## Naming Conventions

Always follow these naming conventions to maintain consistency in the application:

### XAML
- Page name: `PageNamePage.xaml`
- Controls: `x:Name="PageName_ControlTypeDescriptor"` (e.g., `UserSettings_SaveButton`)

### PowerShell
- Functions: `Initialize-PageNamePage`, `Invoke-PageNamePage`
- Script variables: `$script:pageNameLower_variableName`
- Local variables: `$descriptiveVariableName`

### Styles and Behaviors
- Use styles consistent with the rest of the application
- Respect the existing color palette
- Use rounded corners for buttons (`CornerRadius="4"`)

## Recommended Structure for PowerShell Code

Follow this structure for initialization files:

1. **Data Initialization** - Initialize all necessary data
2. **Event Handlers** - Define all event handlers
3. **Page Navigation** - Handle navigation and create data context

Use region comments to organize your code:

```powershell
#region Data Initialization
# ...
#endregion Data Initialization

#region Event Handlers
# ...
#endregion Event Handlers

#region Page Navigation
# ...
#endregion Page Navigation
```

## Best Practices

1. **Logging** - Use `Write-BucketLog` for important actions
2. **Error Handling** - Surround critical code with try/catch blocks
3. **Script Variables** - Prefix with `$script:pageNameLower_` to avoid conflicts
4. **Comments** - Clearly document the intent of the code
5. **Separation of Concerns** - Keep user interface (XAML) separate from logic (PS1)

## Concrete Examples

See existing pages for concrete examples:
- `HomePage` - Simple home page with cards and statistics
- `AboutPage` - Informational page with links and application information
- `SelectImagePage` - Complex page with DataGrids and interactive logic

## Troubleshooting Common Issues

1. **Events Not Triggering** - Check control names and ensure they exactly match the XAML
2. **Bindings Not Working** - Verify properties exist in the DataContext
3. **Page Not Displaying** - Check that the page is correctly added to the NavigationService

For any other questions, refer to the WPF documentation or contact the Bucket development team.
