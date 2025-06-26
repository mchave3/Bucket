# AppUpdateSettingViewModel Class Documentation

## Overview

ViewModel for the application update settings page that manages checking for application updates, displaying version information, and handling update notifications. This class provides functionality to check for both stable and pre-release versions from GitHub releases.

## Location

- **File**: `src/ViewModels/Settings/AppUpdateSettingViewModel.cs`
- **Namespace**: `Bucket.ViewModels`

## Class Definition

```csharp
public partial class AppUpdateSettingViewModel : ObservableObject
```

## Properties

### CurrentVersion
```csharp
[ObservableProperty]
public string currentVersion
```
Displays the current application version with prefix formatting.

### LastUpdateCheck
```csharp
[ObservableProperty]
public string lastUpdateCheck
```
Shows the date of the last update check performed by the user.

### IsUpdateAvailable
```csharp
[ObservableProperty]
public bool isUpdateAvailable
```
Indicates whether a new version (stable or pre-release) is available for download.

### IsLoading
```csharp
[ObservableProperty]
public bool isLoading
```
Controls the loading state during update check operations.

### IsCheckButtonEnabled
```csharp
[ObservableProperty]
public bool isCheckButtonEnabled = true
```
Controls whether the check for updates button is enabled or disabled.

### LoadingStatus
```csharp
[ObservableProperty]
public string loadingStatus = "Status"
```
Displays status messages during update checking operations.

### ChangeLog
```csharp
private string ChangeLog = string.Empty
```
Stores the changelog information for available updates.

## Commands

### CheckForUpdateAsync
```csharp
[RelayCommand]
private async Task CheckForUpdateAsync()
```
Checks for application updates from the GitHub repository, handling both stable and pre-release versions.

**Features:**
- Network connectivity validation
- GitHub API integration for release checking
- Support for both stable and pre-release versions
- Error handling with user feedback
- Updates last check timestamp

### GoToUpdateAsync
```csharp
[RelayCommand]
private async Task GoToUpdateAsync()
```
Opens the GitHub releases page in the default browser for manual update download.

### GetReleaseNotesAsync
```csharp
[RelayCommand]
private async Task GetReleaseNotesAsync()
```
Displays a content dialog with release notes/changelog for the available update.

## Usage Examples

```csharp
// In a settings page code-behind or dependency injection
public sealed partial class AppUpdateSettingPage : Page
{
    public AppUpdateSettingViewModel ViewModel { get; }

    public AppUpdateSettingPage()
    {
        this.InitializeComponent();
        ViewModel = new AppUpdateSettingViewModel();
    }

    // The ViewModel can be used directly in XAML bindings:
    // <TextBlock Text="{x:Bind ViewModel.CurrentVersion}" />
    // <Button Command="{x:Bind ViewModel.CheckForUpdateCommand}" />
}

// Programmatic usage
var updateViewModel = new AppUpdateSettingViewModel();
await updateViewModel.CheckForUpdateCommand.ExecuteAsync(null);

if (updateViewModel.IsUpdateAvailable)
{
    await updateViewModel.GetReleaseNotesCommand.ExecuteAsync(null);
}
```

## Features

- **GitHub Integration**: Automatically checks for updates from GitHub releases
- **Version Management**: Tracks current version and last update check
- **Network Awareness**: Validates network connectivity before update checks
- **Progress Feedback**: Provides loading states and status messages
- **Release Notes**: Displays changelog information in a dialog
- **Dual Version Support**: Handles both stable releases and pre-releases
- **Error Handling**: Comprehensive error handling with user-friendly messages

## Dependencies

- **Windows.System**: For launching external URLs
- **Microsoft.UI.Xaml.Controls**: For ContentDialog
- **CommunityToolkit.Mvvm**: For ObservableObject and RelayCommand
- **Bucket Helpers**: UpdateHelper, NetworkHelper, ProcessInfoHelper
- **Bucket Settings**: For persisting last update check date

## Related Files

- [`AppUpdateSettingPage.md`](../../Views/Settings/AppUpdateSettingPage.md) - Associated view
- [`GeneralSettingViewModel.md`](./GeneralSettingViewModel.md) - Related settings ViewModel
- [`AboutUsSettingViewModel.md`](./AboutUsSettingViewModel.md) - Related settings ViewModel

## Best Practices

- Use async/await pattern for all network operations
- Provide clear feedback during loading operations
- Handle network connectivity issues gracefully
- Store update check timestamps for user reference
- Validate network availability before making requests

## Error Handling

- Network connectivity failures
- GitHub API rate limiting or failures
- Invalid version parsing
- Dialog display errors
- External browser launch failures

## Security Considerations

- **URL Validation**: Ensure GitHub URLs are properly formatted
- **Network Security**: Use HTTPS for all external requests
- **Data Validation**: Validate version information from external sources

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies.
