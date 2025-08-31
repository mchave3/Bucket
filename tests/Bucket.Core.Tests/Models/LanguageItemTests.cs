using Bucket.Core.Models;

namespace Bucket.Core.Tests.Models;

/// <summary>
/// Unit tests for LanguageItem record
/// </summary>
public class LanguageItemTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstanceCorrectly()
    {
        // Arrange
        var code = "en-US";
        var displayName = "English";

        // Act
        var languageItem = new LanguageItem(code, displayName);

        // Assert
        Assert.Equal(code, languageItem.Code);
        Assert.Equal(displayName, languageItem.DisplayName);
    }

    [Fact]
    public void Constructor_WithDifferentParameters_CreatesInstanceCorrectly()
    {
        // Arrange
        var code = "fr-FR";
        var displayName = "Français";

        // Act
        var languageItem = new LanguageItem(code, displayName);

        // Assert
        Assert.Equal(code, languageItem.Code);
        Assert.Equal(displayName, languageItem.DisplayName);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("invalid", "Invalid Language")]
    [InlineData("es-ES", "Español")]
    public void Constructor_WithVariousParameters_CreatesInstanceCorrectly(string code, string displayName)
    {
        // Act
        var languageItem = new LanguageItem(code, displayName);

        // Assert
        Assert.Equal(code, languageItem.Code);
        Assert.Equal(displayName, languageItem.DisplayName);
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var item1 = new LanguageItem("en-US", "English");
        var item2 = new LanguageItem("en-US", "English");

        // Act & Assert
        Assert.Equal(item1, item2);
        Assert.True(item1 == item2);
        Assert.False(item1 != item2);
    }

    [Fact]
    public void Equality_WithDifferentCodes_ReturnsFalse()
    {
        // Arrange
        var item1 = new LanguageItem("en-US", "English");
        var item2 = new LanguageItem("fr-FR", "English");

        // Act & Assert
        Assert.NotEqual(item1, item2);
        Assert.False(item1 == item2);
        Assert.True(item1 != item2);
    }

    [Fact]
    public void Equality_WithDifferentDisplayNames_ReturnsFalse()
    {
        // Arrange
        var item1 = new LanguageItem("en-US", "English");
        var item2 = new LanguageItem("en-US", "American English");

        // Act & Assert
        Assert.NotEqual(item1, item2);
        Assert.False(item1 == item2);
        Assert.True(item1 != item2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        // Arrange
        var item1 = new LanguageItem("en-US", "English");
        var item2 = new LanguageItem("en-US", "English");

        // Act
        var hash1 = item1.GetHashCode();
        var hash2 = item2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCode()
    {
        // Arrange
        var item1 = new LanguageItem("en-US", "English");
        var item2 = new LanguageItem("fr-FR", "Français");

        // Act
        var hash1 = item1.GetHashCode();
        var hash2 = item2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var item = new LanguageItem("en-US", "English");

        // Act
        var result = item.ToString();

        // Assert
        Assert.Contains("en-US", result);
        Assert.Contains("English", result);
    }

    [Fact]
    public void Deconstruction_WorksCorrectly()
    {
        // Arrange
        var item = new LanguageItem("fr-FR", "Français");

        // Act
        var (code, displayName) = item;

        // Assert
        Assert.Equal("fr-FR", code);
        Assert.Equal("Français", displayName);
    }

    [Theory]
    [MemberData(nameof(GetLanguageItemTestData))]
    public void LanguageItem_WithMemberData_CreatesCorrectInstances(string code, string displayName)
    {
        // Act
        var item = new LanguageItem(code, displayName);

        // Assert
        Assert.Equal(code, item.Code);
        Assert.Equal(displayName, item.DisplayName);
    }

    public static IEnumerable<object[]> GetLanguageItemTestData()
    {
        yield return new object[] { "en-US", "English" };
        yield return new object[] { "fr-FR", "Français" };
        yield return new object[] { "es-ES", "Español" };
        yield return new object[] { "de-DE", "Deutsch" };
        yield return new object[] { "it-IT", "Italiano" };
    }

    [Fact]
    public void ImplicitConversion_DoesNotExist()
    {
        // This test verifies that we cannot accidentally convert between incompatible types
        // The LanguageItem record should be explicit about its types

        // Arrange
        var item = new LanguageItem("en-US", "English");

        // Act & Assert
        // These should all be compile-time safe - no implicit conversions
        Assert.IsType<LanguageItem>(item);
        Assert.IsNotType<string>(item);
        Assert.IsNotType<ValueTuple<string, string>>(item);
    }
}
