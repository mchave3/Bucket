using System.Reflection;
using Bucket.Core.Helpers;

namespace Bucket.Core.Tests;

public class VersionHelperTests
{
    [Fact]
    public void GetAppVersion_WithNullAssembly_ReturnsVersion()
    {
        // Act
        var result = VersionHelper.GetAppVersion();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual("Unknown", result);
    }

    [Fact]
    public void GetAppVersion_WithSpecificAssembly_ReturnsAssemblyVersion()
    {
        // Arrange
        var assembly = typeof(VersionHelper).Assembly;

        // Act
        var result = VersionHelper.GetAppVersion(assembly);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAppVersion_WithAssemblyWithInformationalVersion_ReturnsInformationalVersion()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = VersionHelper.GetAppVersion(assembly);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithDefaultPrefix_ReturnsVersionWithV()
    {
        // Act
        var result = VersionHelper.GetAppVersionWithPrefix();

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("v", result);
        Assert.Contains(".", result); // Should contain version numbers
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithCustomPrefix_ReturnsVersionWithCustomPrefix()
    {
        // Arrange
        var customPrefix = "Version ";

        // Act
        var result = VersionHelper.GetAppVersionWithPrefix(prefix: customPrefix);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith(customPrefix, result);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithEmptyPrefix_ReturnsVersionWithoutPrefix()
    {
        // Arrange
        var emptyPrefix = "";

        // Act
        var result = VersionHelper.GetAppVersionWithPrefix(prefix: emptyPrefix);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("v", result.Substring(0, 1));

        // Should be the same as GetAppVersion()
        var expectedVersion = VersionHelper.GetAppVersion();
        Assert.Equal(expectedVersion, result);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithNullPrefix_ReturnsVersionWithNullPrefix()
    {
        // Act
        var result = VersionHelper.GetAppVersionWithPrefix(prefix: null!);

        // Assert
        Assert.NotNull(result);
        // Should start with the version directly since null prefix becomes empty
        var expectedVersion = VersionHelper.GetAppVersion();
        Assert.EndsWith(expectedVersion, result);
    }

    [Fact]
    public void GetAppVersionWithPrefix_WithSpecificAssembly_ReturnsCorrectVersion()
    {
        // Arrange
        var assembly = typeof(VersionHelper).Assembly;
        var prefix = "build-";

        // Act
        var result = VersionHelper.GetAppVersionWithPrefix(assembly, prefix);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith(prefix, result);

        var expectedVersion = VersionHelper.GetAppVersion(assembly);
        Assert.Equal($"{prefix}{expectedVersion}", result);
    }

    [Theory]
    [InlineData("v")]
    [InlineData("Version ")]
    [InlineData("")]
    [InlineData("build-")]
    [InlineData("release_")]
    public void GetAppVersionWithPrefix_WithVariousPrefixes_ReturnsCorrectFormat(string prefix)
    {
        // Act
        var result = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith(prefix, result);

        var versionPart = result.Substring(prefix.Length);
        Assert.NotEmpty(versionPart);
    }

    [Fact]
    public void GetAppVersion_ConsistentResults_WhenCalledMultipleTimes()
    {
        // Act
        var result1 = VersionHelper.GetAppVersion();
        var result2 = VersionHelper.GetAppVersion();
        var result3 = VersionHelper.GetAppVersion();

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void GetAppVersionWithPrefix_ConsistentResults_WhenCalledMultipleTimes()
    {
        // Arrange
        var prefix = "test-";

        // Act
        var result1 = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);
        var result2 = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);
        var result3 = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }
}