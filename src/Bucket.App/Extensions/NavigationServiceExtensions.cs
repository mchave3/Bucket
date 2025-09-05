using System;
using System.Linq;
using System.Threading.Tasks;
using DevWinUI;
using Bucket.App.Helpers;

namespace Bucket.App.Extensions
{
    public static class NavigationServiceExtensions
    {
        /// <summary>
        /// Refreshes the navigation service and UI after a language change.
        /// This method simplifies the dynamic language switching process.
        /// </summary>
        /// <param name="navigationService">The navigation service instance</param>
        /// <param name="languageCode">The new language code</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task RefreshForLanguageChangeAsync(this IJsonNavigationService navigationService, string languageCode)
        {
            await LanguageSwitchingHelper.ApplyDynamicLanguageSwitchingAsync(languageCode);
        }

        /// <summary>
        /// Refreshes the current page content without affecting the navigation menu.
        /// Useful for refreshing localized content on the current page only.
        /// </summary>
        /// <param name="navigationService">The navigation service instance</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task RefreshCurrentPageAsync(this IJsonNavigationService navigationService)
        {
            try
            {
                // Get current page type to navigate back to it
                var currentPageType = navigationService.Frame.Content?.GetType();

                if (currentPageType != null)
                {
                    navigationService.Frame.Navigate(currentPageType);

                    // Remove from backstack to avoid duplicates
                    var lastEntry = navigationService.Frame.BackStack.LastOrDefault();
                    if (lastEntry != null)
                    {
                        navigationService.Frame.BackStack.Remove(lastEntry);
                    }
                }

                await Task.Delay(50); // Small delay to ensure navigation completes
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing current page: {ex.Message}");
            }
        }
    }
}
