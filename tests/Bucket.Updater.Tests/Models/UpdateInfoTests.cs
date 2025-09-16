namespace Bucket.Updater.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Bucket.Updater.Models;
    using Xunit;

    public class UpdateInfoTests
    {
        private readonly UpdateInfo _testClass;

        public UpdateInfoTests()
        {
            _testClass = new UpdateInfo();
        }

        [Fact]
        public void CanSetAndGetVersion()
        {
            // Arrange
            var testValue = "TestValue1530410118";

            // Act
            _testClass.Version = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Version);
        }

        [Fact]
        public void CanSetAndGetTagName()
        {
            // Arrange
            var testValue = "TestValue1979245194";

            // Act
            _testClass.TagName = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.TagName);
        }

        [Fact]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = "TestValue1737233652";

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Name);
        }

        [Fact]
        public void CanSetAndGetBody()
        {
            // Arrange
            var testValue = "TestValue1347245476";

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
        public void CanSetAndGetIsPrerelease()
        {
            // Arrange
            var testValue = false;

            // Act
            _testClass.IsPrerelease = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.IsPrerelease);
        }

        [Fact]
        public void CanSetAndGetAssets()
        {
            // Arrange
            var testValue = new List<UpdateAsset>();

            // Act
            _testClass.Assets = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Assets);
        }

        [Fact]
        public void CanSetAndGetDownloadUrl()
        {
            // Arrange
            var testValue = "TestValue1433272941";

            // Act
            _testClass.DownloadUrl = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.DownloadUrl);
        }

        [Fact]
        public void CanSetAndGetFileSize()
        {
            // Arrange
            var testValue = 394219525L;

            // Act
            _testClass.FileSize = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.FileSize);
        }

        [Fact]
        public void CanSetAndGetChannel()
        {
            // Arrange
            var testValue = UpdateChannel.Release;

            // Act
            _testClass.Channel = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Channel);
        }

        [Fact]
        public void CanSetAndGetArchitecture()
        {
            // Arrange
            var testValue = SystemArchitecture.ARM64;

            // Act
            _testClass.Architecture = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Architecture);
        }
    }
}