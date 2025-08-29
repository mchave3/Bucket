using Bucket.App.Tests.Services;
using Bucket.Core.Models;

namespace Bucket.App.Tests.Localization;

public class LanguageMappingTests
{
    [Theory]
    [InlineData("en-US", "en-US")] // Direct match
    [InlineData("fr-FR", "fr-FR")] // Direct match
    [InlineData("en-GB", "en-US")] // Language family fallback
    [InlineData("fr-CA", "fr-FR")] // Language family fallback
    [InlineData("es-ES", "en-US")] // Unsupported language -> default
    [InlineData("de-DE", "en-US")] // Unsupported language -> default
    public void MockService_WithDifferentSystemLanguages_ShouldReturnCorrectMapping(string systemLanguage, string expectedResult)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(systemLanguage);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("fr-CA")]
    [InlineData("fr-BE")]
    [InlineData("fr-CH")]
    public void MockService_WithFrenchVariants_ShouldMapToFrenchFrance(string frenchVariant)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(frenchVariant);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("fr-FR", result);
    }

    [Theory]
    [InlineData("en-GB")]
    [InlineData("en-AU")]
    [InlineData("en-CA")]
    [InlineData("en-NZ")]
    public void MockService_WithEnglishVariants_ShouldMapToEnglishUS(string englishVariant)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(englishVariant);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal("en-US", result);
    }

    [Theory]
    [InlineData("zh-CN")]
    [InlineData("ja-JP")]
    [InlineData("ko-KR")]
    [InlineData("ar-SA")]
    [InlineData("hi-IN")]
    public void MockService_WithUnsupportedLanguages_ShouldReturnDefault(string unsupportedLanguage)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(unsupportedLanguage);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void MockService_WithInvalidLanguageCodes_ShouldReturnDefault(string invalidLanguage)
    {
        // Arrange
        var mockService = new MockSystemLanguageDetectionService(invalidLanguage);

        // Act
        var result = mockService.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Fact]
    public void MockService_ShouldReturnSystemLanguageCodeAsConfigured()
    {
        // Arrange
        const string expectedLanguage = "fr-CA";
        var mockService = new MockSystemLanguageDetectionService(expectedLanguage);

        // Act
        var result = mockService.GetSystemLanguageCode();

        // Assert
        Assert.Equal(expectedLanguage, result);
    }

    [Fact]
    public void MockService_GetBestMatchingLanguage_ShouldAlwaysReturnSupportedLanguage()
    {
        // Test with various language codes to ensure mapping always returns supported languages
        var testLanguages = new[]
        {
            "en-US", "fr-FR", "en-GB", "fr-CA", "es-ES", "de-DE",
            "zh-CN", "ja-JP", "pt-BR", "it-IT", "ru-RU"
        };

        foreach (var language in testLanguages)
        {
            // Arrange
            var mockService = new MockSystemLanguageDetectionService(language);

            // Act
            var result = mockService.GetBestMatchingLanguage();

            // Assert
            Assert.True(SupportedLanguages.IsSupported(result),
                $"Language '{language}' mapped to unsupported language '{result}'");
        }
    }
}
