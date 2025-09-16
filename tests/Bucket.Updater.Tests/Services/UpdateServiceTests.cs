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

    public class UpdateServiceTests
    {
        private readonly UpdateService _testClass;
        private readonly IConfigurationService _configurationService;
        private readonly IGitHubService _gitHubService;
        private readonly IInstallationService _installationService;

        public UpdateServiceTests()
        {
            _configurationService = Substitute.For<IConfigurationService>();
            _gitHubService = Substitute.For<IGitHubService>();
            _installationService = Substitute.For<IInstallationService>();
            _testClass = new UpdateService(_configurationService, _gitHubService, _installationService);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new UpdateService(_configurationService, _gitHubService, _installationService);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullConfigurationService()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateService(default(IConfigurationService), _gitHubService, _installationService));
        }

        [Fact]
        public void CannotConstructWithNullGitHubService()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateService(_configurationService, default(IGitHubService), _installationService));
        }

        [Fact]
        public void CannotConstructWithNullInstallationService()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateService(_configurationService, _gitHubService, default(IInstallationService)));
        }

        [Fact]
        public async Task CanCallCheckForUpdatesAsync()
        {
            // Arrange
            _configurationService.LoadConfigurationAsync().Returns(new UpdaterConfiguration());
            _gitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>()).Returns(new UpdateInfo
            {
                Version = "TestValue1944306379",
                TagName = "TestValue913443386",
                Name = "TestValue2146105344",
                Body = "TestValue145588333",
                PublishedAt = DateTime.UtcNow,
                IsPrerelease = true,
                Assets = new List<UpdateAsset>(),
                DownloadUrl = "TestValue682640107",
                FileSize = 1351120026L,
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X86
            });

            // Act
            var result = await _testClass.CheckForUpdatesAsync();

            // Assert
            await _configurationService.Received().LoadConfigurationAsync();
            await _gitHubService.Received().CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>());

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallDownloadUpdateAsync()
        {
            // Arrange
            var updateInfo = new UpdateInfo
            {
                Version = "TestValue1085345783",
                TagName = "TestValue88719737",
                Name = "TestValue1497481497",
                Body = "TestValue2024881683",
                PublishedAt = DateTime.UtcNow,
                IsPrerelease = true,
                Assets = new List<UpdateAsset>(),
                DownloadUrl = "TestValue1167627006",
                FileSize = 1361377465L,
                Channel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.X64
            };
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            var cancellationToken = CancellationToken.None;

            _gitHubService.DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long downloaded, long total)>>(), Arg.Any<CancellationToken>()).Returns(new MemoryStream());

            // Act
            var result = await _testClass.DownloadUpdateAsync(updateInfo, progress, cancellationToken);

            // Assert
            await _gitHubService.Received().DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long downloaded, long total)>>(), Arg.Any<CancellationToken>());

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallDownloadUpdateAsyncWithNullUpdateInfo()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.DownloadUpdateAsync(default(UpdateInfo), Substitute.For<IProgress<(long downloaded, long total)>>(), CancellationToken.None));
        }

        [Fact]
        public async Task CanCallInstallUpdateAsync()
        {
            // Arrange
            var msiFilePath = "TestValue1032179395";
            var progress = Substitute.For<IProgress<string>>();
            var cancellationToken = CancellationToken.None;

            _installationService.InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>()).Returns(true);

            // Act
            var result = await _testClass.InstallUpdateAsync(msiFilePath, progress, cancellationToken);

            // Assert
            await _installationService.Received().InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());

            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallInstallUpdateAsyncWithInvalidMsiFilePath(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.InstallUpdateAsync(value, Substitute.For<IProgress<string>>(), CancellationToken.None));
        }

        [Fact]
        public void CanCallCleanupFiles()
        {
            // Arrange
            var downloadPath = "TestValue1086055021";

            // Act
            _testClass.CleanupFiles(downloadPath);

            // Assert
            _installationService.Received().CleanupDownloadedFiles(Arg.Any<string>());

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
            // Act
            _testClass.CleanupAllTemporaryFiles();

            // Assert
            _installationService.Received().CleanupAllTemporaryFiles();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallGetConfiguration()
        {
            // Arrange
            _configurationService.GetConfiguration().Returns(new UpdaterConfiguration());

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            _configurationService.Received().GetConfiguration();

            throw new NotImplementedException("Create or modify test");
        }
    }
}