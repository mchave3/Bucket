using Nucs.JsonSettings;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;

namespace Bucket.App.Common
{
    public static partial class AppHelper
    {
        [System.Diagnostics.CodeAnalysis.DynamicDependency(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All, typeof(AppConfig))]
        public static AppConfig Settings = InitializeSettings();

        private static AppConfig InitializeSettings()
        {
            var config = JsonSettings.Configure<AppConfig>()
                                    .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                                    .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                                    .LoadNow();
            
            // Initialize runtime properties
            config.InitializeRuntimeProperties();
            
            return config;
        }
    }

}
