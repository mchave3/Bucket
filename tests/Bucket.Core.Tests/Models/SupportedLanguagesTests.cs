using Bucket.Core.Models;

namespace Bucket.Core.Tests.Models;

/// <summary>
/// Unit tests for SupportedLanguages static class
/// </summary>
public class SupportedLanguagesTests
{
    [Fact]
    public void DefaultLanguage_HasCorrectValue()
    {
        // Assert
        Assert.Equal("en-US", SupportedLanguages.DefaultLanguage);
    }

    [Fact]
    public void All_ContainsExpectedLanguages()
    {
        // Assert
        Assert.NotNull(SupportedLanguages.All);
        Assert.Contains(SupportedLanguages.All, lang => lang.Code == "en-US" && lang.DisplayName == "English");
        Assert.Contains(SupportedLanguages.All, lang => lang.Code == "fr-FR" && lang.DisplayName == "Français");
    }

    [Fact]
    public void All_IsReadOnly()
    {
        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<LanguageItem>>(SupportedLanguages.All);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void GetByCode_WithValidCode_ReturnsCorrectLanguageItem(string validCode)
    {
        // Act
        var result = SupportedLanguages.GetByCode(validCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validCode, result.Code);
    }

    [Theory]
    [InlineData("EN-US")]
    [InlineData("en-us")]
    [InlineData("FR-FR")]
    [InlineData("fr-fr")]
    public void GetByCode_WithDifferentCasing_ReturnsCorrectLanguageItem(string codeWithDifferentCasing)
    {
        // Act
        var result = SupportedLanguages.GetByCode(codeWithDifferentCasing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Code, codeWithDifferentCasing, StringComparer.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("es-ES")]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void GetByCode_WithInvalidCode_ReturnsNull(string? invalidCode)
    {
        // Act
        var result = SupportedLanguages.GetByCode(invalidCode!);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("en-US", true)]
    [InlineData("fr-FR", true)]
    [InlineData("EN-US", true)]
    [InlineData("fr-fr", true)]
    public void IsSupported_WithSupportedCode_ReturnsTrue(string supportedCode, bool expected)
    {
        // Act
        var result = SupportedLanguages.IsSupported(supportedCode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("de-DE", false)]
    [InlineData("es-ES", false)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void IsSupported_WithUnsupportedCode_ReturnsFalse(string unsupportedCode, bool expected)
    {
        // Act
        var result = SupportedLanguages.IsSupported(unsupportedCode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetDefault_ReturnsDefaultLanguageItem()
    {
        // Act
        var result = SupportedLanguages.GetDefault();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SupportedLanguages.DefaultLanguage, result.Code);
        Assert.Equal("English", result.DisplayName);
    }

    /// <summary>
    /// Test data for MapOSLanguageToSupported method testing
    /// </summary>
    public static TheoryData<string, string> OSLanguageMappingData => new()
    {
        // Direct matches
        { "en-US", "en-US" },
        { "fr-FR", "fr-FR" },

        // Case insensitive matches
        { "EN-US", "en-US" },
        { "Fr-Fr", "fr-FR" },

        // Language family matches
        { "en-GB", "en-US" },
        { "en-CA", "en-US" },
        { "fr-CA", "fr-FR" },
        { "fr-BE", "fr-FR" },

        // Unsupported languages fallback to default
        { "de-DE", "en-US" },
        { "es-ES", "en-US" },
        { "zh-CN", "en-US" },

        // Invalid inputs fallback to default
        { "", "en-US" },
        { "   ", "en-US" },
        { "invalid", "en-US" }
    };

    [Theory]
    [MemberData(nameof(OSLanguageMappingData))]
    public void MapOSLanguageToSupported_WithVariousInputs_ReturnsExpectedMapping(
        string osLanguageCode,
        string expectedMappedCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(expectedMappedCode, result);
    }

    [Fact]
    public void MapOSLanguageToSupported_WithNullInput_ReturnsDefaultLanguage()
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(null!);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public void MapOSLanguageToSupported_WithLanguageFamilyOnly_ReturnsCorrectMapping(string languageFamily)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(languageFamily);

        // Assert
        var expectedLanguage = SupportedLanguages.All.FirstOrDefault(lang =>
            lang.Code.StartsWith(languageFamily, StringComparison.OrdinalIgnoreCase));

        if (expectedLanguage is not null)
        {
            Assert.Equal(expectedLanguage.Code, result);
        }
        else
        {
            Assert.Equal(SupportedLanguages.DefaultLanguage, result);
        }
    }

    [Fact]
    public void MapOSLanguageToSupported_WithComplexLanguageCode_ReturnsCorrectMapping()
    {
        // Arrange - Simulate a complex OS language code with multiple parts
        const string complexLanguageCode = "en-US-x-private";

        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(complexLanguageCode);

        // Assert
        // Should map to en-US since the language family is "en"
        Assert.Equal("en-US", result);
    }
}
