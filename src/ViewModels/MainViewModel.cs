using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Bucket.Models;
using Bucket.Services.WindowsImage;

namespace Bucket.ViewModels
{
    /// <summary>
    /// Main view model for the application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IWindowsImageMountService _mountService;
        private readonly IWindowsImageUnmountService _unmountService;
        private ObservableCollection<MountedImageInfo> _mountedImages = new();

        /// <summary>
        /// Gets the collection of currently mounted images.
        /// </summary>
        public ObservableCollection<MountedImageInfo> MountedImages
        {
            get => _mountedImages;
            set => SetProperty(ref _mountedImages, value);
        }

        /// <summary>
        /// Gets the command to refresh mounted images.
        /// </summary>
        public IAsyncRelayCommand RefreshMountedImagesCommand { get; }

        /// <summary>
        /// Gets the command to open a mount directory.
        /// </summary>
        public IAsyncRelayCommand<MountedImageInfo> OpenMountDirectoryCommand { get; }

        /// <summary>
        /// Gets the command to unmount an image.
        /// </summary>
        public IAsyncRelayCommand<MountedImageInfo> UnmountImageCommand { get; }

        public MainViewModel(IWindowsImageMountService mountService, IWindowsImageUnmountService unmountService)
        {
            _mountService = mountService ?? throw new ArgumentNullException(nameof(mountService));
            _unmountService = unmountService ?? throw new ArgumentNullException(nameof(unmountService));
            
            RefreshMountedImagesCommand = new AsyncRelayCommand(RefreshMountedImagesAsync);
            OpenMountDirectoryCommand = new AsyncRelayCommand<MountedImageInfo>(OpenMountDirectoryAsync);
            UnmountImageCommand = new AsyncRelayCommand<MountedImageInfo>(UnmountImageAsync);

            Logger.Debug("MainViewModel initialized");
            
            // Load mounted images on startup
            _ = RefreshMountedImagesAsync();
        }

        /// <summary>
        /// Refreshes the list of mounted images.
        /// </summary>
        private async Task RefreshMountedImagesAsync()
        {
            try
            {
                // Clean up orphaned mount directories on first load
                if (MountedImages.Count == 0)
                {
                    try
                    {
                        await _mountService.CleanupOrphanedMountDirectoriesAsync();
                        Logger.Debug("Orphaned mount directory cleanup completed");
                    }
                    catch (Exception cleanupEx)
                    {
                        Logger.Warning(cleanupEx, "Failed to cleanup orphaned mount directories during startup");
                    }
                }

                var mountedImages = await _mountService.GetMountedImagesAsync();
                
                MountedImages.Clear();
                foreach (var mount in mountedImages)
                {
                    MountedImages.Add(mount);
                }

                Logger.Information("Refreshed mounted images: {Count} images found", mountedImages.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to refresh mounted images");
            }
        }

        /// <summary>
        /// Opens the mount directory for a mounted image.
        /// </summary>
        private async Task OpenMountDirectoryAsync(MountedImageInfo mountedImage)
        {
            if (mountedImage == null) return;

            try
            {
                await _mountService.OpenMountDirectoryAsync(mountedImage);
                Logger.Information("Opened mount directory: {MountPath}", mountedImage.MountPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to open mount directory: {MountPath}", mountedImage.MountPath);
                await ShowErrorDialogAsync("Open Directory Error", $"Failed to open mount directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Unmounts a mounted image.
        /// </summary>
        private async Task UnmountImageAsync(MountedImageInfo mountedImage)
        {
            if (mountedImage == null) return;

            try
            {
                Logger.Information("Starting unmount operation for image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
                
                // Show progress dialog and perform unmount operation
                await ShowUnmountProgressDialogAsync(mountedImage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to unmount image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
                await ShowErrorDialogAsync("Unmount Error", $"Failed to unmount image: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows a progress dialog for unmount operation.
        /// </summary>
        private async Task ShowUnmountProgressDialogAsync(MountedImageInfo mountedImage)
        {
            var progressDialog = new ContentDialog
            {
                Title = "Unmounting Image",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock { Text = $"Unmounting: {mountedImage.DisplayText}" },
                        new ProgressBar { IsIndeterminate = true },
                        new TextBlock 
                        { 
                            Text = "Please wait while the image is being unmounted...",
                            Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
                        }
                    }
                },
                PrimaryButtonText = null,
                SecondaryButtonText = null,
                CloseButtonText = null,
                DefaultButton = ContentDialogButton.None
            };

            // Set XamlRoot
            if (App.MainWindow?.Content is FrameworkElement element)
            {
                progressDialog.XamlRoot = element.XamlRoot;
            }

            // Start the unmount operation in background
            var unmountTask = Task.Run(async () =>
            {
                try
                {
                    await _unmountService.UnmountImageAsync(mountedImage, true);
                    return true;
                }
                catch
                {
                    return false;
                }
            });

            // Show dialog
            var dialogTask = progressDialog.ShowAsync();

            // Wait for unmount to complete
            var success = await unmountTask;

            // Close dialog
            progressDialog.Hide();

            if (success)
            {
                await RefreshMountedImagesAsync(); // Refresh the list
                Logger.Information("Successfully unmounted image: {ImagePath}, Index: {Index}", mountedImage.ImagePath, mountedImage.Index);
                
                // Show success message
                await ShowInfoDialogAsync("Unmount Successful", "The image has been unmounted successfully.");
            }
            else
            {
                throw new InvalidOperationException("Unmount operation failed.");
            }
        }

        /// <summary>
        /// Shows an information dialog.
        /// </summary>
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
        /// Shows an error dialog.
        /// </summary>
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
    }
}
