using System.Reflection;
using Bucket.Core.Helpers;

namespace Bucket.Core.Tests;

public class VersionHelperTests
{
    [Fact]
    public void GetAppVersion_WithDefaultAssembly_ReturnsValidVersion()
    {
        // Act
        var version = VersionHelper.GetAppVersion();

        // Assert
        Assert.NotNull(version);
        Assert.NotEqual("Unknown", version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public void GetAppVersion_WithNullAssembly_ReturnsValidVersion()
    {
        // Act
        var version = VersionHelper.GetAppVersion(null);

        // Assert
        Assert.NotNull(version);
        Assert.NotEqual("Unknown", version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public void GetAppVersion_WithSpecificAssembly_ReturnsAssemblyVersion()
    {
        // Arrange
        var assembly = typeof(VersionHelper).Assembly;

        // Act
        var version = VersionHelper.GetAppVersion(assembly);

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public void GetAppVersion_WithAssemblyHavingInformationalVersion_ReturnsInformationalVersion()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var version = VersionHelper.GetAppVersion(assembly);

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithDefaultPrefix_ReturnsVersionWithV()
    {
        // Act
        var versionWithPrefix = VersionHelper.GetAppVersionWithPrefix();

        // Assert
        Assert.NotNull(versionWithPrefix);
        Assert.StartsWith("v", versionWithPrefix);
        Assert.True(versionWithPrefix.Length > 1);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithCustomPrefix_ReturnsVersionWithCustomPrefix()
    {
        // Arrange
        const string customPrefix = "version-";

        // Act
        var versionWithPrefix = VersionHelper.GetAppVersionWithPrefix(prefix: customPrefix);

        // Assert
        Assert.NotNull(versionWithPrefix);
        Assert.StartsWith(customPrefix, versionWithPrefix);
        Assert.True(versionWithPrefix.Length > customPrefix.Length);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithEmptyPrefix_ReturnsVersionWithoutPrefix()
    {
        // Arrange
        const string emptyPrefix = "";

        // Act
        var versionWithPrefix = VersionHelper.GetAppVersionWithPrefix(prefix: emptyPrefix);
        var versionWithoutPrefix = VersionHelper.GetAppVersion();

        // Assert
        Assert.Equal(versionWithoutPrefix, versionWithPrefix);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithSpecificAssemblyAndPrefix_ReturnsCorrectFormat()
    {
        // Arrange
        var assembly = typeof(VersionHelper).Assembly;
        const string prefix = "build-";

        // Act
        var versionWithPrefix = VersionHelper.GetAppVersionWithPrefix(assembly, prefix);
        var version = VersionHelper.GetAppVersion(assembly);

        // Assert
        Assert.NotNull(versionWithPrefix);
        Assert.StartsWith(prefix, versionWithPrefix);
        Assert.Equal($"{prefix}{version}", versionWithPrefix);
    }

    [Theory]
    [InlineData("v")]
    [InlineData("version")]
    [InlineData("build-")]
    [InlineData("rel_")]
    [InlineData("")]
    public void GetAppVersionWithPrefix_WithVariousPrefixes_ReturnsCorrectFormat(string prefix)
    {
        // Act
        var versionWithPrefix = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);
        var version = VersionHelper.GetAppVersion();

        // Assert
        Assert.NotNull(versionWithPrefix);
        Assert.Equal($"{prefix}{version}", versionWithPrefix);
    }

    [Fact]
    public void GetAppVersion_ConsistentResults_WhenCalledMultipleTimes()
    {
        // Act
        var version1 = VersionHelper.GetAppVersion();
        var version2 = VersionHelper.GetAppVersion();
        var version3 = VersionHelper.GetAppVersion();

        // Assert
        Assert.Equal(version1, version2);
        Assert.Equal(version2, version3);
    }

    [Fact]
    public void GetAppVersionWithPrefix_ConsistentResults_WhenCalledMultipleTimes()
    {
        // Arrange
        const string prefix = "test-";

        // Act
        var version1 = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);
        var version2 = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);
        var version3 = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);

        // Assert
        Assert.Equal(version1, version2);
        Assert.Equal(version2, version3);
    }
}