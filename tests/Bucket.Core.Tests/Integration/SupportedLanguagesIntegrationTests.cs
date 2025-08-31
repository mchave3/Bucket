using Bucket.Core.Models;
using Bucket.Core.Tests.Fixtures;

namespace Bucket.Core.Tests.Integration;

/// <summary>
/// Integration tests for SupportedLanguages using shared test fixture
/// Demonstrates XUnit collection fixture usage
/// </summary>
[Collection("Localization Collection")]
public class SupportedLanguagesIntegrationTests
{
    private readonly LocalizationTestFixture _fixture;

    public SupportedLanguagesIntegrationTests(LocalizationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SupportedLanguages_ContainsExpectedLanguagesFromFixture()
    {
        // Arrange
        var supportedLanguageCodes = SupportedLanguages.All.Select(l => l.Code).ToList();

        // Act & Assert
        foreach (var testLanguage in _fixture.TestLanguages.Take(2)) // Only first 2 are actually supported
        {
            Assert.Contains(testLanguage.Code, supportedLanguageCodes);
        }
    }

    [Fact]
    public void SupportedLanguages_DefaultLanguageMatchesFixture()
    {
        // Assert
        Assert.Equal(_fixture.DefaultLanguageCode, SupportedLanguages.DefaultLanguage);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void SupportedLanguages_MapOSLanguage_WithSupportedLanguages_ReturnsCorrectMapping(string languageCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(languageCode);

        // Assert
        Assert.Equal(languageCode, result);
        Assert.True(SupportedLanguages.IsSupported(result));
    }

    [Fact]
    public void SupportedLanguages_MapOSLanguage_WithUnsupportedLanguages_ReturnsDefault()
    {
        // Arrange
        var unsupportedLanguages = _fixture.TestLanguages.Skip(2).Select(l => l.Code); // de-DE, es-ES

        // Act & Assert
        foreach (var unsupportedCode in unsupportedLanguages)
        {
            var result = SupportedLanguages.MapOSLanguageToSupported(unsupportedCode);
            Assert.Equal(_fixture.DefaultLanguageCode, result);
        }
    }

    [Fact]
    public void SupportedLanguages_GetByCode_WithFixtureData_ReturnsExpectedResults()
    {
        // Arrange
        var supportedCodes = _fixture.TestLanguages.Take(2).Select(l => l.Code);

        // Act & Assert
        foreach (var code in supportedCodes)
        {
            var result = SupportedLanguages.GetByCode(code);

            Assert.NotNull(result);
            Assert.Equal(code, result.Code);

            var fixtureLanguage = _fixture.TestLanguages.First(l => l.Code == code);
            Assert.Equal(fixtureLanguage.DisplayName, result.DisplayName);
        }
    }

    /// <summary>
    /// Test that demonstrates fixture data can be modified during test execution
    /// </summary>
    [Fact]
    public void LocalizationTestFixture_TestResourceKeys_ContainExpectedEntries()
    {
        // Act
        var resourceKeys = _fixture.TestResourceKeys;

        // Assert
        Assert.NotEmpty(resourceKeys);
        Assert.Contains("Welcome", resourceKeys.Keys);
        Assert.Contains("Settings", resourceKeys.Keys);
        Assert.Contains("Language", resourceKeys.Keys);

        // Verify values
        Assert.Equal("Welcome", resourceKeys["Welcome"]);
        Assert.Equal("Settings", resourceKeys["Settings"]);
    }

    /// <summary>
    /// Performance test using fixture data
    /// </summary>
    [Fact]
    public void SupportedLanguages_BulkOperations_WithFixtureData_PerformWell()
    {
        // Arrange
        const int iterations = 1000;
        var testCodes = _fixture.TestLanguages.Select(l => l.Code).ToArray();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            foreach (var code in testCodes)
            {
                SupportedLanguages.IsSupported(code);
                SupportedLanguages.GetByCode(code);
                SupportedLanguages.MapOSLanguageToSupported(code);
            }
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Bulk operations took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }
}
