using Microsoft.UI.Xaml.Navigation;

namespace Bucket.Views;

public sealed partial class AppUpdateSettingPage : Page
{
    public AppUpdateSettingViewModel ViewModel { get; }

    public AppUpdateSettingPage()
    {
        ViewModel = App.GetService<AppUpdateSettingViewModel>();
        this.InitializeComponent();
    }

    /// <summary>
    /// Called when the page is navigated to.
    /// </summary>
    /// <param name="e">The navigation event arguments.</param>
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Logger.Debug("Navigated to AppUpdateSettingPage");

        try
        {
            // Initialize the ViewModel when navigating to the page
            await ViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to initialize AppUpdateSettingPage");
        }
    }
}
