﻿namespace Bucket.Updater.Common
{
    public static partial class Constants
    {
        public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductNameAndVersion);
    }
}
