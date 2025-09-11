using System.Runtime.InteropServices;

namespace Bucket.Updater.Models
{
    public class UpdaterConfiguration
    {
        public UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Release;
        public SystemArchitecture Architecture { get; set; } = SystemArchitecture.X64;
        public string GitHubOwner { get; set; } = "mchave3";
        public string GitHubRepository { get; set; } = "Bucket";
        public string CurrentVersion { get; set; } = "1.0.0.0";
        public DateTime LastUpdateCheck { get; set; } = DateTime.MinValue;

        public void InitializeRuntimeProperties()
        {
            Architecture = RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.X86 => SystemArchitecture.X86,
                System.Runtime.InteropServices.Architecture.X64 => SystemArchitecture.X64,
                System.Runtime.InteropServices.Architecture.Arm64 => SystemArchitecture.ARM64,
                _ => SystemArchitecture.X64
            };
        }

        public string GetArchitectureString()
        {
            return Architecture switch
            {
                SystemArchitecture.X86 => "x86",
                SystemArchitecture.X64 => "x64",
                SystemArchitecture.ARM64 => "arm64",
                _ => "x64"
            };
        }
    }
}