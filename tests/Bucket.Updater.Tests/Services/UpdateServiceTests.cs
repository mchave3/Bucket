using Bucket.Updater.Models;
using Bucket.Updater.Services;
using Bucket.Updater.Tests.TestData;
using FluentAssertions;
using Moq;

namespace Bucket.Updater.Tests.Services
{
    public class UpdateServiceTests
    {
        private readonly Mock<IConfigurationService> _mockConfigurationService;
        private readonly Mock<IGitHubService> _mockGitHubService;
        private readonly Mock<IInstallationService> _mockInstallationService;
        private readonly UpdateService _sut;

        public UpdateServiceTests()
        {
            _mockConfigurationService = new Mock<IConfigurationService>();
            _mockGitHubService = new Mock<IGitHubService>();
            _mockInstallationService = new Mock<IInstallationService>();
            _sut = new UpdateService(_mockConfigurationService.Object, _mockGitHubService.Object, _mockInstallationService.Object);
        }

        #region CheckForUpdatesAsync Tests

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldLoadConfiguration_WhenCalled()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            _mockConfigurationService.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(configuration);
            
            _mockGitHubService.Setup(x => x.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>()))
                .ReturnsAsync((UpdateInfo?)null);

            // Act
            await _sut.CheckForUpdatesAsync();

            // Assert
            _mockConfigurationService.Verify(x => x.LoadConfigurationAsync(), Times.Once);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldCallGitHubServiceWithCorrectConfiguration()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            _mockConfigurationService.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(configuration);
            
            _mockGitHubService.Setup(x => x.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>()))
                .ReturnsAsync((UpdateInfo?)null);

            // Act
            await _sut.CheckForUpdatesAsync();

            // Assert
            _mockGitHubService.Verify(x => x.CheckForUpdatesAsync(configuration), Times.Once);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnUpdateInfo_WhenUpdateAvailable()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            
            _mockConfigurationService.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(configuration);
            
            _mockGitHubService.Setup(x => x.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>()))
                .ReturnsAsync(updateInfo);

            // Act
            var result = await _sut.CheckForUpdatesAsync();

            // Assert
            result.Should().Be(updateInfo);
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenNoUpdateAvailable()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            
            _mockConfigurationService.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(configuration);
            
            _mockGitHubService.Setup(x => x.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>()))
                .ReturnsAsync((UpdateInfo?)null);

            // Act
            var result = await _sut.CheckForUpdatesAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            _mockConfigurationService.Setup(x => x.LoadConfigurationAsync())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            var result = await _sut.CheckForUpdatesAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdatesAsync_ShouldLogOperationsCorrectly()
        {
            // Arrange
            var configuration = TestFixtures.CreateDefaultConfiguration();
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            
            _mockConfigurationService.Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(configuration);
            
            _mockGitHubService.Setup(x => x.CheckForUpdatesAsync(It.IsAny<UpdaterConfiguration>()))
                .ReturnsAsync(updateInfo);

            // Act
            var result = await _sut.CheckForUpdatesAsync();

            // Assert
            result.Should().NotBeNull();
            _mockConfigurationService.Verify(x => x.LoadConfigurationAsync(), Times.Once);
            _mockGitHubService.Verify(x => x.CheckForUpdatesAsync(configuration), Times.Once);
        }

        #endregion

        #region DownloadUpdateAsync Tests

        [Fact]
        public async Task DownloadUpdateAsync_ShouldCreateCorrectTemporaryPath()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var expectedStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStream);

            // Act
            var result = await _sut.DownloadUpdateAsync(updateInfo, progress.Object, cancellationToken);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith(Path.GetTempPath());
            result.Should().EndWith(".msi");
        }

        [Fact]
        public async Task DownloadUpdateAsync_ShouldCallGitHubServiceWithCorrectParameters()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var expectedStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStream);

            // Act
            await _sut.DownloadUpdateAsync(updateInfo, progress.Object, cancellationToken);

            // Assert
            _mockGitHubService.Verify(x => x.DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                progress.Object,
                cancellationToken
            ), Times.Once);
        }

        [Fact]
        public async Task DownloadUpdateAsync_ShouldSaveStreamToTemporaryFile()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var expectedStream = new MemoryStream(testData);
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStream);

            // Act
            var result = await _sut.DownloadUpdateAsync(updateInfo, progress.Object, cancellationToken);

            // Assert
            result.Should().NotBeNullOrEmpty();
            File.Exists(result).Should().BeTrue();
            
            var savedData = await File.ReadAllBytesAsync(result);
            savedData.Should().BeEquivalentTo(testData);
            
            // Clean up
            if (File.Exists(result))
                File.Delete(result);
        }

        [Fact]
        public async Task DownloadUpdateAsync_ShouldReportProgressCorrectly()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var expectedStream = new MemoryStream(testData);
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStream);

            // Act
            var result = await _sut.DownloadUpdateAsync(updateInfo, progress.Object, cancellationToken);

            // Assert
            _mockGitHubService.Verify(x => x.DownloadUpdateAsync(
                It.IsAny<string>(),
                progress.Object,
                It.IsAny<CancellationToken>()
            ), Times.Once);
            
            // Clean up
            if (File.Exists(result))
                File.Delete(result);
        }

        [Fact]
        public async Task DownloadUpdateAsync_ShouldHandleCancellation_WhenCancellationTokenTriggered()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await _sut.Invoking(s => s.DownloadUpdateAsync(updateInfo, progress.Object, cancellationTokenSource.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task DownloadUpdateAsync_ShouldCleanupOnError_WhenExceptionOccurs()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await _sut.Invoking(s => s.DownloadUpdateAsync(updateInfo, progress.Object, cancellationToken))
                .Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task DownloadUpdateAsync_ShouldReturnDownloadedFilePath_WhenSuccessful()
        {
            // Arrange
            var updateInfo = TestFixtures.CreateValidUpdateInfo();
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var expectedStream = new MemoryStream(testData);
            var progress = new Mock<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;
            
            _mockGitHubService.Setup(x => x.DownloadUpdateAsync(It.IsAny<string>(), It.IsAny<IProgress<(long, long)>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStream);

            // Act
            var result = await _sut.DownloadUpdateAsync(updateInfo, progress.Object, cancellationToken);

            // Assert
            result.Should().NotBeNullOrEmpty();
            Path.IsPathRooted(result).Should().BeTrue();
            result.Should().EndWith(".msi");
            File.Exists(result).Should().BeTrue();
            
            // Clean up
            if (File.Exists(result))
                File.Delete(result);
        }

        #endregion

        #region Other Methods Tests

        [Fact]
        public async Task InstallUpdateAsync_ShouldCallInstallationServiceWithCorrectParameters()
        {
            // Arrange
            var msiFilePath = "C:\\temp\\update.msi";
            var progress = new Mock<IProgress<string>>();
            var cancellationToken = CancellationToken.None;

            // Act
            await _sut.InstallUpdateAsync(msiFilePath, progress.Object, cancellationToken);

            // Assert
            _mockInstallationService.Verify(x => x.InstallUpdateAsync(
                msiFilePath,
                progress.Object,
                cancellationToken
            ), Times.Once);
        }

        [Fact]
        public void GetConfiguration_ShouldReturnConfigurationFromService()
        {
            // Arrange
            var expectedConfiguration = TestFixtures.CreateDefaultConfiguration();
            _mockConfigurationService.Setup(x => x.GetConfiguration())
                .Returns(expectedConfiguration);

            // Act
            var result = _sut.GetConfiguration();

            // Assert
            result.Should().Be(expectedConfiguration);
            _mockConfigurationService.Verify(x => x.GetConfiguration(), Times.Once);
        }

        #endregion
    }
}