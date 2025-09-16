namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using Moq;
    using Xunit;

    public class UpdateServiceTests
    {
        private readonly UpdateService _testClass;
        private readonly Mock<IConfigurationService> _configurationService;
        private readonly Mock<IGitHubService> _gitHubService;
        private readonly Mock<IInstallationService> _installationService;

        public UpdateServiceTests()
        {
            _configurationService = new Mock<IConfigurationService>();
            _gitHubService = new Mock<IGitHubService>();
            _installationService = new Mock<IInstallationService>();
            _testClass = new UpdateService(_configurationService.Object, _gitHubService.Object, _installationService.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new UpdateService(_configurationService.Object, _gitHubService.Object, _installationService.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullConfigurationService()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateService(default(IConfigurationService), _gitHubService.Object, _installationService.Object));
        }

        [Fact]
        public void CannotConstructWithNullGitHubService()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateService(_configurationService.Object, default(IGitHubService), _installationService.Object));
        }

        [Fact]
        public void CannotConstructWithNullInstallationService()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateService(_configurationService.Object, _gitHubService.Object, default(IInstallationService)));
        }

        [Fact]
        public async Task CanCallCheckForUpdatesAsync()
        {
            // Arrange
            _configurationService.Setup(mock => mock.LoadConfigurationAsync()).ReturnsAsync(new UpdaterConfiguration());
            _gitHubService.Setup(mock => mock.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>())).ReturnsAsync(new UpdateInfo
            {
                Version = "TestValue657930372",
                TagName = "TestValue1552843441",
                Name = "TestValue1371159734",
                Body = "TestValue1058117143",
                PublishedAt = DateTime.UtcNow,
                IsPrerelease = true,
                Assets = new List<UpdateAsset>(),
                DownloadUrl = "TestValue1462005315",
                FileSize = 533896588L,
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64
            });

            // Act
            var result = await _testClass.CheckForUpdatesAsync();

            // Assert
            _configurationService.Verify(mock => mock.LoadConfigurationAsync());
            _gitHubService.Verify(mock => mock.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>()));

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallDownloadUpdateAsync()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "TestValue1324693682",
                TagName = "TestValue202662190",
                Name = "TestValue1069859059",
                Body = "TestValue1393638684",
                PublishedAt = DateTime.UtcNow,
                IsPrerelease = false,
                Assets = new List<UpdateAsset>(),
                DownloadUrl = "TestValue29631619",
                FileSize = 1392376076L,
                Channel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.ARM64
            };
            var progress = new Mock<IProgress<(long downloaded, long total)>>().Object;
            var cancellationToken = CancellationToken.None;

            _gitHubService.Setup(mock => mock.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long downloaded, long total)>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new MemoryStream());

            // Act
            var result = await _testClass.DownloadUpdateAsync(updateInfo, progress, cancellationToken);

            // Assert
            _gitHubService.Verify(mock => mock.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long downloaded, long total)>>(), It.IsAny<CancellationToken>()));

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallDownloadUpdateAsyncWithNullUpdateInfo()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.DownloadUpdateAsync(default(UpdateInfo), new Mock<IProgress<(long downloaded, long total)>>().Object, CancellationToken.None));
        }

        [Fact]
        public async Task CanCallInstallUpdateAsync()
        {
            // Arrange
            var msiFilePath = "TestValue730466196";
            var progress = new Mock<IProgress<string>>().Object;
            var cancellationToken = CancellationToken.None;

            _installationService.Setup(mock => mock.InstallUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _testClass.InstallUpdateAsync(msiFilePath, progress, cancellationToken);

            // Assert
            _installationService.Verify(mock => mock.InstallUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<string>>(), It.IsAny<CancellationToken>()));

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
        public void CanCallCleanupFiles()
        {
            // Arrange
            var downloadPath = "TestValue757312874";

            _installationService.Setup(mock => mock.CleanupDownloadedFiles(It.IsAny<string>())).Verifiable();

            // Act
            _testClass.CleanupFiles(downloadPath);

            // Assert
            _installationService.Verify(mock => mock.CleanupDownloadedFiles(It.IsAny<string>()));

            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CannotCallCleanupFilesWithInvalidDownloadPath(string value)
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.CleanupFiles(value));
        }

        [Fact]
        public void CanCallCleanupAllTemporaryFiles()
        {
            // Arrange
            _installationService.Setup(mock => mock.CleanupAllTemporaryFiles()).Verifiable();

            // Act
            _testClass.CleanupAllTemporaryFiles();

            // Assert
            _installationService.Verify(mock => mock.CleanupAllTemporaryFiles());

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetConfiguration()
        {
            // Arrange
            _configurationService.Setup(mock => mock.GetConfiguration()).Returns(new UpdaterConfiguration());

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            _configurationService.Verify(mock => mock.GetConfiguration());

            throw new NotImplementedException("Create or modify test");
        }
    }
}