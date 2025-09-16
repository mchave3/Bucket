namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Bucket.Updater.Models;
    using Bucket.Updater.Services;
    using NSubstitute;
    using Xunit;

    public class ConfigurationServiceTests
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
        public void CanCallGetConfiguration()
        {
            // Arrange
            _appConfigReader.ReadConfiguration().Returns(new UpdaterConfiguration());

            // Act
            var result = _testClass.GetConfiguration();

            // Assert
            _appConfigReader.Received().ReadConfiguration();

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallLoadConfigurationAsync()
        {
            // Arrange
            _appConfigReader.ReadConfigurationAsync().Returns(new UpdaterConfiguration());

            // Act
            var result = await _testClass.LoadConfigurationAsync();

            // Assert
            await _appConfigReader.Received().ReadConfigurationAsync();

            throw new NotImplementedException("Create or modify test");
        }
    }
}