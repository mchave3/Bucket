using Bucket.Core.Helpers;
using Bucket.Core.Models;

namespace Bucket.Core.Tests.Helpers;

/// <summary>
/// Unit tests for LocalizationHelper class
/// </summary>
public class LocalizationHelperTests
{
    [Fact]
    public void ValidateLanguageCode_WithValidLanguageCode_ReturnsLanguageCode()
    {
        // Arrange
        const string validLanguageCode = "en-US";

        // Act
        var result = LocalizationHelper.ValidateLanguageCode(validLanguageCode);

        // Assert
        Assert.Equal(validLanguageCode, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateLanguageCode_WithNullOrWhitespace_ReturnsDefaultLanguage(string? invalidLanguageCode)
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode(invalidLanguageCode);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Fact]
    public void ValidateLanguageCode_WithUnsupportedLanguageCode_ReturnsDefaultLanguage()
    {
        // Arrange
        const string unsupportedLanguageCode = "de-DE";

        // Act
        var result = LocalizationHelper.ValidateLanguageCode(unsupportedLanguageCode);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void ValidateLanguageCode_WithSupportedLanguageCode_ReturnsLanguageCode(string supportedLanguageCode)
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode(supportedLanguageCode);

        // Assert
        Assert.Equal(supportedLanguageCode, result);
    }

    [Fact]
    public void GetLanguageItem_WithValidLanguageCode_ReturnsCorrectLanguageItem()
    {
        // Arrange
        const string validLanguageCode = "fr-FR";
        var expectedLanguageItem = SupportedLanguages.GetByCode(validLanguageCode);

        // Act
        var result = LocalizationHelper.GetLanguageItem(validLanguageCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLanguageItem!.Code, result.Code);
        Assert.Equal(expectedLanguageItem.DisplayName, result.DisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("de-DE")]
    public void GetLanguageItem_WithInvalidLanguageCode_ReturnsDefaultLanguageItem(string? invalidLanguageCode)
    {
        // Arrange
        var expectedLanguageItem = SupportedLanguages.GetDefault();

        // Act
        var result = LocalizationHelper.GetLanguageItem(invalidLanguageCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLanguageItem.Code, result.Code);
        Assert.Equal(expectedLanguageItem.DisplayName, result.DisplayName);
    }

    [Theory]
    [InlineData("en-US", "fr-FR", true)]
    [InlineData("fr-FR", "en-US", true)]
    [InlineData("en-US", "en-US", false)]
    [InlineData("fr-FR", "fr-FR", false)]
    public void ShouldChangeLanguage_WithDifferentLanguageCodes_ReturnsExpectedResult(
        string currentLanguage,
        string newLanguage,
        bool expectedResult)
    {
        // Act
        var result = LocalizationHelper.ShouldChangeLanguage(currentLanguage, newLanguage);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("EN-US", "en-us", false)]
    [InlineData("fr-fr", "FR-FR", false)]
    public void ShouldChangeLanguage_WithCaseInsensitiveComparison_ReturnsFalse(
        string currentLanguage,
        string newLanguage,
        bool expectedResult)
    {
        // Act
        var result = LocalizationHelper.ShouldChangeLanguage(currentLanguage, newLanguage);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldChangeLanguage_WithNullOrWhitespaceNewLanguage_ReturnsFalse(string? newLanguage)
    {
        // Arrange
        const string currentLanguage = "en-US";

        // Act
        var result = LocalizationHelper.ShouldChangeLanguage(currentLanguage, newLanguage);

        // Assert
        Assert.False(result);
    }
}
