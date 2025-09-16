namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Services;
    using Moq;
    using Xunit;

    public class InstallationServiceTests
    {
        private readonly InstallationService _testClass;

        public InstallationServiceTests()
        {
            _testClass = new InstallationService();
        }

        [Fact]
        public async Task CanCallInstallUpdateAsync()
        {
            // Arrange
            var msiFilePath = "TestValue348402166";
            var progress = new Mock<IProgress<string>>().Object;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _testClass.InstallUpdateAsync(msiFilePath, progress, cancellationToken);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallInstallUpdateAsyncWithInvalidMsiFilePath(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.InstallUpdateAsync(value, new Mock<IProgress<string>>().Object, CancellationToken.None));
        }

        [Fact]
        public async Task CanCallValidateMsiFileAsync()
        {
            // Arrange
            var msiFilePath = "TestValue1353373145";

            // Act
            var result = await _testClass.ValidateMsiFileAsync(msiFilePath);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallValidateMsiFileAsyncWithInvalidMsiFilePath(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.ValidateMsiFileAsync(value));
        }

        [Fact]
        public async Task CanCallEnsureBucketProcessStoppedAsync()
        {
            // Arrange
            var progress = new Mock<IProgress<string>>().Object;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _testClass.EnsureBucketProcessStoppedAsync(progress, cancellationToken);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallCleanupDownloadedFiles()
        {
            // Arrange
            var downloadPath = "TestValue79200411";

            // Act
            _testClass.CleanupDownloadedFiles(downloadPath);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CannotCallCleanupDownloadedFilesWithInvalidDownloadPath(string value)
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.CleanupDownloadedFiles(value));
        }

        [Fact]
        public void CanCallCleanupAllTemporaryFiles()
        {
            // Act
            _testClass.CleanupAllTemporaryFiles();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}