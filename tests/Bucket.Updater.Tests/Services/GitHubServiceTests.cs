namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using Moq;
    using Xunit;

    public class GitHubServiceTests
    {
        private readonly GitHubService _testClass;

        public GitHubServiceTests()
        {
            _testClass = new GitHubService();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new GitHubService();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallCheckForUpdatesAsync()
        {
            // Arrange
            var configuration = new UpdaterConfiguration();

            // Act
            var result = await _testClass.CheckForUpdatesAsync(configuration);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallCheckForUpdatesAsyncWithNullConfiguration()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.CheckForUpdatesAsync(default(UpdaterConfiguration)));
        }

        [Fact]
        public async Task CheckForUpdatesAsyncPerformsMapping()
        {
            // Arrange
            var configuration = new UpdaterConfiguration();

            // Act
            var result = await _testClass.CheckForUpdatesAsync(configuration);

            // Assert
            Assert.Equal(configuration.Architecture, result.Architecture);
        }

        [Fact]
        public async Task CanCallDownloadUpdateAsync()
        {
            // Arrange
            var downloadUrl = "TestValue2016645262";
            var progress = new Mock<IProgress<(long downloaded, long total)>>().Object;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _testClass.DownloadUpdateAsync(downloadUrl, progress, cancellationToken);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallDownloadUpdateAsyncWithInvalidDownloadUrl(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.DownloadUpdateAsync(value, new Mock<IProgress<(long downloaded, long total)>>().Object, CancellationToken.None));
        }

        [Fact]
        public async Task CanCallGetReleasesAsync()
        {
            // Arrange
            var owner = "TestValue1752580427";
            var repository = "TestValue148046602";
            var includePrerelease = true;

            // Act
            var result = await _testClass.GetReleasesAsync(owner, repository, includePrerelease);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetReleasesAsyncWithInvalidOwner(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetReleasesAsync(value, "TestValue1823482900", false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetReleasesAsyncWithInvalidRepository(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetReleasesAsync("TestValue796258390", value, true));
        }

        [Fact]
        public void CanCallDispose()
        {
            // Act
            _testClass.Dispose();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }

    public class ProgressStreamTests
    {
        private class TestProgressStream : ProgressStream
        {
            public TestProgressStream(Stream baseStream, long totalBytes, IProgress<(long downloaded, long total)> progress) : base(baseStream, totalBytes, progress)
            {
            }

            public void PublicDispose(bool disposing)
            {
                base.Dispose(disposing);
            }
        }

        private readonly TestProgressStream _testClass;
        private Stream _baseStream;
        private long _totalBytes;
        private readonly Mock<IProgress<(long downloaded, long total)>> _progress;

        public ProgressStreamTests()
        {
            _baseStream = new MemoryStream();
            _totalBytes = 1816354239L;
            _progress = new Mock<IProgress<(long downloaded, long total)>>();
            _testClass = new TestProgressStream(_baseStream, _totalBytes, _progress.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TestProgressStream(_baseStream, _totalBytes, _progress.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullBaseStream()
        {
            Assert.Throws<ArgumentNullException>(() => new TestProgressStream(default(Stream), _totalBytes, _progress.Object));
        }

        [Fact]
        public void CannotConstructWithNullProgress()
        {
            Assert.Throws<ArgumentNullException>(() => new TestProgressStream(_baseStream, _totalBytes, default(IProgress<(long downloaded, long total)>)));
        }

        [Fact]
        public async Task CanCallReadAsync()
        {
            // Arrange
            var buffer = new byte[] { 76, 85, 221, 206 };
            var offset = 1353272748;
            var count = 1160690908;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _testClass.ReadAsync(buffer, offset, count, cancellationToken);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallReadAsyncWithNullBuffer()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.ReadAsync(default(byte[]), 718220272, 927074049, CancellationToken.None));
        }

        [Fact]
        public void CanCallRead()
        {
            // Arrange
            var buffer = new byte[] { 137, 120, 126, 65 };
            var offset = 275748557;
            var count = 1472839095;

            // Act
            var result = _testClass.Read(buffer, offset, count);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallReadWithNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.Read(default(byte[]), 567460878, 635452689));
        }

        [Fact]
        public void CanCallFlush()
        {
            // Act
            _testClass.Flush();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallSeek()
        {
            // Arrange
            var offset = 416761260L;
            var origin = SeekOrigin.End;

            // Act
            var result = _testClass.Seek(offset, origin);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallSetLength()
        {
            // Arrange
            var value = 589020371L;

            // Act
            _testClass.SetLength(value);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallWrite()
        {
            // Arrange
            var buffer = new byte[] { 20, 201, 28, 7 };
            var offset = 315228596;
            var count = 370988962;

            // Act
            _testClass.Write(buffer, offset, count);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallWriteWithNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.Write(default(byte[]), 295155605, 1529685104));
        }

        [Fact]
        public void CanCallDispose()
        {
            // Arrange
            var disposing = false;

            // Act
            _testClass.PublicDispose(disposing);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetCanRead()
        {
            // Assert
            Assert.IsType<bool>(_testClass.CanRead);

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetCanSeek()
        {
            // Assert
            Assert.IsType<bool>(_testClass.CanSeek);

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetCanWrite()
        {
            // Assert
            Assert.IsType<bool>(_testClass.CanWrite);

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanGetLength()
        {
            // Assert
            Assert.IsType<long>(_testClass.Length);

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanSetAndGetPosition()
        {
            // Arrange
            var testValue = 393177461L;

            // Act
            _testClass.Position = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Position);
        }
    }
}