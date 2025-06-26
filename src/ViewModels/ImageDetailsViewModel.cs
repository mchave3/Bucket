using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bucket.Models;
using Bucket.Services.WindowsImage;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace Bucket.ViewModels;

/// <summary>
/// ViewModel for the Windows Image Details page.
/// </summary>
public partial class ImageDetailsViewModel : ObservableObject
{
    private readonly IWindowsImageMetadataService _metadataService;
    private WindowsImageInfo _imageInfo;

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
    /// Initializes a new instance of the ImageDetailsViewModel class.
    /// </summary>
    /// <param name="metadataService">The Windows image metadata service.</param>
    public ImageDetailsViewModel(IWindowsImageMetadataService metadataService)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));

        // Initialize commands
        EditMetadataCommand = new RelayCommand(EditMetadata);
        ExportImageCommand = new AsyncRelayCommand(ExportImageAsync);
        SelectAllIndicesCommand = new RelayCommand(SelectAllIndices);
        SelectNoIndicesCommand = new RelayCommand(SelectNoIndices);
        ApplyUpdatesCommand = new AsyncRelayCommand(ApplyUpdatesAsync);
        MountImageCommand = new AsyncRelayCommand(MountImageAsync);
        ExtractFilesCommand = new AsyncRelayCommand(ExtractFilesAsync);
        RenameImageCommand = new AsyncRelayCommand(RenameImageAsync);
        EditIndexCommand = new RelayCommand<WindowsImageIndex>(EditIndex);

        Logger.Information("ImageDetailsViewModel initialized");
    }

    #region Commands

    /// <summary>
    /// Gets the command to edit image metadata.
    /// </summary>
    public ICommand EditMetadataCommand { get; }

    /// <summary>
    /// Gets the command to export the image.
    /// </summary>
    public ICommand ExportImageCommand { get; }

    /// <summary>
    /// Gets the command to select all indices.
    /// </summary>
    public ICommand SelectAllIndicesCommand { get; }

    /// <summary>
    /// Gets the command to select no indices.
    /// </summary>
    public ICommand SelectNoIndicesCommand { get; }

    /// <summary>
    /// Gets the command to apply updates to the image.
    /// </summary>
    public ICommand ApplyUpdatesCommand { get; }

    /// <summary>
    /// Gets the command to mount the image.
    /// </summary>
    public ICommand MountImageCommand { get; }

    /// <summary>
    /// Gets the command to extract files from the image.
    /// </summary>
    public ICommand ExtractFilesCommand { get; }

    /// <summary>
    /// Gets the command to rename the image.
    /// </summary>
    public ICommand RenameImageCommand { get; }

    /// <summary>
    /// Gets the command to edit an index.
    /// </summary>
    public ICommand EditIndexCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the image information to display.
    /// </summary>
    /// <param name="imageInfo">The image information to display.</param>
    public void SetImageInfo(WindowsImageInfo imageInfo)
    {
        ImageInfo = imageInfo;
        OnPropertyChanged(nameof(HasSourceIso));
        Logger.Information("Set image info for details view: {Name}", imageInfo?.Name);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Opens the metadata editing dialog.
    /// </summary>
    private void EditMetadata()
    {
        if (ImageInfo == null) return;

        Logger.Information("Opening metadata editor for image: {Name}", ImageInfo.Name);

        // TODO: Implement metadata editing dialog
        _ = ShowInfoDialogAsync("Edit Metadata",
            "Metadata editing functionality will be available in a future update.");
    }

    /// <summary>
    /// Exports the image to a new location.
    /// </summary>
    private async Task ExportImageAsync()
    {
        if (ImageInfo == null) return;

        Logger.Information("Starting image export for: {Name}", ImageInfo.Name);

        try
        {
            // TODO: Implement image export functionality
            await ShowInfoDialogAsync("Export Image",
                "Image export functionality will be available in a future update.");
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

        Logger.Information("Selecting all indices for image: {Name}", ImageInfo.Name);

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

        Logger.Information("Deselecting all indices for image: {Name}", ImageInfo.Name);

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

        Logger.Information("Starting update application for image: {Name}", ImageInfo.Name);

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

        Logger.Information("Starting image mount for: {Name}", ImageInfo.Name);

        try
        {
            // TODO: Implement image mounting
            await ShowInfoDialogAsync("Mount Image",
                "Image mounting functionality will be available in a future update.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to mount image: {Name}", ImageInfo.Name);
            await ShowErrorDialogAsync("Mount Error", $"Failed to mount image: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts files from the Windows image.
    /// </summary>
    private async Task ExtractFilesAsync()
    {
        if (ImageInfo == null) return;

        Logger.Information("Starting file extraction for image: {Name}", ImageInfo.Name);

        try
        {
            // TODO: Implement file extraction
            await ShowInfoDialogAsync("Extract Files",
                "File extraction functionality will be available in a future update.");
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
    private async void EditIndex(WindowsImageIndex imageIndex)
    {
        if (ImageInfo == null || imageIndex == null) return;

        Logger.Information("Opening edit dialog for index {Index}: {Name}", imageIndex.Index, imageIndex.Name);

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
                var oldName = imageIndex.Name;
                var oldDescription = imageIndex.Description;

                dialog.UpdateImageIndex(imageIndex);

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
                    await ShowErrorDialogAsync("Update Failed",
                        "The index was updated successfully, but failed to save changes to the metadata file. You may need to restart the application.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to edit index {Index}: {Name}", imageIndex.Index, imageIndex.Name);
            await ShowErrorDialogAsync("Edit Error", $"Failed to edit index: {ex.Message}");
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

    #endregion
}
