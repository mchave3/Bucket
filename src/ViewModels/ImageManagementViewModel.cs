using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bucket.Models;
using Bucket.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Windows.Storage.Pickers;

namespace Bucket.ViewModels;

/// <summary>
/// ViewModel for the Windows Image Management page.
/// </summary>
public partial class ImageManagementViewModel : ObservableObject
{
    private readonly WindowsImageService _windowsImageService;

    private ObservableCollection<WindowsImageInfo> _images = new();
    private WindowsImageInfo _selectedImage;
    private bool _isLoading;
    private string _statusMessage = "Ready";
    private bool _hasImages;
    private string _searchText = string.Empty;
    private ObservableCollection<WindowsImageInfo> _filteredImages = new();

    /// <summary>
    /// Gets or sets the collection of Windows images.
    /// </summary>
    public ObservableCollection<WindowsImageInfo> Images
    {
        get => _images;
        set => SetProperty(ref _images, value);
    }

    /// <summary>
    /// Gets or sets the currently selected image.
    /// </summary>
    public WindowsImageInfo SelectedImage
    {
        get => _selectedImage;
        set => SetProperty(ref _selectedImage, value);
    }

    /// <summary>
    /// Gets or sets whether the ViewModel is currently loading data.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Gets or sets the current status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Gets or sets whether there are any images available.
    /// </summary>
    public bool HasImages
    {
        get => _hasImages;
        set => SetProperty(ref _hasImages, value);
    }

    /// <summary>
    /// Gets or sets the search text for filtering images.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    /// <summary>
    /// Gets or sets the filtered collection of Windows images.
    /// </summary>
    public ObservableCollection<WindowsImageInfo> FilteredImages
    {
        get => _filteredImages;
        set
        {
            SetProperty(ref _filteredImages, value);
            OnPropertyChanged(nameof(FilteredImagesCountText));
        }
    }

    /// <summary>
    /// Gets the formatted count text for filtered images.
    /// </summary>
    public string FilteredImagesCountText =>
        FilteredImages.Count == 1 ? "1 image" : $"{FilteredImages.Count} images";

    /// <summary>
    /// Initializes a new instance of the ImageManagementViewModel class.
    /// </summary>
    public ImageManagementViewModel()
    {
        _windowsImageService = new WindowsImageService();

        // Initialize commands
        RefreshCommand = new AsyncRelayCommand(RefreshImagesAsync);
        ImportFromIsoCommand = new AsyncRelayCommand(ImportFromIsoAsync);
        ImportFromWimCommand = new AsyncRelayCommand(ImportFromWimAsync);
        DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedImageAsync, CanDeleteSelected);
        DeleteSelectedFromDiskCommand = new AsyncRelayCommand(DeleteSelectedImageFromDiskAsync, CanDeleteSelected);
        ViewImageDetailsCommand = new RelayCommand<WindowsImageInfo>(ViewImageDetails);

        // Watch for property changes
        PropertyChanged += OnPropertyChanged;

        Logger.Information("ImageManagementViewModel initialized");
    }

    #region Commands

    /// <summary>
    /// Gets the command to refresh the images list.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Gets the command to import images from an ISO file.
    /// </summary>
    public ICommand ImportFromIsoCommand { get; }

    /// <summary>
    /// Gets the command to import a WIM file directly.
    /// </summary>
    public ICommand ImportFromWimCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected image from the list.
    /// </summary>
    public ICommand DeleteSelectedCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected image from both list and disk.
    /// </summary>
    public ICommand DeleteSelectedFromDiskCommand { get; }

    /// <summary>
    /// Gets the command to view detailed information about an image.
    /// </summary>
    public ICommand ViewImageDetailsCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the ViewModel by loading existing images.
    /// </summary>
    public async Task InitializeAsync()
    {
        Logger.Information("Initializing ImageManagementViewModel");
        await RefreshImagesAsync();
    }

    /// <summary>
    /// Updates the search filter and refreshes the filtered images.
    /// </summary>
    /// <param name="searchText">The search text to filter by.</param>
    public void UpdateSearchFilter(string searchText)
    {
        SearchText = searchText;
        FilterImages();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Refreshes the images collection from the service.
    /// </summary>
    private async Task RefreshImagesAsync()
    {
        Logger.Information("Refreshing Windows images list");

        try
        {
            IsLoading = true;
            StatusMessage = "Loading images...";

            var images = await _windowsImageService.GetImagesAsync();

            Images.Clear();
            foreach (var image in images)
            {
                Images.Add(image);
            }

            FilterImages();
            HasImages = Images.Count > 0;

            StatusMessage = $"Loaded {Images.Count} images";
            Logger.Information("Successfully refreshed {Count} Windows images", Images.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to refresh Windows images");
            StatusMessage = $"Error loading images: {ex.Message}";

            // Show error dialog
            await ShowErrorDialogAsync("Error Loading Images", $"Failed to load Windows images: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Imports images from an ISO file.
    /// </summary>
    private async Task ImportFromIsoAsync()
    {
        Logger.Information("Starting ISO import process");

        try
        {
            var picker = new FilePicker(WindowNative.GetWindowHandle(App.MainWindow))
            {
                FileTypeChoices = new Dictionary<string, IList<string>>
                {
                    { "ISO Files", new List<string> { "*.iso" } }
                },
                DefaultFileExtension = "ISO Files",
                Title = "Select an ISO file to import",
                ShowAllFilesOption = false
            };

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                StatusMessage = $"Selected ISO: {file.Name}";
                Logger.Information("User selected ISO file: {FilePath}", file.Path);

                // TODO: Implement ISO mounting and WIM extraction
                await ShowInfoDialogAsync("ISO Import",
                    $"ISO file selected: {file.Name}\n\nISO mounting and extraction will be implemented in the next update.");
            }
            else
            {
                StatusMessage = "ISO import cancelled";
                Logger.Information("User cancelled ISO file selection");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start ISO import");
            await ShowErrorDialogAsync("Import Error", $"Failed to start ISO import: {ex.Message}");
        }
    }

    /// <summary>
    /// Imports a WIM file directly.
    /// </summary>
    private async Task ImportFromWimAsync()
    {
        Logger.Information("Starting WIM import process");

        try
        {
            // Open file picker for WIM/ESD files
            var picker = new Windows.Storage.Pickers.FileOpenPicker();

            // Get the window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".wim");
            picker.FileTypeFilter.Add(".esd");
            picker.FileTypeFilter.Add(".swm");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                StatusMessage = $"Analyzing {file.Name}...";
                Logger.Information("User selected WIM file: {FilePath}", file.Path);

                IsLoading = true;

                try
                {
                    // Get a friendly name for the image
                    var imageName = Path.GetFileNameWithoutExtension(file.Name);

                    // Import the image using the service
                    var importedImage = await _windowsImageService.ImportImageAsync(
                        file.Path,
                        imageName,
                        sourceIsoPath: "",
                        progress: new Progress<string>(message => StatusMessage = message));

                    // Refresh the images list
                    await RefreshImagesAsync();

                    StatusMessage = $"Successfully imported {importedImage.Name}";
                    Logger.Information("Successfully imported WIM file: {Name} with {IndexCount} indices",
                        importedImage.Name, importedImage.IndexCount);

                    await ShowInfoDialogAsync("Import Successful",
                        $"Successfully imported '{importedImage.Name}' with {importedImage.IndexCount} Windows editions.");
                }
                catch (Exception importEx)
                {
                    Logger.Error(importEx, "Failed to import WIM file: {FilePath}", file.Path);
                    StatusMessage = $"Failed to import {file.Name}";
                    await ShowErrorDialogAsync("Import Failed",
                        $"Failed to import '{file.Name}':\n\n{importEx.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
            else
            {
                StatusMessage = "WIM import cancelled";
                Logger.Information("User cancelled WIM file selection");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start WIM import");
            await ShowErrorDialogAsync("Import Error", $"Failed to start WIM import: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes the selected image from the collection only.
    /// </summary>
    private async Task DeleteSelectedImageAsync()
    {
        if (SelectedImage == null) return;

        Logger.Information("Deleting selected image from collection: {Name}", SelectedImage.Name);

        try
        {
            var result = await ShowConfirmationDialogAsync(
                "Delete Image",
                $"Are you sure you want to remove '{SelectedImage.Name}' from the collection?\n\nThe image file will remain on disk.");

            if (result)
            {
                await _windowsImageService.DeleteImageAsync(SelectedImage, deleteFromDisk: false);
                await RefreshImagesAsync();
                SelectedImage = null;
                StatusMessage = "Image removed from collection";
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete image from collection");
            await ShowErrorDialogAsync("Delete Error", $"Failed to delete image: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes the selected image from both collection and disk.
    /// </summary>
    private async Task DeleteSelectedImageFromDiskAsync()
    {
        if (SelectedImage == null) return;

        Logger.Information("Deleting selected image from disk: {Name}", SelectedImage.Name);

        try
        {
            var result = await ShowConfirmationDialogAsync(
                "Delete Image and File",
                $"Are you sure you want to permanently delete '{SelectedImage.Name}' and its file?\n\nThis action cannot be undone!");

            if (result)
            {
                await _windowsImageService.DeleteImageAsync(SelectedImage, deleteFromDisk: true);
                await RefreshImagesAsync();
                SelectedImage = null;
                StatusMessage = "Image and file deleted";
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete image from disk");
            await ShowErrorDialogAsync("Delete Error", $"Failed to delete image file: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the selected image can be deleted.
    /// </summary>
    /// <returns>True if an image is selected, false otherwise.</returns>
    private bool CanDeleteSelected()
    {
        return SelectedImage != null;
    }

    /// <summary>
    /// Views detailed information about the specified image.
    /// </summary>
    /// <param name="image">The image to view details for.</param>
    private void ViewImageDetails(WindowsImageInfo image)
    {
        if (image == null) return;

        Logger.Information("Viewing details for image: {Name}", image.Name);

        try
        {
            // Navigate to the image details page
            var navService = App.GetService<IJsonNavigationService>();
            navService.NavigateTo(typeof(Views.ImageDetailsPage), image);

            StatusMessage = $"Viewing details for {image.Name}";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to navigate to image details for: {Name}", image.Name);
            StatusMessage = $"Failed to open details for {image.Name}";
        }
    }

    /// <summary>
    /// Filters the images based on the search text.
    /// </summary>
    private void FilterImages()
    {
        FilteredImages.Clear();

        var filteredItems = string.IsNullOrWhiteSpace(SearchText)
            ? Images
            : Images.Where(img =>
                img.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                img.ImageType.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                img.Indices.Any(idx =>
                    idx.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    idx.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    idx.Architecture.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

        foreach (var item in filteredItems)
        {
            FilteredImages.Add(item);        }

        Logger.Debug("Filtered images: {Count} of {Total}", FilteredImages.Count, Images.Count);
        OnPropertyChanged(nameof(FilteredImagesCountText));
    }

    /// <summary>
    /// Handles property change events.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The property change event arguments.</param>
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedImage):
                // Update command can execute states
                ((AsyncRelayCommand)DeleteSelectedCommand).NotifyCanExecuteChanged();
                ((AsyncRelayCommand)DeleteSelectedFromDiskCommand).NotifyCanExecuteChanged();
                break;

            case nameof(SearchText):
                FilterImages();
                break;
        }
    }

    /// <summary>
    /// Shows an error dialog with the specified title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The error message.</param>
    private async Task ShowErrorDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        // Get XamlRoot from the main window
        if (App.MainWindow?.Content is FrameworkElement element)
        {
            dialog.XamlRoot = element.XamlRoot;
        }

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Shows an information dialog with the specified title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The information message.</param>
    private async Task ShowInfoDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        // Get XamlRoot from the main window
        if (App.MainWindow?.Content is FrameworkElement element)
        {
            dialog.XamlRoot = element.XamlRoot;
        }

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Shows a confirmation dialog with the specified title and message.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The confirmation message.</param>
    /// <returns>True if the user confirmed, false otherwise.</returns>
    private async Task<bool> ShowConfirmationDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Secondary
        };

        // Get XamlRoot from the main window
        if (App.MainWindow?.Content is FrameworkElement element)
        {
            dialog.XamlRoot = element.XamlRoot;
        }

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    #endregion
}
