# MicrosoftUpdateCatalogPage Class Documentation

## Overview
WinUI 3 page that provides a comprehensive interface for searching, filtering, downloading, and exporting Windows updates from the Microsoft Update Catalog. Features a modern, responsive design with advanced filtering capabilities.

## Location
- **File**: `src/Views/MicrosoftUpdateCatalogPage.xaml` and `src/Views/MicrosoftUpdateCatalogPage.xaml.cs`
- **Namespace**: `Bucket.Views`

## Class Definition
```csharp
public sealed partial class MicrosoftUpdateCatalogPage : Page
```

## Constructor
```csharp
public MicrosoftUpdateCatalogPage()
```

Initializes the page, retrieves the ViewModel from dependency injection, and sets up event handlers.

## Properties
| Property | Type | Description |
|----------|------|-------------|
| `ViewModel` | `MicrosoftUpdateCatalogViewModel` | The view model for data binding and commands |

## UI Structure

### Main Layout
- **Grid** with 4 rows:
  1. Header with title
  2. Search and filter controls
  3. Results data grid
  4. Action bar with buttons

### Search Section
- **Search Mode Toggle**: RadioButtons for OS-based or custom query search
- **OS Search Panel**: ComboBoxes for OS, Version, Update Type, Architecture
- **Custom Search Panel**: TextBox for free-form search queries
- **Advanced Filters**: Expander with additional filter options

### Filter Options
- **Exclusion Filters**: CheckBoxes for preview, dynamic, and framework updates
- **Date Filters**: CalendarDatePickers for date range
- **Sort Options**: ComboBox for sort field and order
- **Pagination**: NumberBox for maximum pages

### Results Display
- **DataGrid**: Displays search results with columns:
  - Select (checkbox)
  - Title
  - Products
  - Classification
  - Last Updated
  - Size
  - Version

### Action Bar
- **Selection Actions**: Select All / Deselect All buttons
- **Download Section**: Selected count, folder selector, download path display
- **Export Button**: Export results to Excel
- **Download Button**: Download selected updates

### Progress Overlay
- Modal overlay shown during downloads
- Progress bar and status text
- Cancel button

## Event Handlers

### OnNavigatedTo
```csharp
protected override void OnNavigatedTo(NavigationEventArgs e)
```
Logs navigation to the page.

### OnNavigatedFrom
```csharp
protected override void OnNavigatedFrom(NavigationEventArgs e)
```
Logs navigation away from the page.

### OnDataGridSelectionChanged
```csharp
private void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
```
Synchronizes DataGrid selection with ViewModel's SelectedUpdates collection.

## Data Bindings

### Key Bindings
- `SearchMode` ↔ Search mode RadioButtons
- `Updates` → DataGrid ItemsSource
- `IsSearching` → Search button enable state, progress visibility
- `IsDownloading` → Download overlay visibility
- `SearchProgress` / `DownloadProgress` → Progress bars
- `SearchStatus` / `DownloadStatus` → Status text displays

## Converters Used
- `EnumToIntConverter` - For RadioButtons enum binding
- `SearchModeToVisibilityConverter` - Show/hide search panels
- `BooleanNegationConverter` - Invert boolean for enable states
- `ZeroToTrueConverter` - Progress bar indeterminate state
- `ZeroToVisibleConverter` - Empty state visibility
- `DateFormatConverter` - Format dates in grid
- `GreaterThanZeroToBoolConverter` - Enable buttons based on count
- `StringFormatConverter` - Format selected count text

## Usage Examples

### Basic Page Navigation
```csharp
// Navigate to the page
Frame.Navigate(typeof(MicrosoftUpdateCatalogPage));
```

### Programmatic Search
```csharp
// Access ViewModel after navigation
var page = Frame.Content as MicrosoftUpdateCatalogPage;
var viewModel = page.ViewModel;

// Configure and search
viewModel.SelectedOperatingSystem = "Windows 11";
await viewModel.SearchUpdatesCommand.ExecuteAsync(null);
```

## Features
- Responsive layout with proper spacing
- Modern WinUI 3 design patterns
- Comprehensive search and filter UI
- Real-time progress indication
- Multi-select data grid
- Modal download progress
- Keyboard navigation support
- Accessibility compliant

## Dependencies
- `MicrosoftUpdateCatalogViewModel` for business logic
- WinUI 3 controls (DataGrid, RadioButtons, Expander, etc.)
- Custom value converters
- CommunityToolkit.WinUI for DataGrid

## Related Files
- [`MicrosoftUpdateCatalogViewModel.md`](../ViewModels/MicrosoftUpdateCatalogViewModel.md) - Associated ViewModel
- [`MSCatalogUpdate.md`](../Models/MSCatalogUpdate.md) - Data model
- Converter documentation files in Common folder

## Best Practices
1. Always use x:Bind for performance
2. Handle DataGrid selection through events
3. Use proper spacing and margins
4. Implement keyboard navigation
5. Provide visual feedback for all operations

## Error Handling
- ViewModel handles all errors and updates status
- UI remains responsive during long operations
- Cancellation properly supported
- Empty states clearly communicated

## Accessibility
- All controls properly labeled
- Keyboard navigation fully supported
- Screen reader compatible
- High contrast theme support

---

**Note**: This documentation may have been generated automatically by AI and could potentially contain errors. Please verify the information against the actual source code and report any discrepancies. 