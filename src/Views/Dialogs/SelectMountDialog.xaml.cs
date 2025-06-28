using Bucket.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Bucket.Views.Dialogs;

/// <summary>
/// Dialog for selecting which mounted image to unmount when multiple images are mounted.
/// </summary>
public sealed partial class SelectMountDialog : ContentDialog
{
    /// <summary>
    /// Gets the selected mounted image, or null if none was selected.
    /// </summary>
    public MountedImageInfo SelectedMount { get; private set; }

    /// <summary>
    /// Initializes a new instance of the SelectMountDialog.
    /// </summary>
    /// <param name="mountedImages">The list of mounted images to choose from.</param>
    public SelectMountDialog(IEnumerable<MountedImageInfo> mountedImages)
    {
        this.InitializeComponent();
        
        // Set the mounted images as the source for the ListView
        MountListView.ItemsSource = mountedImages?.ToList();
        
        // Select the first item by default
        if (MountListView.Items.Count > 0)
        {
            MountListView.SelectedIndex = 0;
        }
        
        // Handle primary button click
        this.PrimaryButtonClick += OnPrimaryButtonClick;
        
        // Enable primary button only when an item is selected
        this.IsPrimaryButtonEnabled = MountListView.SelectedItem != null;
        MountListView.SelectionChanged += (s, e) => 
        {
            this.IsPrimaryButtonEnabled = MountListView.SelectedItem != null;
        };
    }

    /// <summary>
    /// Handles the primary button click event.
    /// </summary>
    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        SelectedMount = MountListView.SelectedItem as MountedImageInfo;
    }
} 