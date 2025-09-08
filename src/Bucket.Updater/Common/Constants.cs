﻿namespace Bucket.Updater.Common
{
    public static partial class Constants
    {
        public static readonly string RootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket");
        public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
        public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Updater-Log.log");
    }
}
