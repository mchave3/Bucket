using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Bucket.Models;
using Bucket.Services;
using Bucket.Services.MSCatalog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Bucket.ViewModels;

public partial class MicrosoftUpdateCatalogViewModel : ObservableObject
{
    private readonly IMSCatalogService _msUpdateService;
    private readonly IWindowsVersionsConfigService _configService;
    private CancellationTokenSource _searchCancellationTokenSource;
    private CancellationTokenSource _downloadCancellationTokenSource;

    [ObservableProperty]
    private ObservableCollection<MSCatalogUpdate> updates = new();

    [ObservableProperty]
    private ObservableCollection<MSCatalogUpdate> selectedUpdates = new();

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool isDownloading;

    [ObservableProperty]
    private double searchProgress;

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private string downloadStatus = string.Empty;

    [ObservableProperty]
    private string searchStatus = string.Empty;

    [ObservableProperty]
    private SearchMode searchMode = SearchMode.OperatingSystem;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedOperatingSystem = "Windows 11";

    [ObservableProperty]
    private string selectedVersion = "24H2";

    [ObservableProperty]
    private string selectedUpdateType = "Cumulative Update";

    [ObservableProperty]
    private string selectedArchitecture = "x64";

    [ObservableProperty]
    private bool excludePreview = true;

    [ObservableProperty]
    private bool excludeDynamic = true;

    [ObservableProperty]
    private bool excludeFramework = false;

    [ObservableProperty]
    private DateTimeOffset? minDate;

    [ObservableProperty]
    private DateTimeOffset? maxDate;

    [ObservableProperty]
    private string sortBy = "Date";

    [ObservableProperty]
    private bool sortDescending = true;

    [ObservableProperty]
    private int maxPages = 5;

    [ObservableProperty]
    private string defaultDownloadPath;

    public ObservableCollection<string> OperatingSystems { get; } = new();

    public ObservableCollection<string> Versions { get; } = new();

    public ObservableCollection<string> UpdateTypes { get; } = new();

    public ObservableCollection<string> Architectures { get; } = new();

    public ObservableCollection<string> SortOptions { get; } = new()
    {
        "Date",
        "Size",
        "Title"
    };

    public MicrosoftUpdateCatalogViewModel(IMSCatalogService msUpdateService, IWindowsVersionsConfigService configService)
    {
        _msUpdateService = msUpdateService ?? throw new ArgumentNullException(nameof(msUpdateService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        
        // Set default download path
        DefaultDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket", "Updates");
        
        // Initialize with some default values
        MinDate = DateTimeOffset.Now.AddMonths(-6);
        MaxDate = DateTimeOffset.Now;

        // Initialize collections asynchronously
        _ = InitializeCollectionsAsync();

        Logger.Information("MicrosoftUpdateCatalogViewModel initialized");
    }

    [RelayCommand]
    private async Task SearchUpdatesAsync()
    {
        if (IsSearching)
        {
            Logger.Warning("Search already in progress");
            return;
        }

        Logger.Information("Starting update search");
        
        // Cancel any existing search
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsSearching = true;
            SearchProgress = 0;
            Updates.Clear();
            SelectedUpdates.Clear();
            SearchStatus = "Searching for updates...";

            var request = BuildSearchRequest();
            
            var results = await _msUpdateService.SearchUpdatesAsync(request, _searchCancellationTokenSource.Token);
            
            foreach (var update in results)
            {
                Updates.Add(update);
            }

            SearchStatus = $"Found {Updates.Count} updates";
            Logger.Information("Search completed. Found {Count} updates", Updates.Count);
        }
        catch (OperationCanceledException)
        {
            SearchStatus = "Search cancelled";
            Logger.Information("Search cancelled by user");
        }
        catch (Exception ex)
        {
            SearchStatus = $"Error: {ex.Message}";
            Logger.Error(ex, "Error during search");
        }
        finally
        {
            IsSearching = false;
            SearchProgress = 0;
        }
    }

    [RelayCommand]
    private void CancelSearch()
    {
        if (IsSearching)
        {
            _searchCancellationTokenSource?.Cancel();
            Logger.Information("Cancelling search");
        }
    }

    [RelayCommand]
    private async Task DownloadSelectedUpdatesAsync()
    {
        if (IsDownloading || !SelectedUpdates.Any())
        {
            return;
        }

        Logger.Information("Starting download of {Count} selected updates", SelectedUpdates.Count);

        // Cancel any existing download
        _downloadCancellationTokenSource?.Cancel();
        _downloadCancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsDownloading = true;
            DownloadProgress = 0;
            DownloadStatus = "Preparing download...";

            // Create download folder structure
            var downloadPath = CreateDownloadPath();
            
            var progress = new Progress<DownloadProgress>(p =>
            {
                DownloadProgress = p.OverallProgress;
                DownloadStatus = $"Downloading {p.CurrentFile} ({p.CurrentFileIndex}/{p.TotalFiles})... {p.CurrentFileProgress:F0}%";
            });

            var success = await _msUpdateService.DownloadMultipleUpdatesAsync(
                SelectedUpdates, 
                downloadPath, 
                progress, 
                _downloadCancellationTokenSource.Token);

            DownloadStatus = success ? $"Downloaded {SelectedUpdates.Count} updates successfully" : "Some downloads failed";
            Logger.Information("Download completed. Success: {Success}", success);
        }
        catch (OperationCanceledException)
        {
            DownloadStatus = "Download cancelled";
            Logger.Information("Download cancelled by user");
        }
        catch (Exception ex)
        {
            DownloadStatus = $"Error: {ex.Message}";
            Logger.Error(ex, "Error during download");
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        if (IsDownloading)
        {
            _downloadCancellationTokenSource?.Cancel();
            Logger.Information("Cancelling download");
        }
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        if (!Updates.Any())
        {
            return;
        }

        Logger.Information("Exporting updates to Excel");

        try
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
            savePicker.SuggestedFileName = $"MSCatalog_Updates_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Get the window handle
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var success = await _msUpdateService.ExportToExcelAsync(Updates, file.Path);
                if (success)
                {
                    Logger.Information("Successfully exported to {Path}", file.Path);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error exporting to Excel");
        }
    }

    [RelayCommand]
    private async Task SelectDownloadFolderAsync()
    {
        try
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            // Get the window handle
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                DefaultDownloadPath = folder.Path;
                Logger.Information("Download path changed to: {Path}", DefaultDownloadPath);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error selecting download folder");
        }
    }

    [RelayCommand]
    private void SelectAllUpdates()
    {
        foreach (var update in Updates)
        {
            update.IsSelected = true;
            if (!SelectedUpdates.Contains(update))
            {
                SelectedUpdates.Add(update);
            }
        }
    }

    [RelayCommand]
    private void DeselectAllUpdates()
    {
        foreach (var update in Updates)
        {
            update.IsSelected = false;
        }
        SelectedUpdates.Clear();
    }

    private MSCatalogSearchRequest BuildSearchRequest()
    {
        var request = new MSCatalogSearchRequest
        {
            Mode = SearchMode,
            SearchQuery = SearchQuery,
            OperatingSystem = SelectedOperatingSystem,
            Version = SelectedVersion,
            UpdateType = SelectedUpdateType == "All" ? null : SelectedUpdateType,
            Architecture = SelectedArchitecture == "All" ? null : SelectedArchitecture,
            IncludePreview = !ExcludePreview,
            IncludeDynamic = !ExcludeDynamic,
            ExcludeFramework = ExcludeFramework,
            FromDate = MinDate?.DateTime,
            ToDate = MaxDate?.DateTime,
            SortBy = SortBy,
            Descending = SortDescending,
            AllPages = false, // Use MaxPages limit instead of all pages
            MaxPages = MaxPages
        };

        return request;
    }

    private string CreateDownloadPath()
    {
        var basePath = DefaultDownloadPath;
        
        // Create structured folder based on search criteria
        if (SearchMode == SearchMode.OperatingSystem)
        {
            var os = SelectedOperatingSystem.Replace(" ", "_");
            var version = SelectedVersion;
            var updateType = (SelectedUpdateType == "All" ? "Updates" : SelectedUpdateType).Replace(" ", "_");
            
            basePath = Path.Combine(basePath, os, version, updateType);
        }
        else
        {
            // For custom queries, create a folder based on date
            basePath = Path.Combine(basePath, "CustomQueries", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        return basePath;
    }

    partial void OnSearchModeChanged(SearchMode value)
    {
        Logger.Information("Search mode changed to: {Mode}", value);
    }

    partial void OnSelectedOperatingSystemChanged(string value)
    {
        Logger.Information("Operating system changed to: {OS}", value);
        _ = UpdateVersionsAsync();
    }

    partial void OnSelectedVersionChanged(string value)
    {
        Logger.Information("Version changed to: {Version}", value);
        _ = UpdateArchitecturesAndUpdateTypesAsync();
    }

    private async Task InitializeCollectionsAsync()
    {
        try
        {
            // Load operating systems
            var operatingSystems = await _configService.GetOperatingSystemsAsync();
            foreach (var os in operatingSystems)
            {
                OperatingSystems.Add(os);
            }

            // Set default OS and load its versions
            if (OperatingSystems.Any())
            {
                SelectedOperatingSystem = OperatingSystems.First();
                await UpdateVersionsAsync();
            }

            Logger.Information("Collections initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error initializing collections");
        }
    }

    private async Task UpdateVersionsAsync()
    {
        try
        {
            Versions.Clear();
            
            if (string.IsNullOrEmpty(SelectedOperatingSystem))
                return;

            var versions = await _configService.GetVersionsForOperatingSystemAsync(SelectedOperatingSystem);
            foreach (var version in versions)
            {
                Versions.Add(version);
            }

            // Set default version
            if (Versions.Any())
            {
                SelectedVersion = Versions.First();
                await UpdateArchitecturesAndUpdateTypesAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error updating versions for OS: {OS}", SelectedOperatingSystem);
        }
    }

    private async Task UpdateArchitecturesAndUpdateTypesAsync()
    {
        try
        {
            Architectures.Clear();
            UpdateTypes.Clear();
            
            if (string.IsNullOrEmpty(SelectedOperatingSystem) || string.IsNullOrEmpty(SelectedVersion))
                return;

            // Update architectures
            var architectures = await _configService.GetArchitecturesForVersionAsync(SelectedOperatingSystem, SelectedVersion);
            foreach (var arch in architectures)
            {
                Architectures.Add(arch);
            }

            // Update update types
            var updateTypes = await _configService.GetUpdateTypesForVersionAsync(SelectedOperatingSystem, SelectedVersion);
            foreach (var updateType in updateTypes)
            {
                UpdateTypes.Add(updateType);
            }

            // Set defaults
            if (Architectures.Any())
            {
                SelectedArchitecture = Architectures.Contains("x64") ? "x64" : Architectures.First();
            }

            if (UpdateTypes.Any())
            {
                SelectedUpdateType = UpdateTypes.Contains("Cumulative Update") ? "Cumulative Update" : UpdateTypes.First();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error updating architectures and update types for OS: {OS}, Version: {Version}", 
                SelectedOperatingSystem, SelectedVersion);
        }
    }

    public void HandleUpdateSelectionChanged(MSCatalogUpdate update, bool isSelected)
    {
        update.IsSelected = isSelected;
        
        if (isSelected && !SelectedUpdates.Contains(update))
        {
            SelectedUpdates.Add(update);
        }
        else if (!isSelected && SelectedUpdates.Contains(update))
        {
            SelectedUpdates.Remove(update);
        }
    }
} 