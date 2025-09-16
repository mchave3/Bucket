namespace Bucket.Updater.Tests.Models.GitHub
{
    using System;
    using System.Collections.Generic;
    using Bucket.Updater.Models.GitHub;
    using Xunit;

    public class GitHubReleaseTests
    {
        private readonly GitHubRelease _testClass;

        public GitHubReleaseTests()
        {
            _testClass = new GitHubRelease();
        }

        [Fact]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = 2134331627L;

            // Act
            _testClass.Id = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Id);
        }

        [Fact]
        public void CanSetAndGetTagName()
        {
            // Arrange
            var testValue = "TestValue110477464";

            // Act
            _testClass.TagName = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.TagName);
        }

        [Fact]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = "TestValue1659749158";

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Name);
        }

        [Fact]
        public void CanSetAndGetBody()
        {
            // Arrange
            var testValue = "TestValue2063643112";

            // Act
            _testClass.Body = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Body);
        }

        [Fact]
        public void CanSetAndGetPublishedAt()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.PublishedAt = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.PublishedAt);
        }

        [Fact]
        public void CanSetAndGetPrerelease()
        {
            // Arrange
            var testValue = false;

            // Act
            _testClass.Prerelease = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Prerelease);
        }

        [Fact]
        public void CanSetAndGetAssets()
        {
            // Arrange
            var testValue = new List<GitHubAsset>();

            // Act
            _testClass.Assets = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Assets);
        }
    }
}