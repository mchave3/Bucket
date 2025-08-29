using Bucket.Core.Models;

namespace Bucket.Core.Tests;

public class SupportedLanguagesTests
{
    [Fact]
    public void DefaultLanguage_ShouldBeEnglishUS()
    {
        // Assert
        Assert.Equal("en-US", SupportedLanguages.DefaultLanguage);
    }

    [Fact]
    public void All_ShouldContainExpectedLanguages()
    {
        // Assert
        Assert.NotNull(SupportedLanguages.All);
        Assert.Contains(SupportedLanguages.All, lang => lang.Code == "en-US" && lang.DisplayName == "English");
        Assert.Contains(SupportedLanguages.All, lang => lang.Code == "fr-FR" && lang.DisplayName == "Français");
        Assert.Equal(2, SupportedLanguages.All.Count);
    }

    [Theory]
    [InlineData("en-US", true)]
    [InlineData("fr-FR", true)]
    [InlineData("es-ES", false)]
    [InlineData("de-DE", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsSupported_ShouldReturnCorrectResult(string languageCode, bool expectedResult)
    {
        // Act
        var result = SupportedLanguages.IsSupported(languageCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("EN-US")]
    [InlineData("en-us")]
    [InlineData("En-Us")]
    public void IsSupported_ShouldBeCaseInsensitive(string languageCode)
    {
        // Act
        var result = SupportedLanguages.IsSupported(languageCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetByCode_WithValidCode_ShouldReturnLanguageItem()
    {
        // Act
        var result = SupportedLanguages.GetByCode("en-US");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("en-US", result.Code);
        Assert.Equal("English", result.DisplayName);
    }

    [Fact]
    public void GetByCode_WithInvalidCode_ShouldReturnNull()
    {
        // Act
        var result = SupportedLanguages.GetByCode("es-ES");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("EN-US")]
    [InlineData("fr-fr")]
    [InlineData("Fr-Fr")]
    public void GetByCode_ShouldBeCaseInsensitive(string languageCode)
    {
        // Act
        var result = SupportedLanguages.GetByCode(languageCode);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void GetDefault_ShouldReturnDefaultLanguage()
    {
        // Act
        var result = SupportedLanguages.GetDefault();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SupportedLanguages.DefaultLanguage, result.Code);
        Assert.Equal("English", result.DisplayName);
    }

    [Theory]
    [InlineData("en-US", "en-US")] // Direct match
    [InlineData("fr-FR", "fr-FR")] // Direct match
    [InlineData("en-GB", "en-US")] // Language family match
    [InlineData("fr-CA", "fr-FR")] // Language family match
    [InlineData("fr-BE", "fr-FR")] // Language family match
    [InlineData("en-AU", "en-US")] // Language family match
    [InlineData("es-ES", "en-US")] // No match, return default
    [InlineData("de-DE", "en-US")] // No match, return default
    [InlineData("", "en-US")] // Empty, return default
    [InlineData(null, "en-US")] // Null, return default
    [InlineData("   ", "en-US")] // Whitespace, return default
    public void MapOSLanguageToSupported_ShouldReturnCorrectMapping(string osLanguageCode, string expectedResult)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("EN-US", "en-US")]
    [InlineData("Fr-Fr", "fr-FR")]
    [InlineData("EN-GB", "en-US")]
    [InlineData("FR-CA", "fr-FR")]
    public void MapOSLanguageToSupported_ShouldBeCaseInsensitive(string osLanguageCode, string expectedResult)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("fr-CA-quebec", "fr-FR")] // Should match language family even with multiple parts
    [InlineData("en-US-posix", "en-US")] // Should match exactly even with extra parts
    public void MapOSLanguageToSupported_WithComplexLanguageCodes_ShouldHandleCorrectly(string osLanguageCode, string expectedResult)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguageCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void MapOSLanguageToSupported_WithUnknownLanguageFamily_ShouldReturnDefault()
    {
        // Arrange
        var unknownLanguageCodes = new[] { "zh-CN", "ja-JP", "ar-SA", "hi-IN" };

        foreach (var code in unknownLanguageCodes)
        {
            // Act
            var result = SupportedLanguages.MapOSLanguageToSupported(code);

            // Assert
            Assert.Equal(SupportedLanguages.DefaultLanguage, result);
        }
    }

    [Fact]
    public void All_ShouldBeReadOnly()
    {
        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<LanguageItem>>(SupportedLanguages.All);
    }

    [Fact]
    public void LanguageItem_ShouldBeRecord()
    {
        // Arrange
        var lang1 = new LanguageItem("en-US", "English");
        var lang2 = new LanguageItem("en-US", "English");
        var lang3 = new LanguageItem("fr-FR", "Français");

        // Assert
        Assert.Equal(lang1, lang2); // Records should be equal by value
        Assert.NotEqual(lang1, lang3);
        Assert.Equal(lang1.GetHashCode(), lang2.GetHashCode());
        Assert.NotEqual(lang1.GetHashCode(), lang3.GetHashCode());
    }
}
