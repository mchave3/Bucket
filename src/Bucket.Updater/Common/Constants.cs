namespace Bucket.Updater.Common
{
    /// <summary>
    /// Provides common constants used throughout the Bucket Updater application.
    /// </summary>
    public static partial class Constants
    {
        /// <summary>
        /// Gets the root directory path for the Bucket application.
        /// This directory is located in the system's CommonApplicationData folder.
        /// </summary>
        /// <value>
        /// The full path to the Bucket root directory.
        /// </value>
        public static readonly string RootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket");
        
        /// <summary>
        /// Gets the directory path for application logging.
        /// This directory is a subdirectory of the Bucket root directory.
        /// </summary>
        /// <value>
        /// The full path to the logging directory.
        /// </value>
        public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
        
        /// <summary>
        /// Gets the full path to the updater log file.
        /// This file is used to record updater events and errors.
        /// </summary>
        /// <value>
        /// The full path to the "Updater-Log.log" file.
        /// </value>
        public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Updater-Log.log");
    }
}
