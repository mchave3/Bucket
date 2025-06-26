using Bucket.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Bucket.Views;

/// <summary>
/// A page for viewing and editing detailed information about a Windows image.
/// </summary>
public sealed partial class ImageDetailsPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// </summary>
    public ImageDetailsViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the ImageDetailsPage class.
    /// </summary>
    public ImageDetailsPage()
    {
        ViewModel = App.GetService<ImageDetailsViewModel>();
        this.DataContext = ViewModel;
        this.InitializeComponent();
        Logger.Information("ImageDetailsPage initialized");
    }

    /// <summary>
    /// Called when the page is navigated to.
    /// </summary>
    /// <param name="e">The navigation event arguments.</param>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Logger.Information("Navigated to ImageDetailsPage");

        // Pass the image information to the ViewModel
        if (e.Parameter is Models.WindowsImageInfo imageInfo)
        {
            ViewModel.SetImageInfo(imageInfo);
        }
    }

    /// <summary>
    /// Handles the click event for edit index buttons.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event arguments.</param>
    private void EditIndexButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Models.WindowsImageIndex index)
        {
            ViewModel?.EditIndexCommand?.Execute(index);
        }
    }
}
