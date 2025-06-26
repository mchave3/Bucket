using Microsoft.UI.Xaml.Controls;
using Bucket.Models;
using System;

namespace Bucket.Views.Dialogs;

/// <summary>
/// Dialog for editing Windows image index properties.
/// </summary>
public sealed partial class EditIndexDialog : ContentDialog
{
    /// <summary>
    /// Gets or sets the index number (read-only for display).
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the architecture (read-only for display).
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index description.
    /// </summary>
    public string IndexDescription { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the EditIndexDialog class.
    /// </summary>
    /// <param name="imageIndex">The Windows image index to edit.</param>
    public EditIndexDialog(WindowsImageIndex imageIndex)
    {
        if (imageIndex == null)
            throw new ArgumentNullException(nameof(imageIndex));

        // Copy values from the image index
        Index = imageIndex.Index;
        Architecture = imageIndex.Architecture;
        IndexName = imageIndex.Name;
        IndexDescription = imageIndex.Description;

        this.InitializeComponent();

        // Handle button click events
        this.PrimaryButtonClick += EditIndexDialog_PrimaryButtonClick;

        Logger.Information("EditIndexDialog created for index {Index}", Index);
    }

    /// <summary>
    /// Handles the primary button click event to validate and save changes.
    /// </summary>
    private void EditIndexDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(IndexName))
        {
            // TODO: Show validation error
            args.Cancel = true;
            return;
        }

        // Validation passed, allow dialog to close
    }

    /// <summary>
    /// Updates the provided image index with the edited values.
    /// </summary>
    /// <param name="imageIndex">The image index to update.</param>
    public void UpdateImageIndex(WindowsImageIndex imageIndex)
    {
        if (imageIndex == null)
            throw new ArgumentNullException(nameof(imageIndex));

        var oldName = imageIndex.Name;
        var oldDescription = imageIndex.Description;

        imageIndex.Name = IndexName?.Trim() ?? string.Empty;
        imageIndex.Description = IndexDescription?.Trim() ?? string.Empty;

        Logger.Information("Updated index {Index}: Name '{OldName}' → '{NewName}', Description changed: {DescriptionChanged}",
            Index, oldName, imageIndex.Name, oldDescription != imageIndex.Description);
    }
}
