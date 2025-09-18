namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using NSubstitute;
    using Xunit;

    public sealed class ConfigurationServiceTests
    {
        private readonly ConfigurationService _testClass;
        private readonly IAppConfigReader _appConfigReader;

        public ConfigurationServiceTests()
        {
            _appConfigReader = Substitute.For<IAppConfigReader>();
            _testClass = new ConfigurationService(_appConfigReader);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ConfigurationService(_appConfigReader);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullAppConfigReader()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(default(IAppConfigReader)));
        }

        [Fact]
        public void GetConfigurationWithValidConfigShouldReturnAndCacheConfiguration()
        {
            // Arrange
            var expectedConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket"
            };
            expectedConfig.InitializeRuntimeProperties();

            _appConfigReader.ReadConfiguration().Returns(expectedConfig);

            // Act
            var result1 = _testClass.GetConfiguration();
            var result2 = _testClass.GetConfiguration(); // Should use cache

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(UpdateChannel.Release, result1.UpdateChannel);
            Assert.Equal("mchave3", result1.GitHubOwner);
            Assert.Equal("Bucket", result1.GitHubRepository);
            Assert.Same(result1, result2); // Should be the same cached instance

            // Verify AppConfigReader was called only once due to caching
            _appConfigReader.Received(1).ReadConfiguration();
        }

        [Fact]
        public void GetConfigurationWhenConfigReaderThrowsShouldReturnDefaultConfiguration()
        {
            // Arrange
            _appConfigReader.ReadConfiguration().Returns(x => throw new FileNotFoundException("AppConfig.json not found"));

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default value
            Assert.Equal("mchave3", result.GitHubOwner); // Default value
            Assert.Equal("Bucket", result.GitHubRepository); // Default value
            Assert.Equal("1.0.0.0", result.CurrentVersion); // Default value
        }

        [Fact]
        public void GetConfigurationWhenConfigReaderReturnsNullShouldReturnDefaultConfiguration()
        {
            // Arrange
            _appConfigReader.ReadConfiguration().Returns((UpdaterConfiguration?)null);

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default value
            Assert.Equal("mchave3", result.GitHubOwner); // Default value
            Assert.Equal("Bucket", result.GitHubRepository); // Default value
        }

        [Fact]
        public async Task LoadConfigurationAsyncWithValidConfigShouldReturnAndCacheConfiguration()
        {
            // Arrange
            var expectedConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Nightly,
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo",
                CurrentVersion = "24.1.15.1-Nightly"
            };
            expectedConfig.InitializeRuntimeProperties();

            _appConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(expectedConfig));

            // Act
            var result1 = await _testClass.LoadConfigurationAsync();
            var result2 = await _testClass.LoadConfigurationAsync(); // Should use cache

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(UpdateChannel.Nightly, result1.UpdateChannel);
            Assert.Equal("testowner", result1.GitHubOwner);
            Assert.Equal("testrepo", result1.GitHubRepository);
            Assert.Contains("Nightly", result1.CurrentVersion, StringComparison.OrdinalIgnoreCase);
            Assert.Same(result1, result2); // Should be the same cached instance

            // Verify AppConfigReader was called only once due to caching
            await _appConfigReader.Received(1).ReadConfigurationAsync();
        }

        [Fact]
        public async Task LoadConfigurationAsyncWhenConfigReaderThrowsShouldReturnDefaultConfiguration()
        {
            // Arrange
            _appConfigReader.ReadConfigurationAsync().Returns<UpdaterConfiguration?>(x => throw new JsonException("Invalid JSON format"));

            // Act
            var result = await _testClass.LoadConfigurationAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default value
            Assert.Equal("mchave3", result.GitHubOwner); // Default value
            Assert.Equal("Bucket", result.GitHubRepository); // Default value
        }

        [Fact]
        public async Task LoadConfigurationAsyncWhenConfigReaderReturnsNullShouldReturnDefaultConfiguration()
        {
            // Arrange
            _appConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(null));

            // Act
            var result = await _testClass.LoadConfigurationAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(UpdateChannel.Release, result.UpdateChannel); // Default value
            Assert.Equal("mchave3", result.GitHubOwner); // Default value
            Assert.Equal("Bucket", result.GitHubRepository); // Default value
        }

        [Fact]
        public async Task LoadConfigurationAsyncReturnsCachedConfigAfterSyncLoad()
        {
            // Arrange
            var expectedConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Release,
                GitHubOwner = "mchave3",
                GitHubRepository = "Bucket"
            };
            expectedConfig.InitializeRuntimeProperties();

            // Set up both sync and async mocks for complete coverage
            _appConfigReader.ReadConfiguration().Returns(expectedConfig);
            _appConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(expectedConfig));

            // Act
            var syncResult = _testClass.GetConfiguration(); // Load synchronously first
            var asyncResult = await _testClass.LoadConfigurationAsync(); // Should use cache

            // Assert
            Assert.NotNull(syncResult);
            Assert.NotNull(asyncResult);
            Assert.Same(syncResult, asyncResult); // Should be the same cached instance
        }

        [Fact]
        public async Task GetConfigurationReturnsCachedConfigAfterAsyncLoad()
        {
            // Arrange
            var expectedConfig = new UpdaterConfiguration
            {
                UpdateChannel = UpdateChannel.Nightly,
                GitHubOwner = "testowner",
                GitHubRepository = "testrepo"
            };
            expectedConfig.InitializeRuntimeProperties();

            _appConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(expectedConfig));

            // Act - Load async first, then sync
            var asyncResult = await _testClass.LoadConfigurationAsync();
            var syncResult = _testClass.GetConfiguration(); // Should use cache

            // Assert
            Assert.NotNull(asyncResult);
            Assert.NotNull(syncResult);
            Assert.Same(asyncResult, syncResult); // Should be the same cached instance
        }

        [Fact]
        public void GetConfigurationWithDifferentExceptionTypesShouldAlwaysReturnDefault()
        {
            // Test various exception types that might occur during config reading
            var exceptions = new Exception[]
            {
                new FileNotFoundException("Config file not found"),
                new JsonException("Invalid JSON"),
                new UnauthorizedAccessException("Access denied"),
                new IOException("I/O error"),
                new ArgumentException("Invalid argument")
            };

            foreach (var exception in exceptions)
            {
                // Arrange
                var mockReader = Substitute.For<IAppConfigReader>();
                mockReader.ReadConfiguration().Returns(x => throw exception);
                var testService = new ConfigurationService(mockReader);

                // Act
                var result = testService.GetConfiguration();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(UpdateChannel.Release, result.UpdateChannel);
                Assert.Equal("mchave3", result.GitHubOwner);
                Assert.Equal("Bucket", result.GitHubRepository);
            }
        }

        [Fact]
        public async Task LoadConfigurationAsyncWithDifferentExceptionTypesShouldAlwaysReturnDefault()
        {
            // Test various exception types that might occur during async config reading
            var exceptions = new Exception[]
            {
                new TaskCanceledException("Operation cancelled"),
                new JsonException("Invalid JSON format"),
                new FileNotFoundException("Config file missing")
            };

            foreach (var exception in exceptions)
            {
                // Arrange
                var mockReader = Substitute.For<IAppConfigReader>();
                mockReader.ReadConfigurationAsync().Returns<UpdaterConfiguration?>(x => throw exception);
                var testService = new ConfigurationService(mockReader);

                // Act
                var result = await testService.LoadConfigurationAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(UpdateChannel.Release, result.UpdateChannel);
                Assert.Equal("mchave3", result.GitHubOwner);
                Assert.Equal("Bucket", result.GitHubRepository);
            }
        }

        [Fact]
        public void GetConfigurationShouldInitializeRuntimePropertiesOnDefault()
        {
            // Arrange
            _appConfigReader.ReadConfiguration().Returns((UpdaterConfiguration?)null);

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GetArchitectureString()); // Should have valid architecture string
        }

        [Fact]
        public async Task LoadConfigurationAsyncShouldInitializeRuntimePropertiesOnDefault()
        {
            // Arrange
            _appConfigReader.ReadConfigurationAsync().Returns(Task.FromResult<UpdaterConfiguration?>(null));

            // Act
            var result = await _testClass.LoadConfigurationAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.GetArchitectureString()); // Should have valid architecture string
        }
    }
}