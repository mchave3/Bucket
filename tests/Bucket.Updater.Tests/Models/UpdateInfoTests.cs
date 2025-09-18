namespace Bucket.Updater.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bucket.Updater.Models;
    using Xunit;

    public class UpdateInfoTests
    {
        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            // Act
            var updateInfo = new UpdateInfo();

            // Assert
            Assert.Equal(string.Empty, updateInfo.Version);
            Assert.Equal(string.Empty, updateInfo.TagName);
            Assert.Equal(string.Empty, updateInfo.Name);
            Assert.Equal(string.Empty, updateInfo.Body);
            Assert.Equal(string.Empty, updateInfo.DownloadUrl);
            Assert.Equal(DateTime.MinValue, updateInfo.PublishedAt);
            Assert.False(updateInfo.IsPrerelease);
            Assert.Equal(0, updateInfo.FileSize);
            Assert.Equal(UpdateChannel.Release, updateInfo.Channel);
            Assert.Equal(SystemArchitecture.X64, updateInfo.Architecture);
            Assert.NotNull(updateInfo.Assets);
            Assert.Empty(updateInfo.Assets);
        }

        [Fact]
        public void PropertiesShouldAllowGetAndSet()
        {
            // Arrange
            var updateInfo = new UpdateInfo();
            var publishedDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var testAssets = new List<UpdateAsset>
            {
                new UpdateAsset { Name = "test.msi", Size = 12345 }
            };

            // Act
            updateInfo.Version = "24.1.15";
            updateInfo.TagName = "v24.1.15";
            updateInfo.Name = "Release 24.1.15";
            updateInfo.Body = "Bug fixes and improvements";
            updateInfo.PublishedAt = publishedDate;
            updateInfo.IsPrerelease = true;
            updateInfo.Assets = testAssets;
            updateInfo.DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi";
            updateInfo.FileSize = 52428800; // 50MB
            updateInfo.Channel = UpdateChannel.Nightly;
            updateInfo.Architecture = SystemArchitecture.ARM64;

            // Assert
            Assert.Equal("24.1.15", updateInfo.Version);
            Assert.Equal("v24.1.15", updateInfo.TagName);
            Assert.Equal("Release 24.1.15", updateInfo.Name);
            Assert.Equal("Bug fixes and improvements", updateInfo.Body);
            Assert.Equal(publishedDate, updateInfo.PublishedAt);
            Assert.True(updateInfo.IsPrerelease);
            Assert.Same(testAssets, updateInfo.Assets);
            Assert.Equal("https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi", updateInfo.DownloadUrl);
            Assert.Equal(52428800, updateInfo.FileSize);
            Assert.Equal(UpdateChannel.Nightly, updateInfo.Channel);
            Assert.Equal(SystemArchitecture.ARM64, updateInfo.Architecture);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void VersionShouldAcceptEmptyOrNullValues(string? version)
        {
            // Arrange
            var updateInfo = new UpdateInfo();

            // Act
            updateInfo.Version = version ?? string.Empty;

            // Assert
            Assert.Equal(version ?? string.Empty, updateInfo.Version);
        }

        [Theory]
        [InlineData("24.1.15")]
        [InlineData("v24.1.15")]
        [InlineData("24.1.15.1")]
        [InlineData("24.1.15-Nightly")]
        public void VersionShouldAcceptValidVersionFormats(string version)
        {
            // Arrange
            var updateInfo = new UpdateInfo();

            // Act
            updateInfo.Version = version;

            // Assert
            Assert.Equal(version, updateInfo.Version);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1024)]
        [InlineData(52428800)] // 50MB
        [InlineData(1073741824)] // 1GB
        public void FileSizeShouldAcceptValidFileSizes(long fileSize)
        {
            // Arrange
            var updateInfo = new UpdateInfo();

            // Act
            updateInfo.FileSize = fileSize;

            // Assert
            Assert.Equal(fileSize, updateInfo.FileSize);
        }

        [Fact]
        public void AssetsShouldMaintainListReference()
        {
            // Arrange
            var updateInfo = new UpdateInfo();
            var originalAssets = updateInfo.Assets;
            var newAsset = new UpdateAsset { Name = "NewAsset.msi" };

            // Act
            updateInfo.Assets.Add(newAsset);

            // Assert
            Assert.Same(originalAssets, updateInfo.Assets);
            Assert.Single(updateInfo.Assets);
            Assert.Equal("NewAsset.msi", updateInfo.Assets.First().Name);
        }

        [Fact]
        public void UpdateInfoShouldSupportReleaseChannel()
        {
            // Arrange
            var updateInfo = new UpdateInfo();

            // Act
            updateInfo.Channel = UpdateChannel.Release;
            updateInfo.IsPrerelease = false;

            // Assert
            Assert.Equal(UpdateChannel.Release, updateInfo.Channel);
            Assert.False(updateInfo.IsPrerelease);
        }

        [Fact]
        public void UpdateInfoShouldSupportNightlyChannel()
        {
            // Arrange
            var updateInfo = new UpdateInfo();

            // Act
            updateInfo.Channel = UpdateChannel.Nightly;
            updateInfo.IsPrerelease = true;

            // Assert
            Assert.Equal(UpdateChannel.Nightly, updateInfo.Channel);
            Assert.True(updateInfo.IsPrerelease);
        }

        [Theory]
        [InlineData(SystemArchitecture.X86)]
        [InlineData(SystemArchitecture.X64)]
        [InlineData(SystemArchitecture.ARM64)]
        public void ArchitectureShouldSupportAllSupportedArchitectures(SystemArchitecture architecture)
        {
            // Arrange
            var updateInfo = new UpdateInfo();

            // Act
            updateInfo.Architecture = architecture;

            // Assert
            Assert.Equal(architecture, updateInfo.Architecture);
        }

        [Fact]
        public void UpdateInfoShouldCreateCompleteReleaseInfo()
        {
            // Arrange & Act
            var updateInfo = new UpdateInfo
            {
                Version = "24.1.15",
                TagName = "v24.1.15",
                Name = "Bucket Release 24.1.15",
                Body = "# What's Changed\n- Fixed critical bug\n- Performance improvements",
                PublishedAt = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                IsPrerelease = false,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi",
                FileSize = 52428800,
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset { Name = "Bucket--x64.msi", Size = 52428800, ContentType = "application/x-msi" },
                    new UpdateAsset { Name = "Bucket--x86.msi", Size = 48234496, ContentType = "application/x-msi" },
                    new UpdateAsset { Name = "Bucket--arm64.msi", Size = 51380224, ContentType = "application/x-msi" }
                }
            };

            // Assert
            Assert.Equal("24.1.15", updateInfo.Version);
            Assert.Equal("v24.1.15", updateInfo.TagName);
            Assert.Equal("Bucket Release 24.1.15", updateInfo.Name);
            Assert.Contains("Fixed critical bug", updateInfo.Body, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc), updateInfo.PublishedAt);
            Assert.False(updateInfo.IsPrerelease);
            Assert.Equal("https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi", updateInfo.DownloadUrl);
            Assert.Equal(52428800, updateInfo.FileSize);
            Assert.Equal(UpdateChannel.Release, updateInfo.Channel);
            Assert.Equal(SystemArchitecture.X64, updateInfo.Architecture);
            Assert.Equal(3, updateInfo.Assets.Count);
            Assert.All(updateInfo.Assets, asset => Assert.EndsWith(".msi", asset.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UpdateInfoShouldCreateCompleteNightlyInfo()
        {
            // Arrange & Act
            var updateInfo = new UpdateInfo
            {
                Version = "24.1.15.1",
                TagName = "v24.1.15.1-Nightly",
                Name = "Nightly Build 24.1.15.1",
                Body = "Automated nightly build from dev branch",
                PublishedAt = DateTime.UtcNow,
                IsPrerelease = true,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15.1-Nightly/Bucket--x64.msi",
                FileSize = 52428800,
                Channel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.X64
            };

            // Assert
            Assert.Equal("24.1.15.1", updateInfo.Version);
            Assert.EndsWith("-Nightly", updateInfo.TagName, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Nightly Build", updateInfo.Name, StringComparison.OrdinalIgnoreCase);
            Assert.True(updateInfo.IsPrerelease);
            Assert.Equal(UpdateChannel.Nightly, updateInfo.Channel);
            Assert.Contains("Nightly", updateInfo.DownloadUrl, StringComparison.OrdinalIgnoreCase);
        }
    }
}