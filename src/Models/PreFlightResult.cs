namespace Bucket.Models;
    /// <summary>
    /// Represents the result of pre-flight system checks performed at application startup.
    /// </summary>
    public class PreFlightResult
    {
        #region Critical Checks (Block startup if failed)

        /// <summary>
        /// Indicates whether the application is running with administrator privileges.
        /// </summary>
        public bool AdminPrivileges { get; set; }

        /// <summary>
        /// Indicates whether the required directory structure was created successfully.
        /// </summary>
        public bool DirectoryStructure { get; set; }

        /// <summary>
        /// Indicates whether write permissions are available in all required directories.
        /// </summary>
        public bool WritePermissions { get; set; }

        /// <summary>
        /// Indicates whether sufficient disk space is available for operations.
        /// </summary>
        public bool DiskSpace { get; set; }

        /// <summary>
        /// Indicates whether all required system tools (DISM, PowerShell) are available.
        /// </summary>
        public bool SystemTools { get; set; }

        #endregion

        #region Recommended Checks (Warnings only)

        /// <summary>
        /// Indicates whether the Windows version is compatible with the application.
        /// </summary>
        public bool WindowsVersion { get; set; }

        /// <summary>
        /// Indicates whether required Windows services are running.
        /// </summary>
        public bool WindowsServices { get; set; }

        /// <summary>
        /// Indicates whether network connectivity to Microsoft servers is available.        /// </summary>
        public bool NetworkConnectivity { get; set; }

        #endregion

        #region Configuration Checks (Auto-repair)

        /// <summary>
        /// Indicates whether configuration files were created or repaired successfully.
        /// </summary>
        public bool ConfigFiles { get; set; }

        #endregion

        #region Summary Properties

        /// <summary>
        /// Gets a value indicating whether all critical checks passed.
        /// </summary>
        public bool AllCriticalChecksPassed =>
            AdminPrivileges && DirectoryStructure && WritePermissions && DiskSpace && SystemTools;

        /// <summary>
        /// Gets a list of failed critical checks.
        /// </summary>
        public List<string> FailedCriticalChecks
        {
            get
            {
                var failed = new List<string>();
                if (!AdminPrivileges) failed.Add("Administrator Privileges");
                if (!DirectoryStructure) failed.Add("Directory Structure");
                if (!WritePermissions) failed.Add("Write Permissions");
                if (!DiskSpace) failed.Add("Disk Space");
                if (!SystemTools) failed.Add("System Tools");
                return failed;
            }
        }

        /// <summary>
        /// Gets a list of failed recommended checks.
        /// </summary>
        public List<string> FailedRecommendedChecks
        {
            get
            {
                var failed = new List<string>();
                if (!WindowsVersion) failed.Add("Windows Version");
                if (!WindowsServices) failed.Add("Windows Services");
                if (!NetworkConnectivity) failed.Add("Network Connectivity");
                return failed;
            }
        }

        /// <summary>
        /// Collection of detailed error messages from the pre-flight checks.
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();        /// <summary>
        /// Collection of warning messages from the pre-flight checks.
        /// </summary>
        public List<string> WarningMessages { get; set; } = new List<string>();

        #endregion
    }
