using System.Reflection;

namespace Bucket.Core.Helpers
{
    public static class VersionHelper
    {
        public static string GetAppVersion(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            string? infoVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            return $"{infoVersion ?? assembly.GetName().Version?.ToString() ?? "Unknown"}";
        }

        public static string GetAppVersionWithPrefix(Assembly? assembly = null, string prefix = "v")
        {
            return $"{prefix}{GetAppVersion(assembly)}";
        }
    }
}
