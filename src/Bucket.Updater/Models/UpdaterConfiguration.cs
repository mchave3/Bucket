using System.Runtime.InteropServices;

namespace Bucket.Updater.Models
{
    /// <summary>
    /// Configuration settings for the updater system
    /// </summary>
    public class UpdaterConfiguration
    {
        /// <summary>
        /// Update channel to check for releases
        /// </summary>
        public UpdateChannel UpdateChannel { get; set; } = UpdateChannel.Release;
        
        /// <summary>
        /// System architecture for update filtering
        /// </summary>
        public SystemArchitecture Architecture { get; set; } = SystemArchitecture.X64;
        
        /// <summary>
        /// GitHub repository owner
        /// </summary>
        public string GitHubOwner { get; set; } = "mchave3";
        
        /// <summary>
        /// GitHub repository name
        /// </summary>
        public string GitHubRepository { get; set; } = "Bucket";
        
        /// <summary>
        /// Currently installed version
        /// </summary>
        public string CurrentVersion { get; set; } = "1.0.0.0";
        
        /// <summary>
        /// Timestamp of last update check
        /// </summary>
        public DateTime LastUpdateCheck { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Initializes architecture based on current runtime environment
        /// </summary>
        public void InitializeRuntimeProperties()
        {
            // Detect current system architecture automatically
            Architecture = RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.X86 => SystemArchitecture.X86,
                System.Runtime.InteropServices.Architecture.X64 => SystemArchitecture.X64,
                System.Runtime.InteropServices.Architecture.Arm64 => SystemArchitecture.ARM64,
                _ => SystemArchitecture.X64 // Default to x64 for unknown architectures
            };
        }

        /// <summary>
        /// Gets the architecture as a string for file naming and filtering
        /// </summary>
        /// <returns>Architecture string (x86, x64, arm64)</returns>
        public string GetArchitectureString()
        {
            return Architecture switch
            {
                SystemArchitecture.X86 => "x86",
                SystemArchitecture.X64 => "x64",
                SystemArchitecture.ARM64 => "arm64",
                _ => "x64" // Default fallback
            };
        }
    }
}