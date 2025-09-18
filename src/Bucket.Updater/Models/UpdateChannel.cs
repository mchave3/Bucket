namespace Bucket.Updater.Models
{
    /// <summary>
    /// Update release channels for different types of builds
    /// </summary>
    public enum UpdateChannel
    {
        /// <summary>
        /// Stable release channel
        /// </summary>
        Release,
        
        /// <summary>
        /// Nightly development builds
        /// </summary>
        Nightly
    }
}