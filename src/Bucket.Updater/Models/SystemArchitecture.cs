namespace Bucket.Updater.Models
{
    /// <summary>
    /// System processor architectures supported by the updater
    /// </summary>
    public enum SystemArchitecture
    {
        /// <summary>
        /// 32-bit x86 architecture
        /// </summary>
        X86,
        
        /// <summary>
        /// 64-bit x86 architecture
        /// </summary>
        X64,
        
        /// <summary>
        /// 64-bit ARM architecture
        /// </summary>
        ARM64
    }
}