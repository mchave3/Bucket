namespace Bucket.Updater.Tests.Models.GitHub
{
    using System;
    using Bucket.Updater.Models.GitHub;
    using Xunit;

    public class GitHubAssetTests
    {
        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            // Act
            var gitHubAsset = new GitHubAsset();

            // Assert
            Assert.Equal(0, gitHubAsset.Id);
            Assert.Equal(string.Empty, gitHubAsset.Name);
            Assert.Equal(string.Empty, gitHubAsset.BrowserDownloadUrl);
            Assert.Equal(0, gitHubAsset.Size);
            Assert.Equal(string.Empty, gitHubAsset.ContentType);
        }

        [Fact]
        public void PropertiesShouldAllowGetAndSet()
        {
            // Arrange
            var gitHubAsset = new GitHubAsset();
            var testId = 123456789L;
            var testName = "Bucket--x64.msi";
            var testUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi";
            var testSize = 52428800L;
            var testContentType = "application/x-msi";

            // Act
            gitHubAsset.Id = testId;
            gitHubAsset.Name = testName;
            gitHubAsset.BrowserDownloadUrl = testUrl;
            gitHubAsset.Size = testSize;
            gitHubAsset.ContentType = testContentType;

            // Assert
            Assert.Equal(testId, gitHubAsset.Id);
            Assert.Equal(testName, gitHubAsset.Name);
            Assert.Equal(testUrl, gitHubAsset.BrowserDownloadUrl);
            Assert.Equal(testSize, gitHubAsset.Size);
            Assert.Equal(testContentType, gitHubAsset.ContentType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(123456789)]
        [InlineData(999999999999)]
        public void IdShouldAcceptValidValues(long id)
        {
            // Arrange
            var gitHubAsset = new GitHubAsset();

            // Act
            gitHubAsset.Id = id;

            // Assert
            Assert.Equal(id, gitHubAsset.Id);
        }

        [Theory]
        [InlineData("Bucket--x64.msi")]
        [InlineData("Bucket--x86.msi")]
        [InlineData("Bucket--arm64.msi")]
        [InlineData("Source.zip")]
        [InlineData("")]
        public void NameShouldAcceptVariousFileNames(string name)
        {
            // Arrange
            var gitHubAsset = new GitHubAsset();

            // Act
            gitHubAsset.Name = name;

            // Assert
            Assert.Equal(name, gitHubAsset.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1024)]
        [InlineData(52428800)]
        [InlineData(1073741824)]
        public void SizeShouldAcceptValidFileSizes(long size)
        {
            // Arrange
            var gitHubAsset = new GitHubAsset();

            // Act
            gitHubAsset.Size = size;

            // Assert
            Assert.Equal(size, gitHubAsset.Size);
        }

        [Theory]
        [InlineData("application/x-msi")]
        [InlineData("application/zip")]
        [InlineData("application/octet-stream")]
        [InlineData("text/plain")]
        [InlineData("")]
        public void ContentTypeShouldAcceptValidMimeTypes(string contentType)
        {
            // Arrange
            var gitHubAsset = new GitHubAsset();

            // Act
            gitHubAsset.ContentType = contentType;

            // Assert
            Assert.Equal(contentType, gitHubAsset.ContentType);
        }

        [Fact]
        public void GitHubAssetShouldCreateCompleteX64MsiAsset()
        {
            // Arrange & Act
            var gitHubAsset = new GitHubAsset
            {
                Id = 123456789,
                Name = "Bucket--x64.msi",
                BrowserDownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.1.15/Bucket--x64.msi",
                Size = 52428800,
                ContentType = "application/x-msi"
            };

            // Assert
            Assert.Equal(123456789, gitHubAsset.Id);
            Assert.Equal("Bucket--x64.msi", gitHubAsset.Name);
            Assert.Contains("x64", gitHubAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".msi", gitHubAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith("https://", gitHubAsset.BrowserDownloadUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("/releases/download/", gitHubAsset.BrowserDownloadUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(52428800, gitHubAsset.Size);
            Assert.Equal("application/x-msi", gitHubAsset.ContentType);
        }

        [Fact]
        public void GitHubAssetShouldCreateCompleteSourceCodeAsset()
        {
            // Arrange & Act
            var gitHubAsset = new GitHubAsset
            {
                Id = 987654321,
                Name = "Source code (zip)",
                BrowserDownloadUrl = "https://github.com/mchave3/Bucket/archive/refs/tags/v24.1.15.zip",
                Size = 12345678,
                ContentType = "application/zip"
            };

            // Assert
            Assert.Equal(987654321, gitHubAsset.Id);
            Assert.Equal("Source code (zip)", gitHubAsset.Name);
            Assert.Contains("zip", gitHubAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("/archive/", gitHubAsset.BrowserDownloadUrl, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".zip", gitHubAsset.BrowserDownloadUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(12345678, gitHubAsset.Size);
            Assert.Equal("application/zip", gitHubAsset.ContentType);
        }

        [Theory]
        [InlineData("Bucket--x64.msi", "x64")]
        [InlineData("Bucket--x86.msi", "x86")]
        [InlineData("Bucket--arm64.msi", "arm64")]
        public void NameShouldIndicateArchitectureForMsiFiles(string name, string expectedArchitecture)
        {
            // Arrange
            var gitHubAsset = new GitHubAsset();

            // Act
            gitHubAsset.Name = name;

            // Assert
            Assert.Contains(expectedArchitecture, gitHubAsset.Name, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".msi", gitHubAsset.Name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GitHubAssetShouldHandleEmptyValues()
        {
            // Arrange & Act
            var gitHubAsset = new GitHubAsset
            {
                Id = 0,
                Name = "",
                BrowserDownloadUrl = "",
                Size = 0,
                ContentType = ""
            };

            // Assert
            Assert.Equal(0, gitHubAsset.Id);
            Assert.Equal(string.Empty, gitHubAsset.Name);
            Assert.Equal(string.Empty, gitHubAsset.BrowserDownloadUrl);
            Assert.Equal(0, gitHubAsset.Size);
            Assert.Equal(string.Empty, gitHubAsset.ContentType);
        }
    }
}