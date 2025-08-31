using Bucket.Core.Models;

namespace Bucket.Core.Tests.Models;

/// <summary>
/// Unit tests for LanguageItem record
/// </summary>
public class LanguageItemTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesLanguageItem()
    {
        // Arrange
        const string code = "en-US";
        const string displayName = "English";

        // Act
        var languageItem = new LanguageItem(code, displayName);

        // Assert
        Assert.Equal(code, languageItem.Code);
        Assert.Equal(displayName, languageItem.DisplayName);
    }

    [Fact]
    public void Equality_WithSameCodeAndDisplayName_AreEqual()
    {
        // Arrange
        var languageItem1 = new LanguageItem("en-US", "English");
        var languageItem2 = new LanguageItem("en-US", "English");

        // Act & Assert
        Assert.Equal(languageItem1, languageItem2);
        Assert.True(languageItem1 == languageItem2);
        Assert.False(languageItem1 != languageItem2);
    }

    [Fact]
    public void Equality_WithDifferentCode_AreNotEqual()
    {
        // Arrange
        var languageItem1 = new LanguageItem("en-US", "English");
        var languageItem2 = new LanguageItem("fr-FR", "English");

        // Act & Assert
        Assert.NotEqual(languageItem1, languageItem2);
        Assert.False(languageItem1 == languageItem2);
        Assert.True(languageItem1 != languageItem2);
    }

    [Fact]
    public void Equality_WithDifferentDisplayName_AreNotEqual()
    {
        // Arrange
        var languageItem1 = new LanguageItem("en-US", "English");
        var languageItem2 = new LanguageItem("en-US", "American English");

        // Act & Assert
        Assert.NotEqual(languageItem1, languageItem2);
        Assert.False(languageItem1 == languageItem2);
        Assert.True(languageItem1 != languageItem2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        // Arrange
        var languageItem1 = new LanguageItem("en-US", "English");
        var languageItem2 = new LanguageItem("en-US", "English");

        // Act
        var hashCode1 = languageItem1.GetHashCode();
        var hashCode2 = languageItem2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCodes()
    {
        // Arrange
        var languageItem1 = new LanguageItem("en-US", "English");
        var languageItem2 = new LanguageItem("fr-FR", "Français");

        // Act
        var hashCode1 = languageItem1.GetHashCode();
        var hashCode2 = languageItem2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void ToString_ReturnsExpectedStringRepresentation()
    {
        // Arrange
        var languageItem = new LanguageItem("en-US", "English");

        // Act
        var result = languageItem.ToString();

        // Assert
        Assert.Contains("en-US", result);
        Assert.Contains("English", result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("en-US", "")]
    [InlineData("", "English")]
    public void Constructor_WithEmptyStrings_CreatesLanguageItemWithEmptyValues(string code, string displayName)
    {
        // Act
        var languageItem = new LanguageItem(code, displayName);

        // Assert
        Assert.Equal(code, languageItem.Code);
        Assert.Equal(displayName, languageItem.DisplayName);
    }

    [Fact]
    public void Deconstruction_WithValidLanguageItem_ReturnsCodeAndDisplayName()
    {
        // Arrange
        var languageItem = new LanguageItem("fr-FR", "Français");

        // Act
        var (code, displayName) = languageItem;

        // Assert
        Assert.Equal("fr-FR", code);
        Assert.Equal("Français", displayName);
    }
}
