using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace Bucket.App.Common
{
    [GenerateAutoSaveOnChange]
    public partial class AppConfig : NotifiyingJsonSettings, IVersionable
    {
        // Version of the application
        [EnforcedVersion("1.0.0.0")]
        public Version Version { get; set; } = new Version(1, 0, 0, 0);

        // File name of the application configuration
        private string fileName { get; set; } = Constants.AppConfigPath;

        // Whether to use developer mode
        private bool useDeveloperMode { get; set; }

        // Last time the application was updated
        private string lastUpdateCheck { get; set; }

        // Selected language for the application
        private string selectedLanguage { get; set; } = "en-US";

        // Whether this is the first time the application has been started
        private bool hasBeenStartedBefore { get; set; } = false;

        // Docs: https://github.com/Nucs/JsonSettings
    }
}
