using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bucket.Models;

public class WindowsVersionsConfig
{
    [JsonPropertyName("operatingSystems")]
    public Dictionary<string, OperatingSystemConfig> OperatingSystems { get; set; } = new();

    [JsonPropertyName("architectures")]
    public Dictionary<string, ArchitectureConfig> Architectures { get; set; } = new();

    [JsonPropertyName("updateTypes")]
    public Dictionary<string, UpdateTypeConfig> UpdateTypes { get; set; } = new();

    [JsonPropertyName("metadata")]
    public ConfigMetadata Metadata { get; set; } = new();
}

public class OperatingSystemConfig
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("versions")]
    public List<VersionConfig> Versions { get; set; } = new();
}

public class VersionConfig
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("buildNumber")]
    public string BuildNumber { get; set; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("supportedArchitectures")]
    public List<string> SupportedArchitectures { get; set; } = new();

    [JsonPropertyName("supportedUpdateTypes")]
    public List<string> SupportedUpdateTypes { get; set; } = new();
}

public class ArchitectureConfig
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class UpdateTypeConfig
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class ConfigMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
} 