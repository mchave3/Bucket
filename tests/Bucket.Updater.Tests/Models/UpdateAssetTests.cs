namespace Bucket.Updater.Tests.Models
{
    using System;
    using Bucket.Updater.Models;
    using Xunit;

    public class UpdateAssetTests
    {
        private readonly UpdateAsset _testClass;

        public UpdateAssetTests()
        {
            _testClass = new UpdateAsset();
        }

        [Fact]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = "TestValue253584009";

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Name);
        }

        [Fact]
        public void CanSetAndGetDownloadUrl()
        {
            // Arrange
            var testValue = "TestValue2009012526";

            // Act
            _testClass.DownloadUrl = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.DownloadUrl);
        }

        [Fact]
        public void CanSetAndGetSize()
        {
            // Arrange
            var testValue = 2092816064L;

            // Act
            _testClass.Size = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Size);
        }

        [Fact]
        public void CanSetAndGetContentType()
        {
            // Arrange
            var testValue = "TestValue2142183205";

            // Act
            _testClass.ContentType = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.ContentType);
        }
    }
}