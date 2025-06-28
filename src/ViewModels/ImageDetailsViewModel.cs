using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bucket.Models;
using Bucket.Services.WindowsImage;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Linq;
using System.Collections.ObjectModel;

namespace Bucket.ViewModels;

/// <summary>
/// ViewModel for the Windows Image Details page.
/// </summary>
public partial class ImageDetailsViewModel : ObservableObject
{
    private readonly IWindowsImageMetadataService _metadataService;
    private readonly IWindowsImageIndexEditingService _indexEditingService;
    private readonly IWindowsImageMountService _mountService;
    private readonly IWindowsImageUnmountService _unmountService;
    private WindowsImageInfo _imageInfo;
    private ObservableCollection<MountedImageInfo> _mountedImages = new();

    /// <summary>
    /// Gets or sets the image information being displayed.
    /// </summary>
    public WindowsImageInfo ImageInfo
    {
        get => _imageInfo;
        set => SetProperty(ref _imageInfo, value);
    }

    /// <summary>
    /// Gets whether the image has a source ISO path.
    /// </summary>
    public bool HasSourceIso => !string.IsNullOrWhiteSpace(ImageInfo?.SourceIsoPath);

    /// <summary>
    /// Gets the collection of currently mounted images for this image file.
    /// </summary>
    public ObservableCollection<MountedImageInfo> MountedImages
    {
        get => _mountedImages;
        set => SetProperty(ref _mountedImages, value);
    }

    /// <summary>
    /// Initializes a new instance of the ImageDetailsViewModel class.
    /// </summary>
    /// <param name="metadataService">The Windows image metadata service.</param>
    /// <param name="indexEditingService">The Windows image index editing service.</param>
    /// <param name="mountService">The Windows image mount service.</param>
    public ImageDetailsViewModel(IWindowsImageMetadataService metadataService, IWindowsImageIndexEditingService indexEditingService, IWindowsImageMountService mountService, IWindowsImageUnmountService unmountService)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _indexEditingService = indexEditingService ?? throw new ArgumentNullException(nameof(indexEditingService));
        _mountService = mountService ?? throw new ArgumentNullException(nameof(mountService));
        _unmountService = unmountService ?? throw new ArgumentNullException(nameof(unmountService));

        // Initialize commands
        EditMetadataCommand = new AsyncRelayCommand(EditMetadata);
        ExportImageCommand = new AsyncRelayCommand(ExportImageAsync);
        SelectAllIndicesCommand = new RelayCommand(SelectAllIndices);
        SelectNoIndicesCommand = new RelayCommand(SelectNoIndices);
        ApplyUpdatesCommand = new AsyncRelayCommand(ApplyUpdatesAsync);
        MountImageCommand = new AsyncRelayCommand(MountImageAsync);
        UnmountImageCommand = new AsyncRelayCommand(UnmountImageAsync);
        OpenMountDirectoryCommand = new AsyncRelayCommand(OpenMountDirectoryAsync);
        DeleteImageCommand = new AsyncRelayCommand(DeleteImageAsync);
        MakeIsoCommand = new AsyncRelayCommand(MakeIsoAsync);
        MergeSWMCommand = new AsyncRelayCommand(MergeSWMAsync);
        RebuildImageCommand = new AsyncRelayCommand(RebuildImageAsync);
        ExtractFilesCommand = new AsyncRelayCommand(ExtractFilesAsync);
        RenameImageCommand = new AsyncRelayCommand(RenameImageAsync);
        EditIndexCommand = new AsyncRelayCommand<WindowsImageIndex>(EditIndex);

        Logger.Debug("ImageDetailsViewModel initialized");
    }

    #region Commands

    /// <summary>
    /// Gets the command to edit image metadata.
    /// </summary>
    public IAsyncRelayCommand EditMetadataCommand { get; }

    /// <summary>
    /// Gets the command to export the image.
    /// </summary>
    public IAsyncRelayCommand ExportImageCommand { get; }

    /// <summary>
    /// Gets the command to select all indices.
    /// </summary>
    public IRelayCommand SelectAllIndicesCommand { get; }

    /// <summary>
    /// Gets the command to select no indices.
    /// </summary>
    public IRelayCommand SelectNoIndicesCommand { get; }

    /// <summary>
    /// Gets the command to apply updates to the image.
    /// </summary>
    public IAsyncRelayCommand ApplyUpdatesCommand { get; }

    /// <summary>
    /// Gets the command to mount the image.
    /// </summary>
    public IAsyncRelayCommand MountImageCommand { get; }

    /// <summary>
    /// Gets the command to unmount the image.
    /// </summary>
    public IAsyncRelayCommand UnmountImageCommand { get; }

    /// <summary>
    /// Gets the command to open the mount directory.
    /// </summary>
    public IAsyncRelayCommand OpenMountDirectoryCommand { get; }

    /// <summary>
    /// Gets the command to delete the image.
    /// </summary>
    public IAsyncRelayCommand DeleteImageCommand { get; }

    /// <summary>
    /// Gets the command to create an ISO from the image.
    /// </summary>
    public IAsyncRelayCommand MakeIsoCommand { get; }

    /// <summary>
    /// Gets the command to merge SWM files.
    /// </summary>
    public IAsyncRelayCommand MergeSWMCommand { get; }

    /// <summary>
    /// Gets the command to rebuild the image.
    /// </summary>
    public IAsyncRelayCommand RebuildImageCommand { get; }

    /// <summary>
    /// Gets the command to extract files from the image.
    /// </summary>
    public IAsyncRelayCommand ExtractFilesCommand { get; }

    /// <summary>
    /// Gets the command to rename the image.
    /// </summary>
    public IAsyncRelayCommand RenameImageCommand { get; }

    /// <summary>
    /// Gets the command to edit an index.
    /// </summary>
    public IAsyncRelayCommand<WindowsImageIndex> EditIndexCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the image information to display.
    /// </summary>
    /// <param name="imageInfo">The image information to display.</param>
    public async void SetImageInfo(WindowsImageInfo imageInfo)
    {
        ImageInfo = imageInfo;
        OnPropertyChanged(nameof(HasSourceIso));
        Logger.Information("Set image info for details view: {Name}", imageInfo?.Name);

        // Refresh mounted images for this image file
        await RefreshMountedImagesAsync();
    }

    /// <summary>
    /// Refreshes the list of mounted images for the current image file.
    /// </summary>
    public async Task RefreshMountedImagesAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            var allMountedImages = await _mountService.GetMountedImagesAsync();
            var currentImageMounts = allMountedImages
                .Where(m => string.Equals(m.ImagePath, ImageInfo.FilePath, StringComparison.OrdinalIgnoreCase))
                .ToList();

            MountedImages.Clear();
            foreach (var mount in currentImageMounts)
            {
                MountedImages.Add(mount);
            }

            Logger.Debug("Refreshed mounted images: {Count} mounts found for {ImagePath}", currentImageMounts.Count, ImageInfo.FilePath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to refresh mounted images for {ImagePath}", ImageInfo?.FilePath);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Opens the metadata editing dialog.
    /// </summary>
    private async Task EditMetadata()
    {
        if (ImageInfo == null) return;

        Logger.Information("Opening metadata editor for image: {Name}", ImageInfo.Name);

        try
        {
            // Show information about available editing options
            await ShowInfoDialogAsync("Edit Metadata",
                "To edit index metadata (names and descriptions), use the edit buttons next to each Windows edition below.\n\n" +
                "Each index can be edited individually to customize the display name and description that appears in Windows setup and deployment tools.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to show metadata editing info for image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Error", $"Failed to show metadata editing options: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports the image to a new location.
    /// </summary>
    private async Task ExportImageAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            // TODO: Implement image export functionality
            await ShowInfoDialogAsync("Export Image",
                "Image export functionality will be available in a future update.");
            Logger.Information("Image export dialog shown for: {Name}", ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to export image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Export Error", $"Failed to export image: {ex.Message}");
        }
    }

    /// <summary>
    /// Selects all Windows indices.
    /// </summary>
    private void SelectAllIndices()
    {
        if (ImageInfo?.Indices == null) return;

        Logger.Debug("Selecting all indices for image: {Name}", ImageInfo.Name);

        foreach (var index in ImageInfo.Indices)
        {
            index.IsIncluded = true;
        }
    }

    /// <summary>
    /// Deselects all Windows indices.
    /// </summary>
    private void SelectNoIndices()
    {
        if (ImageInfo?.Indices == null) return;

        Logger.Debug("Deselecting all indices for image: {Name}", ImageInfo.Name);

        foreach (var index in ImageInfo.Indices)
        {
            index.IsIncluded = false;
        }
    }

    /// <summary>
    /// Applies Windows updates to the selected indices.
    /// </summary>
    private async Task ApplyUpdatesAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            var selectedIndices = ImageInfo.Indices.Where(i => i.IsIncluded).ToList();
            if (selectedIndices.Count == 0)
            {
                await ShowInfoDialogAsync("No Selection",
                    "Please select at least one Windows edition to apply updates to.");
                return;
            }

            // TODO: Implement update application
            await ShowInfoDialogAsync("Apply Updates",
                $"Update application for {selectedIndices.Count} selected edition(s) will be available in a future update.");
            Logger.Information("Update application dialog shown for {Count} selected indices in image: {Name}", 
                selectedIndices.Count, ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to apply updates to image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Update Error", $"Failed to apply updates: {ex.Message}");
        }
    }

    /// <summary>
    /// Mounts the Windows image for exploration.
    /// </summary>
    private async Task MountImageAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            var selectedIndices = ImageInfo.Indices.Where(i => i.IsIncluded).ToList();
            if (selectedIndices.Count == 0)
            {
                await ShowInfoDialogAsync("No Selection",
                    "Please select at least one Windows edition to mount.");
                return;
            }

            if (selectedIndices.Count > 1)
            {
                await ShowInfoDialogAsync("Multiple Selection",
                    "Please select only one Windows edition to mount at a time.");
                return;
            }

            var selectedIndex = selectedIndices.First();

            // Check if already mounted
            if (await _mountService.IsImageMountedAsync(ImageInfo.FilePath, selectedIndex.Index))
            {
                await ShowInfoDialogAsync("Already Mounted",
                    $"Index {selectedIndex.Index} ({selectedIndex.Name}) is already mounted.");
                return;
            }

            await ShowEditProgressDialogAsync("Mount Image", async (progress, cancellationToken) =>
            {
                var mountedImage = await _mountService.MountImageAsync(
                    ImageInfo.FilePath,
                    selectedIndex.Index,
                    ImageInfo.Name,
                    selectedIndex.Name,
                    progress,
                    cancellationToken);

                // Refresh mounted images list
                await RefreshMountedImagesAsync();

                progress?.Report($"Successfully mounted to: {mountedImage.MountPath}");
                Logger.Information("Successfully mounted image: {ImagePath}, Index: {Index}", ImageInfo.FilePath, selectedIndex.Index);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to mount image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Mount Error", $"Failed to mount image: {ex.Message}");
        }
    }

    /// <summary>
    /// Unmounts the selected mounted image.
    /// </summary>
    private async Task UnmountImageAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            var mountedImages = MountedImages.ToList();
            if (mountedImages.Count == 0)
            {
                await ShowInfoDialogAsync("No Mounted Images",
                    "There are no mounted images for this WIM file.");
                return;
            }

            MountedImageInfo selectedMount = null;
            if (mountedImages.Count == 1)
            {
                selectedMount = mountedImages.First();
            }
            else
            {
                // Show selection dialog for multiple mounts
                var selectDialog = new Views.Dialogs.SelectMountDialog(mountedImages);
                
                // Get XamlRoot from the main window
                if (App.MainWindow?.Content is FrameworkElement element)
                {
                    selectDialog.XamlRoot = element.XamlRoot;
                }
                
                var result = await selectDialog.ShowAsync();
                
                if (result != ContentDialogResult.Primary || selectDialog.SelectedMount == null)
                {
                    Logger.Debug("User cancelled mount selection or no mount selected");
                    return;
                }
                
                selectedMount = selectDialog.SelectedMount;
                Logger.Information("User selected mount for unmounting: Index {Index}, Path: {MountPath}", 
                    selectedMount.Index, selectedMount.MountPath);
            }

            await ShowEditProgressDialogAsync("Unmount Image", async (progress, cancellationToken) =>
            {
                await _unmountService.UnmountImageAsync(selectedMount, true, progress, cancellationToken);

                // Refresh mounted images list
                await RefreshMountedImagesAsync();

                progress?.Report("Successfully unmounted image");
                Logger.Information("Successfully unmounted image: {ImagePath}, Index: {Index}", selectedMount.ImagePath, selectedMount.Index);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to unmount image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Unmount Error", $"Failed to unmount image: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens the mount directory for the mounted image.
    /// </summary>
    private async Task OpenMountDirectoryAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            var mountedImages = MountedImages.ToList();
            if (mountedImages.Count == 0)
            {
                await ShowInfoDialogAsync("No Mounted Images",
                    "There are no mounted images for this WIM file.");
                return;
            }

            // If only one mount, open it directly
            if (mountedImages.Count == 1)
            {
                await _mountService.OpenMountDirectoryAsync(mountedImages.First());
                Logger.Information("Opened mount directory for: {MountPath}", mountedImages.First().MountPath);
            }
            else
            {
                // TODO: Show selection dialog for multiple mounts
                await ShowInfoDialogAsync("Multiple Mounts",
                    $"Multiple images are mounted:\n\n{string.Join("\n", mountedImages.Select(m => $"Index {m.Index}: {m.MountPath}"))}\n\nPlease navigate to the desired mount directory manually.");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open mount directory for image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Open Directory Error", $"Failed to open mount directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes the Windows image.
    /// </summary>
    private async Task DeleteImageAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            // TODO: Implement image deletion
            await ShowInfoDialogAsync("Delete Image",
                "Image deletion functionality will be available in a future update.");
            Logger.Information("Image deletion dialog shown for: {Name}", ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to delete image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Delete Error", $"Failed to delete image: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates an ISO from the Windows image.
    /// </summary>
    private async Task MakeIsoAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            // TODO: Implement ISO creation
            await ShowInfoDialogAsync("Make ISO",
                "ISO creation functionality will be available in a future update.");
            Logger.Information("ISO creation dialog shown for: {Name}", ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to create ISO from image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("ISO Creation Error", $"Failed to create ISO: {ex.Message}");
        }
    }

    /// <summary>
    /// Merges SWM files into a single WIM.
    /// </summary>
    private async Task MergeSWMAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            // TODO: Implement SWM merging
            await ShowInfoDialogAsync("Merge SWM",
                "SWM merging functionality will be available in a future update.");
            Logger.Information("SWM merging dialog shown for: {Name}", ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to merge SWM files for image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("SWM Merge Error", $"Failed to merge SWM files: {ex.Message}");
        }
    }

    /// <summary>
    /// Rebuilds the Windows image with maximum compression.
    /// </summary>
    private async Task RebuildImageAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            // TODO: Implement image rebuilding
            await ShowInfoDialogAsync("Rebuild Image",
                "Image rebuilding functionality will be available in a future update.");
            Logger.Information("Image rebuilding dialog shown for: {Name}", ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to rebuild image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Rebuild Error", $"Failed to rebuild image: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts files from the Windows image.
    /// </summary>
    private async Task ExtractFilesAsync()
    {
        if (ImageInfo == null) return;

        try
        {
            // TODO: Implement file extraction
            await ShowInfoDialogAsync("Extract Files",
                "File extraction functionality will be available in a future update.");
            Logger.Information("File extraction dialog shown for: {Name}", ImageInfo.Name);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to extract files from image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Extraction Error", $"Failed to extract files: {ex.Message}");
        }
    }    /// <summary>
    /// Renames the Windows image.
    /// </summary>
    private async Task RenameImageAsync()
    {
        if (ImageInfo == null) return;

        Logger.Information("Opening rename dialog for image: {Name}", ImageInfo.Name);

        try
        {
            var dialog = new Views.Dialogs.RenameImageDialog(ImageInfo);

            // Get XamlRoot from the main window
            if (App.MainWindow?.Content is FrameworkElement element)
            {
                dialog.XamlRoot = element.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var success = dialog.UpdateImageInfo(ImageInfo);

                if (success)
                {
                    // Save the updated image metadata to images.json
                    try
                    {
                        await _metadataService.UpdateImageAsync(ImageInfo);
                        Logger.Information("Image metadata updated in images.json for '{Name}'", ImageInfo.Name);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to update image metadata in images.json for '{Name}'", ImageInfo.Name);
                        await ShowErrorDialogAsync("Update Failed",
                            "The image was renamed successfully, but failed to update the metadata file. You may need to restart the application.");
                        return;
                    }

                    Logger.Information("Image successfully renamed to '{NewName}'", ImageInfo.Name);

                    // Notify that properties may have changed
                    OnPropertyChanged(nameof(ImageInfo));
                }
                else
                {
                    await ShowErrorDialogAsync("Rename Failed", "Failed to rename the image. The target file may already exist or be in use.");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to rename image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Rename Error", $"Failed to rename image: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens the edit dialog for a Windows image index.
    /// </summary>
    /// <param name="imageIndex">The image index to edit.</param>
    private async Task EditIndex(WindowsImageIndex imageIndex)
    {
        if (ImageInfo == null || imageIndex == null) return;

        Logger.Information("Opening edit dialog for index {Index}: {Name}", imageIndex.Index, imageIndex.Name);

        var oldName = imageIndex.Name;
        var oldDescription = imageIndex.Description;

        try
        {
            var dialog = new Views.Dialogs.EditIndexDialog(imageIndex);

            // Get XamlRoot from the main window
            if (App.MainWindow?.Content is FrameworkElement element)
            {
                dialog.XamlRoot = element.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Logger.Information("Dialog confirmed for index {Index}. Before update: Name='{OldName}', Description='{OldDescription}'",
                    imageIndex.Index, oldName, oldDescription);

                // Update the in-memory model first
                dialog.UpdateImageIndex(imageIndex);

                Logger.Information("After dialog update for index {Index}: Name='{NewName}', Description='{NewDescription}'",
                    imageIndex.Index, imageIndex.Name, imageIndex.Description);

                // CRITICAL: Update the actual object in ImageInfo.Indices collection
                var originalIndex = ImageInfo.Indices?.FirstOrDefault(i => i.Index == imageIndex.Index);
                if (originalIndex != null)
                {
                    originalIndex.Name = imageIndex.Name;
                    originalIndex.Description = imageIndex.Description;
                    Logger.Information("Updated original index object in ImageInfo.Indices: Index {Index}, Name='{Name}', Description='{Description}'",
                        originalIndex.Index, originalIndex.Name, originalIndex.Description);
                }
                else
                {
                    Logger.Error("Could not find original index {Index} in ImageInfo.Indices collection", imageIndex.Index);
                }

                // Check if we need to update the physical WIM file
                var nameChanged = oldName != imageIndex.Name;
                var descriptionChanged = oldDescription != imageIndex.Description;

                Logger.Information("Change detection for index {Index}: nameChanged={NameChanged}, descriptionChanged={DescriptionChanged}",
                    imageIndex.Index, nameChanged, descriptionChanged);

                if (nameChanged || descriptionChanged)
                {
                    Logger.Information("Updating WIM file metadata for index {Index}: Name={NameChanged}, Description={DescriptionChanged}",
                        imageIndex.Index, nameChanged, descriptionChanged);

                    // Show progress dialog while updating WIM metadata
                    await ShowEditProgressDialogAsync("Updating WIM Metadata", async (progress, cancellationToken) =>
                    {
                        progress.Report("Validating WIM file access...");

                        // Validate WIM file accessibility
                        if (!_indexEditingService.IsWimFileAccessible(ImageInfo.FilePath))
                        {
                            throw new InvalidOperationException("The WIM file is currently in use by another process or cannot be accessed. Please close any applications that might be using the file and try again.");
                        }

                        progress.Report("Updating WIM file metadata...");

                        // Update the physical WIM file
                        var wimUpdateSuccess = false;
                        try
                        {
                            if (nameChanged && descriptionChanged)
                            {
                                wimUpdateSuccess = await _indexEditingService.UpdateIndexMetadataAsync(
                                    ImageInfo.FilePath, imageIndex.Index, oldName, imageIndex.Name,
                                    oldDescription, imageIndex.Description, cancellationToken);
                            }
                            else if (nameChanged)
                            {
                                wimUpdateSuccess = await _indexEditingService.UpdateIndexNameAsync(
                                    ImageInfo.FilePath, imageIndex.Index, oldName, imageIndex.Name, cancellationToken);
                            }
                            else if (descriptionChanged)
                            {
                                wimUpdateSuccess = await _indexEditingService.UpdateIndexDescriptionAsync(
                                    ImageInfo.FilePath, imageIndex.Index, oldDescription, imageIndex.Description, cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to update WIM file metadata for index {Index}", imageIndex.Index);
                            throw new InvalidOperationException($"Failed to update WIM file metadata: {ex.Message}");
                        }

                        if (!wimUpdateSuccess)
                        {
                            throw new InvalidOperationException("Failed to update the WIM file metadata.");
                        }

                        progress.Report("Saving metadata changes...");

                        Logger.Information("About to save image metadata. ImageInfo.Id={ImageId}, Index {Index} current state: Name='{Name}', Description='{Description}'",
                            ImageInfo.Id, imageIndex.Index, imageIndex.Name, imageIndex.Description);

                        // Save the updated image metadata to images.json
                        try
                        {
                            await _metadataService.UpdateImageAsync(ImageInfo);
                            Logger.Information("Index {Index} updated and metadata saved: Name '{OldName}' → '{NewName}', Description changed: {DescriptionChanged}",
                                imageIndex.Index, oldName, imageIndex.Name, oldDescription != imageIndex.Description);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to update image metadata in images.json after editing index {Index}", imageIndex.Index);
                            throw new InvalidOperationException("The WIM file was updated successfully, but failed to save changes to the metadata file. You may need to restart the application to see the changes reflected in the interface.");
                        }

                        progress.Report("Update completed successfully!");

                        Logger.Information("Successfully updated WIM file metadata for index {Index}", imageIndex.Index);
                    });

                    Logger.Information("Index {Index} editing completed successfully", imageIndex.Index);

                    // Refresh the current page data to show updated information
                    await RefreshImageDataAsync();
                }
                else
                {
                    // No WIM changes needed, just save to metadata file
                    try
                    {
                        await _metadataService.UpdateImageAsync(ImageInfo);
                        Logger.Information("Index {Index} metadata saved (no WIM changes needed)", imageIndex.Index);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to update image metadata in images.json for index {Index}", imageIndex.Index);
                        await ShowErrorDialogAsync("Metadata Save Failed",
                            "Failed to save changes to the metadata file. You may need to restart the application to see the changes reflected in the interface.");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Information("Index editing was cancelled by user");
            // Revert changes in memory
            imageIndex.Name = oldName;
            imageIndex.Description = oldDescription;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to edit index {Index}: {Name}", imageIndex.Index, imageIndex.Name);
            await ShowErrorDialogAsync("Edit Error", $"Failed to edit index: {ex.Message}");

            // Revert changes in memory on error
            imageIndex.Name = oldName;
            imageIndex.Description = oldDescription;
        }
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
    /// Shows an edit progress dialog with cancellation support.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="editOperation">The edit operation to execute.</param>
    private async Task ShowEditProgressDialogAsync(string title, Func<IProgress<string>, CancellationToken, Task> editOperation)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var progress = new Progress<string>();
        var progressText = "Starting update...";

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

        // Start the edit operation
        var editTask = editOperation(progress, cancellationTokenSource.Token);

        // Show dialog and wait for completion or cancellation
        var dialogTask = dialog.ShowAsync().AsTask();

        var completedTask = await Task.WhenAny(editTask, dialogTask);

        if (completedTask == dialogTask)
        {
            // Dialog was closed (cancelled)
            cancellationTokenSource.Cancel();
            try
            {
                await editTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }
            throw new OperationCanceledException("Edit operation was cancelled by user");
        }
        else
        {
            // Edit completed, close dialog
            dialog.Hide();
            await editTask; // Propagate any exceptions
        }
    }

    /// <summary>
    /// Refreshes the current image data to reflect any changes made.
    /// </summary>
    private async Task RefreshImageDataAsync()
    {
        if (ImageInfo == null)
        {
            Logger.Warning("Cannot refresh image data: ImageInfo is null");
            return;
        }

        try
        {
            Logger.Information("Refreshing image data for: {Name}", ImageInfo.Name);

            // Get the updated image data from the metadata service
            var allImages = await _metadataService.GetImagesAsync();
            var updatedImage = allImages.FirstOrDefault(img => 
                string.Equals(img.FilePath, ImageInfo.FilePath, StringComparison.OrdinalIgnoreCase));

            if (updatedImage != null)
            {
                // Update the current ImageInfo with the refreshed data
                ImageInfo = updatedImage;
                OnPropertyChanged(nameof(HasSourceIso));
                
                Logger.Information("Successfully refreshed image data for: {Name}", ImageInfo.Name);
            }
            else
            {
                Logger.Warning("Could not find updated image data for: {Name}", ImageInfo.Name);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to refresh image data for: {Name}", ImageInfo?.Name);
            // Don't show error dialog here as it might interrupt the user flow
            // The existing data will remain displayed
        }
    }



    #endregion
}
