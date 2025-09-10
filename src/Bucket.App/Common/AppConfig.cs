using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;
using System.Runtime.InteropServices;
using Bucket.Core.Helpers;

namespace Bucket.App.Common
{
    [GenerateAutoSaveOnChange]
    public partial class AppConfig : NotifiyingJsonSettings, IVersionable
    {
        // File name of the application configuration
        private string fileName { get; set; } = Constants.AppConfigPath;

        // Application metadata
        [EnforcedVersion("1.0.0.0")]
        public Version Version { get; set; } = new Version(1, 0, 0, 0);
        private string updateChannel { get; set; } = "Release";
        private string architecture { get; set; } = "x64";

        // Update settings
        public string GitHubOwner { get; set; } = "mchave3";
        public string GitHubRepository { get; set; } = "Bucket";

        // Application state
        private bool hasBeenStartedBefore { get; set; } = false;
        private string lastUpdateCheck { get; set; }

        // User preferences
        private string selectedLanguage { get; set; } = "en-US";
        private bool useDeveloperMode { get; set; }


        public void InitializeRuntimeProperties()
        {
            // Set the version to the current application version
            Version = new Version(VersionHelper.GetAppVersion().Split('-')[0]);

            // Automatically detect the update channel based on version
            updateChannel = VersionHelper.GetAppVersion().Contains("-Nightly") ? "Nightly" : "Release";

            // Automatically detect the system architecture
            architecture = RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.X86 => "x86",
                System.Runtime.InteropServices.Architecture.X64 => "x64",
                System.Runtime.InteropServices.Architecture.Arm64 => "arm64",
                _ => "x64" // Default value
            };
        }

        // Docs: https://github.com/Nucs/JsonSettings
    }
}
