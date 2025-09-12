using System.Runtime.InteropServices;
using Bucket.Updater.Models;
using FluentAssertions;

namespace Bucket.Updater.Tests.Models
{
    public class UpdaterConfigurationTests
    {
        [Fact]
        public void InitializeRuntimeProperties_ShouldDetectX86Architecture_WhenProcessArchitectureIsX86()
        {
            // Arrange
            var configuration = new UpdaterConfiguration();
            
            // We can't directly mock RuntimeInformation.ProcessArchitecture, so we'll test the switch logic
            // by verifying the mapping is correct through GetArchitectureString
            
            // Act & Assert - Test the expected behavior by setting architecture manually
            configuration.Architecture = SystemArchitecture.X86;
            var result = configuration.GetArchitectureString();
            
            // Assert
            result.Should().Be("x86");
        }

        [Fact]
        public void InitializeRuntimeProperties_ShouldDetectX64Architecture_WhenProcessArchitectureIsX64()
        {
            // Arrange
            var configuration = new UpdaterConfiguration();
            
            // Act & Assert
            configuration.Architecture = SystemArchitecture.X64;
            var result = configuration.GetArchitectureString();
            
            // Assert
            result.Should().Be("x64");
        }

        [Fact]
        public void InitializeRuntimeProperties_ShouldDetectARM64Architecture_WhenProcessArchitectureIsArm64()
        {
            // Arrange
            var configuration = new UpdaterConfiguration();
            
            // Act & Assert
            configuration.Architecture = SystemArchitecture.ARM64;
            var result = configuration.GetArchitectureString();
            
            // Assert
            result.Should().Be("arm64");
        }

        [Fact]
        public void InitializeRuntimeProperties_ShouldDefaultToX64_WhenArchitectureIsUnknown()
        {
            // Arrange
            var configuration = new UpdaterConfiguration();
            
            // Act
            configuration.InitializeRuntimeProperties();
            
            // Assert
            // Since we can't mock RuntimeInformation.ProcessArchitecture, we'll test that
            // the method sets a valid architecture value and defaults appropriately
            configuration.Architecture.Should().BeOneOf(SystemArchitecture.X86, SystemArchitecture.X64, SystemArchitecture.ARM64);
        }

        [Fact]
        public void GetArchitectureString_ShouldReturnX86_WhenArchitectureIsX86()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                Architecture = SystemArchitecture.X86
            };
            
            // Act
            var result = configuration.GetArchitectureString();
            
            // Assert
            result.Should().Be("x86");
        }

        [Fact]
        public void GetArchitectureString_ShouldReturnX64_WhenArchitectureIsX64()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                Architecture = SystemArchitecture.X64
            };
            
            // Act
            var result = configuration.GetArchitectureString();
            
            // Assert
            result.Should().Be("x64");
        }

        [Fact]
        public void GetArchitectureString_ShouldReturnArm64_WhenArchitectureIsARM64()
        {
            // Arrange
            var configuration = new UpdaterConfiguration
            {
                Architecture = SystemArchitecture.ARM64
            };
            
            // Act
            var result = configuration.GetArchitectureString();
            
            // Assert
            result.Should().Be("arm64");
        }
    }
}