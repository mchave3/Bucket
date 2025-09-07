using System;
using System.Linq;
using System.Threading.Tasks;
using DevWinUI;
using Bucket.Core.Services;

namespace Bucket.App.Services
{
    /// <summary>
    /// WinUI platform-specific UI refresher for navigation menu and UI elements
    /// </summary>
    public class WinUIPlatformUIRefresher : IPlatformUIRefresher
    {
        /// <summary>
        /// Refreshes WinUI platform-specific UI elements after language change
        /// </summary>
        /// <param name="languageCode">New language code</param>
        public async Task RefreshUIAsync(string languageCode)
        {
            try
            {
                // Step 1: Set the system language override for WinUI
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                // Step 2: Navigate to HomePage for better UX after language change
                var homePage = typeof(Bucket.App.Views.HomeLandingPage);
                App.Current.NavService.Frame.Navigate(homePage);

                // Step 3: Remove from backstack to avoid duplicates
                var lastEntry = App.Current.NavService.Frame.BackStack.LastOrDefault();
                if (lastEntry != null)
                {
                    App.Current.NavService.Frame.BackStack.Remove(lastEntry);
                }

                // Step 4: Clear the navigation data source (critical for menu refresh)
                try
                {
                    DataSource.Instance.Groups.Clear();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not clear DataSource: {ex.Message}");
                }

                // Step 5: Reset the JsonNavigationService
                var navService = App.Current.NavService as JsonNavigationService;
                if (navService != null)
                {
                    navService.Reset();
                }

                // Step 6: Reinitialize the main window navigation
                if (MainWindow.Instance != null)
                {
                    MainWindow.Instance.ReInitialize();
                }

                // Small delay to ensure reinitialization completes
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during WinUI UI refresh: {ex.Message}");
                throw;
            }
        }
    }
}