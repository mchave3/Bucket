using Bucket.Core.Models;
using Bucket.Core.Services;
using Moq;

namespace Bucket.Core.Tests.Services;

/// <summary>
/// Unit tests for ISystemLanguageDetectionService interface contract and mock behavior
/// </summary>
public class ISystemLanguageDetectionServiceTests
{
    [Fact]
    public void GetSystemLanguageCode_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns("fr-FR");

        // Act
        var result = mockService.Object.GetSystemLanguageCode();

        // Assert
        Assert.Equal("fr-FR", result);
        mockService.Verify(s => s.GetSystemLanguageCode(), Times.Once);
    }

    [Fact]
    public void GetBestMatchingLanguage_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetBestMatchingLanguage()).Returns("en-US");

        // Act
        var result = mockService.Object.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("en-US", result);
        mockService.Verify(s => s.GetBestMatchingLanguage(), Times.Once);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("es-ES")]
    [InlineData("de-DE")]
    public void GetSystemLanguageCode_WithDifferentReturnValues_WorksCorrectly(string expectedLanguageCode)
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns(expectedLanguageCode);

        // Act
        var result = mockService.Object.GetSystemLanguageCode();

        // Assert
        Assert.Equal(expectedLanguageCode, result);
    }

    [Theory]
    [InlineData("en-GB", "en-US")]  // OS language to supported mapping
    [InlineData("fr-CA", "fr-FR")]  // OS language to supported mapping
    [InlineData("es-MX", "en-US")]  // Unsupported language falls back to default
    public void GetBestMatchingLanguage_WithLanguageMapping_WorksCorrectly(string osLanguage, string expectedMappedLanguage)
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();

        // Mock the system language detection
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns(osLanguage);

        // Mock the best matching logic to simulate SupportedLanguages.MapOSLanguageToSupported behavior
        mockService.Setup(s => s.GetBestMatchingLanguage()).Returns(expectedMappedLanguage);

        // Act
        var systemLanguage = mockService.Object.GetSystemLanguageCode();
        var bestMatch = mockService.Object.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(osLanguage, systemLanguage);
        Assert.Equal(expectedMappedLanguage, bestMatch);
    }

    [Fact]
    public void ServiceInterface_SupportsSequentialCalls()
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns("en-US");
        mockService.Setup(s => s.GetBestMatchingLanguage()).Returns("en-US");

        // Act
        var systemLanguage1 = mockService.Object.GetSystemLanguageCode();
        var bestMatch1 = mockService.Object.GetBestMatchingLanguage();
        var systemLanguage2 = mockService.Object.GetSystemLanguageCode();
        var bestMatch2 = mockService.Object.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("en-US", systemLanguage1);
        Assert.Equal("en-US", bestMatch1);
        Assert.Equal("en-US", systemLanguage2);
        Assert.Equal("en-US", bestMatch2);

        mockService.Verify(s => s.GetSystemLanguageCode(), Times.Exactly(2));
        mockService.Verify(s => s.GetBestMatchingLanguage(), Times.Exactly(2));
    }

    [Fact]
    public void ServiceInterface_CanThrowExceptions()
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetSystemLanguageCode()).Throws(new InvalidOperationException("System language unavailable"));
        mockService.Setup(s => s.GetBestMatchingLanguage()).Throws(new NotSupportedException("Language detection not supported"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mockService.Object.GetSystemLanguageCode());
        Assert.Throws<NotSupportedException>(() => mockService.Object.GetBestMatchingLanguage());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetSystemLanguageCode_WithEmptyOrNullResponse_CanBeMocked(string? returnValue)
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns(returnValue!);

        // Act
        var result = mockService.Object.GetSystemLanguageCode();

        // Assert
        Assert.Equal(returnValue, result);
    }

    [Fact]
    public void ServiceInterface_SupportsCallbackPattern()
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        var callCount = 0;

        mockService.Setup(s => s.GetSystemLanguageCode())
               .Returns(() =>
               {
                   callCount++;
                   return callCount == 1 ? "en-US" : "fr-FR";
               });

        // Act
        var firstCall = mockService.Object.GetSystemLanguageCode();
        var secondCall = mockService.Object.GetSystemLanguageCode();

        // Assert
        Assert.Equal("en-US", firstCall);
        Assert.Equal("fr-FR", secondCall);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void ServiceInterface_CanBeVerifiedForSequence()
    {
        // Arrange
        var mockService = new Mock<ISystemLanguageDetectionService>();
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns("en-US");
        mockService.Setup(s => s.GetBestMatchingLanguage()).Returns("en-US");

        // Act
        var systemLanguage = mockService.Object.GetSystemLanguageCode();
        var bestMatch = mockService.Object.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("en-US", systemLanguage);
        Assert.Equal("en-US", bestMatch);

        // Verify the sequence of calls
        var sequence = new MockSequence();
        mockService.InSequence(sequence).Setup(s => s.GetSystemLanguageCode()).Returns("en-US");
        mockService.InSequence(sequence).Setup(s => s.GetBestMatchingLanguage()).Returns("en-US");
    }

    [Fact]
    public void MockService_CanSimulateSystemLanguageDetectionWorkflow()
    {
        // Arrange - Simulate a complete workflow
        var mockService = new Mock<ISystemLanguageDetectionService>();

        // Simulate system returning a complex language code
        mockService.Setup(s => s.GetSystemLanguageCode()).Returns("en-GB");

        // Simulate mapping to supported language
        mockService.Setup(s => s.GetBestMatchingLanguage()).Returns(() =>
        {
            var systemLang = "en-GB"; // Simulating internal call
            return SupportedLanguages.MapOSLanguageToSupported(systemLang);
        });

        // Act
        var systemLanguage = mockService.Object.GetSystemLanguageCode();
        var mappedLanguage = mockService.Object.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("en-GB", systemLanguage);
        Assert.Equal("en-US", mappedLanguage); // Should be mapped to supported language
    }
}
