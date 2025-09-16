namespace Bucket.Updater.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Bucket.Updater.Services;
    using Xunit;

    public class AppConfigReaderTests
    {
        private readonly AppConfigReader _testClass;

        public AppConfigReaderTests()
        {
            _testClass = new AppConfigReader();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new AppConfigReader();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanCallReadConfiguration()
        {
            // Act
            var result = _testClass.ReadConfiguration();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallReadConfigurationAsync()
        {
            // Act
            var result = await _testClass.ReadConfigurationAsync();

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}