using Bucket.Core.Helpers;
using Bucket.Core.Models;

namespace Bucket.Core.Tests.Categories;

/// <summary>
/// Tests organized by categories and traits
/// Demonstrates XUnit test organization best practices
/// </summary>
public class CategorizedLocalizationTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "LocalizationHelper")]
    public void ValidateLanguageCode_UnitTest_BasicValidation()
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode("en-US");

        // Assert
        Assert.Equal("en-US", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Component", "SupportedLanguages")]
    public void SupportedLanguages_UnitTest_DefaultLanguage()
    {
        // Assert
        Assert.Equal("en-US", SupportedLanguages.DefaultLanguage);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Component", "LocalizationHelper")]
    [Trait("Priority", "High")]
    public void LocalizationHelper_IntegrationTest_ValidateAndRetrieve()
    {
        // Arrange
        const string testCode = "fr-FR";

        // Act
        var validatedCode = LocalizationHelper.ValidateLanguageCode(testCode);
        var languageItem = LocalizationHelper.GetLanguageItem(validatedCode);

        // Assert
        Assert.Equal(testCode, validatedCode);
        Assert.Equal(testCode, languageItem.Code);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [Trait("Category", "Unit")]
    [Trait("Component", "SupportedLanguages")]
    [Trait("TestType", "DataDriven")]
    public void SupportedLanguages_DataDrivenTest_ValidLanguageCodes(string languageCode)
    {
        // Act
        var isSupported = SupportedLanguages.IsSupported(languageCode);
        var languageItem = SupportedLanguages.GetByCode(languageCode);

        // Assert
        Assert.True(isSupported);
        Assert.NotNull(languageItem);
    }

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Component", "SupportedLanguages")]
    [Trait("Priority", "Medium")]
    public void SupportedLanguages_PerformanceTest_IsSupported()
    {
        // Arrange
        const int iterations = 10000;
        var testCodes = new[] { "en-US", "fr-FR", "de-DE", "es-ES" };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            foreach (var code in testCodes)
            {
                SupportedLanguages.IsSupported(code);
            }
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Performance test took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("Component", "LocalizationHelper")]
    [Trait("Priority", "Low")]
    public void LocalizationHelper_EdgeCase_NullInputHandling()
    {
        // Act & Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, LocalizationHelper.ValidateLanguageCode(null));
        Assert.NotNull(LocalizationHelper.GetLanguageItem(null));
        Assert.False(LocalizationHelper.ShouldChangeLanguage("en-US", null!));
    }

    [Theory]
    [InlineData("EN-US", "en-US")]
    [InlineData("fr-fr", "fr-FR")]
    [InlineData("Fr-Fr", "fr-FR")]
    [Trait("Category", "Edge")]
    [Trait("Component", "SupportedLanguages")]
    [Trait("TestType", "CaseInsensitive")]
    public void SupportedLanguages_EdgeCase_CaseInsensitiveMatching(string input, string expected)
    {
        // Act
        var result = SupportedLanguages.GetByCode(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result.Code);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    [Trait("Component", "All")]
    [Trait("Priority", "Critical")]
    public void LocalizationComponents_SmokeTest_AllBasicFunctionsWork()
    {
        // Test SupportedLanguages
        Assert.NotEmpty(SupportedLanguages.All);
        Assert.NotNull(SupportedLanguages.GetDefault());

        // Test LocalizationHelper
        var validCode = LocalizationHelper.ValidateLanguageCode("en-US");
        Assert.Equal("en-US", validCode);

        var langItem = LocalizationHelper.GetLanguageItem("fr-FR");
        Assert.NotNull(langItem);

        var shouldChange = LocalizationHelper.ShouldChangeLanguage("en-US", "fr-FR");
        Assert.True(shouldChange);

        // Test LanguageItem
        var item = new LanguageItem("test", "Test");
        Assert.Equal("test", item.Code);
        Assert.Equal("Test", item.DisplayName);
    }

    /// <summary>
    /// Test specifically marked for CI/CD environments
    /// </summary>
    [Fact]
    [Trait("Category", "CI")]
    [Trait("Environment", "Build")]
    public void LocalizationComponents_CITest_QuickValidation()
    {
        // Quick validation suitable for CI pipelines
        Assert.True(SupportedLanguages.IsSupported("en-US"));
        Assert.True(SupportedLanguages.IsSupported("fr-FR"));
        Assert.False(SupportedLanguages.IsSupported("de-DE"));
    }

    /// <summary>
    /// Test that should be skipped in certain conditions
    /// </summary>
    [Fact(Skip = "Long running test - run manually when needed")]
    [Trait("Category", "Manual")]
    [Trait("Component", "Performance")]
    public void LocalizationComponents_ManualTest_ExtensivePerformance()
    {
        // This test would take a long time and is marked to be skipped
        // It demonstrates the Skip parameter usage
        const int iterations = 1000000;

        for (int i = 0; i < iterations; i++)
        {
            LocalizationHelper.ValidateLanguageCode("en-US");
        }

        Assert.True(true); // Would only execute if Skip is removed
    }
}
