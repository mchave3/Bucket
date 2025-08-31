using System.Globalization;
using Bucket.Core.Helpers;
using Bucket.Core.Models;

namespace Bucket.Core.Tests.Helpers;

/// <summary>
/// Advanced unit tests for LocalizationHelper demonstrating XUnit best practices
/// </summary>
public class LocalizationHelperAdvancedTests
{
    /// <summary>
    /// Custom test data class for language validation scenarios
    /// </summary>
    public class LanguageValidationTestData : TheoryData<string?, string, string>
    {
        public LanguageValidationTestData()
        {
            // Format: input, expected output, description
            Add(null, SupportedLanguages.DefaultLanguage, "Null input");
            Add("", SupportedLanguages.DefaultLanguage, "Empty input");
            Add("   ", SupportedLanguages.DefaultLanguage, "Whitespace input");
            Add("en-US", "en-US", "Valid English US");
            Add("fr-FR", "fr-FR", "Valid French France");
            Add("EN-US", "en-US", "Case insensitive English US");
            Add("Fr-Fr", "fr-FR", "Mixed case French France");
            Add("de-DE", SupportedLanguages.DefaultLanguage, "Unsupported German");
            Add("es-ES", SupportedLanguages.DefaultLanguage, "Unsupported Spanish");
            Add("invalid", SupportedLanguages.DefaultLanguage, "Invalid format");
        }
    }

    [Theory]
    [ClassData(typeof(LanguageValidationTestData))]
    public void ValidateLanguageCode_WithCustomTestData_ReturnsExpectedResult(
        string? input,
        string expectedOutput,
        string description)
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode(input);

        // Assert
        Assert.Equal(expectedOutput, result);
    }

    /// <summary>
    /// Test data generator method for complex scenarios
    /// </summary>
    public static IEnumerable<object[]> GetLanguageChangeScenarios()
    {
        yield return new object[] { "en-US", "fr-FR", true, "Different languages" };
        yield return new object[] { "fr-FR", "en-US", true, "Reverse different languages" };
        yield return new object[] { "en-US", "en-US", false, "Same language" };
        yield return new object[] { "fr-FR", "fr-FR", false, "Same French language" };
        yield return new object[] { "en-US", "EN-US", false, "Case insensitive same" };
        yield return new object[] { "fr-fr", "FR-FR", false, "Case insensitive French" };
        yield return new object[] { "en-US", "", false, "Empty new language" };
        yield return new object[] { "fr-FR", "   ", false, "Whitespace new language" };
    }

    [Theory]
    [MemberData(nameof(GetLanguageChangeScenarios))]
    public void ShouldChangeLanguage_WithComplexScenarios_ReturnsExpectedResult(
        string currentLanguage,
        string newLanguage,
        bool expectedResult,
        string scenario)
    {
        // Act
        var result = LocalizationHelper.ShouldChangeLanguage(currentLanguage, newLanguage);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetLanguageItem_WithSupportedLanguages_ReturnsCorrectItems()
    {
        // Arrange
        var supportedLanguages = new[] { "en-US", "fr-FR" };

        // Act & Assert
        foreach (var languageCode in supportedLanguages)
        {
            var result = LocalizationHelper.GetLanguageItem(languageCode);

            Assert.NotNull(result);
            Assert.Equal(languageCode, result.Code);
            Assert.NotEmpty(result.DisplayName);
        }
    }

    [Theory]
    [InlineData("en-US", "English")]
    [InlineData("fr-FR", "Français")]
    public void GetLanguageItem_WithSpecificLanguages_ReturnsCorrectDisplayNames(
        string languageCode,
        string expectedDisplayName)
    {
        // Act
        var result = LocalizationHelper.GetLanguageItem(languageCode);

        // Assert
        Assert.Equal(expectedDisplayName, result.DisplayName);
    }

    /// <summary>
    /// Performance test to ensure methods execute quickly
    /// </summary>
    [Fact]
    public void ValidateLanguageCode_PerformanceTest_ExecutesQuickly()
    {
        // Arrange
        const int iterations = 10000;
        var testCodes = new[] { "en-US", "fr-FR", "de-DE", null, "", "invalid" };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            foreach (var code in testCodes)
            {
                LocalizationHelper.ValidateLanguageCode(code);
            }
        }

        stopwatch.Stop();

        // Assert - Should complete in reasonable time (under 1 second for 60k calls)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Performance test took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    /// <summary>
    /// Edge case testing with unusual but valid inputs
    /// </summary>
    [Theory]
    [InlineData("en-US-x-private", SupportedLanguages.DefaultLanguage)] // Extended language tag
    [InlineData("en", SupportedLanguages.DefaultLanguage)] // Language without region
    [InlineData("EN", SupportedLanguages.DefaultLanguage)] // Uppercase language without region
    [InlineData("en-us", SupportedLanguages.DefaultLanguage)] // Lowercase region
    public void ValidateLanguageCode_WithEdgeCases_HandlesGracefully(
        string edgeCase,
        string expectedResult)
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode(edgeCase);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Test using current system culture as a reference
    /// </summary>
    [Fact]
    public void ValidateLanguageCode_WithSystemCulture_ReturnsAppropriateResult()
    {
        // Arrange
        var systemCulture = CultureInfo.CurrentUICulture.Name;

        // Act
        var result = LocalizationHelper.ValidateLanguageCode(systemCulture);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Should either return the system culture if supported, or default
        var isSystemCultureSupported = SupportedLanguages.IsSupported(systemCulture);
        if (isSystemCultureSupported)
        {
            Assert.Equal(systemCulture, result);
        }
        else
        {
            Assert.Equal(SupportedLanguages.DefaultLanguage, result);
        }
    }

    /// <summary>
    /// Integration test combining multiple helper methods
    /// </summary>
    [Fact]
    public void LocalizationHelper_IntegrationTest_WorksCorrectly()
    {
        // Arrange
        const string testLanguage = "fr-FR";

        // Act
        var validatedLanguage = LocalizationHelper.ValidateLanguageCode(testLanguage);
        var languageItem = LocalizationHelper.GetLanguageItem(validatedLanguage);
        var shouldChange = LocalizationHelper.ShouldChangeLanguage("en-US", validatedLanguage);

        // Assert
        Assert.Equal(testLanguage, validatedLanguage);
        Assert.Equal(testLanguage, languageItem.Code);
        Assert.Equal("Français", languageItem.DisplayName);
        Assert.True(shouldChange);
    }
}
