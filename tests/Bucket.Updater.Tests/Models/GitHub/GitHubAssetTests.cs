namespace Bucket.Updater.Tests.Models.GitHub
{
    using System;
    using Bucket.Updater.Models.GitHub;
    using Xunit;

    public class GitHubAssetTests
    {
        private readonly GitHubAsset _testClass;

        public GitHubAssetTests()
        {
            _testClass = new GitHubAsset();
        }

        [Fact]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = 2121830755L;

            // Act
            _testClass.Id = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Id);
        }

        [Fact]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = "TestValue1680494134";

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Name);
        }

        [Fact]
        public void CanSetAndGetBrowserDownloadUrl()
        {
            // Arrange
            var testValue = "TestValue329939568";

            // Act
            _testClass.BrowserDownloadUrl = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.BrowserDownloadUrl);
        }

        [Fact]
        public void CanSetAndGetSize()
        {
            // Arrange
            var testValue = 1064243655L;

            // Act
            _testClass.Size = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Size);
        }

        [Fact]
        public void CanSetAndGetContentType()
        {
            // Arrange
            var testValue = "TestValue1819937180";

            // Act
            _testClass.ContentType = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.ContentType);
        }
    }
}