namespace Bucket.Updater.Tests.Models
{
    using System;
    using System.Runtime.InteropServices;
    using Bucket.Updater.Models;
    using Xunit;

    public class UpdaterConfigurationTests
    {
        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            // Act
            var config = new UpdaterConfiguration();

            // Assert
            Assert.Equal(UpdateChannel.Release, config.UpdateChannel);
            Assert.Equal(SystemArchitecture.X64, config.Architecture);
            Assert.Equal("mchave3", config.GitHubOwner);
            Assert.Equal("Bucket", config.GitHubRepository);
            Assert.Equal("1.0.0.0", config.CurrentVersion);
            Assert.Equal(DateTime.MinValue, config.LastUpdateCheck);
        }

        [Fact]
        public void PropertiesShouldAllowGetAndSet()
        {
            // Arrange
            var config = new UpdaterConfiguration();
            var lastUpdateCheck = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            // Act
            config.UpdateChannel = UpdateChannel.Nightly;
            config.Architecture = SystemArchitecture.ARM64;
            config.GitHubOwner = "testowner";
            config.GitHubRepository = "testrepo";
            config.CurrentVersion = "24.1.15";
            config.LastUpdateCheck = lastUpdateCheck;

            // Assert
            Assert.Equal(UpdateChannel.Nightly, config.UpdateChannel);
            Assert.Equal(SystemArchitecture.ARM64, config.Architecture);
            Assert.Equal("testowner", config.GitHubOwner);
            Assert.Equal("testrepo", config.GitHubRepository);
            Assert.Equal("24.1.15", config.CurrentVersion);
            Assert.Equal(lastUpdateCheck, config.LastUpdateCheck);
        }

        [Theory]
        [InlineData(UpdateChannel.Release)]
        [InlineData(UpdateChannel.Nightly)]
        public void UpdateChannelShouldSupportAllChannels(UpdateChannel channel)
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.UpdateChannel = channel;

            // Assert
            Assert.Equal(channel, config.UpdateChannel);
        }

        [Theory]
        [InlineData(SystemArchitecture.X86)]
        [InlineData(SystemArchitecture.X64)]
        [InlineData(SystemArchitecture.ARM64)]
        public void ArchitectureShouldSupportAllArchitectures(SystemArchitecture architecture)
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.Architecture = architecture;

            // Assert
            Assert.Equal(architecture, config.Architecture);
        }

        [Theory]
        [InlineData("1.0.0.0")]
        [InlineData("24.1.15")]
        [InlineData("24.1.15.1")]
        [InlineData("24.1.15-Nightly")]
        public void CurrentVersionShouldAcceptValidVersionFormats(string version)
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.CurrentVersion = version;

            // Assert
            Assert.Equal(version, config.CurrentVersion);
        }

        [Theory]
        [InlineData("mchave3", "Bucket")]
        [InlineData("microsoft", "vscode")]
        [InlineData("owner", "repo")]
        public void GitHubRepositoryInfoShouldAcceptValidValues(string owner, string repo)
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.GitHubOwner = owner;
            config.GitHubRepository = repo;

            // Assert
            Assert.Equal(owner, config.GitHubOwner);
            Assert.Equal(repo, config.GitHubRepository);
        }

        [Fact]
        public void InitializeRuntimePropertiesShouldDetectCurrentArchitecture()
        {
            // Arrange
            var config = new UpdaterConfiguration();
            var expectedArchitecture = RuntimeInformation.ProcessArchitecture switch
            {
                System.Runtime.InteropServices.Architecture.X86 => SystemArchitecture.X86,
                System.Runtime.InteropServices.Architecture.X64 => SystemArchitecture.X64,
                System.Runtime.InteropServices.Architecture.Arm64 => SystemArchitecture.ARM64,
                _ => SystemArchitecture.X64
            };

            // Act
            config.InitializeRuntimeProperties();

            // Assert
            Assert.Equal(expectedArchitecture, config.Architecture);
        }

        [Theory]
        [InlineData(SystemArchitecture.X86, "x86")]
        [InlineData(SystemArchitecture.X64, "x64")]
        [InlineData(SystemArchitecture.ARM64, "arm64")]
        public void GetArchitectureStringShouldReturnCorrectString(SystemArchitecture architecture, string expected)
        {
            // Arrange
            var config = new UpdaterConfiguration { Architecture = architecture };

            // Act
            var result = config.GetArchitectureString();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetArchitectureStringShouldReturnDefaultForUnknownArchitecture()
        {
            // Arrange
            var config = new UpdaterConfiguration { Architecture = (SystemArchitecture)999 };

            // Act
            var result = config.GetArchitectureString();

            // Assert
            Assert.Equal("x64", result);
        }

        [Fact]
        public void UpdaterConfigurationShouldCreateCompleteReleaseConfiguration()
        {
            // Arrange & Act
            var config = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                CurrentVersion = "24.1.15",
                LastUpdateCheck = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
            };

            // Assert
            Assert.Equal(UpdateChannel.Release, config.UpdateChannel);
            Assert.Equal(SystemArchitecture.X64, config.Architecture);
            Assert.Equal("mchave3", config.GitHubOwner);
            Assert.Equal("Bucket", config.GitHubRepository);
            Assert.Equal("24.1.15", config.CurrentVersion);
            Assert.Equal("x64", config.GetArchitectureString());
            Assert.NotEqual(DateTime.MinValue, config.LastUpdateCheck);
        }

        [Fact]
        public void UpdaterConfigurationShouldCreateCompleteNightlyConfiguration()
        {
            // Arrange & Act
            var config = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.ARM64,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                CurrentVersion = "24.1.15.1-Nightly",
                LastUpdateCheck = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(UpdateChannel.Nightly, config.UpdateChannel);
            Assert.Equal(SystemArchitecture.ARM64, config.Architecture);
            Assert.Equal("mchave3", config.GitHubOwner);
            Assert.Equal("Bucket", config.GitHubRepository);
            Assert.Contains("Nightly", config.CurrentVersion, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("arm64", config.GetArchitectureString());
        }

        [Fact]
        public void LastUpdateCheckShouldTrackUpdateTiming()
        {
            // Arrange
            var config = new UpdaterConfiguration();
            var beforeUpdate = DateTime.UtcNow;

            // Act
            config.LastUpdateCheck = DateTime.UtcNow;
            var afterUpdate = DateTime.UtcNow;

            // Assert
            Assert.True(config.LastUpdateCheck >= beforeUpdate);
            Assert.True(config.LastUpdateCheck <= afterUpdate);
            Assert.NotEqual(DateTime.MinValue, config.LastUpdateCheck);
        }

        [Fact]
        public void ConfigurationShouldSupportX86Architecture()
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.Architecture = SystemArchitecture.X86;

            // Assert
            Assert.Equal(SystemArchitecture.X86, config.Architecture);
            Assert.Equal("x86", config.GetArchitectureString());
        }

        [Fact]
        public void ConfigurationShouldSupportX64Architecture()
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.Architecture = SystemArchitecture.X64;

            // Assert
            Assert.Equal(SystemArchitecture.X64, config.Architecture);
            Assert.Equal("x64", config.GetArchitectureString());
        }

        [Fact]
        public void ConfigurationShouldSupportARM64Architecture()
        {
            // Arrange
            var config = new UpdaterConfiguration();

            // Act
            config.Architecture = SystemArchitecture.ARM64;

            // Assert
            Assert.Equal(SystemArchitecture.ARM64, config.Architecture);
            Assert.Equal("arm64", config.GetArchitectureString());
        }
    }
}