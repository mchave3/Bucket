﻿using Nucs.JsonSettings;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;

namespace Bucket.Common
{    public static partial class AppHelper
    {
        public static AppConfig Settings = JsonSettings.Configure<AppConfig>()
                                   .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                                   .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                                   .WithFileName(Constants.AppConfigPath)
                                   .LoadNow();
    }

}
