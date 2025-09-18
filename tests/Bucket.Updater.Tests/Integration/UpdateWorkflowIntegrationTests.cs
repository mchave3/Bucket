namespace Bucket.Updater.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using NSubstitute;
    using Xunit;

    /// <summary>
    /// Integration tests for the complete update workflow from check to installation
    /// </summary>
    public sealed class UpdateWorkflowIntegrationTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _appConfigPath;
        private readonly IAppConfigReader _mockAppConfigReader;
        private readonly IGitHubService _mockGitHubService;
        private readonly IInstallationService _mockInstallationService;
        private readonly IConfigurationService _configurationService;
        private readonly UpdateService _updateService;
        private bool _disposed;

        public UpdateWorkflowIntegrationTests()
        {
            // Setup test environment
            _testDirectory = Path.Combine(Path.GetTempPath(), "BucketUpdateTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _appConfigPath = Path.Combine(_testDirectory, "AppConfig.json");

            // Setup mocks
            _mockAppConfigReader = Substitute.For<IAppConfigReader>();
            _mockGitHubService = Substitute.For<IGitHubService>();
            _mockInstallationService = Substitute.For<IInstallationService>();

            // Setup services
            _configurationService = new ConfigurationService(_mockAppConfigReader);
            _updateService = new UpdateService(_configurationService, _mockGitHubService, _mockInstallationService);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
                _disposed = true;
            }
        }

        [Fact]
        public async Task CompleteUpdateWorkflowWithReleaseChannelShouldSucceed()
        {
            // Arrange - Setup realistic release channel scenario
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                CurrentVersion = "24.1.15",
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.X64
            };
            currentConfig.InitializeRuntimeProperties();

            var newerVersion = "24.2.1";
            var updateInfo = new UpdateInfo
            {
                Version = newerVersion,
                TagName = "v24.2.1",
                Name = "Release 24.2.1",
                Body = "Bug fixes and improvements",
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                FileSize = 15728640, // 15MB
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket--x64.msi",
                        DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                        Size = 15728640, // 15MB
                        ContentType = "application/x-msi"
                    }
                }
            };

            // Setup mocks
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(updateInfo));
            _mockGitHubService.DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(new MemoryStream()));
            _mockInstallationService.InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            // Act - Execute complete workflow
            var checkResult = await _updateService.CheckForUpdatesAsync();
            Assert.NotNull(checkResult);
            Assert.Equal(newerVersion, checkResult.Version);

            var downloadResult = await _updateService.DownloadUpdateAsync(updateInfo);
            Assert.NotNull(downloadResult);

            var installResult = await _updateService.InstallUpdateAsync(downloadResult);
            Assert.True(installResult);

            // Assert - Verify all services were called correctly
            await _mockGitHubService.Received(1).CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>());
            await _mockGitHubService.Received(1).DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>());
            await _mockInstallationService.Received(1).InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CompleteUpdateWorkflowWithNightlyChannelShouldSucceed()
        {
            // Arrange - Setup realistic nightly channel scenario
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Nightly,
                CurrentVersion = "24.1.15.100-Nightly",
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.ARM64
            };
            currentConfig.InitializeRuntimeProperties();

            var newerVersion = "24.1.15.105-Nightly";
            var updateInfo = new UpdateInfo
            {
                Version = newerVersion,
                TagName = "nightly-24.1.15.105",
                Name = "Nightly Build 24.1.15.105",
                Body = "Latest nightly build with experimental features",
                Channel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.ARM64,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/nightly-24.1.15.105/Bucket--arm64.msi",
                FileSize = 16777216, // 16MB
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket--arm64.msi",
                        DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/nightly-24.1.15.105/Bucket--arm64.msi",
                        Size = 16777216, // 16MB
                        ContentType = "application/x-msi"
                    }
                }
            };

            // Setup mocks
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(updateInfo));
            _mockGitHubService.DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(new MemoryStream()));
            _mockInstallationService.InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            // Act - Execute complete workflow
            var checkResult = await _updateService.CheckForUpdatesAsync();
            var downloadResult = await _updateService.DownloadUpdateAsync(updateInfo);
            var installResult = await _updateService.InstallUpdateAsync(downloadResult);

            // Assert
            Assert.NotNull(checkResult);
            Assert.True(installResult);
            Assert.Equal(UpdateChannel.Nightly, checkResult.Channel);
            Assert.Contains("Nightly", checkResult.Version, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateWorkflowWhenNoUpdatesAvailableShouldReturnNull()
        {
            // Arrange - Current version is latest
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                CurrentVersion = "24.2.1", // Already latest
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.X64
            };
            currentConfig.InitializeRuntimeProperties();

            // Setup mocks - No update available
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(null));

            // Act
            var checkResult = await _updateService.CheckForUpdatesAsync();

            // Assert
            Assert.Null(checkResult);

            // Download and install should not be called
            await _mockGitHubService.DidNotReceive().DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>());
            await _mockInstallationService.DidNotReceive().InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdateWorkflowWithDownloadFailureShouldHandleGracefully()
        {
            // Arrange
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                CurrentVersion = "24.1.15",
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.X64
            };
            currentConfig.InitializeRuntimeProperties();

            var updateInfo = new UpdateInfo
            {
                Version = "24.2.1",
                TagName = "v24.2.1",
                Name = "Release 24.2.1",
                Body = "Bug fixes and improvements",
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                DownloadUrl = "https://invalid-url.example.com/file.msi",
                FileSize = 15728640,
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket--x64.msi",
                        DownloadUrl = "https://invalid-url.example.com/file.msi",
                        Size = 15728640,
                        ContentType = "application/x-msi"
                    }
                }
            };

            // Setup mocks - Download fails
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(updateInfo));
            _mockGitHubService.DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>())
                .Returns<Task<Stream>>(x => { throw new HttpRequestException("Network error"); });

            // Act & Assert
            var checkResult = await _updateService.CheckForUpdatesAsync();
            Assert.NotNull(checkResult);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _updateService.DownloadUpdateAsync(updateInfo));

            // Install should not be attempted after download failure
            await _mockInstallationService.DidNotReceive().InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdateWorkflowWithInstallationFailureShouldHandleGracefully()
        {
            // Arrange
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                CurrentVersion = "24.1.15",
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.X64
            };
            currentConfig.InitializeRuntimeProperties();

            var updateInfo = new UpdateInfo
            {
                Version = "24.2.1",
                TagName = "v24.2.1",
                Name = "Release 24.2.1",
                Body = "Bug fixes and improvements",
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                FileSize = 15728640,
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket--x64.msi",
                        DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                        Size = 15728640,
                        ContentType = "application/x-msi"
                    }
                }
            };

            // Setup mocks - Installation fails
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(updateInfo));
            _mockGitHubService.DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Stream>(new MemoryStream()));
            _mockInstallationService.InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(false)); // Installation fails

            // Act
            var checkResult = await _updateService.CheckForUpdatesAsync();
            var downloadResult = await _updateService.DownloadUpdateAsync(updateInfo);
            var installResult = await _updateService.InstallUpdateAsync(downloadResult);

            // Assert
            Assert.NotNull(checkResult);
            Assert.NotNull(downloadResult);
            Assert.False(installResult); // Installation should fail
        }

        [Fact]
        public async Task UpdateWorkflowWithConfigurationFallbackShouldSucceed()
        {
            // Arrange - AppConfig.json doesn't exist, should use defaults
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(null));

            var updateInfo = new UpdateInfo
            {
                Version = "24.2.1",
                TagName = "v24.2.1",
                Name = "Release 24.2.1",
                Body = "Bug fixes and improvements",
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                FileSize = 15728640,
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket--x64.msi",
                        DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                        Size = 15728640,
                        ContentType = "application/x-msi"
                    }
                }
            };

            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(updateInfo));

            // Act
            var checkResult = await _updateService.CheckForUpdatesAsync();

            // Assert
            Assert.NotNull(checkResult);

            // Verify default configuration was used
            await _mockGitHubService.Received(1).CheckForUpdatesAsync(
                Arg.Is<UpdaterConfiguration>(config =>
                    config.GitHubOwner == "mchave3" &&
                    config.GitHubRepository == "Bucket" &&
                    config.UpdateChannel == UpdateChannel.Release));
        }

        [Fact]
        public async Task UpdateWorkflowWithProgressReportingShouldWork()
        {
            // Arrange
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                CurrentVersion = "24.1.15",
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.X64
            };
            currentConfig.InitializeRuntimeProperties();

            var updateInfo = new UpdateInfo
            {
                Version = "24.2.1",
                TagName = "v24.2.1",
                Name = "Release 24.2.1",
                Body = "Bug fixes and improvements",
                Channel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                FileSize = 15728640,
                Assets = new List<UpdateAsset>
                {
                    new UpdateAsset
                    {
                        Name = "Bucket--x64.msi",
                        DownloadUrl = "https://github.com/mchave3/Bucket/releases/download/v24.2.1/Bucket--x64.msi",
                        Size = 15728640,
                        ContentType = "application/x-msi"
                    }
                }
            };

            var progressValues = new List<(long downloaded, long total)>();
            var installationMessages = new List<string>();

            // Setup mocks
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromResult<UpdateInfo?>(updateInfo));

            // Simulate progress reporting during download
            _mockGitHubService.DownloadUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<(long, long)>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var progressArg = callInfo.Arg<IProgress<(long, long)>>();
                    // Simulate progress updates
                    progressArg?.Report((0, 15728640));
                    progressArg?.Report((3932160, 15728640)); // 25%
                    progressArg?.Report((7864320, 15728640)); // 50%
                    progressArg?.Report((11796480, 15728640)); // 75%
                    progressArg?.Report((15728640, 15728640)); // 100%
                    return Task.FromResult<Stream>(new MemoryStream());
                });

            _mockInstallationService.InstallUpdateAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var progressArg = callInfo.Arg<IProgress<string>>();
                    progressArg?.Report("Starting installation...");
                    progressArg?.Report("Installation completed successfully");
                    return Task.FromResult(true);
                });

            // Act
            var downloadProgress = new Progress<(long downloaded, long total)>(p => progressValues.Add(p));
            var installProgress = new Progress<string>(msg => installationMessages.Add(msg));

            var downloadResult = await _updateService.DownloadUpdateAsync(updateInfo, downloadProgress);
            var installResult = await _updateService.InstallUpdateAsync(downloadResult, installProgress);

            // Assert
            Assert.NotNull(downloadResult);
            Assert.True(installResult);
            Assert.NotEmpty(progressValues);
            Assert.Contains(progressValues, p => p.downloaded == 0 && p.total == 15728640);
            Assert.Contains(progressValues, p => p.downloaded == 15728640 && p.total == 15728640);
            Assert.NotEmpty(installationMessages);
            Assert.Contains(installationMessages, msg => msg.Contains("Installation completed", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task UpdateWorkflowWithCancellationShouldRespectToken()
        {
            // Arrange
            var currentConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                CurrentVersion = "24.1.15",
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                Architecture = SystemArchitecture.X64
            };
            currentConfig.InitializeRuntimeProperties();

            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync(); // Cancel immediately

            // Setup mocks
            _mockAppConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(currentConfig));
            _mockGitHubService.CheckForUpdatesAsync(Arg.Any<UpdaterConfiguration>())
                .Returns(Task.FromException<UpdateInfo?>(new OperationCanceledException()));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _updateService.CheckForUpdatesAsync());
        }
    }
}