using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;
using System.Runtime.InteropServices;

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

        // Update channel of the application (Release/Nightly)
        private string updateChannel { get; set; } = "Release";

        // Architecture of the application (x86/x64/arm64)
        private string architecture { get; set; } = "x64";

        public void InitializeRuntimeProperties()
        {
            // Automatically detect the update channel based on version
            updateChannel = ProcessInfoHelper.Version.Contains("-Nightly") ? "Nightly" : "Release";
            
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
