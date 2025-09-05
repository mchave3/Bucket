using System;
using System.Linq;
using System.Threading.Tasks;
using DevWinUI;

namespace Bucket.App.Helpers
{
    public static class LanguageSwitchingHelper
    {
        /// <summary>
        /// Applies dynamic language switching to refresh the entire application UI
        /// including the NavigationView menu and current page content.
        /// </summary>
        /// <param name="languageCode">The language code to switch to (e.g., "en-US", "fr-FR")</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task ApplyDynamicLanguageSwitchingAsync(string languageCode)
        {
            try
            {
                // Step 1: Set the system language override
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                // Step 2: Navigate to HomePage after language change
                // This provides a better UX by showing the refreshed home page
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

                // Step 5: Reset the navigation service
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

                // Language change complete - user stays on HomePage for better UX
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during dynamic language switching: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if the given page type is a settings page
        /// </summary>
        /// <param name="pageType">The page type to check</param>
        /// <returns>True if it's a settings page, false otherwise</returns>
        private static bool IsSettingsPage(Type pageType)
        {
            return pageType.Name.Contains("Setting") ||
                   pageType.Namespace?.Contains("Settings") == true ||
                   pageType.Name == "SettingsPage";
        }

        /// <summary>
        /// Restarts the application with the specified language.
        /// This is the simplest approach but requires app restart.
        /// </summary>
        /// <param name="languageCode">The language code to switch to</param>
        public static void RestartWithLanguage(string languageCode)
        {
            // Set the system language override
            Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;

            // Restart the application
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
        }
    }
}
