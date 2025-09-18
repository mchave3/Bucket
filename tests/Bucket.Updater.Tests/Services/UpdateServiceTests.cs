namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using NSubstitute;
    using Xunit;

    public class UpdateServiceTests : IDisposable
    {
        private readonly IConfigurationService _configurationService;
        private readonly IGitHubService _gitHubService;
        private readonly IInstallationService _installationService;
        private readonly UpdateService _updateService;
        private bool _disposed;

        public UpdateServiceTests()
        {
            _configurationService = Substitute.For<IConfigurationService>();
            _gitHubService = Substitute.For<IGitHubService>();
            _installationService = Substitute.For<IInstallationService>();
            _updateService = new UpdateService(_configurationService, _gitHubService, _installationService);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // UpdateService doesn't implement IDisposable
                _disposed = true;
            }
        }

        [Fact]
        public void ConstructorShouldThrowOnNullConfigurationService()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UpdateService(null!, _gitHubService, _installationService));
        }

        [Fact]
        public void ConstructorShouldThrowOnNullGitHubService()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UpdateService(_configurationService, null!, _installationService));
        }

        [Fact]
        public void ConstructorShouldThrowOnNullInstallationService()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UpdateService(_configurationService, _gitHubService, null!));
        }

        [Fact]
        public void ConstructorShouldCreateValidInstance()
        {
            // Act
            var service = new UpdateService(_configurationService, _gitHubService, _installationService);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task CheckForUpdatesAsyncShouldLoadConfigurationAndCheckUpdates()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo",
                CurrentVersion = "1.0.0",
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64
            };

            var expectedUpdate = new UpdateInfo
            {
                Version = "2.0.0",
                TagName = "v2.0.0",
                Name = "Version 2.0.0",
                Body = "Release notes for version 2.0.0",
                PublishedAt = DateTime.UtcNow,
                IsPrerelease = false,
                Assets = [],
                DownloadUrl = "https://github.com/testowner/testrepo/releases/download/v2.0.0/app-2.0.0-x64.msi",
                FileSize = 1024 * 1024,
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64
            };

            _configurationService.LoadConfigurationAsync()
                .Returns(configuration);

            _gitHubService.CheckForUpdatesAsync(configuration)
                .Returns(expectedUpdate);

            // Act
            var result = await _updateService.CheckForUpdatesAsync();

            // Assert
            Assert.Equal(expectedUpdate, result);
            await _configurationService.Received(1).LoadConfigurationAsync();
            await _gitHubService.Received(1).CheckForUpdatesAsync(configuration);
        }

        [Fact]
        public async Task CheckForUpdatesAsyncShouldReturnNullWhenNoUpdates()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo",
                CurrentVersion = "2.0.0", // Same version
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64
            };

            _configurationService.LoadConfigurationAsync()
                .Returns(configuration);

            _gitHubService.CheckForUpdatesAsync(configuration)
                .Returns((UpdateInfo?)null);

            // Act
            var result = await _updateService.CheckForUpdatesAsync();

            // Assert
            Assert.Null(result);
            await _configurationService.Received(1).LoadConfigurationAsync();
            await _gitHubService.Received(1).CheckForUpdatesAsync(configuration);
        }

        [Fact]
        public async Task CheckForUpdatesAsyncShouldReturnNullOnException()
        {
            // Arrange
            _configurationService.LoadConfigurationAsync()
                .Returns(Task.FromException<UpdaterConfiguration>(new InvalidOperationException("Configuration loading failed")));

            // Act
            var result = await _updateService.CheckForUpdatesAsync();

            // Assert
            Assert.Null(result);
            await _configurationService.Received(1).LoadConfigurationAsync();
            await _gitHubService.DidNotReceive().CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>());
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldThrowOnNullUpdateInfo()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _updateService.DownloadUpdateAsync(null!));
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldDownloadToTempDirectory()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "2.0.0",
                DownloadUrl = "https://github.com/test/test/releases/download/v2.0.0/app-2.0.0-x64.msi",
                FileSize = 1024 * 1024
            };

            var downloadStream = new MemoryStream(new byte[1024 * 1024]); // 1MB of zeros
            _gitHubService.DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                Arg.Any<IProgress<(long downloaded, long total)>>(),
                Arg.Any<CancellationToken>())
                .Returns(downloadStream);

            // Act
            var result = await _updateService.DownloadUpdateAsync(updateInfo);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.Contains("BucketUpdater", result, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("app-2.0.0-x64.msi", result, StringComparison.OrdinalIgnoreCase);

            await _gitHubService.Received(1).DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                Arg.Any<IProgress<(long downloaded, long total)>>(),
                Arg.Any<CancellationToken>());

            // Cleanup
            if (File.Exists(result))
                File.Delete(result);
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldReportProgress()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "2.0.0",
                DownloadUrl = "https://github.com/test/test/releases/download/v2.0.0/app-2.0.0-x64.msi",
                FileSize = 1024
            };

            var downloadStream = new MemoryStream(new byte[1024]);
            var progressReports = new List<(long downloaded, long total)>();
            var progress = new Progress<(long downloaded, long total)>(report => progressReports.Add(report));

            _gitHubService.DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                Arg.Any<IProgress<(long downloaded, long total)>>(),
                Arg.Any<CancellationToken>())
                .Returns(downloadStream)
                .AndDoes(callInfo =>
                {
                    var progressArg = callInfo.Arg<IProgress<(long downloaded, long total)>>();
                    progressArg?.Report((512, 1024)); // 50%
                    progressArg?.Report((1024, 1024)); // 100%
                });

            // Act
            var result = await _updateService.DownloadUpdateAsync(updateInfo, progress);

            // Assert
            Assert.NotNull(result);
            // Progress events should be forwarded to GitHubService
            await _gitHubService.Received(1).DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                progress,
                Arg.Any<CancellationToken>());

            // Cleanup
            if (File.Exists(result))
                File.Delete(result);
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldValidateFileSize()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "2.0.0",
                DownloadUrl = "https://github.com/test/test/releases/download/v2.0.0/app-2.0.0-x64.msi",
                FileSize = 2048 // Expected 2KB
            };

            var downloadStream = new MemoryStream(new byte[1024]); // Only 1KB - significant difference
            _gitHubService.DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                Arg.Any<IProgress<(long downloaded, long total)>>(),
                Arg.Any<CancellationToken>())
                .Returns(downloadStream);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _updateService.DownloadUpdateAsync(updateInfo));
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldOverwriteExistingFile()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "2.0.0",
                DownloadUrl = "https://github.com/test/test/releases/download/v2.0.0/app-2.0.0-x64.msi",
                FileSize = 1024
            };

            var tempDir = Path.Combine(Path.GetTempPath(), "BucketUpdater");
            Directory.CreateDirectory(tempDir);
            var existingFile = Path.Combine(tempDir, "app-2.0.0-x64.msi");
            await File.WriteAllTextAsync(existingFile, "old content");

            var downloadStream = new MemoryStream(new byte[1024]);
            _gitHubService.DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                Arg.Any<IProgress<(long downloaded, long total)>>(),
                Arg.Any<CancellationToken>())
                .Returns(downloadStream);

            // Act
            var result = await _updateService.DownloadUpdateAsync(updateInfo);

            // Assert
            Assert.Equal(existingFile, result);
            Assert.Equal(1024, new FileInfo(result).Length);

            // Cleanup
            if (File.Exists(result))
                File.Delete(result);
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldSupportCancellation()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "2.0.0",
                DownloadUrl = "https://github.com/test/test/releases/download/v2.0.0/app-2.0.0-x64.msi",
                FileSize = 1024
            };

            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            _gitHubService.DownloadUpdateAsync(
                updateInfo.DownloadUrl,
                Arg.Any<IProgress<(long downloaded, long total)>>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromException<Stream>(new OperationCanceledException()));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _updateService.DownloadUpdateAsync(updateInfo, cancellationToken: cancellationTokenSource.Token));
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldThrowOnNullFilePath()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _updateService.InstallUpdateAsync(null!));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _updateService.InstallUpdateAsync(""));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _updateService.InstallUpdateAsync("   "));
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldDelegateToInstallationService()
        {
            // Arrange
            var msiFilePath = @"C:\temp\update.msi";
            var progress = new Progress<string>();
            var cancellationToken = CancellationToken.None;

            _installationService.InstallUpdateAsync(msiFilePath, progress, cancellationToken)
                .Returns(true);

            // Act
            var result = await _updateService.InstallUpdateAsync(msiFilePath, progress, cancellationToken);

            // Assert
            Assert.True(result);
            await _installationService.Received(1).InstallUpdateAsync(msiFilePath, progress, cancellationToken);
        }

        [Fact]
        public async Task InstallUpdateAsyncShouldReturnFalseOnFailure()
        {
            // Arrange
            var msiFilePath = @"C:\temp\update.msi";

            _installationService.InstallUpdateAsync(msiFilePath, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            var result = await _updateService.InstallUpdateAsync(msiFilePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CleanupFilesShouldThrowOnNullPath()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _updateService.CleanupFiles(null!));

            Assert.Throws<ArgumentNullException>(() =>
                _updateService.CleanupFiles(""));

            Assert.Throws<ArgumentNullException>(() =>
                _updateService.CleanupFiles("   "));
        }

        [Fact]
        public void CleanupFilesShouldDelegateToInstallationService()
        {
            // Arrange
            var downloadPath = @"C:\temp\update.msi";

            // Act
            _updateService.CleanupFiles(downloadPath);

            // Assert
            _installationService.Received(1).CleanupDownloadedFiles(downloadPath);
        }

        [Fact]
        public void CleanupAllTemporaryFilesShouldDelegateToInstallationService()
        {
            // Act
            _updateService.CleanupAllTemporaryFiles();

            // Assert
            _installationService.Received(1).CleanupAllTemporaryFiles();
        }

        [Fact]
        public void GetConfigurationShouldReturnConfigurationFromService()
        {
            // Arrange
            var expectedConfiguration = new UpdaterConfiguration
            {
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo",
                CurrentVersion = "1.0.0",
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64
            };

            _configurationService.GetConfiguration()
                .Returns(expectedConfiguration);

            // Act
            var result = _updateService.GetConfiguration();

            // Assert
            Assert.Equal(expectedConfiguration, result);
            _configurationService.Received(1).GetConfiguration();
        }
    }
}