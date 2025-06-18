namespace Bucket.Common;

public static partial class Constants
{
    // AppData directory structure (existing configuration)
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductNameAndVersion);
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");

    // ProgramData directory structure (for Windows image management)
    public static readonly string WorkingDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket");
    public static readonly string UpdatesDirectoryPath = Path.Combine(WorkingDirectoryPath, "Updates");
    public static readonly string StagingDirectoryPath = Path.Combine(WorkingDirectoryPath, "Staging");
    public static readonly string MountDirectoryPath = Path.Combine(WorkingDirectoryPath, "Mount");
    public static readonly string CompletedWIMsDirectoryPath = Path.Combine(WorkingDirectoryPath, "CompletedWIMs");
    public static readonly string ImportedWIMsDirectoryPath = Path.Combine(WorkingDirectoryPath, "ImportedWIMs");
    public static readonly string ConfigsDirectoryPath = Path.Combine(WorkingDirectoryPath, "Configs");
    public static readonly string WorkingLogsDirectoryPath = Path.Combine(WorkingDirectoryPath, "Logs");

    // Configuration files in ProgramData
    public static readonly string WIMsConfigPath = Path.Combine(ConfigsDirectoryPath, "WIMs.xml");
    public static readonly string WorkingLogFilePath = Path.Combine(WorkingLogsDirectoryPath, "Bucket.log");

    // Minimum system requirements
    public const long MinimumDiskSpaceGB = 15;
    public const int MinimumWindowsBuild = 17763; // Windows 10 version 1809
}
