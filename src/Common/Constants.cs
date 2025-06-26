namespace Bucket.Common;

public static partial class Constants
{
    // ProgramData directory structure (main application configuration)
    public static readonly string RootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Bucket");
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Logs");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Bucket.log");
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");

    // ProgramData directory structure (for Windows image management)
    public static readonly string UpdatesDirectoryPath = Path.Combine(RootDirectoryPath, "Updates");
    public static readonly string StagingDirectoryPath = Path.Combine(RootDirectoryPath, "Staging");
    public static readonly string MountDirectoryPath = Path.Combine(RootDirectoryPath, "Mount");
    public static readonly string CompletedWIMsDirectoryPath = Path.Combine(RootDirectoryPath, "CompletedWIMs");
    public static readonly string ImportedWIMsDirectoryPath = Path.Combine(RootDirectoryPath, "ImportedWIMs");
    public static readonly string ConfigsDirectoryPath = Path.Combine(RootDirectoryPath, "Configs");
    public static readonly string TempDirectoryPath = Path.Combine(RootDirectoryPath, "Temp");
    public static readonly string WIMsConfigPath = Path.Combine(ConfigsDirectoryPath, "WIMs.xml");

    // Minimum system requirements
    public const long MinimumDiskSpaceGB = 15;
    public const int MinimumWindowsBuild = 17763; // Windows 10 version 1809
}
