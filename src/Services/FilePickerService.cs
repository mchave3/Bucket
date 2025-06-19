using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Bucket.Services;

/// <summary>
/// Service for handling file picker operations in WinUI 3.
/// </summary>
public class FilePickerService
{
    /// <summary>
    /// Opens a file picker dialog to select an ISO file.
    /// </summary>
    /// <returns>The selected ISO file, or null if cancelled.</returns>
    public async Task<StorageFile> PickIsoFileAsync()
    {
        Logger.Information("Opening ISO file picker");

        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".iso");
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            // Get the current window's handle for WinUI 3
            var window = App.MainWindow;
            if (window != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                Logger.Information("Selected ISO file: {FileName}", file.Name);
            }
            else
            {
                Logger.Information("ISO file selection cancelled");
            }

            return file;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open ISO file picker");
            throw;
        }
    }

    /// <summary>
    /// Opens a file picker dialog to select a WIM or ESD file.
    /// </summary>
    /// <returns>The selected WIM/ESD file, or null if cancelled.</returns>
    public async Task<StorageFile> PickWimFileAsync()
    {
        Logger.Information("Opening WIM/ESD file picker");

        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".wim");
            picker.FileTypeFilter.Add(".esd");
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            // Get the current window's handle for WinUI 3
            var window = App.MainWindow;
            if (window != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                Logger.Information("Selected WIM/ESD file: {FileName}", file.Name);
            }
            else
            {
                Logger.Information("WIM/ESD file selection cancelled");
            }

            return file;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open WIM/ESD file picker");
            throw;
        }
    }

    /// <summary>
    /// Opens a folder picker dialog to select a destination folder.
    /// </summary>
    /// <returns>The selected folder, or null if cancelled.</returns>
    public async Task<StorageFolder> PickFolderAsync()
    {
        Logger.Information("Opening folder picker");

        try
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");

            // Get the current window's handle for WinUI 3
            var window = App.MainWindow;
            if (window != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                Logger.Information("Selected folder: {FolderName}", folder.Name);
            }
            else
            {
                Logger.Information("Folder selection cancelled");
            }

            return folder;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to open folder picker");
            throw;
        }
    }
}
