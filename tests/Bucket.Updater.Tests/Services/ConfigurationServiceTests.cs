namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using Moq;
    using Xunit;

    public class ConfigurationServiceTests
    {
        private readonly ConfigurationService _testClass;
        private readonly Mock<IAppConfigReader> _appConfigReader;

        public ConfigurationServiceTests()
        {
            _appConfigReader = new Mock<IAppConfigReader>();
            _testClass = new ConfigurationService(_appConfigReader.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ConfigurationService(_appConfigReader.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullAppConfigReader()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(default(IAppConfigReader)));
        }

        [Fact]
        public void CanCallGetConfiguration()
        {
            // Arrange
            _appConfigReader.Setup(mock => mock.ReadConfiguration()).Returns(new UpdaterConfiguration());

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            _appConfigReader.Verify(mock => mock.ReadConfiguration());

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallLoadConfigurationAsync()
        {
            // Arrange
            _appConfigReader.Setup(mock => mock.ReadConfigurationAsync()).ReturnsAsync(new UpdaterConfiguration());

            // Act
            var result = await _testClass.LoadConfigurationAsync();

            // Assert
            _appConfigReader.Verify(mock => mock.ReadConfigurationAsync());

            throw new NotImplementedException("Create or modify test");
        }
    }
}