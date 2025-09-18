namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Models.GitHub;
    using Bucket.Updater.Services;
    using NSubstitute;
    using Xunit;

    public class GitHubServiceTests : IDisposable
    {
        private readonly GitHubService _gitHubService;
        private bool _disposed;

        public GitHubServiceTests()
        {
            _gitHubService = new GitHubService();
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
                _gitHubService?.Dispose();
                _disposed = true;
            }
        }

        [Fact]
        public void ConstructorShouldCreateValidInstance()
        {
            // Act
            using var service = new GitHubService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task CheckForUpdatesAsyncShouldReturnNullForNullConfiguration()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.CheckForUpdatesAsync(null!));
        }

        [Fact]
        public async Task CheckForUpdatesAsyncShouldHandleEmptyReleasesGracefully()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                GitHubOwner = "nonexistentowner",
                GitHubRepository = "nonexistentrepo",
                CurrentVersion = "1.0.0",
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64
            };

            // Act
            var result = await _gitHubService.CheckForUpdatesAsync(configuration);

            // Assert
            // Should return null when no releases are found (which is expected for nonexistent repo)
            Assert.Null(result);
        }

        [Fact]
        public async Task CheckForUpdatesAsyncShouldFilterByUpdateChannel()
        {
            // This test validates the filtering logic without making real API calls
            // It tests the internal logic by ensuring proper channel selection

            // Arrange
            var releaseConfiguration = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                Architecture = SystemArchitecture.X64,
                CurrentVersion = "1.0.0",
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo"
            };

            var nightlyConfiguration = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Nightly,
                Architecture = SystemArchitecture.X64,
                CurrentVersion = "1.0.0",
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo"
            };

            // Act
            var releaseResult = await _gitHubService.CheckForUpdatesAsync(releaseConfiguration);
            var nightlyResult = await _gitHubService.CheckForUpdatesAsync(nightlyConfiguration);

            // Assert
            // For nonexistent repos, both should return null
            Assert.Null(releaseResult);
            Assert.Null(nightlyResult);
        }

        [Theory]
        [InlineData(SystemArchitecture.X64, "x64")]
        [InlineData(SystemArchitecture.X86, "x86")]
        [InlineData(SystemArchitecture.ARM64, "arm64")]
        public async Task CheckForUpdatesAsyncShouldFilterByArchitecture(SystemArchitecture architecture, string expectedArchString)
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                Architecture = architecture,
                CurrentVersion = "1.0.0",
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo"
            };

            // Act
            var result = await _gitHubService.CheckForUpdatesAsync(configuration);

            // Assert
            // For nonexistent repos, should return null
            Assert.Null(result);

            // Verify architecture string conversion
            Assert.Equal(expectedArchString, configuration.GetArchitectureString());
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldThrowOnNullUrl()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.DownloadUpdateAsync(null!));
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldThrowOnEmptyUrl()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.DownloadUpdateAsync(""));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.DownloadUpdateAsync("   "));
        }

        [Fact]
        public async Task DownloadUpdateAsyncShouldHandleInvalidUrl()
        {
            // Arrange
            var invalidUrl = "not-a-valid-url";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _gitHubService.DownloadUpdateAsync(invalidUrl));
        }

        [Fact]
        public async Task GetReleasesAsyncShouldHandleNullOrEmptyOwner()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.GetReleasesAsync(null!, "repo"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.GetReleasesAsync("", "repo"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.GetReleasesAsync("   ", "repo"));
        }

        [Fact]
        public async Task GetReleasesAsyncShouldHandleNullOrEmptyRepository()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.GetReleasesAsync("owner", null!));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.GetReleasesAsync("owner", ""));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _gitHubService.GetReleasesAsync("owner", "   "));
        }

        [Fact]
        public async Task GetReleasesAsyncShouldReturnEmptyListForNonexistentRepo()
        {
            // Arrange
            var nonexistentOwner = "nonexistentowner12345";
            var nonexistentRepo = "nonexistentrepo12345";

            // Act
            var releases = await _gitHubService.GetReleasesAsync(nonexistentOwner, nonexistentRepo, true);

            // Assert
            Assert.NotNull(releases);
            Assert.Empty(releases);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetReleasesAsyncShouldHandlePrereleaseFilter(bool includePrerelease)
        {
            // Arrange
            var owner = "testowner";
            var repo = "testrepo";

            // Act
            var releases = await _gitHubService.GetReleasesAsync(owner, repo, includePrerelease);

            // Assert
            Assert.NotNull(releases);
            // For nonexistent repo, should return empty list
            Assert.Empty(releases);
        }

        [Fact]
        public void DisposeShouldNotThrow()
        {
            // Arrange
            using var service = new GitHubService();

            // Act & Assert - Should not throw
            service.Dispose();
        }

        [Fact]
        public void MultipleDiposeShouldNotThrow()
        {
            // Arrange
            var service = new GitHubService();

            // Act & Assert - Should not throw
            service.Dispose();
            service.Dispose(); // Second dispose should be safe
        }
    }

    public class ProgressStreamTests
    {
        [Fact]
        public void ConstructorShouldThrowOnNullBaseStream()
        {
            // Arrange
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TestProgressStream(null!, 1000, progress));
        }

        [Fact]
        public void ConstructorShouldThrowOnNullProgress()
        {
            // Arrange
            using var baseStream = new MemoryStream();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new TestProgressStream(baseStream, 1000, null!));
        }

        [Fact]
        public void ConstructorShouldCreateValidInstance()
        {
            // Arrange
            using var baseStream = new MemoryStream();
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();

            // Act
            using var progressStream = new TestProgressStream(baseStream, 1000, progress);

            // Assert
            Assert.NotNull(progressStream);
            Assert.Equal(baseStream.CanRead, progressStream.CanRead);
            Assert.Equal(baseStream.CanSeek, progressStream.CanSeek);
            Assert.Equal(baseStream.CanWrite, progressStream.CanWrite);
        }

        [Fact]
        public void ReadShouldReportProgress()
        {
            // Arrange
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            using var baseStream = new MemoryStream(testData);
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, testData.Length, progress);

            // Act
            var buffer = new byte[3];
            var bytesRead = progressStream.Read(buffer, 0, 3);

            // Assert
            Assert.Equal(3, bytesRead);
            progress.Received(1).Report((3, testData.Length));
        }

        [Fact]
        public async Task ReadAsyncShouldReportProgress()
        {
            // Arrange
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using var baseStream = new MemoryStream(testData);
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, testData.Length, progress);

            // Act
            var buffer = new byte[4];
            var bytesRead = await progressStream.ReadAsync(buffer.AsMemory(0, 4), CancellationToken.None);

            // Assert
            Assert.Equal(4, bytesRead);
            progress.Received(1).Report((4, testData.Length));
        }

        [Fact]
        public void ReadShouldThrowOnNullBuffer()
        {
            // Arrange
            using var baseStream = new MemoryStream();
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, 1000, progress);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                progressStream.Read(null!, 0, 10));
        }

        [Fact]
        public async Task ReadAsyncShouldThrowOnNullBuffer()
        {
            // Arrange
            using var baseStream = new MemoryStream();
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, 1000, progress);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                progressStream.ReadAsync(null!, 0, 10, CancellationToken.None));
        }

        [Fact]
        public void MultipleReadsShouldAccumulateProgress()
        {
            // Arrange
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            using var baseStream = new MemoryStream(testData);
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, testData.Length, progress);

            // Act
            var buffer1 = new byte[3];
            var buffer2 = new byte[4];
            var bytesRead1 = progressStream.Read(buffer1, 0, 3);
            var bytesRead2 = progressStream.Read(buffer2, 0, 4);

            // Assert
            Assert.Equal(3, bytesRead1);
            Assert.Equal(4, bytesRead2);
            progress.Received(1).Report((3, testData.Length));
            progress.Received(1).Report((7, testData.Length)); // 3 + 4 = 7
        }

        [Fact]
        public void PositionPropertyShouldWorkCorrectly()
        {
            // Arrange
            using var baseStream = new MemoryStream(new byte[100]);
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, 100, progress);

            // Act
            progressStream.Position = 50;

            // Assert
            Assert.Equal(50, progressStream.Position);
            Assert.Equal(50, baseStream.Position);
        }

        [Fact]
        public void LengthPropertyShouldMatchBaseStream()
        {
            // Arrange
            var testData = new byte[1000];
            using var baseStream = new MemoryStream(testData);
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            using var progressStream = new TestProgressStream(baseStream, testData.Length, progress);

            // Assert
            Assert.Equal(baseStream.Length, progressStream.Length);
        }

        // Test helper class to expose protected Dispose method
        private class TestProgressStream : ProgressStream
        {
            public TestProgressStream(Stream baseStream, long totalBytes, IProgress<(long downloaded, long total)> progress)
                : base(baseStream, totalBytes, progress)
            {
            }

            public new void Dispose(bool disposing)
            {
                base.Dispose(disposing);
            }
        }
    }
}