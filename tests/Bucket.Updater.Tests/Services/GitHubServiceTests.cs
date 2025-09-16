namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using NSubstitute;
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
            var downloadUrl = "TestValue29014691";
            var progress = Substitute.For<IProgress<(long downloaded, long total)>>();
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.DownloadUpdateAsync(value, Substitute.For<IProgress<(long downloaded, long total)>>(), CancellationToken.None));
        }

        [Fact]
        public async Task CanCallGetReleasesAsync()
        {
            // Arrange
            var owner = "TestValue1685794576";
            var repository = "TestValue1496139061";
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetReleasesAsync(value, "TestValue789152761", true));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetReleasesAsyncWithInvalidRepository(string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetReleasesAsync("TestValue170187224", value, true));
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
        private readonly IProgress<(long downloaded, long total)> _progress;

        public ProgressStreamTests()
        {
            _baseStream = new MemoryStream();
            _totalBytes = 785568750L;
            _progress = Substitute.For<IProgress<(long downloaded, long total)>>();
            _testClass = new TestProgressStream(_baseStream, _totalBytes, _progress);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TestProgressStream(_baseStream, _totalBytes, _progress);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullBaseStream()
        {
            Assert.Throws<ArgumentNullException>(() => new TestProgressStream(default(Stream), _totalBytes, _progress));
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
            var buffer = new byte[] { 246, 84, 37, 95 };
            var offset = 453806236;
            var count = 790262142;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _testClass.ReadAsync(buffer, offset, count, cancellationToken);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallReadAsyncWithNullBuffer()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.ReadAsync(default(byte[]), 1798203477, 1127539907, CancellationToken.None));
        }

        [Fact]
        public void CanCallRead()
        {
            // Arrange
            var buffer = new byte[] { 135, 62, 21, 241 };
            var offset = 1326379695;
            var count = 941736119;

            // Act
            var result = _testClass.Read(buffer, offset, count);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallReadWithNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.Read(default(byte[]), 1397260580, 1726648899));
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
            var offset = 951690817L;
            var origin = SeekOrigin.Current;

            // Act
            var result = _testClass.Seek(offset, origin);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallSetLength()
        {
            // Arrange
            var value = 2018842933L;

            // Act
            _testClass.SetLength(value);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallWrite()
        {
            // Arrange
            var buffer = new byte[] { 244, 110, 123, 108 };
            var offset = 1553237461;
            var count = 808728613;

            // Act
            _testClass.Write(buffer, offset, count);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallWriteWithNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => _testClass.Write(default(byte[]), 308217903, 1191179432));
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
            var testValue = 1923903528L;

            // Act
            _testClass.Position = testValue;

            // Assert
            Assert.Equal(testValue, _testClass.Position);
        }
    }
}