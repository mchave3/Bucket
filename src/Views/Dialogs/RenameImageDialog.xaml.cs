using Microsoft.UI.Xaml.Controls;
using Bucket.Models;
using System;

namespace Bucket.Views.Dialogs;

/// <summary>
/// Dialog for renaming Windows image.
/// </summary>
public sealed partial class RenameImageDialog : ContentDialog
{
    /// <summary>
    /// Gets or sets the file name (read-only for display).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image type (read-only for display).
    /// </summary>
    public string ImageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image name.
    /// </summary>
    public string ImageName { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the RenameImageDialog class.
    /// </summary>
    /// <param name="imageInfo">The Windows image info to rename.</param>
    public RenameImageDialog(WindowsImageInfo imageInfo)
    {
        if (imageInfo == null)
            throw new ArgumentNullException(nameof(imageInfo));

        // Copy values from the image info
        FileName = imageInfo.FileName;
        ImageType = imageInfo.ImageTypeDisplay;
        ImageName = imageInfo.Name;

        this.InitializeComponent();

        // Handle button click events
        this.PrimaryButtonClick += RenameImageDialog_PrimaryButtonClick;

        Logger.Information("RenameImageDialog created for image '{Name}'", imageInfo.Name);
    }

    /// <summary>
    /// Handles the primary button click event to validate the new name.
    /// </summary>
    private void RenameImageDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(ImageName))
        {
            ShowValidationError("Name Required", "Please enter a name for the image.");
            args.Cancel = true;
            return;
        }

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (ImageName.ToCharArray().Any(c => invalidChars.Contains(c)))
        {
            ShowValidationError("Invalid Characters", "The name contains invalid characters. Please remove any of the following characters: " + string.Join(" ", invalidChars));
            args.Cancel = true;
            return;
        }

        // Validation passed, allow dialog to close
    }

    /// <summary>
    /// Updates the provided image info with the new name and renames the physical file.
    /// </summary>
    /// <param name="imageInfo">The image info to update.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    public bool UpdateImageInfo(WindowsImageInfo imageInfo)
    {
        if (imageInfo == null)
            throw new ArgumentNullException(nameof(imageInfo));

        var newName = ImageName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(newName))
            return false;

        var oldName = imageInfo.Name;
        var oldFilePath = imageInfo.FilePath;

        try
        {
            // Calculate new file path
            var directory = Path.GetDirectoryName(oldFilePath);
            var extension = Path.GetExtension(oldFilePath);
            var newFilePath = Path.Combine(directory, newName + extension);

            // Check if target file already exists
            if (File.Exists(newFilePath) && !string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warning("Cannot rename image: Target file already exists at {NewPath}", newFilePath);
                return false;
            }

            // Rename the physical file if the name actually changed
            if (!string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(oldFilePath, newFilePath);
                Logger.Information("Physical file renamed from '{OldPath}' to '{NewPath}'", oldFilePath, newFilePath);

                // Update file path in model (FileName will be automatically updated)
                imageInfo.FilePath = newFilePath;
            }

            // Update display name
            imageInfo.Name = newName;

            Logger.Information("Image renamed: Display name '{OldName}' → '{NewName}', File renamed: {FileRenamed}",
                oldName, newName, !string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase));

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to rename image from '{OldName}' to '{NewName}'", oldName, newName);
            return false;
        }
    }

    /// <summary>
    /// Shows a validation error to the user.
    /// </summary>
    /// <param name="title">The error title.</param>
    /// <param name="message">The error message.</param>
    private async void ShowValidationError(string title, string message)
    {
        var errorDialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await errorDialog.ShowAsync();
    }
}
