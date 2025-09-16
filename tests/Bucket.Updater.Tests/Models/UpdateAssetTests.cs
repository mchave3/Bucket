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
            var testValue = "TestValue453527662";

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Name);
        }

        [Fact]
        public void CanSetAndGetDownloadUrl()
        {
            // Arrange
            var testValue = "TestValue469254261";

            // Act
            _testClass.DownloadUrl = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.DownloadUrl);
        }

        [Fact]
        public void CanSetAndGetSize()
        {
            // Arrange
            var testValue = 990776636L;

            // Act
            _testClass.Size = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Size);
        }

        [Fact]
        public void CanSetAndGetContentType()
        {
            // Arrange
            var testValue = "TestValue281395077";

            // Act
            _testClass.ContentType = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.ContentType);
        }
    }
}