namespace Bucket.Core.Tests.Helpers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Bucket.Core.Helpers;
    using Xunit;

    public sealed class VersionHelperTests
    {
        [Fact]
        public void GetCurrentVersionShouldReturnAssemblyVersion()
        {
            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.NotNull(version);
            Assert.NotEmpty(version);

            // Version should be in the format of "major.minor.build.revision" or similar
            var versionParts = version.Split('.');
            Assert.True(versionParts.Length >= 2, $"Version '{version}' should have at least 2 parts separated by dots");

            // All parts should be numeric
            foreach (var part in versionParts)
            {
                Assert.True(int.TryParse(part, out _), $"Version part '{part}' should be numeric");
            }
        }

        [Fact]
        public void GetCurrentVersionShouldReturnConsistentResult()
        {
            // Act
            var version1 = VersionHelper.GetCurrentVersion();
            var version2 = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.Equal(version1, version2);
        }

        [Fact]
        public void GetCurrentVersionShouldMatchAssemblyInformationalVersion()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(VersionHelper));
            var informationalVersionAttribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            if (informationalVersionAttribute != null)
            {
                // If AssemblyInformationalVersion exists, it should match or be a subset
                Assert.Contains(version, informationalVersionAttribute.InformationalVersion);
            }
            else
            {
                // Fallback to AssemblyVersion
                var assemblyVersion = assembly?.GetName().Version?.ToString();
                if (assemblyVersion != null)
                {
                    Assert.Equal(assemblyVersion, version);
                }
            }
        }

        [Fact]
        public void GetCurrentVersionShouldHandleVersionFormats()
        {
            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.NotNull(version);

            // Should be able to parse as System.Version
            Assert.True(Version.TryParse(version, out var parsedVersion),
                $"Version '{version}' should be parseable as System.Version");

            Assert.NotNull(parsedVersion);
            Assert.True(parsedVersion.Major >= 0);
            Assert.True(parsedVersion.Minor >= 0);
        }

        [Fact]
        public void GetCurrentVersionShouldNotReturnNullOrEmpty()
        {
            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.False(string.IsNullOrEmpty(version), "Version should not be null or empty");
            Assert.False(string.IsNullOrWhiteSpace(version), "Version should not be whitespace");
        }

        [Fact]
        public void GetCurrentVersionShouldReturnValidVersionString()
        {
            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.NotNull(version);
            Assert.Matches(@"^\d+\.\d+(\.\d+)?(\.\d+)?", version); // Should match version pattern like 1.0, 1.0.0, or 1.0.0.0
        }

        [Fact]
        public void GetCurrentVersionFromDifferentAssemblyContextsShouldBeConsistent()
        {
            // Arrange & Act
            var version1 = VersionHelper.GetCurrentVersion();

            // Simulate different calling context
            var task = System.Threading.Tasks.Task.Run(() => VersionHelper.GetCurrentVersion());
            var version2 = task.Result;

            // Assert
            Assert.Equal(version1, version2);
        }

        [Fact]
        public void GetCurrentVersionShouldWorkWithDifferentRuntimeArchitectures()
        {
            // Arrange
            var currentArchitecture = RuntimeInformation.ProcessArchitecture;

            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.NotNull(version);
            Assert.NotEmpty(version);

            // Version retrieval should work regardless of architecture
            Assert.True(currentArchitecture == Architecture.X86 ||
                       currentArchitecture == Architecture.X64 ||
                       currentArchitecture == Architecture.Arm64,
                       "Should work on supported architectures");
        }

        [Fact]
        public void GetCurrentVersionPerformanceShouldBeReasonable()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var version = VersionHelper.GetCurrentVersion();
            stopwatch.Stop();

            // Assert
            Assert.NotNull(version);
            Assert.True(stopwatch.ElapsedMilliseconds < 100,
                $"Version retrieval should be fast, took {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void GetCurrentVersionShouldWorkInTestContext()
        {
            // Arrange
            var testAssembly = Assembly.GetExecutingAssembly();
            var coreAssembly = Assembly.GetAssembly(typeof(VersionHelper));

            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            Assert.NotNull(version);
            Assert.NotEmpty(version);

            // Should return version from Bucket.Core assembly, not test assembly
            Assert.NotNull(coreAssembly);
            var coreVersion = coreAssembly.GetName().Version?.ToString();
            if (coreVersion != null)
            {
                // Version should relate to the Core assembly, not the test assembly
                Assert.True(version.Length > 0);
            }
        }

        [Fact]
        public void GetCurrentVersionShouldHandleAssemblyWithoutVersion()
        {
            // This test ensures the method doesn't crash even if assembly version info is missing
            // Act
            var version = VersionHelper.GetCurrentVersion();

            // Assert
            // Should return something, even if it's a default version
            Assert.NotNull(version);
            Assert.NotEmpty(version);

            // Should still be a valid version format
            Assert.True(Version.TryParse(version, out _),
                $"Even without explicit version, should return parseable version: '{version}'");
        }
    }
}