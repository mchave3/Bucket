namespace Bucket.Updater.Tests.Models
{
    using System;
    using Bucket.Updater.Models;
    using Xunit;

    public class UpdateAssetTests
    {
        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            // Act
            var updateAsset = new UpdateAsset();

            // Assert
            Assert.Equal(string.Empty, updateAsset.Name);
            Assert.Equal(string.Empty, updateAsset.DownloadUrl);
            Assert.Equal(0, updateAsset.Size);
            Assert.Equal(string.Empty, updateAsset.ContentType);
        }

        [Fact]
        public void PropertiesShouldAllowGetAndSet()
        {
            // Arrange
            var updateAsset = new UpdateAsset();
            var testName = "Bucket--x64.msi";
            var testUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi";
            var testSize = 52428800L; // 50MB
            var testContentType = "application/x-msi";

            // Act
            updateAsset.Name = testName;
            updateAsset.DownloadUrl = testUrl;
            updateAsset.Size = testSize;
            updateAsset.ContentType = testContentType;

            // Assert
            Assert.Equal(testName, updateAsset.Name);
            Assert.Equal(testUrl, updateAsset.DownloadUrl);
            Assert.Equal(testSize, updateAsset.Size);
            Assert.Equal(testContentType, updateAsset.ContentType);
        }

        [Theory]
        [InlineData("Bucket--x64.msi")]
        [InlineData("Bucket--x86.msi")]
        [InlineData("Bucket--arm64.msi")]
        [InlineData("installer.exe")]
        [InlineData("")]
        public void NameShouldAcceptVariousFileNames(string fileName)
        {
            // Arrange
            var updateAsset = new UpdateAsset();

            // Act
            updateAsset.Name = fileName;

            // Assert
            Assert.Equal(fileName, updateAsset.Name);
        }

        [Theory]
        [InlineData("https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi")]
        [InlineData("https://api.github.com/repos/mchave3/Bucket/releases/assets/12345")]
        [InlineData("")]
        [InlineData("invalid-url")]
        public void DownloadUrlShouldAcceptVariousUrls(string url)
        {
            // Arrange
            var updateAsset = new UpdateAsset();

            // Act
            updateAsset.DownloadUrl = url;

            // Assert
            Assert.Equal(url, updateAsset.DownloadUrl);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1024)] // 1KB
        [InlineData(1048576)] // 1MB
        [InlineData(52428800)] // 50MB
        [InlineData(1073741824)] // 1GB
        [InlineData(-1)] // Edge case: negative size
        public void SizeShouldAcceptVariousSizes(long size)
        {
            // Arrange
            var updateAsset = new UpdateAsset();

            // Act
            updateAsset.Size = size;

            // Assert
            Assert.Equal(size, updateAsset.Size);
        }

        [Theory]
        [InlineData("application/x-msi")]
        [InlineData("application/octet-stream")]
        [InlineData("application/zip")]
        [InlineData("text/plain")]
        [InlineData("")]
        public void ContentTypeShouldAcceptVariousContentTypes(string contentType)
        {
            // Arrange
            var updateAsset = new UpdateAsset();

            // Act
            updateAsset.ContentType = contentType;

            // Assert
            Assert.Equal(contentType, updateAsset.ContentType);
        }

        [Fact]
        public void UpdateAssetShouldCreateCompleteX64MsiAsset()
        {
            // Arrange & Act
            var updateAsset = new UpdateAsset
            {
                Name = "Bucket--x64.msi",
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi",
                Size = 52428800,
                ContentType = "application/x-msi"
            };

            // Assert
            Assert.Equal("Bucket--x64.msi", updateAsset.Name);
            Assert.Contains("x64", updateAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".msi", updateAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith("https://", updateAsset.DownloadUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(52428800, updateAsset.Size);
            Assert.Equal("application/x-msi", updateAsset.ContentType);
        }

        [Fact]
        public void UpdateAssetShouldCreateCompleteX86MsiAsset()
        {
            // Arrange & Act
            var updateAsset = new UpdateAsset
            {
                Name = "Bucket--x86.msi",
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x86.msi",
                Size = 48234496,
                ContentType = "application/x-msi"
            };

            // Assert
            Assert.Equal("Bucket--x86.msi", updateAsset.Name);
            Assert.Contains("x86", updateAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".msi", updateAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(48234496, updateAsset.Size);
            Assert.Equal("application/x-msi", updateAsset.ContentType);
        }

        [Fact]
        public void UpdateAssetShouldCreateCompleteARM64MsiAsset()
        {
            // Arrange & Act
            var updateAsset = new UpdateAsset
            {
                Name = "Bucket--arm64.msi",
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--arm64.msi",
                Size = 51380224,
                ContentType = "application/x-msi"
            };

            // Assert
            Assert.Equal("Bucket--arm64.msi", updateAsset.Name);
            Assert.Contains("arm64", updateAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".msi", updateAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(51380224, updateAsset.Size);
            Assert.Equal("application/x-msi", updateAsset.ContentType);
        }

        [Fact]
        public void UpdateAssetShouldHandleEmptyValues()
        {
            // Arrange & Act
            var updateAsset = new UpdateAsset
            {
                Name = "",
                DownloadUrl = "",
                Size = 0,
                ContentType = ""
            };

            // Assert
            Assert.Equal(string.Empty, updateAsset.Name);
            Assert.Equal(string.Empty, updateAsset.DownloadUrl);
            Assert.Equal(0, updateAsset.Size);
            Assert.Equal(string.Empty, updateAsset.ContentType);
        }

        [Theory]
        [InlineData("Bucket--x64.msi", "x64")]
        [InlineData("Bucket--x86.msi", "x86")]
        [InlineData("Bucket--arm64.msi", "arm64")]
        public void NameShouldIndicateArchitecture(string fileName, string expectedArchitecture)
        {
            // Arrange
            var updateAsset = new UpdateAsset();

            // Act
            updateAsset.Name = fileName;

            // Assert
            Assert.Contains(expectedArchitecture, updateAsset.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}