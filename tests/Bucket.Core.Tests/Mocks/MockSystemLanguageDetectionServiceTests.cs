using Bucket.Core.Models;
using Bucket.Core.Services;

namespace Bucket.Core.Tests.Mocks;

/// <summary>
/// Mock implementation of ISystemLanguageDetectionService for testing without WinRT dependencies
/// </summary>
public class MockSystemLanguageDetectionService : ISystemLanguageDetectionService
{
    private readonly string _systemLanguageCode;
    private readonly string? _bestMatchingLanguage;

    public MockSystemLanguageDetectionService(string systemLanguageCode, string? bestMatchingLanguage = null)
    {
        _systemLanguageCode = systemLanguageCode ?? throw new ArgumentNullException(nameof(systemLanguageCode));
        _bestMatchingLanguage = bestMatchingLanguage;
    }

    public string GetSystemLanguageCode()
    {
        return _systemLanguageCode;
    }

    public string GetBestMatchingLanguage()
    {
        // If a specific best matching language is provided, use it
        // Otherwise, use the mapping logic from SupportedLanguages
        return _bestMatchingLanguage ?? SupportedLanguages.MapOSLanguageToSupported(_systemLanguageCode);
    }
}

/// <summary>
/// Tests for the mock system language detection service
/// </summary>
public class MockSystemLanguageDetectionServiceTests
{
    [Fact]
    public void Constructor_WithValidSystemLanguageCode_CreatesInstance()
    {
        // Arrange & Act
        var mockService = new MockSystemLanguageDetectionService("fr-CA");

        // Assert
        Assert.NotNull(mockService);
    }

    [Fact]
    public void Constructor_WithNullSystemLanguageCode_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MockSystemLanguageDetectionService(null!));
    }

    [Fact]
    public void GetSystemLanguageCode_ReturnsConfiguredLanguageCode()
    {
        // Arrange
        var expectedLanguageCode = "fr-CA";
        var mockService = new MockSystemLanguageDetectionService(expectedLanguageCode);

        // Act
        var result = mockService.GetSystemLanguageCode();

        // Assert
        Assert.Equal(expectedLanguageCode, result);
    }

    [Theory]
    [InlineData("en-US", "en-US")]
    [InlineData("fr-FR", "fr-FR")]
    [InlineData("en-GB", "en-US")]
    [InlineData("fr-CA", "fr-FR")]
    [InlineData("es-ES", "en-US")]
    public void GetBestMatchingLanguage_WithoutCustomMapping_UsesSupportedLanguagesMapping(string systemLanguage, string expectedMapping)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(systemLanguage);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(expectedMapping, result);
    }

    [Fact]
    public void GetBestMatchingLanguage_WithCustomMapping_ReturnsCustomMapping()
    {
        // Arrange
        var systemLanguage = "de-DE";
        var customMapping = "fr-FR";
        var mockService = new MockSystemLanguageDetectionService(systemLanguage, customMapping);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(customMapping, result);
    }

    [Fact]
    public void MockService_ImplementsInterface()
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService("en-US");

        // Act & Assert
        Assert.IsAssignableFrom<ISystemLanguageDetectionService>(mockService);
    }

    [Fact]
    public void MockService_CanBeUsedInDependencyInjection()
    {
        // Arrange - Simulate DI container registration
        ISystemLanguageDetectionService service = new MockSystemLanguageDetectionService("fr-FR");

        // Act
        var systemLanguage = service.GetSystemLanguageCode();
        var bestMatch = service.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("fr-FR", systemLanguage);
        Assert.Equal("fr-FR", bestMatch);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("xyz-123")]
    public void GetBestMatchingLanguage_WithInvalidSystemLanguage_ReturnsDefaultLanguage(string invalidSystemLanguage)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(invalidSystemLanguage);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Fact]
    public void MockService_ConsistentBehavior_AcrossMultipleCalls()
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService("en-GB");

        // Act
        var systemLanguage1 = mockService.GetSystemLanguageCode();
        var systemLanguage2 = mockService.GetSystemLanguageCode();
        var bestMatch1 = mockService.GetBestMatchingLanguage();
        var bestMatch2 = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(systemLanguage1, systemLanguage2);
        Assert.Equal(bestMatch1, bestMatch2);
        Assert.Equal("en-GB", systemLanguage1);
        Assert.Equal("en-US", bestMatch1);
    }
}
