using Bucket.Core.Models;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Bucket.Core.Tests.Attributes;

/// <summary>
/// Custom data attribute for localization test data
/// Demonstrates creating custom XUnit data attributes
/// </summary>
public class SupportedLanguageDataAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return SupportedLanguages.All.Select(lang => new object[] { lang.Code, lang.DisplayName });
    }
}

/// <summary>
/// Custom data attribute for unsupported language test data
/// </summary>
public class UnsupportedLanguageDataAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var unsupportedLanguages = new[]
        {
            new { Code = "de-DE", DisplayName = "Deutsch" },
            new { Code = "es-ES", DisplayName = "Español" },
            new { Code = "it-IT", DisplayName = "Italiano" },
            new { Code = "pt-BR", DisplayName = "Português (Brasil)" },
            new { Code = "zh-CN", DisplayName = "中文 (简体)" }
        };

        return unsupportedLanguages.Select(lang => new object[] { lang.Code, lang.DisplayName });
    }
}

/// <summary>
/// Tests demonstrating custom attributes and test output
/// </summary>
public class CustomAttributeLocalizationTests
{
    private readonly ITestOutputHelper _output;

    public CustomAttributeLocalizationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [SupportedLanguageData]
    public void SupportedLanguages_WithCustomAttribute_AllLanguagesAreSupported(string languageCode, string displayName)
    {
        // Log test execution details
        _output.WriteLine($"Testing supported language: {languageCode} ({displayName})");

        // Act
        var isSupported = SupportedLanguages.IsSupported(languageCode);
        var retrievedLanguage = SupportedLanguages.GetByCode(languageCode);

        // Assert
        Assert.True(isSupported, $"Language {languageCode} should be supported");
        Assert.NotNull(retrievedLanguage);
        Assert.Equal(languageCode, retrievedLanguage.Code);
        Assert.Equal(displayName, retrievedLanguage.DisplayName);

        _output.WriteLine($"✓ Language {languageCode} validation passed");
    }

    [Theory]
    [UnsupportedLanguageData]
    public void SupportedLanguages_WithUnsupportedLanguages_ReturnsExpectedBehavior(string languageCode, string displayName)
    {
        // Log test execution details
        _output.WriteLine($"Testing unsupported language: {languageCode} ({displayName})");

        // Act
        var isSupported = SupportedLanguages.IsSupported(languageCode);
        var retrievedLanguage = SupportedLanguages.GetByCode(languageCode);
        var mappedLanguage = SupportedLanguages.MapOSLanguageToSupported(languageCode);

        // Assert
        Assert.False(isSupported, $"Language {languageCode} should not be supported");
        Assert.Null(retrievedLanguage);
        Assert.Equal(SupportedLanguages.DefaultLanguage, mappedLanguage);

        _output.WriteLine($"✓ Language {languageCode} correctly identified as unsupported");
    }

    [Fact]
    public void SupportedLanguages_LogAllSupportedLanguages_ForDocumentation()
    {
        // This test serves as documentation and validation
        _output.WriteLine("Currently supported languages:");
        _output.WriteLine("============================");

        foreach (var language in SupportedLanguages.All)
        {
            _output.WriteLine($"Code: {language.Code} | Display Name: {language.DisplayName}");

            // Validate each language
            Assert.True(SupportedLanguages.IsSupported(language.Code));
            Assert.NotNull(SupportedLanguages.GetByCode(language.Code));
        }

        _output.WriteLine($"\nTotal supported languages: {SupportedLanguages.All.Count}");
        _output.WriteLine($"Default language: {SupportedLanguages.DefaultLanguage}");
    }

    [Theory]
    [InlineData("en-GB", "en-US")] // British English -> American English
    [InlineData("en-CA", "en-US")] // Canadian English -> American English
    [InlineData("fr-CA", "fr-FR")] // Canadian French -> France French
    [InlineData("fr-BE", "fr-FR")] // Belgian French -> France French
    [InlineData("de-AT", "en-US")] // Austrian German -> Default (English)
    [InlineData("es-MX", "en-US")] // Mexican Spanish -> Default (English)
    public void SupportedLanguages_MapOSLanguage_WithRegionalVariants_MapsCorrectly(
        string osLanguage,
        string expectedMapping)
    {
        // Log the mapping being tested
        _output.WriteLine($"Testing OS language mapping: {osLanguage} -> {expectedMapping}");

        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(osLanguage);

        // Assert
        Assert.Equal(expectedMapping, result);
        Assert.True(SupportedLanguages.IsSupported(result),
            "Mapped language must be supported");

        _output.WriteLine($"✓ Mapping successful: {osLanguage} -> {result}");
    }

    [Fact]
    public void LanguageItem_CreateAndValidate_AllProperties()
    {
        // Arrange
        const string testCode = "test-TEST";
        const string testDisplayName = "Test Language";

        _output.WriteLine($"Creating LanguageItem with code '{testCode}' and display name '{testDisplayName}'");

        // Act
        var languageItem = new LanguageItem(testCode, testDisplayName);

        // Assert
        Assert.Equal(testCode, languageItem.Code);
        Assert.Equal(testDisplayName, languageItem.DisplayName);

        // Test string representation
        var stringRepresentation = languageItem.ToString();
        _output.WriteLine($"String representation: {stringRepresentation}");

        Assert.Contains(testCode, stringRepresentation);
        Assert.Contains(testDisplayName, stringRepresentation);

        // Test deconstruction
        var (code, displayName) = languageItem;
        Assert.Equal(testCode, code);
        Assert.Equal(testDisplayName, displayName);

        _output.WriteLine("✓ LanguageItem validation completed successfully");
    }

    /// <summary>
    /// Test that demonstrates conditional test execution based on environment
    /// </summary>
    [Fact]
    public void ConditionalTest_BasedOnEnvironment()
    {
        var isRunningInCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        _output.WriteLine($"Running in CI environment: {isRunningInCI}");
        _output.WriteLine($"Current culture: {System.Globalization.CultureInfo.CurrentCulture.Name}");
        _output.WriteLine($"Current UI culture: {System.Globalization.CultureInfo.CurrentUICulture.Name}");

        // Basic validation that works in any environment
        Assert.True(SupportedLanguages.All.Count > 0);

        if (!isRunningInCI)
        {
            // Additional tests that might only work in development environment
            _output.WriteLine("Running additional development-only tests");

            // These tests might require specific system configuration
            Assert.True(SupportedLanguages.All.Count >= 2, "Should have at least 2 supported languages");
        }
    }
}
