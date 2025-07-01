# 📋 Specifications: Native C# Implementation of the Microsoft Update Catalog Service

## 🎯 Project Objective

Rewrite the functionality of the PowerShell module MSCatalogLTS as a native C# service within the WinUI 3 Bucket application. The new "Microsoft Update Catalog" page will allow users to search, view, and download Windows updates directly from the graphical interface, using a fully managed C# implementation with no dependency on PowerShell scripts or modules.

## 📐 Technical Architecture

### 🏗️ File Structure to Create

```
src/
├── Views/
│   ├── MicrosoftUpdateCatalogPage.xaml
│   └── MicrosoftUpdateCatalogPage.xaml.cs
├── ViewModels/
│   └── MicrosoftUpdateCatalogViewModel.cs
├── Services/
│   └── MSCatalog/
│       ├── IMSCatalogService.cs
│       └── MSCatalogService.cs
└── Models/
    ├── MSCatalogUpdate.cs
    └── MSCatalogSearchRequest.cs

docs/
├── Views/
│   └── MicrosoftUpdateCatalogPage.md
├── ViewModels/
│   └── MicrosoftUpdateCatalogViewModel.md
└── Services/
    └── MSCatalog/
        ├── IMSCatalogService.md
        └── MSCatalogService.md
```

## 🎨 User Interface Design

### 📱 Proposed Page Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ Microsoft Update Catalog                                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ ┌─── Search Panel ──────────────────────────────────────────┐   │
│ │                                                           │   │
│ │ Search Type: [○ Search Query] [○ Operating System]       │   │
│ │                                                           │   │
│ │ ┌─ Search Query Mode ─────────────────────────────────┐   │   │
│ │ │ Search: [________________________] [Search] [🔄]   │   │   │
│ │ │ ☐ Strict Search  ☐ All Pages                       │   │   │
│ │ └─────────────────────────────────────────────────────┘   │   │
│ │                                                           │   │
│ │ ┌─ Operating System Mode ─────────────────────────────┐   │   │
│ │ │ OS: [Windows 11 ▼] Version: [24H2 ▼]              │   │   │
│ │ │ Update Type: [Cumulative Updates ▼]                │   │   │
│ │ │ Architecture: [x64 ▼]                              │   │   │
│ │ └─────────────────────────────────────────────────────┘   │   │
│ │                                                           │   │
│ │ ┌─ Filters ───────────────────────────────────────────┐   │   │
│ │ │ ☐ Include Preview  ☐ Include Dynamic               │   │   │
│ │ │ ☐ Exclude Framework  ☐ Only Framework              │   │   │
│ │ │ Date Range: [From: ____] [To: ____]                │   │   │
│ │ │ Size: Min[___]MB Max[___]MB                         │   │   │
│ │ └─────────────────────────────────────────────────────┘   │   │
│ └───────────────────────────────────────────────────────────┘   │
│                                                                 │
│ ┌─── Results Panel ──────────────────────────────────────────┐   │
│ │                                                           │   │
│ │ Found: 42 updates  Sort by: [Date ▼] [↕ Desc]           │   │
│ │                                                           │   │
│ │ ┌─────────────────────────────────────────────────────┐   │   │
│ │ │ Title                          │Class.│Date │Size   │   │   │
│ │ ├────────────────────────────────┼──────┼─────┼───────┤   │   │
│ │ │ 2024-12 Cumulative Update...   │Secur.│12/10│302 MB │   │   │
│ │ │ [Details] [Download] [Export]  │      │     │       │   │   │
│ │ ├────────────────────────────────┼──────┼─────┼───────┤   │   │
│ │ │ 2024-11 Cumulative Update...   │Secur.│11/12│298 MB │   │   │
│ │ │ [Details] [Download] [Export]  │      │     │       │   │   │
│ │ └─────────────────────────────────────────────────────┘   │   │
│ │                                                           │   │
│ │ [Select All] [Download Selected] [Export to Excel]       │   │
│ └───────────────────────────────────────────────────────────┘   │
│                                                                 │
│ ┌─── Status Panel ───────────────────────────────────────────┐   │
│ │ Status: Ready │ [Progress Bar] │ [Cancel]                  │   │
│ └───────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### 🎛️ Detailed Interface Components

- **Search Panel**: RadioButtons for mode, TextBox, ComboBoxes, CheckBoxes, Search button.
- **Filters Panel**: Advanced options, DatePickers, NumberBoxes.
- **Results Panel**: DataGrid with actions, sorting, selection, group actions.
- **Status Panel**: Status indicator, progress bar, cancel button.

## 🔧 Technical Specifications

### 📊 Data Models

```csharp
public class MSCatalogUpdate
{
    public string Title { get; set; }
    public string Products { get; set; }
    public string Classification { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Version { get; set; }
    public string Size { get; set; }
    public long SizeInBytes { get; set; }
    public string Guid { get; set; }
    public string[] FileNames { get; set; }
    public bool IsSelected { get; set; }
}

public class MSCatalogSearchRequest
{
    public SearchMode Mode { get; set; }
    public string SearchQuery { get; set; }
    public string OperatingSystem { get; set; }
    public string Version { get; set; }
    public string UpdateType { get; set; }
    public string Architecture { get; set; }
    public bool StrictSearch { get; set; }
    public bool AllPages { get; set; }
    public bool IncludePreview { get; set; }
    public bool IncludeDynamic { get; set; }
    public bool ExcludeFramework { get; set; }
    public bool GetFramework { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public double? MinSize { get; set; }
    public double? MaxSize { get; set; }
    public string SortBy { get; set; }
    public bool Descending { get; set; }
}

public enum SearchMode
{
    SearchQuery,
    OperatingSystem
}
```

### 🔌 Integration Service

#### IMSCatalogService.cs
```csharp
public interface IMSCatalogService
{
    Task<IEnumerable<MSCatalogUpdate>> SearchUpdatesAsync(MSCatalogSearchRequest request, CancellationToken cancellationToken = default);
    Task<bool> DownloadUpdateAsync(MSCatalogUpdate update, string destinationPath, IProgress<double> progress = null, CancellationToken cancellationToken = default);
    Task<bool> DownloadMultipleUpdatesAsync(IEnumerable<MSCatalogUpdate> updates, string destinationPath, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default);
    Task<bool> ExportToExcelAsync(IEnumerable<MSCatalogUpdate> updates, string filePath, CancellationToken cancellationToken = default);
}
```

**All logic for searching, HTML parsing, downloading, and Excel export must be implemented natively in C# using .NET libraries such as HttpClient, HtmlAgilityPack for .NET, and EPPlus or ClosedXML for Excel export. There must be no dependency on PowerShell, PowerShell modules, or script execution.**

### 🎭 ViewModel

```csharp
[ObservableProperty] private SearchMode selectedSearchMode;
[ObservableProperty] private string searchQuery;
[ObservableProperty] private string selectedOperatingSystem;
[ObservableProperty] private string selectedVersion;
[ObservableProperty] private string selectedUpdateType;
[ObservableProperty] private string selectedArchitecture;
[ObservableProperty] private bool strictSearch;
[ObservableProperty] private bool allPages;
[ObservableProperty] private bool includePreview;
[ObservableProperty] private bool includeDynamic;
[ObservableProperty] private bool excludeFramework;
[ObservableProperty] private bool getFramework;
[ObservableProperty] private DateTime? fromDate;
[ObservableProperty] private DateTime? toDate;
[ObservableProperty] private double? minSize;
[ObservableProperty] private double? maxSize;
[ObservableProperty] private string selectedSortBy;
[ObservableProperty] private bool descending;
[ObservableProperty] private ObservableCollection<MSCatalogUpdate> searchResults;
[ObservableProperty] private bool isLoading;
[ObservableProperty] private string statusMessage;
[ObservableProperty] private double progressValue;
[ObservableProperty] private bool canCancel;
```

**Main Commands:**
```csharp
public IAsyncRelayCommand SearchCommand { get; }
public IAsyncRelayCommand<MSCatalogUpdate> DownloadUpdateCommand { get; }
public IAsyncRelayCommand DownloadSelectedCommand { get; }
public IAsyncRelayCommand ExportToExcelCommand { get; }
public IRelayCommand SelectAllCommand { get; }
public IRelayCommand ClearSelectionCommand { get; }
public IRelayCommand CancelCommand { get; }
```

## 🔗 Application Integration

### 📍 Navigation

#### Edit `AppData.json`
```json
{
  "UniqueId": "Bucket.Views.MicrosoftUpdateCatalogPage",
  "Title": "Update Catalog",
  "Subtitle": "Microsoft Update Catalog",
  "ImagePath": "ms-appx:///Assets/Fluent/Update.png",
  "HideItem": false
}
```

### 🔧 Dependency Injection

#### Edit `App.xaml.cs`
```csharp
// In ConfigureServices()
services.AddTransient<MicrosoftUpdateCatalogViewModel>();
services.AddSingleton<IMSCatalogService, MSCatalogService>();
```

## 📋 Detailed Features

- **Search**: Query mode, OS mode, advanced filters, strict search, pagination.
- **Results Display**: Sortable table, multi-selection, row actions, result counter.
- **Download**: Individual and batch, real-time progress, cancellation, error handling.
- **Export**: Excel export, selective export, full metadata.

## 🎨 Design and UX

- Light/dark themes, Fluent icons, consistent with Bucket.
- Responsive design, resizable panels, touch-friendly.
- Accessibility: screen reader, keyboard navigation, contrast, tooltips.

## 🔒 Security and Performance

- Input validation, secure download, file path verification, permission checks.
- Async search, background downloads, caching, pagination, cancellation.

## 📝 Error Handling

- Network errors, service errors, file errors, validation errors.
- Clear user messages, suggestions, logs, toast notifications.

## 📦 Default Storage Location for Downloads

**Default base folder:**  
`C:\ProgramData\Bucket\Updates`

**Folder structure logic:**  
For each downloaded update, the application must create a subfolder hierarchy based on the update's metadata:

- **Level 1:** Operating System (e.g., `Windows 11`)
- **Level 2:** Version (e.g., `24H2`)
- **Level 3:** Update Type (e.g., `Cumulative Updates`, `Security Updates`, etc.)

**Example:**  
For a cumulative update for Windows 11 24H2, the full path will be:  
`C:\ProgramData\Bucket\Updates\Windows 11\24H2\Cumulative Updates\`

**Rules:**
- This structure must be created automatically for each download if it does not already exist.
- The update file(s) must be saved in the appropriate subfolder according to their OS, version, and update type.
- This logic applies to all update types and combinations (e.g., .NET Framework, Security Updates, etc.).

**Pseudocode for path construction:**
```csharp
var basePath = @"C:\ProgramData\Bucket\Updates";
var osFolder = update.OperatingSystem; // e.g., "Windows 11"
var versionFolder = update.Version;    // e.g., "24H2"
var typeFolder = update.Classification ?? update.UpdateType; // e.g., "Cumulative Updates"
var fullPath = Path.Combine(basePath, osFolder, versionFolder, typeFolder);
Directory.CreateDirectory(fullPath);
// Save file(s) to fullPath
```

**Note**: This specifications document was generated automatically and may contain errors. Please verify the information against the actual project needs and report any discrepancies. 

The project delivers a fully native C# implementation of the Microsoft Update Catalog features.

## 🔎 Reference to Original PowerShell Module Logic

For all business logic (search, filtering, HTML parsing, download, error handling, etc.), the original MSCatalogLTS PowerShell module serves as a functional reference.

- The C# implementation must reproduce all features and behaviors present in the PowerShell module, but using .NET libraries and idioms.
- The PowerShell code (especially `Get-MSCatalogUpdate`, `Save-MSCatalogUpdate`, and related helpers) should be consulted for:
  - Query construction and parameter mapping
  - HTML parsing and data extraction logic
  - Filtering and sorting rules
  - Download and file management logic
  - Error handling and edge cases

**The C# code must not call or embed PowerShell, but should faithfully reproduce the business logic and user experience.**

## 🧩 Code Consistency and Project Standards

All code (Views, ViewModels, Services, Models) for the Microsoft Update Catalog feature must strictly follow the same architectural, naming, and implementation conventions as the rest of the Bucket application.

- **Follow the MVVM pattern** as implemented in other pages (e.g., use of CommunityToolkit.Mvvm, `[ObservableProperty]`, `[RelayCommand]`, etc.).
- **Naming conventions** (PascalCase for classes/methods/properties, camelCase for locals, etc.) must be respected.
- **File structure**: Place files in the same folders and with the same naming patterns as existing features (e.g., `src/Views/`, `src/ViewModels/`, etc.).
- **Dependency injection**: Register and inject services in the same way as other services (see `App.xaml.cs`).
- **UI/UX**: XAML pages must use the same design system, resource dictionaries, and theming as the rest of the app.
- **Logging, error handling, and progress reporting**: Use the same logging and error handling patterns as in other services and ViewModels.
- **Documentation**: Generate and structure documentation for new classes in the same way as for existing ones (see `docs/`).
- **Testing**: Write unit and integration tests following the same structure and tools as for other features.

**The goal is to ensure that the new code is fully consistent, maintainable, and indistinguishable in style and structure from the rest of the Bucket project.** 