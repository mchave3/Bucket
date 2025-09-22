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
                // Step 1: Set the system language override for WinUI and DevWinUI
                Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                // Step 2: Use the new DevWinUI ReInitialize() method to refresh navigation
                var navService = App.Current.NavService as JsonNavigationService;
                if (navService != null)
                {
                    navService.ReInitialize();
                }

                // Small delay to ensure reinitialization completes
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during WinUI UI refresh: {ex.Message}");
                throw;
            }
        }
    }
}