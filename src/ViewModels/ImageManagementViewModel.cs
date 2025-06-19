using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bucket.Models;
using Bucket.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace Bucket.ViewModels;

/// <summary>
/// ViewModel for the Windows Image Management page.
/// </summary>
public partial class ImageManagementViewModel : ObservableObject
{
    private readonly WindowsImageService _windowsImageService;
    private readonly FilePickerService _filePickerService;
    private readonly IsoImportService _isoImportService;

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
    /// Gets the display name for the selected image or a default message.
    /// </summary>
    public string SelectedImageDisplayName =>
        SelectedImage?.Name ?? "No image selected";

    /// <summary>
    /// Initializes a new instance of the ImageManagementViewModel class.
    /// </summary>
    public ImageManagementViewModel()
    {
        _windowsImageService = new WindowsImageService();
        _filePickerService = new FilePickerService();
        _isoImportService = new IsoImportService();

        // Initialize commands
        RefreshCommand = new AsyncRelayCommand(RefreshImagesAsync);
        ImportFromIsoCommand = new AsyncRelayCommand(ImportFromIsoAsync);
        ImportFromWimCommand = new AsyncRelayCommand(ImportFromWimAsync); DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedImageAsync, CanDeleteSelected);
        DeleteSelectedFromDiskCommand = new AsyncRelayCommand(DeleteSelectedImageFromDiskAsync, CanDeleteSelected);
        ViewImageDetailsCommand = new RelayCommand<WindowsImageInfo>(ViewImageDetails);

        // Placeholder commands for detail panel
        ExtractSelectedIndicesCommand = new RelayCommand(() => { /* TODO: Implement */ });
        MountImageCommand = new RelayCommand(() => { /* TODO: Implement */ });
        ValidateImageCommand = new RelayCommand(() => { /* TODO: Implement */ });

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
    public ICommand DeleteSelectedFromDiskCommand { get; }    /// <summary>
    /// Gets the command to view detailed information about an image.
    /// </summary>
    public ICommand ViewImageDetailsCommand { get; }

    /// <summary>
    /// Gets the command to extract selected indices from an image.
    /// </summary>
    public ICommand ExtractSelectedIndicesCommand { get; }

    /// <summary>
    /// Gets the command to mount an image.
    /// </summary>
    public ICommand MountImageCommand { get; }

    /// <summary>
    /// Gets the command to validate an image's integrity.
    /// </summary>
    public ICommand ValidateImageCommand { get; }

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
            // Open file picker for ISO files
            var isoFile = await _filePickerService.PickIsoFileAsync();
            if (isoFile == null)
            {
                Logger.Information("ISO import cancelled by user");
                return;
            }

            // Show import progress dialog
            await ShowImportProgressDialogAsync("Importing from ISO", async (progress, cancellationToken) =>
            {
                var imageInfo = await _isoImportService.ImportFromIsoAsync(
                    isoFile,
                    "", // Let service generate name from ISO
                    progress,
                    cancellationToken);

                // Refresh the images list
                await RefreshImagesAsync();

                // Select the newly imported image
                SelectedImage = FilteredImages.FirstOrDefault(img => img.Id == imageInfo.Id);
            });

            StatusMessage = "ISO import completed successfully";
        }
        catch (OperationCanceledException)
        {
            Logger.Information("ISO import cancelled by user");
            StatusMessage = "ISO import cancelled";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to import from ISO");
            StatusMessage = $"ISO import failed: {ex.Message}";
            await ShowErrorDialogAsync("Import Error", $"Failed to import from ISO: {ex.Message}");
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
            var wimFile = await _filePickerService.PickWimFileAsync();
            if (wimFile == null)
            {
                Logger.Information("WIM import cancelled by user");
                return;
            }

            // Show import progress dialog
            await ShowImportProgressDialogAsync("Importing WIM/ESD", async (progress, cancellationToken) =>
            {
                var imageInfo = await _isoImportService.ImportFromWimAsync(
                    wimFile,
                    "", // Let service generate name from WIM
                    progress,
                    cancellationToken);

                // Refresh the images list
                await RefreshImagesAsync();

                // Select the newly imported image
                SelectedImage = FilteredImages.FirstOrDefault(img => img.Id == imageInfo.Id);
            });

            StatusMessage = "WIM import completed successfully";
        }
        catch (OperationCanceledException)
        {
            Logger.Information("WIM import cancelled by user");
            StatusMessage = "WIM import cancelled";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to import from WIM");
            StatusMessage = $"WIM import failed: {ex.Message}";
            await ShowErrorDialogAsync("Import Error", $"Failed to import from WIM: {ex.Message}");
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

        // TODO: Open image details dialog or page
        StatusMessage = $"Viewing details for {image.Name}";
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
        {            case nameof(SelectedImage):
                // Update command can execute states
                ((AsyncRelayCommand)DeleteSelectedCommand).NotifyCanExecuteChanged();
                ((AsyncRelayCommand)DeleteSelectedFromDiskCommand).NotifyCanExecuteChanged();
                // Update selected image display name
                OnPropertyChanged(nameof(SelectedImageDisplayName));
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
    }    /// <summary>
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

    /// <summary>
    /// Shows an import progress dialog with cancellation support.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="importOperation">The import operation to execute.</param>
    private async Task ShowImportProgressDialogAsync(string title, Func<IProgress<string>, CancellationToken, Task> importOperation)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var progress = new Progress<string>();
        var progressText = "Starting import...";

        var dialog = new ContentDialog
        {
            Title = title,
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        // Create progress content
        var progressPanel = new StackPanel { Spacing = 16, Margin = new Thickness(0, 16, 0, 16) };

        var progressRing = new Microsoft.UI.Xaml.Controls.ProgressRing { IsActive = true, Width = 48, Height = 48 };
        var progressLabel = new TextBlock
        {
            Text = progressText,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        progressPanel.Children.Add(progressRing);
        progressPanel.Children.Add(progressLabel);

        dialog.Content = progressPanel;

        // Update progress text
        progress.ProgressChanged += (s, text) =>
        {
            progressLabel.Text = text;
        };

        // Get XamlRoot from the main window
        if (App.MainWindow?.Content is FrameworkElement element)
        {
            dialog.XamlRoot = element.XamlRoot;
        }

        // Start the import operation
        var importTask = importOperation(progress, cancellationTokenSource.Token);

        // Show dialog and wait for completion or cancellation
        var dialogTask = dialog.ShowAsync().AsTask();

        var completedTask = await Task.WhenAny(importTask, dialogTask);

        if (completedTask == dialogTask)
        {
            // Dialog was closed (cancelled)
            cancellationTokenSource.Cancel();
            try
            {
                await importTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }
            throw new OperationCanceledException("Import operation was cancelled by user");
        }
        else
        {
            // Import completed, close dialog
            dialog.Hide();
            await importTask; // Propagate any exceptions
        }
    }

    #endregion
}
