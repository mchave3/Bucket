using Bucket.Updater.Models;
using FluentAssertions;

namespace Bucket.Updater.Tests.Models
{
    public class UpdateAssetTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAllPropertiesWithDefaults()
        {
            // Act
            var updateAsset = new UpdateAsset();

            // Assert
            updateAsset.Name.Should().BeEmpty();
            updateAsset.DownloadUrl.Should().BeEmpty();
            updateAsset.Size.Should().Be(0);
            updateAsset.ContentType.Should().BeEmpty();
        }
    }
}