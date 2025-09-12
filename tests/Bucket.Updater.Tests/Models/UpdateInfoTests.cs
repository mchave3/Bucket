using Bucket.Updater.Models;
using FluentAssertions;

namespace Bucket.Updater.Tests.Models
{
    public class UpdateInfoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAllPropertiesWithDefaults()
        {
            // Act
            var updateInfo = new UpdateInfo();

            // Assert
            updateInfo.Version.Should().BeEmpty();
            updateInfo.Body.Should().BeEmpty();
            updateInfo.PublishedAt.Should().Be(default(DateTime));
            updateInfo.IsPrerelease.Should().BeFalse();
            updateInfo.Assets.Should().NotBeNull().And.BeEmpty();
            updateInfo.DownloadUrl.Should().BeEmpty();
            updateInfo.FileSize.Should().Be(0);
        }

        [Fact]
        public void Properties_ShouldAllowAssignment_WhenValidValuesProvided()
        {
            // Arrange
            var version = "1.2.0";
            var body = "Test release notes";
            var publishedAt = new DateTime(2025, 9, 12, 10, 0, 0, DateTimeKind.Utc);
            var isPrerelease = true;
            var downloadUrl = "https://example.com/test.msi";
            var fileSize = 1000000L;
            var assets = new List<UpdateAsset>
            {
                new UpdateAsset
                {
                    Name = "test.msi",
                    DownloadUrl = downloadUrl,
                    Size = fileSize
                }
            };

            // Act
            var updateInfo = new UpdateInfo
            {
                Version = version,
                Body = body,
                PublishedAt = publishedAt,
                IsPrerelease = isPrerelease,
                DownloadUrl = downloadUrl,
                FileSize = fileSize,
                Assets = assets
            };

            // Assert
            updateInfo.Version.Should().Be(version);
            updateInfo.Body.Should().Be(body);
            updateInfo.PublishedAt.Should().Be(publishedAt);
            updateInfo.IsPrerelease.Should().Be(isPrerelease);
            updateInfo.DownloadUrl.Should().Be(downloadUrl);
            updateInfo.FileSize.Should().Be(fileSize);
            updateInfo.Assets.Should().BeEquivalentTo(assets);
        }
    }
}