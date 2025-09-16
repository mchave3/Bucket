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
            var testValue = "TestValue2065854430";

            // Act
            _testClass.Version = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Version);
        }

        [Fact]
        public void CanSetAndGetTagName()
        {
            // Arrange
            var testValue = "TestValue661530158";

            // Act
            _testClass.TagName = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.TagName);
        }

        [Fact]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = "TestValue348211891";

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Name);
        }

        [Fact]
        public void CanSetAndGetBody()
        {
            // Arrange
            var testValue = "TestValue345661239";

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
            var testValue = true;

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
            var testValue = "TestValue948937811";

            // Act
            _testClass.DownloadUrl = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.DownloadUrl);
        }

        [Fact]
        public void CanSetAndGetFileSize()
        {
            // Arrange
            var testValue = 439872591L;

            // Act
            _testClass.FileSize = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.FileSize);
        }

        [Fact]
        public void CanSetAndGetChannel()
        {
            // Arrange
            var testValue = UpdateChannel.Nightly;

            // Act
            _testClass.Channel = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Channel);
        }

        [Fact]
        public void CanSetAndGetArchitecture()
        {
            // Arrange
            var testValue = SystemArchitecture.X64;

            // Act
            _testClass.Architecture = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Architecture);
        }
    }
}