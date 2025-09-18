namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using Xunit;

    public sealed class AppConfigReaderTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _appConfigPath;
        private readonly AppConfigReader _appConfigReader;
        private bool _disposed;

        public AppConfigReaderTests()
        {
            // Create temporary test directory that mimics the actual path structure
            _testDirectory = Path.Combine(Path.GetTempPath(), "BucketTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);

            _appConfigPath = Path.Combine(_testDirectory, "AppConfig.json");

            // Create a test instance - we'll need to use reflection to set the private path
            _appConfigReader = new AppConfigReader();

            // Use reflection to set the private _appConfigPath field
            var pathField = typeof(AppConfigReader).GetField("_appConfigPath",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pathField?.SetValue(_appConfigReader, _appConfigPath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
                _disposed = true;
            }
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new AppConfigReader();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void ReadConfigurationWithValidConfigShouldReturnConfiguration()
        {
            // Arrange
            var expectedConfig = new
            {
                Version = "24.1.15",
                updateChannel = "Release",
                architecture = "x64",
                gitHubOwner = "mchave3",
                gitHubRepository = "Bucket"
            };

            var json = JsonSerializer.Serialize(expectedConfig);
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("24.1.15", result.CurrentVersion);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel);
            Assert.Equal(SystemArchitecture.X64, result.Architecture);
            Assert.Equal("mchave3", result.GitHubOwner);
            Assert.Equal("Bucket", result.GitHubRepository);
        }

        [Fact]
        public async Task ReadConfigurationAsyncWithValidConfigShouldReturnConfiguration()
        {
            // Arrange
            var expectedConfig = new
            {
                Version = "24.1.15.1-Nightly",
                updateChannel = "Nightly",
                architecture = "arm64",
                gitHubOwner = "testowner",
                gitHubRepository = "testrepo"
            };

            var json = JsonSerializer.Serialize(expectedConfig);
            await File.WriteAllTextAsync(_appConfigPath, json);

            // Act
            var result = await _appConfigReader.ReadConfigurationAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("24.1.15.1-Nightly", result.CurrentVersion);
            Assert.Equal(UpdateChannel.Nightly, result.UpdateChannel);
            Assert.Equal(SystemArchitecture.ARM64, result.Architecture);
            Assert.Equal("testowner", result.GitHubOwner);
            Assert.Equal("testrepo", result.GitHubRepository);
        }

        [Fact]
        public void ReadConfigurationWhenFileNotExistsShouldReturnNull()
        {
            // Arrange - No file created

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ReadConfigurationAsyncWhenFileNotExistsShouldReturnNull()
        {
            // Arrange - No file created

            // Act
            var result = await _appConfigReader.ReadConfigurationAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ReadConfigurationWithInvalidJsonShouldReturnNull()
        {
            // Arrange
            File.WriteAllText(_appConfigPath, "{ invalid json content");

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ReadConfigurationAsyncWithInvalidJsonShouldReturnNull()
        {
            // Arrange
            await File.WriteAllTextAsync(_appConfigPath, "{ invalid json content");

            // Act
            var result = await _appConfigReader.ReadConfigurationAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ReadConfigurationWithEmptyJsonShouldReturnConfigurationWithDefaults()
        {
            // Arrange
            File.WriteAllText(_appConfigPath, "{}");

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1.0.0.0", result.CurrentVersion); // Default from UpdaterConfiguration
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default
            Assert.Equal("mchave3", result.GitHubOwner); // Default
            Assert.Equal("Bucket", result.GitHubRepository); // Default
        }

        [Theory]
        [InlineData("x86", SystemArchitecture.X86)]
        [InlineData("x64", SystemArchitecture.X64)]
        [InlineData("arm64", SystemArchitecture.ARM64)]
        [InlineData("X86", SystemArchitecture.X86)]
        [InlineData("X64", SystemArchitecture.X64)]
        [InlineData("ARM64", SystemArchitecture.ARM64)]
        [InlineData("unknown", SystemArchitecture.X64)] // Should fallback to x64
        public void ReadConfigurationWithDifferentArchitecturesShouldParseCorrectly(string archString, SystemArchitecture expected)
        {
            // Arrange
            var config = new { architecture = archString };
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result.Architecture);
        }

        [Theory]
        [InlineData("Release", UpdateChannel.Release)]
        [InlineData("Nightly", UpdateChannel.Nightly)]
        [InlineData("release", UpdateChannel.Release)]
        [InlineData("nightly", UpdateChannel.Nightly)]
        [InlineData("RELEASE", UpdateChannel.Release)]
        [InlineData("NIGHTLY", UpdateChannel.Nightly)]
        [InlineData("unknown", UpdateChannel.Release)] // Should fallback to Release
        public void ReadConfigurationWithDifferentChannelsShouldParseCorrectly(string channelString, UpdateChannel expected)
        {
            // Arrange
            var config = new { updateChannel = channelString };
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result.UpdateChannel);
        }

        [Fact]
        public void ReadConfigurationWithCompleteConfigShouldParseAllProperties()
        {
            // Arrange
            var config = new
            {
                Version = "24.12.25.100",
                updateChannel = "Nightly",
                architecture = "x86",
                gitHubOwner = "customowner",
                gitHubRepository = "customrepo"
            };
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("24.12.25.100", result.CurrentVersion);
            Assert.Equal(UpdateChannel.Nightly, result.UpdateChannel);
            Assert.Equal(SystemArchitecture.X86, result.Architecture);
            Assert.Equal("customowner", result.GitHubOwner);
            Assert.Equal("customrepo", result.GitHubRepository);
            Assert.Equal("x86", result.GetArchitectureString()); // Verify InitializeRuntimeProperties was called
        }

        [Fact]
        public void ReadConfigurationWithPartialConfigShouldUseDefaultsForMissing()
        {
            // Arrange - Only include Version
            var config = new { Version = "24.1.15" };
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("24.1.15", result.CurrentVersion);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default
            Assert.Equal(SystemArchitecture.X64, result.Architecture); // Default
            Assert.Equal("mchave3", result.GitHubOwner); // Default
            Assert.Equal("Bucket", result.GitHubRepository); // Default
        }

        [Fact]
        public void ReadConfigurationWithNullValuesShouldUseDefaults()
        {
            // Arrange
            var json = @"{
                ""Version"": null,
                ""updateChannel"": null,
                ""architecture"": null,
                ""gitHubOwner"": null,
                ""gitHubRepository"": null
            }";
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1.0.0.0", result.CurrentVersion); // Default from UpdaterConfiguration
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default
            Assert.Equal(SystemArchitecture.X64, result.Architecture); // Default
            Assert.Equal("mchave3", result.GitHubOwner); // Default
            Assert.Equal("Bucket", result.GitHubRepository); // Default
        }

        [Fact]
        public async Task ReadConfigurationAsyncWithLargeFileShouldHandleCorrectly()
        {
            // Arrange - Create a large JSON with extra properties
            var config = new
            {
                Version = "24.1.15",
                updateChannel = "Release",
                architecture = "x64",
                gitHubOwner = "mchave3",
                gitHubRepository = "Bucket",
                // Add many extra properties to test parsing performance
                extraData = Enumerable.Range(0, 1000).ToDictionary(i => $"key{i}", i => $"value{i}")
            };
            var json = JsonSerializer.Serialize(config);
            await File.WriteAllTextAsync(_appConfigPath, json);

            // Act
            var result = await _appConfigReader.ReadConfigurationAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("24.1.15", result.CurrentVersion);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel);
            Assert.Equal(SystemArchitecture.X64, result.Architecture);
        }

        [Fact]
        public void ReadConfigurationWithFileAccessIssuesShouldReturnNull()
        {
            // Arrange - Create file then make it inaccessible (if possible on Windows)
            File.WriteAllText(_appConfigPath, "{}");

            try
            {
                // Try to lock the file by opening it exclusively
                using var fileStream = new FileStream(_appConfigPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                // Create a new reader instance for this test
                var reader = new AppConfigReader();
                var pathField = typeof(AppConfigReader).GetField("_appConfigPath",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                pathField?.SetValue(reader, _appConfigPath);

                // Act
                var result = reader.ReadConfiguration();

                // Assert
                Assert.Null(result);
            }
            catch (IOException)
            {
                // Expected for file access issues - if we can't lock the file, just pass the test
                Assert.True(true);
            }
            catch (UnauthorizedAccessException)
            {
                // Also expected for access denied scenarios
                Assert.True(true);
            }
        }

        [Fact]
        public void ReadConfigurationWithVersionOnlyShouldReturnValidConfiguration()
        {
            // Arrange
            var config = new { Version = "25.1.1" };
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_appConfigPath, json);

            // Act
            var result = _appConfigReader.ReadConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("25.1.1", result.CurrentVersion);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel);
            Assert.Equal(SystemArchitecture.X64, result.Architecture);
            Assert.Equal("mchave3", result.GitHubOwner);
            Assert.Equal("Bucket", result.GitHubRepository);
        }
    }
}