using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace Bucket.Common;

[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.0.0.0")]
    public Version Version { get; set; } = new Version(1, 0, 0, 0);

    private string fileName { get; set; } = Constants.AppConfigPath;

    private bool useDeveloperMode { get; set; }
    private string lastUpdateCheck { get; set; }

    // Pre-flight and system configuration
    private bool preFlightCompleted { get; set; } = false;
    private DateTime lastPreFlightCheck { get; set; } = DateTime.MinValue;
    private bool skipAdminCheck { get; set; } = false;
    private bool skipDiskSpaceCheck { get; set; } = false;
    private string customWorkingDirectory { get; set; } = "";

    // Migration tracking
    private bool configMigratedToProgramData { get; set; } = false;
    private string lastMigrationVersion { get; set; } = "";

    // Docs: https://github.com/Nucs/JsonSettings
}
