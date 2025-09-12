using Bucket.Updater.Models;
using Bucket.Updater.Services;
using Bucket.Updater.Tests.TestData;
using FluentAssertions;
using Moq;

namespace Bucket.Updater.Tests.Services
{
    public class ConfigurationServiceTests
    {
        private readonly Mock<IAppConfigReader> _mockAppConfigReader;
        private readonly ConfigurationService _sut;

        public ConfigurationServiceTests()
        {
            _mockAppConfigReader = new Mock<IAppConfigReader>();
            _sut = new ConfigurationService(_mockAppConfigReader.Object);
        }

        #region GetConfiguration Tests

        [Fact]
        public void GetConfiguration_ShouldReturnCachedConfiguration_WhenAlreadyLoaded()
        {
            // Arrange
            var expectedConfiguration = TestFixtures.CreateDefaultConfiguration();
            
            _mockAppConfigReader.Setup(x => x.ReadConfiguration())
                .Returns(expectedConfiguration);

            // Load configuration first to cache it
            var initialLoad = _sut.GetConfiguration();

            // Reset the mock to verify it's not called again
            _mockAppConfigReader.Reset();

            // Act
            var result = _sut.GetConfiguration();

            // Assert
            result.Should().BeSameAs(initialLoad);
            _mockAppConfigReader.Verify(x => x.ReadConfiguration(), Times.Never);
        }

        [Fact]
        public void GetConfiguration_ShouldLoadConfigurationSynchronously_WhenNotCached()
        {
            // Arrange
            var expectedConfiguration = TestFixtures.CreateDefaultConfiguration();
            
            _mockAppConfigReader.Setup(x => x.ReadConfiguration())
                .Returns(expectedConfiguration);

            // Act
            var result = _sut.GetConfiguration();

            // Assert
            result.Should().BeEquivalentTo(expectedConfiguration);
            _mockAppConfigReader.Verify(x => x.ReadConfiguration(), Times.Once);
        }

        [Fact]
        public void GetConfiguration_ShouldCacheConfiguration_AfterLoading()
        {
            // Arrange
            var expectedConfiguration = TestFixtures.CreateDefaultConfiguration();
            
            _mockAppConfigReader.Setup(x => x.ReadConfiguration())
                .Returns(expectedConfiguration);

            // Act
            var firstCall = _sut.GetConfiguration();
            var secondCall = _sut.GetConfiguration();

            // Assert
            firstCall.Should().BeSameAs(secondCall);
            _mockAppConfigReader.Verify(x => x.ReadConfiguration(), Times.Once);
        }

        #endregion

        #region LoadConfigurationAsync Tests

        [Fact]
        public async Task LoadConfigurationAsync_ShouldReturnCachedConfiguration_WhenAlreadyLoaded()
        {
            // Arrange
            var expectedConfiguration = TestFixtures.CreateDefaultConfiguration();
            
            _mockAppConfigReader.Setup(x => x.ReadConfigurationAsync())
                .ReturnsAsync(expectedConfiguration);

            // Load configuration first to cache it
            var initialLoad = await _sut.LoadConfigurationAsync();

            // Reset the mock to verify it's not called again
            _mockAppConfigReader.Reset();

            // Act
            var result = await _sut.LoadConfigurationAsync();

            // Assert
            result.Should().BeSameAs(initialLoad);
            _mockAppConfigReader.Verify(x => x.ReadConfigurationAsync(), Times.Never);
        }

        [Fact]
        public async Task LoadConfigurationAsync_ShouldLoadFromAppConfigReader_WhenNotCached()
        {
            // Arrange
            var expectedConfiguration = TestFixtures.CreateDefaultConfiguration();
            
            _mockAppConfigReader.Setup(x => x.ReadConfigurationAsync())
                .ReturnsAsync(expectedConfiguration);

            // Act
            var result = await _sut.LoadConfigurationAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedConfiguration);
            _mockAppConfigReader.Verify(x => x.ReadConfigurationAsync(), Times.Once);
        }

        [Fact]
        public async Task LoadConfigurationAsync_ShouldCallInitializeRuntimeProperties_WhenConfigurationLoaded()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket",
                CurrentVersion = "1.0.0.0"
            };
            
            _mockAppConfigReader.Setup(x => x.ReadConfigurationAsync())
                .ReturnsAsync(configuration);

            // Act
            var result = await _sut.LoadConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            // Verify that InitializeRuntimeProperties was called by checking that Architecture is set
            result.Architecture.Should().BeOneOf(SystemArchitecture.X86, SystemArchitecture.X64, SystemArchitecture.ARM64);
        }

        [Fact]
        public async Task LoadConfigurationAsync_ShouldUseDefaultConfiguration_WhenAppConfigReaderReturnsNull()
        {
            // Arrange
            _mockAppConfigReader.Setup(x => x.ReadConfigurationAsync())
                .ReturnsAsync((UpdaterConfiguration?)null);

            // Act
            var result = await _sut.LoadConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            result.UpdateChannel.Should().Be(UpdateChannel.Release);
            result.GitHubOwner.Should().Be("mchave3");
            result.GitHubRepository.Should().Be("Bucket");
            result.CurrentVersion.Should().Be("1.0.0.0");
            result.Architecture.Should().BeOneOf(SystemArchitecture.X86, SystemArchitecture.X64, SystemArchitecture.ARM64);
        }

        [Fact]
        public async Task LoadConfigurationAsync_ShouldReturnDefaultConfiguration_WhenExceptionOccurs()
        {
            // Arrange
            _mockAppConfigReader.Setup(x => x.ReadConfigurationAsync())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            var result = await _sut.LoadConfigurationAsync();

            // Assert
            result.Should().NotBeNull();
            result.UpdateChannel.Should().Be(UpdateChannel.Release);
            result.GitHubOwner.Should().Be("mchave3");
            result.GitHubRepository.Should().Be("Bucket");
            result.CurrentVersion.Should().Be("1.0.0.0");
            result.Architecture.Should().BeOneOf(SystemArchitecture.X86, SystemArchitecture.X64, SystemArchitecture.ARM64);
        }

        #endregion
    }
}