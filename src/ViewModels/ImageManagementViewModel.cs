using System.Collections.ObjectModel;
using System.ComponentModel;
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
        set
        {
            Logger.Information("SelectedImage changing from {OldValue} to {NewValue}",
                _selectedImage?.Name ?? "null", value?.Name ?? "null");
            SetProperty(ref _selectedImage, value);
            Logger.Information("SelectedImage changed. Current value: {CurrentValue}",
                _selectedImage?.Name ?? "null");
        }
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
        set
        {
            SetProperty(ref _hasImages, value);
            OnPropertyChanged(nameof(ShowEmptyState));
            OnPropertyChanged(nameof(ShowImagesList));
        }
    }

    /// <summary>
    /// Gets or sets the search text for filtering images.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterImages();
            }
        }
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
    /// Gets whether the empty state should be shown (no images).
    /// </summary>
    public bool ShowEmptyState => !HasImages;

    /// <summary>
    /// Gets whether the images list should be shown (has images).
    /// </summary>
    public bool ShowImagesList => HasImages;

    /// <summary>
    /// Initializes a new instance of the ImageManagementViewModel class.
    /// </summary>
    /// <param name="windowsImageService">The Windows image service for handling image operations.</param>
    public ImageManagementViewModel(WindowsImageService windowsImageService)
    {
        _windowsImageService = windowsImageService ?? throw new ArgumentNullException(nameof(windowsImageService));

        // Initialize commands
        RefreshCommand = new AsyncRelayCommand(RefreshImagesAsync);
        ImportFromIsoCommand = new AsyncRelayCommand(ImportFromIsoAsync);
        ImportFromWimCommand = new AsyncRelayCommand(ImportFromWimAsync);
        DeleteSelectedCommand = new AsyncRelayCommand(DeleteSelectedImageAsync, CanDeleteSelected);
        DeleteSelectedFromDiskCommand = new AsyncRelayCommand(DeleteSelectedImageFromDiskAsync, CanDeleteSelected);
        ViewImageDetailsCommand = new RelayCommand<WindowsImageInfo>(ViewImageDetails);
        LoadIndexDetailsCommand = new AsyncRelayCommand<WindowsImageIndex>(LoadIndexDetailsAsync);

        // Move commands for image reordering
        MoveImageUpCommand = new AsyncRelayCommand(MoveImageUp, CanMoveUp);
        MoveImageDownCommand = new AsyncRelayCommand(MoveImageDown, CanMoveDown);

        // Placeholder commands for detail panel
        ExtractSelectedIndicesCommand = new RelayCommand(() => { /* TODO: Implement */ });
        MountImageCommand = new RelayCommand(() => { /* TODO: Implement */ });
        ValidateImageCommand = new RelayCommand(() => { /* TODO: Implement */ });

        // Watch for property changes
        PropertyChanged += OnPropertyChanged;

        Logger.Information("ImageManagementViewModel initialized with injected WindowsImageService");
    }

    #region Commands

    /// <summary>
    /// Gets the command to refresh the images list.
    /// </summary>
    public IAsyncRelayCommand RefreshCommand { get; }

    /// <summary>
    /// Gets the command to import images from an ISO file.
    /// </summary>
    public IAsyncRelayCommand ImportFromIsoCommand { get; }

    /// <summary>
    /// Gets the command to import a WIM file directly.
    /// </summary>
    public IAsyncRelayCommand ImportFromWimCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected image from the list.
    /// </summary>
    public IAsyncRelayCommand DeleteSelectedCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected image from both list and disk.
    /// </summary>
    public IAsyncRelayCommand DeleteSelectedFromDiskCommand { get; }    /// <summary>
    /// Gets the command to view detailed information about an image.
    /// </summary>
    public IRelayCommand<WindowsImageInfo> ViewImageDetailsCommand { get; }

    /// <summary>
    /// Gets the command to extract selected indices from an image.
    /// </summary>
    public IRelayCommand ExtractSelectedIndicesCommand { get; }

    /// <summary>
    /// Gets the command to mount an image.
    /// </summary>
    public IRelayCommand MountImageCommand { get; }

    /// <summary>
    /// Gets the command to validate an image's integrity.
    /// </summary>
    public IRelayCommand ValidateImageCommand { get; }

    /// <summary>
    /// Gets the command to load detailed information for a specific image index.
    /// </summary>
    public IAsyncRelayCommand<WindowsImageIndex> LoadIndexDetailsCommand { get; }

    /// <summary>
    /// Gets the command to move the selected image up in the list.
    /// </summary>
    public IAsyncRelayCommand MoveImageUpCommand { get; }

    /// <summary>
    /// Gets the command to move the selected image down in the list.
    /// </summary>
    public IAsyncRelayCommand MoveImageDownCommand { get; }

    /// <summary>
    /// Gets whether the selected image can be moved up in the list.
    /// </summary>
    public bool CanMoveImageUp => SelectedImage != null && Images.IndexOf(SelectedImage) > 0;

    /// <summary>
    /// Gets whether the selected image can be moved down in the list.
    /// </summary>
    public bool CanMoveImageDown => SelectedImage != null && Images.IndexOf(SelectedImage) < Images.Count - 1;

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
    }

    /// <summary>
    /// Imports images from an ISO file.
    /// </summary>
    private async Task ImportFromIsoAsync()
    {
        Logger.Information("Starting ISO import process");

        try
        {
            // Use FilePicker to select a single ISO file
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

            // Check if a file was selected
            if (file != null)
            {
                StatusMessage = $"Analyzing {file.Name}...";
                Logger.Information("User selected ISO file: {FilePath}", file.Path);

                try
                {
                    // Import the image using the ISO import service
                    await ShowImportProgressDialogAsync("Importing from ISO", async (progress, cancellationToken) =>
                    {
                        var importedImage = await _windowsImageService.ImportFromIsoAsync(
                            file,
                            customName: "",
                            progress: progress,
                            cancellationToken: cancellationToken);
                    });

                    // Refresh the images list
                    await RefreshImagesAsync();

                    StatusMessage = $"Successfully imported from {file.Name}";
                    Logger.Information("Successfully imported ISO: {Name}", file.Name);

                    await ShowInfoDialogAsync("Import Successful",
                        $"Successfully imported Windows image(s) from '{file.Name}'.");
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = "ISO import cancelled";
                    Logger.Information("ISO import was cancelled by user");
                }
                catch (Exception importEx)
                {
                    Logger.Error(importEx, "Failed to import ISO: {FilePath}", file.Path);
                    StatusMessage = $"Failed to import {file.Name}";
                    await ShowErrorDialogAsync("Import Failed",
                        $"Failed to import '{file.Name}':\n\n{importEx.Message}");
                }
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
    /// Imports a WIM or ESD file.
    /// </summary>
    private async Task ImportFromWimAsync()
    {
        Logger.Information("Starting WIM import process");

        try
        {
            // Use FilePicker to select a WIM or ESD file
            var picker = new FilePicker(WindowNative.GetWindowHandle(App.MainWindow))
            {
                FileTypeChoices = new Dictionary<string, IList<string>>
                {
                    { "WIM Files", new List<string> { "*.wim" } },
                    { "ESD Files", new List<string> { "*.esd" } }
                },
                DefaultFileExtension = "WIM Files",
                Title = "Select a WIM/ESD file to import",
                ShowAllFilesOption = false
            };
            var file = await picker.PickSingleFileAsync();

            // Check if a file was selected
            if (file != null)
            {
                StatusMessage = $"Analyzing {file.Name}...";
                Logger.Information("User selected file: {FilePath}", file.Path);

                try
                {
                    // Import the image using the WIM import service with progress dialog
                    WindowsImageInfo importedImage = null;
                    await ShowImportProgressDialogAsync("Importing WIM/ESD", async (progress, cancellationToken) =>
                    {
                        // Import the image using the service
                        importedImage = await _windowsImageService.ImportFromWimAsync(
                            file,
                            customName: "",
                            progress: progress,
                            cancellationToken: cancellationToken);
                    });

                    // Refresh the images list
                    await RefreshImagesAsync();

                    StatusMessage = $"Successfully imported {importedImage.Name}";
                    Logger.Information("Successfully imported file: {Name} with {IndexCount} indices",
                        importedImage.Name, importedImage.IndexCount);

                    await ShowInfoDialogAsync("Import Successful",
                        $"Successfully imported '{importedImage.Name}' with {importedImage.IndexCount} Windows editions.");
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = "WIM/ESD import cancelled";
                    Logger.Information("WIM/ESD import was cancelled by user");
                }
                catch (Exception importEx)
                {
                    Logger.Error(importEx, "Failed to import file: {FilePath}", file.Path);
                    StatusMessage = $"Failed to import {file.Name}";
                    await ShowErrorDialogAsync("Import Failed",
                        $"Failed to import '{file.Name}':\n\n{importEx.Message}");
                }
            }
            else
            {
                StatusMessage = "WIM/ESD import cancelled";
                Logger.Information("User cancelled WIM/ESD file selection");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start WIM/ESD import");
            await ShowErrorDialogAsync("Import Error", $"Failed to start WIM/ESD import: {ex.Message}");
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
    /// Checks if the selected image can be moved up.
    /// </summary>
    /// <returns>True if the image can be moved up, false otherwise.</returns>
    private bool CanMoveUp()
    {
        return CanMoveImageUp;
    }

    /// <summary>
    /// Checks if the selected image can be moved down.
    /// </summary>
    /// <returns>True if the image can be moved down, false otherwise.</returns>
    private bool CanMoveDown()
    {
        return CanMoveImageDown;
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
    /// Loads detailed information for a specific Windows image index.
    /// </summary>
    /// <param name="index">The Windows image index to load details for.</param>
    private async Task LoadIndexDetailsAsync(WindowsImageIndex index)
    {
        if (index == null || SelectedImage == null)
        {
            Logger.Warning("Cannot load index details: index or selected image is null");
            return;
        }

        try
        {
            Logger.Information("Loading detailed information for index {Index} in {ImageName}", index.Index, SelectedImage.Name);
            StatusMessage = $"Loading detailed information for {index.Name}...";

            // Get detailed information for the specific index
            var detailedIndex = await _windowsImageService.GetImageIndexDetailsAsync(
                SelectedImage.FilePath,
                index.Index,
                new Progress<string>(message => StatusMessage = message)
            );

            if (detailedIndex != null)
            {
                // Find the matching index in the selected image and update it with detailed info
                var existingIndex = SelectedImage.Indices.FirstOrDefault(i => i.Index == index.Index);
                if (existingIndex != null)
                {
                    // Update the existing index with detailed information
                    var indexToUpdate = SelectedImage.Indices.IndexOf(existingIndex);
                    if (indexToUpdate >= 0)
                    {
                        SelectedImage.Indices[indexToUpdate] = detailedIndex;
                        Logger.Information("Updated index {Index} with detailed information", index.Index);
                    }
                }
            }

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load detailed information for index {Index}", index.Index);
            StatusMessage = $"Error loading details: {ex.Message}";
        }
    }

    /// <summary>
    /// Moves the selected image up in the list.
    /// </summary>
    private async Task MoveImageUp()
    {
        if (SelectedImage == null) return;

        var currentIndex = Images.IndexOf(SelectedImage);
        if (currentIndex > 0)
        {
            var imageName = SelectedImage.Name ?? "Unnamed Image";
            Logger.Information("Moving image '{ImageName}' up from position {From} to {To}",
                imageName, currentIndex, currentIndex - 1);

            // Move in the main collection
            Images.Move(currentIndex, currentIndex - 1);

            // Update the filtered collection to maintain consistency
            var filteredIndex = FilteredImages.IndexOf(SelectedImage);
            if (filteredIndex >= 0 && filteredIndex > 0)
            {
                FilteredImages.Move(filteredIndex, filteredIndex - 1);
            }

            // Update command states
            UpdateMoveCommandStates();

            // Save the new order to file
            try
            {
                await _windowsImageService.SaveImagesOrderAsync(Images.ToList());
                StatusMessage = $"Moved '{imageName}' up";
                Logger.Information("Successfully saved new order after moving image up");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save image order after moving up");
                StatusMessage = $"Moved '{imageName}' up (order not saved)";
            }
        }
    }

    /// <summary>
    /// Moves the selected image down in the list.
    /// </summary>
    private async Task MoveImageDown()
    {
        if (SelectedImage == null) return;

        var currentIndex = Images.IndexOf(SelectedImage);
        if (currentIndex < Images.Count - 1)
        {
            var imageName = SelectedImage.Name ?? "Unnamed Image";
            Logger.Information("Moving image '{ImageName}' down from position {From} to {To}",
                imageName, currentIndex, currentIndex + 1);

            // Move in the main collection
            Images.Move(currentIndex, currentIndex + 1);

            // Update the filtered collection to maintain consistency
            var filteredIndex = FilteredImages.IndexOf(SelectedImage);
            if (filteredIndex >= 0 && filteredIndex < FilteredImages.Count - 1)
            {
                FilteredImages.Move(filteredIndex, filteredIndex + 1);
            }

            // Update command states
            UpdateMoveCommandStates();

            // Save the new order to file
            try
            {
                await _windowsImageService.SaveImagesOrderAsync(Images.ToList());
                StatusMessage = $"Moved '{imageName}' down";
                Logger.Information("Successfully saved new order after moving image down");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save image order after moving down");
                StatusMessage = $"Moved '{imageName}' down (order not saved)";
            }
        }
    }

    /// <summary>
    /// Updates the move command states based on the current selection.
    /// </summary>
    private void UpdateMoveCommandStates()
    {
        OnPropertyChanged(nameof(CanMoveImageUp));
        OnPropertyChanged(nameof(CanMoveImageDown));
        ((IAsyncRelayCommand)MoveImageUpCommand).NotifyCanExecuteChanged();
        ((IAsyncRelayCommand)MoveImageDownCommand).NotifyCanExecuteChanged();
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
        Logger.Debug("Property changed: {PropertyName}", e.PropertyName);

        switch (e.PropertyName)
        {
            case nameof(SelectedImage):
                Logger.Information("SelectedImage property changed - updating related properties");
                // Update command can execute states
                ((AsyncRelayCommand)DeleteSelectedCommand).NotifyCanExecuteChanged();
                ((AsyncRelayCommand)DeleteSelectedFromDiskCommand).NotifyCanExecuteChanged();
                // Update move command states
                UpdateMoveCommandStates();
                // Update selected image display name
                OnPropertyChanged(nameof(SelectedImageDisplayName));
                Logger.Information("SelectedImageDisplayName updated to: {DisplayName}", SelectedImageDisplayName);
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
