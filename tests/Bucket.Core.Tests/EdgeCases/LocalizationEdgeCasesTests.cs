using System.Collections.Concurrent;
using Bucket.Core.Helpers;
using Bucket.Core.Models;

namespace Bucket.Core.Tests.EdgeCases;

/// <summary>
/// Tests for edge cases and boundary conditions in localization functionality
/// </summary>
public class LocalizationEdgeCasesTests
{
    private static readonly string[] s_testCodes = new[] { "en-US", "fr-FR", "invalid", "", "EN-US", "fr-fr" };
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void LocalizationHelper_ValidateLanguageCode_WithWhitespaceVariations_ReturnsDefault(string? languageCode)
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode(languageCode);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData("EN-US")]
    [InlineData("en-us")]
    [InlineData("En-Us")]
    [InlineData("eN-uS")]
    [InlineData("FR-FR")]
    [InlineData("fr-fr")]
    [InlineData("Fr-Fr")]
    [InlineData("fR-fR")]
    public void LocalizationHelper_ValidateLanguageCode_WithCaseVariations_ReturnsCorrectCode(string languageCode)
    {
        // Act
        var result = LocalizationHelper.ValidateLanguageCode(languageCode);

        // Assert
        var normalizedInput = languageCode.ToUpperInvariant();
        var expected = normalizedInput switch
        {
            "EN-US" => "en-US",
            "FR-FR" => "fr-FR",
            _ => SupportedLanguages.DefaultLanguage
        };
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("en-US ")]  // Trailing space
    [InlineData(" en-US")]  // Leading space
    [InlineData(" en-US ")] // Both
    [InlineData("fr-FR\t")]  // Trailing tab
    [InlineData("\tfr-FR")]  // Leading tab
    public void SupportedLanguages_GetByCode_WithWhitespaceInCode_ReturnsNull(string languageCode)
    {
        // Act
        var result = SupportedLanguages.GetByCode(languageCode);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    [InlineData("EN")]
    [InlineData("FR")]
    public void SupportedLanguages_MapOSLanguageToSupported_WithLanguageFamilyOnly_MapsCorrectly(string languageFamily)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(languageFamily);

        // Assert
        var expected = languageFamily.ToUpperInvariant() switch
        {
            "EN" => "en-US",
            "FR" => "fr-FR",
            _ => SupportedLanguages.DefaultLanguage
        };
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("en-US-POSIX")]
    [InlineData("fr-FR-u-ca-gregory")]
    [InlineData("en-Latn-US")]
    [InlineData("fr-Latn-FR")]
    [InlineData("en-US-x-private")]
    public void SupportedLanguages_MapOSLanguageToSupported_WithComplexLanguageCodes_MapsToBasicSupported(string complexLanguageCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(complexLanguageCode);

        // Assert
        var languageFamily = complexLanguageCode.Split('-')[0].ToUpperInvariant();
        var expected = languageFamily switch
        {
            "EN" => "en-US",
            "FR" => "fr-FR",
            _ => SupportedLanguages.DefaultLanguage
        };
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("x-x")]
    [InlineData("1-2")]
    [InlineData("@@@@")]
    [InlineData("en_US")]  // Underscore instead of hyphen
    [InlineData("fr_FR")]  // Underscore instead of hyphen
    public void SupportedLanguages_MapOSLanguageToSupported_WithInvalidFormats_ReturnsDefault(string invalidLanguageCode)
    {
        // Act
        var result = SupportedLanguages.MapOSLanguageToSupported(invalidLanguageCode);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void LocalizationHelper_ShouldChangeLanguage_WithEmptyCurrentLanguage_HandlesSafely(string? currentLanguage)
    {
        // Act
        var result1 = LocalizationHelper.ShouldChangeLanguage(currentLanguage, "fr-FR");
        var result2 = LocalizationHelper.ShouldChangeLanguage(currentLanguage, "");
        var result3 = LocalizationHelper.ShouldChangeLanguage(currentLanguage, null);

        // Assert
        Assert.True(result1);  // Should change to valid language
        Assert.False(result2); // Should not change to empty
        Assert.False(result3); // Should not change to null
    }

    [Fact]
    public void SupportedLanguages_All_IsImmutable()
    {
        // Arrange
        var originalCount = SupportedLanguages.All.Count;
        var originalLanguages = SupportedLanguages.All.ToList();

        // Act & Assert - Verify we cannot modify the collection
        Assert.Throws<NotSupportedException>(() =>
        {
            var mutableList = (IList<LanguageItem>)SupportedLanguages.All;
            mutableList.Add(new LanguageItem("test", "test"));
        });

        // Verify the collection is still unchanged
        Assert.Equal(originalCount, SupportedLanguages.All.Count);
        Assert.Equal(originalLanguages, SupportedLanguages.All);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void LanguageItem_WithWhitespaceValues_HandlesCorrectly(string whitespaceValue)
    {
        // Act
        var item = new LanguageItem(whitespaceValue, whitespaceValue);

        // Assert
        Assert.Equal(whitespaceValue, item.Code);
        Assert.Equal(whitespaceValue, item.DisplayName);
    }

    [Fact]
    public void LanguageItem_Equality_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var item1 = new LanguageItem(null!, null!);
        var item2 = new LanguageItem(null!, null!);
        var item3 = new LanguageItem("en-US", null!);
        var item4 = new LanguageItem(null!, "English");

        // Act & Assert
        Assert.Equal(item1, item2);
        Assert.NotEqual(item1, item3);
        Assert.NotEqual(item1, item4);
        Assert.NotEqual(item3, item4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void LocalizationHelper_PerformanceTest_WithRepeatedCalls_PerformsWell(int iterations)
    {
        // Arrange
        var languageCodes = new[] { "en-US", "fr-FR", "invalid", "", null };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            foreach (var code in languageCodes)
            {
                LocalizationHelper.ValidateLanguageCode(code);
                LocalizationHelper.GetLanguageItem(code);
                LocalizationHelper.ShouldChangeLanguage("en-US", code);
            }
        }

        stopwatch.Stop();

        // Assert
        // Performance should be reasonable - this is more of a smoke test
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Performance test took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task SupportedLanguages_ThreadSafety_ConcurrentAccess_IsThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var results = new ConcurrentBag<LanguageItem?>();
        var random = new Random();

        // Act - Multiple threads accessing SupportedLanguages concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var testCode = s_testCodes[j % s_testCodes.Length];

                    var result1 = SupportedLanguages.GetByCode(testCode);
                    var result2 = SupportedLanguages.IsSupported(testCode);
                    var result3 = SupportedLanguages.MapOSLanguageToSupported(testCode);
                    var result4 = SupportedLanguages.GetDefault();

                    results.Add(result1);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - No exceptions should be thrown and operations should complete
        Assert.False(results.IsEmpty);
        Assert.DoesNotContain(results, r => r?.Code == null && r?.DisplayName != null);
    }

    [Theory]
    [InlineData("zh-CN")]   // Chinese
    [InlineData("ja-JP")]   // Japanese
    [InlineData("ko-KR")]   // Korean
    [InlineData("ar-SA")]   // Arabic
    [InlineData("hi-IN")]   // Hindi
    [InlineData("ru-RU")]   // Russian
    [InlineData("pt-BR")]   // Portuguese Brazil
    [InlineData("es-MX")]   // Spanish Mexico
    public void SupportedLanguages_WithVariousUnsupportedLanguages_FallsBackToDefault(string unsupportedLanguage)
    {
        // Act
        var mapped = SupportedLanguages.MapOSLanguageToSupported(unsupportedLanguage);
        var isSupported = SupportedLanguages.IsSupported(unsupportedLanguage);
        var getByCode = SupportedLanguages.GetByCode(unsupportedLanguage);
        var validated = LocalizationHelper.ValidateLanguageCode(unsupportedLanguage);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, mapped);
        Assert.False(isSupported);
        Assert.Null(getByCode);
        Assert.Equal(SupportedLanguages.DefaultLanguage, validated);
    }

    [Fact]
    public void LocalizationHelper_GetLanguageItem_AlwaysReturnsNonNull()
    {
        // Arrange
        var testInputs = new string?[]
        {
            null, "", "   ", "invalid", "123", "en-US", "fr-FR", "es-ES", "EN-US", "fr-fr"
        };

        // Act & Assert
        foreach (var input in testInputs)
        {
            var result = LocalizationHelper.GetLanguageItem(input);
            Assert.NotNull(result);
            Assert.NotNull(result.Code);
            Assert.NotNull(result.DisplayName);
            Assert.True(SupportedLanguages.IsSupported(result.Code));
        }
    }
}
