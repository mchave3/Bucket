using System.Reflection;
using Bucket.Core.Helpers;

namespace Bucket.Core.Tests.Helpers;

public class VersionHelperTests
{
    [Fact]
    public void GetAppVersion_ShouldReturnVersionString()
    {
        var version = VersionHelper.GetAppVersion();

        Assert.NotNull(version);
        Assert.NotEmpty(version);
        Assert.NotEqual("Unknown", version);
    }

    [Fact]
    public void GetAppVersionWithPrefix_ShouldReturnVersionWithDefaultPrefix()
    {
        var version = VersionHelper.GetAppVersionWithPrefix();

        Assert.NotNull(version);
        Assert.StartsWith("v", version);
    }

    [Theory]
    [InlineData("v")]
    [InlineData("version-")]
    [InlineData("")]
    public void GetAppVersionWithPrefix_ShouldReturnVersionWithCustomPrefix(string prefix)
    {
        var version = VersionHelper.GetAppVersionWithPrefix(prefix: prefix);

        Assert.NotNull(version);
        Assert.StartsWith(prefix, version);
    }

    [Fact]
    public void GetAppVersion_WithSpecificAssembly_ShouldReturnAssemblyVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = VersionHelper.GetAppVersion(assembly);

        Assert.NotNull(version);
        Assert.NotEmpty(version);
    }
}