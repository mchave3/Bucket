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
    private readonly IWindowsImageIndexEditingService _indexEditingService;
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
    /// <param name="indexEditingService">The Windows image index editing service.</param>
    public ImageDetailsViewModel(IWindowsImageMetadataService metadataService, IWindowsImageIndexEditingService indexEditingService)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _indexEditingService = indexEditingService ?? throw new ArgumentNullException(nameof(indexEditingService));

        // Initialize commands
        EditMetadataCommand = new AsyncRelayCommand(EditMetadata);
        ExportImageCommand = new AsyncRelayCommand(ExportImageAsync);
        SelectAllIndicesCommand = new RelayCommand(SelectAllIndices);
        SelectNoIndicesCommand = new RelayCommand(SelectNoIndices);
        ApplyUpdatesCommand = new AsyncRelayCommand(ApplyUpdatesAsync);
        MountImageCommand = new AsyncRelayCommand(MountImageAsync);
        ExtractFilesCommand = new AsyncRelayCommand(ExtractFilesAsync);
        RenameImageCommand = new AsyncRelayCommand(RenameImageAsync);
        EditIndexCommand = new AsyncRelayCommand<WindowsImageIndex>(EditIndex);

        Logger.Information("ImageDetailsViewModel initialized");
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

                    // Navigate back to Image Management page to show updated data
                    NavigateToImageManagement();
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
    /// Navigates back to the Image Management page.
    /// </summary>
    private void NavigateToImageManagement()
    {
        try
        {
            // Get the main window's navigation frame
            if (App.MainWindow?.Content is FrameworkElement mainContent)
            {
                // Find the NavigationView in the MainWindow
                var navView = FindChild<NavigationView>(mainContent);
                if (navView?.Content is Frame navFrame)
                {
                    // Navigate to ImageManagementPage
                    navFrame.Navigate(typeof(Views.ImageManagementPage));
                    Logger.Information("Navigated back to Image Management page after successful edit");
                    return;
                }
            }

            Logger.Warning("Could not find navigation frame to navigate back to Image Management page");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error navigating back to Image Management page");
        }
    }

    /// <summary>
    /// Finds a child control of the specified type in the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of control to find.</typeparam>
    /// <param name="parent">The parent element to search from.</param>
    /// <returns>The found control or null if not found.</returns>
    private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        for (var i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
                return typedChild;

            var foundChild = FindChild<T>(child);
            if (foundChild != null)
                return foundChild;
        }

        return null;
    }

    #endregion
}
