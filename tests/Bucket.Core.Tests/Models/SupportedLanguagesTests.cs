using Bucket.Core.Models;

namespace Bucket.Core.Tests.Models;

/// <summary>
/// Unit tests for SupportedLanguages class
/// </summary>
public class SupportedLanguagesTests
{
    [Fact]
    public void DefaultLanguage_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("en-US", SupportedLanguages.DefaultLanguage);
    }

    [Fact]
    public void All_ContainsExpectedLanguages()
    {
        // Act
        var allLanguages = SupportedLanguages.All;

        // Assert
        Assert.NotNull(allLanguages);
        Assert.Equal(2, allLanguages.Count);

        Assert.Contains(allLanguages, lang => lang.Code == "en-US" && lang.DisplayName == "English");
        Assert.Contains(allLanguages, lang => lang.Code == "fr-FR" && lang.DisplayName == "Français");
    }

    [Fact]
    public void All_IsReadOnly()
    {
        // Act
        var allLanguages = SupportedLanguages.All;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<LanguageItem>>(allLanguages);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void GetByCode_WithValidCode_ReturnsCorrectLanguageItem(string code)
    {
        // Act
        var result = SupportedLanguages.GetByCode(code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.Code);
    }

    [Theory]
    [InlineData("EN-US", "en-US")]
    [InlineData("fr-fr", "fr-FR")]
    [InlineData("En-Us", "en-US")]
    [InlineData("FR-fr", "fr-FR")]
    public void GetByCode_WithDifferentCasing_ReturnsCorrectLanguageItem(string inputCode, string expectedCode)
    {
        // Act
        var result = SupportedLanguages.GetByCode(inputCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCode, result.Code);
    }

    [Theory]
    [InlineData("es-ES")]
    [InlineData("de-DE")]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData(null)]
    public void GetByCode_WithInvalidCode_ReturnsNull(string? code)
    {
        // Act
        var result = SupportedLanguages.GetByCode(code!);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("EN-US")]
    [InlineData("fr-fr")]
    public void IsSupported_WithSupportedCode_ReturnsTrue(string code)
    {
        // Act
        var result = SupportedLanguages.IsSupported(code);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("es-ES")]
    [InlineData("de-DE")]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("123")]
    public void IsSupported_WithUnsupportedCode_ReturnsFalse(string code)
    {
        // Act
        var result = SupportedLanguages.IsSupported(code);

        // Assert
        Assert.False(result);
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

    [Theory]
    [InlineData("en-US", "en-US")]
    [InlineData("fr-FR", "fr-FR")]
    [InlineData("EN-US", "en-US")]
    [InlineData("fr-fr", "fr-FR")]
    public void MapOSLanguageToSupported_WithDirectMatch_ReturnsExactMatch(string osLanguageCode, string expectedCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(expectedCode, result);
    }

    [Theory]
    [InlineData("en-GB", "en-US")]  // English variant maps to en-US
    [InlineData("en-CA", "en-US")]  // English variant maps to en-US
    [InlineData("fr-CA", "fr-FR")]  // French variant maps to fr-FR
    [InlineData("fr-BE", "fr-FR")]  // French variant maps to fr-FR
    public void MapOSLanguageToSupported_WithLanguageFamily_ReturnsMatchingFamily(string osLanguageCode, string expectedCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(expectedCode, result);
    }

    [Theory]
    [InlineData("es-ES")]      // Spanish - not supported
    [InlineData("de-DE")]      // German - not supported
    [InlineData("it-IT")]      // Italian - not supported
    [InlineData("zh-CN")]      // Chinese - not supported
    [InlineData("ja-JP")]      // Japanese - not supported
    [InlineData("invalid")]    // Invalid code
    [InlineData("123")]        // Numeric code
    public void MapOSLanguageToSupported_WithUnsupportedLanguage_ReturnsDefaultLanguage(string osLanguageCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MapOSLanguageToSupported_WithNullOrWhitespace_ReturnsDefaultLanguage(string? osLanguageCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode!);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData("EN")]         // Language family only, uppercase
    [InlineData("fr")]         // Language family only, lowercase
    [InlineData("FR")]         // Language family only, uppercase
    [InlineData("en")]         // Language family only, lowercase
    public void MapOSLanguageToSupported_WithLanguageFamilyOnly_ReturnsMatchingFamily(string osLanguageCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        var expected = osLanguageCode.ToUpperInvariant() switch
        {
            "EN" => "en-US",
            "FR" => "fr-FR",
            _ => SupportedLanguages.DefaultLanguage
        };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MapOSLanguageToSupported_WithComplexLanguageCode_HandlesCorrectly()
    {
        // Arrange - Test with more complex language codes
        var testCases = new[]
        {
            ("en-US-POSIX", "en-US"),      // Complex English variant
            ("fr-FR-u-ca-gregory", "fr-FR"), // Complex French variant
            ("en-Latn-US", "en-US"),       // Script tag included
            ("fr-Latn-FR", "fr-FR")        // Script tag included
        };

        foreach (var (input, expected) in testCases)
        {
            // Act
            var result = SupportedLanguages.MapOSLanguageToSupported(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
