namespace Bucket.Updater.Tests.Models
{
    using System;
    using Bucket.Updater.Models;
    using Xunit;

    public class UpdaterConfigurationTests
    {
        private readonly UpdaterConfiguration _testClass;

        public UpdaterConfigurationTests()
        {
            _testClass = new UpdaterConfiguration();
        }

        [Fact]
        public void CanCallInitializeRuntimeProperties()
        {
            // Act
            _testClass.InitializeRuntimeProperties();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetArchitectureString()
        {
            // Act
            var result = _testClass.GetArchitectureString();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanSetAndGetUpdateChannel()
        {
            // Arrange
            var testValue = UpdateChannel.Nightly;

            // Act
            _testClass.UpdateChannel = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.UpdateChannel);
        }

        [Fact]
        public void CanSetAndGetArchitecture()
        {
            // Arrange
            var testValue = SystemArchitecture.X86;

            // Act
            _testClass.Architecture = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Architecture);
        }

        [Fact]
        public void CanSetAndGetGitHubOwner()
        {
            // Arrange
            var testValue = "TestValue1349354479";

            // Act
            _testClass.GitHubOwner = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.GitHubOwner);
        }

        [Fact]
        public void CanSetAndGetGitHubRepository()
        {
            // Arrange
            var testValue = "TestValue1436378387";

            // Act
            _testClass.GitHubRepository = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.GitHubRepository);
        }

        [Fact]
        public void CanSetAndGetCurrentVersion()
        {
            // Arrange
            var testValue = "TestValue1892784408";

            // Act
            _testClass.CurrentVersion = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.CurrentVersion);
        }

        [Fact]
        public void CanSetAndGetLastUpdateCheck()
        {
            // Arrange
            var testValue = DateTime.UtcNow;

            // Act
            _testClass.LastUpdateCheck = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.LastUpdateCheck);
        }
    }
}