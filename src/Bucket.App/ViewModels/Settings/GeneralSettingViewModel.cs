using System.Threading.Tasks;
using Bucket.App.Helpers;

namespace Bucket.App.ViewModels
{
    public partial class GeneralSettingViewModel : ObservableObject
    {
        /// <summary>
        /// Applies language change with dynamic UI refresh.
        /// </summary>
        /// <param name="languageCode">The language code to switch to</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ApplyLanguageChangeAsync(string languageCode)
        {
            try
            {
                await LanguageSwitchingHelper.ApplyDynamicLanguageSwitchingAsync(languageCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during language change: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Applies language change by restarting the application.
        /// This is the most reliable method but requires app restart.
        /// </summary>
        /// <param name="languageCode">The language code to switch to</param>
        public void ApplyLanguageChangeWithRestart(string languageCode)
        {
            LanguageSwitchingHelper.RestartWithLanguage(languageCode);
        }
    }
}
