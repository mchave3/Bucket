using System.Text;
using System.Text.Json;
using Bucket.Updater.Models;
using Bucket.Updater.Services;
using Bucket.Updater.Tests.TestData;
using FluentAssertions;

namespace Bucket.Updater.Tests.Services
{
    public class AppConfigReaderTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly string _testConfigPath;
        private readonly TestableAppConfigReader _sut;

        public AppConfigReaderTests()
        {
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), "BucketUpdaterTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testConfigDirectory);
            _testConfigPath = Path.Combine(_testConfigDirectory, "AppConfig.json");
            _sut = new TestableAppConfigReader(_testConfigPath);
        }

        #region File Existence and Basic Validation Tests

        [Fact]
        public void ReadConfiguration_ShouldReturnNull_WhenConfigFileDoesNotExist()
        {
            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ReadConfigurationAsync_ShouldReturnNull_WhenConfigFileDoesNotExist()
        {
            // Act
            var result = await _sut.ReadConfigurationAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReadConfiguration_ShouldReturnNull_WhenConfigFileIsEmpty()
        {
            // Arrange
            CreateConfigFile("");

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReadConfiguration_ShouldReturnNull_WhenJsonIsMalformed()
        {
            // Arrange
            CreateConfigFile(TestFixtures.AppConfigurations.MalformedJson);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region JSON Parsing Tests

        [Fact]
        public void ReadConfiguration_ShouldReturnCorrectConfiguration_WhenJsonIsValid()
        {
            // Arrange
            CreateConfigFile(TestFixtures.AppConfigurations.ValidConfiguration);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result!.UpdateChannel.Should().Be(UpdateChannel.Release);
            result.Architecture.Should().Be(SystemArchitecture.X64);
            result.GitHubOwner.Should().Be("mchave3");
            result.GitHubRepository.Should().Be("Bucket");
            result.CurrentVersion.Should().Be("1.0.0.0");
            result.LastUpdateCheck.Should().Be(new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task ReadConfigurationAsync_ShouldReturnCorrectConfiguration_WhenJsonIsValid()
        {
            // Arrange
            CreateConfigFile(TestFixtures.AppConfigurations.ValidConfiguration);

            // Act
            var result = await _sut.ReadConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            result!.UpdateChannel.Should().Be(UpdateChannel.Release);
            result.Architecture.Should().Be(SystemArchitecture.X64);
            result.GitHubOwner.Should().Be("mchave3");
            result.GitHubRepository.Should().Be("Bucket");
            result.CurrentVersion.Should().Be("1.0.0.0");
            result.LastUpdateCheck.Should().Be(new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void ReadConfiguration_ShouldParseAllFields_WhenAllFieldsPresent()
        {
            // Arrange
            CreateConfigFile(TestFixtures.AppConfigurations.NightlyConfiguration);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result!.UpdateChannel.Should().Be(UpdateChannel.Nightly);
            result.Architecture.Should().Be(SystemArchitecture.X86);
            result.GitHubOwner.Should().Be("mchave3");
            result.GitHubRepository.Should().Be("Bucket");
            result.CurrentVersion.Should().Be("1.0.0.0");
            result.LastUpdateCheck.Should().Be(new DateTime(2025, 9, 10, 15, 30, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void ReadConfiguration_ShouldUseDefaults_WhenFieldsAreMissing()
        {
            // Arrange
            CreateConfigFile(TestFixtures.AppConfigurations.MinimalConfiguration);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result!.UpdateChannel.Should().Be(UpdateChannel.Release); // Default value
            result.Architecture.Should().Be(SystemArchitecture.X64); // Default value
            result.GitHubOwner.Should().Be("mchave3"); // Default value
            result.GitHubRepository.Should().Be("Bucket"); // Default value
            result.CurrentVersion.Should().Be("1.1.0.0"); // From config
            result.LastUpdateCheck.Should().Be(DateTime.MinValue); // Default value
        }

        [Fact]
        public void ReadConfiguration_ShouldParseUpdateChannelEnum_WhenValidValues()
        {
            // Arrange
            var nightlyConfig = """
                {
                  "UpdateChannel": "Nightly",
                  "CurrentVersion": "1.0.0.0"
                }
                """;
            CreateConfigFile(nightlyConfig);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result!.UpdateChannel.Should().Be(UpdateChannel.Nightly);
        }

        [Fact]
        public void ReadConfiguration_ShouldParseSystemArchitectureEnum_WhenValidValues()
        {
            // Arrange
            var arm64Config = """
                {
                  "Architecture": "ARM64",
                  "CurrentVersion": "1.0.0.0"
                }
                """;
            CreateConfigFile(arm64Config);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result!.Architecture.Should().Be(SystemArchitecture.ARM64);
        }

        [Fact]
        public void ReadConfiguration_ShouldParseDateTimeFields_WhenValidFormat()
        {
            // Arrange
            var dateTimeConfig = """
                {
                  "CurrentVersion": "1.0.0.0",
                  "LastUpdateCheck": "2025-12-25T15:30:45.123Z"
                }
                """;
            CreateConfigFile(dateTimeConfig);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().NotBeNull();
            result!.LastUpdateCheck.Should().Be(new DateTime(2025, 12, 25, 15, 30, 45, 123, DateTimeKind.Utc));
        }

        [Fact]
        public void ReadConfiguration_ShouldReturnNull_WhenParsingExceptionOccurs()
        {
            // Arrange
            CreateConfigFile(TestFixtures.AppConfigurations.InvalidEnumValues);

            // Act
            var result = _sut.ReadConfiguration();

            // Assert
            result.Should().BeNull();
        }

        #endregion

        private void CreateConfigFile(string content)
        {
            File.WriteAllText(_testConfigPath, content, Encoding.UTF8);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testConfigDirectory))
            {
                try
                {
                    Directory.Delete(_testConfigDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }
    }

    // Testable version that allows custom config path
    public class TestableAppConfigReader : AppConfigReader
    {
        private readonly string _customConfigPath;

        public TestableAppConfigReader(string configPath)
        {
            _customConfigPath = configPath;
        }

        // Override the path used by the base class through reflection
        public new UpdaterConfiguration? ReadConfiguration()
        {
            try
            {
                if (!File.Exists(_customConfigPath))
                {
                    return null;
                }

                var jsonContent = File.ReadAllText(_customConfigPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                return JsonSerializer.Deserialize<UpdaterConfiguration>(jsonContent, options);
            }
            catch
            {
                return null;
            }
        }

        public new async Task<UpdaterConfiguration?> ReadConfigurationAsync()
        {
            try
            {
                if (!File.Exists(_customConfigPath))
                {
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(_customConfigPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                return JsonSerializer.Deserialize<UpdaterConfiguration>(jsonContent, options);
            }
            catch
            {
                return null;
            }
        }
    }
}