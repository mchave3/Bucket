using Bucket.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Bucket.Views;

/// <summary>
/// A page for managing Windows images and their editions.
/// </summary>
public sealed partial class ImageManagementPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// </summary>
    public ImageManagementViewModel ViewModel
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the ImageManagementPage class.
    /// </summary>
    public ImageManagementPage()
    {
        ViewModel = App.GetService<ImageManagementViewModel>();
        this.DataContext = ViewModel;
        this.InitializeComponent();
        Logger.Debug("ImageManagementPage initialized");
    }

    /// <summary>
    /// Called when the page is navigated to.
    /// </summary>
    /// <param name="e">The navigation event arguments.</param>
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Logger.Debug("Navigated to ImageManagementPage");

        try
        {
            // Initialize the ViewModel when navigating to the page
            await ViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize ImageManagementPage");
        }
    }
}