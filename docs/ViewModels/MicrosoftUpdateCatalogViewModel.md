# MicrosoftUpdateCatalogViewModel Class Documentation

## Overview
ViewModel for the Microsoft Update Catalog page that manages search, filtering, download, and export functionality for Windows updates. Implements the MVVM pattern using CommunityToolkit.Mvvm.

## Location
- **File**: `src/ViewModels/MicrosoftUpdateCatalogViewModel.cs`
- **Namespace**: `Bucket.ViewModels`

## Class Definition
```csharp
public partial class MicrosoftUpdateCatalogViewModel : ObservableObject
```

## Constructor
```csharp
public MicrosoftUpdateCatalogViewModel(IMSCatalogService msUpdateService)
```

**Parameters:**
- `msUpdateService`: The injected MS Catalog service for update operations

## Properties

### Collections
| Property | Type | Description |
|----------|------|-------------|
| `Updates` | `ObservableCollection<MSCatalogUpdate>` | All updates from search results |
| `SelectedUpdates` | `ObservableCollection<MSCatalogUpdate>` | Currently selected updates for download |
| `OperatingSystems` | `ObservableCollection<string>` | Available OS options |
| `Versions` | `ObservableCollection<string>` | Available version options |
| `UpdateTypes` | `ObservableCollection<string>` | Available update type options |
| `Architectures` | `ObservableCollection<string>` | Available architecture options |
| `SortOptions` | `ObservableCollection<string>` | Available sort field options |

### State Properties
| Property | Type | Description |
|----------|------|-------------|
| `IsSearching` | `bool` | Indicates if search is in progress |
| `IsDownloading` | `bool` | Indicates if download is in progress |
| `SearchProgress` | `double` | Search progress (0-100) |
| `DownloadProgress` | `double` | Download progress (0-100) |
| `SearchStatus` | `string` | Current search status message |
| `DownloadStatus` | `string` | Current download status message |

### Search Parameters
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SearchMode` | `SearchMode` | `OperatingSystem` | Current search mode |
| `SearchQuery` | `string` | Empty | Custom search query text |
| `SelectedOperatingSystem` | `string` | "Windows 11" | Selected OS for search |
| `SelectedVersion` | `string` | "22H2" | Selected OS version |
| `SelectedUpdateType` | `string` | "Cumulative Update" | Selected update type |
| `SelectedArchitecture` | `string` | "x64" | Selected architecture |

### Filter Options
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ExcludePreview` | `bool` | true | Exclude preview updates |
| `ExcludeDynamic` | `bool` | true | Exclude dynamic updates |
| `ExcludeFramework` | `bool` | false | Exclude .NET Framework updates |
| `MinDate` | `DateTime?` | 6 months ago | Minimum date filter |
| `MaxDate` | `DateTime?` | Today | Maximum date filter |
| `SortBy` | `string` | "Date" | Sort field |
| `SortDescending` | `bool` | true | Sort order |
| `MaxPages` | `int` | 5 | Maximum pages to retrieve |

### Configuration
| Property | Type | Description |
|----------|------|-------------|
| `DefaultDownloadPath` | `string` | Default download directory path |

## Commands

### SearchUpdatesCommand
Initiates a search for updates based on current parameters.
```csharp
[RelayCommand]
private async Task SearchUpdatesAsync()
```

### CancelSearchCommand
Cancels the current search operation.
```csharp
[RelayCommand]
private void CancelSearch()
```

### DownloadSelectedUpdatesCommand
Downloads all selected updates to the specified directory.
```csharp
[RelayCommand]
private async Task DownloadSelectedUpdatesAsync()
```

### CancelDownloadCommand
Cancels the current download operation.
```csharp
[RelayCommand]
private void CancelDownload()
```

### ExportToExcelCommand
Exports search results to an Excel file.
```csharp
[RelayCommand]
private async Task ExportToExcelAsync()
```

### SelectDownloadFolderCommand
Opens folder picker to change download directory.
```csharp
[RelayCommand]
private async Task SelectDownloadFolderAsync()
```

### SelectAllUpdatesCommand
Selects all updates in the current results.
```csharp
[RelayCommand]
private void SelectAllUpdates()
```

### DeselectAllUpdatesCommand
Deselects all updates.
```csharp
[RelayCommand]
private void DeselectAllUpdates()
```

## Methods

### HandleUpdateSelectionChanged
Handles individual update selection changes from the UI.
```csharp
public void HandleUpdateSelectionChanged(MSCatalogUpdate update, bool isSelected)
```

## Usage Examples

### Basic Search Operation
```csharp
// ViewModel is injected via DI
var viewModel = GetService<MicrosoftUpdateCatalogViewModel>();

// Configure search
viewModel.SearchMode = SearchMode.OperatingSystem;
viewModel.SelectedOperatingSystem = "Windows 11";
viewModel.SelectedVersion = "23H2";

// Execute search
await viewModel.SearchUpdatesCommand.ExecuteAsync(null);
```

### Download Selected Updates
```csharp
// Select updates
foreach (var update in viewModel.Updates.Take(5))
{
    viewModel.HandleUpdateSelectionChanged(update, true);
}

// Start download
await viewModel.DownloadSelectedUpdatesCommand.ExecuteAsync(null);
```

## Features
- Dual search modes (OS-based and custom query)
- Real-time progress tracking
- Cancellable operations
- Automatic folder structure creation
- Excel export functionality
- Comprehensive filtering options
- Batch selection operations
- Configurable download paths

## Dependencies
- `IMSCatalogService` for catalog operations
- `CommunityToolkit.Mvvm` for MVVM implementation
- Windows Storage Pickers for file/folder selection
- Serilog for logging

## Related Files
- [`MicrosoftUpdateCatalogPage.md`](../Views/MicrosoftUpdateCatalogPage.md) - Associated view
- [`IMSCatalogService.md`](../Services/MSCatalog/IMSCatalogService.md) - Service interface
- [`MSCatalogUpdate.md`](../Models/MSCatalogUpdate.md) - Update model

## Best Practices
1. Always check IsSearching/IsDownloading before operations
2. Use cancellation tokens for long operations
3. Validate search parameters before searching
4. Handle selection changes through HandleUpdateSelectionChanged
5. Log all significant user actions

## Error Handling
- Search errors displayed in SearchStatus
- Download errors displayed in DownloadStatus
- Cancellation handled gracefully
- File picker errors logged but don't crash

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 