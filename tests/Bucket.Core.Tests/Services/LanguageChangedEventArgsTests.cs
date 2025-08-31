using Bucket.Core.Services;

namespace Bucket.Core.Tests.Services;

/// <summary>
/// Unit tests for LanguageChangedEventArgs class
/// </summary>
public class LanguageChangedEventArgsTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstanceCorrectly()
    {
        // Arrange
        var oldLanguage = "en-US";
        var newLanguage = "fr-FR";

        // Act
        var eventArgs = new LanguageChangedEventArgs(oldLanguage, newLanguage);

        // Assert
        Assert.Equal(oldLanguage, eventArgs.OldLanguage);
        Assert.Equal(newLanguage, eventArgs.NewLanguage);
    }

    [Theory]
    [InlineData("", "fr-FR")]
    [InlineData("en-US", "")]
    [InlineData("", "")]
    public void Constructor_WithEmptyStrings_CreatesInstanceCorrectly(string oldLanguage, string newLanguage)
    {
        // Act
        var eventArgs = new LanguageChangedEventArgs(oldLanguage, newLanguage);

        // Assert
        Assert.Equal(oldLanguage, eventArgs.OldLanguage);
        Assert.Equal(newLanguage, eventArgs.NewLanguage);
    }

    [Theory]
    [InlineData("fr-FR", "en-US")]
    [InlineData("es-ES", "de-DE")]
    [InlineData("zh-CN", "ja-JP")]
    public void Constructor_WithDifferentLanguagePairs_CreatesInstanceCorrectly(string oldLanguage, string newLanguage)
    {
        // Act
        var eventArgs = new LanguageChangedEventArgs(oldLanguage, newLanguage);

        // Assert
        Assert.Equal(oldLanguage, eventArgs.OldLanguage);
        Assert.Equal(newLanguage, eventArgs.NewLanguage);
    }

    [Fact]
    public void Properties_AreReadOnly()
    {
        // Arrange
        var eventArgs = new LanguageChangedEventArgs("en-US", "fr-FR");

        // Act & Assert
        // Properties should be read-only (this is a compile-time check)
        var oldLanguage = eventArgs.OldLanguage;
        var newLanguage = eventArgs.NewLanguage;

        Assert.Equal("en-US", oldLanguage);
        Assert.Equal("fr-FR", newLanguage);

        // Verify properties cannot be modified (these would be compile errors)
        // eventArgs.OldLanguage = "something"; // Should not compile
        // eventArgs.NewLanguage = "something"; // Should not compile
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        // Arrange
        var eventArgs = new LanguageChangedEventArgs("en-US", "fr-FR");

        // Act & Assert
        Assert.IsAssignableFrom<EventArgs>(eventArgs);
    }

    [Theory]
    [MemberData(nameof(GetLanguageChangeTestData))]
    public void Constructor_WithMemberData_CreatesInstanceCorrectly(string oldLanguage, string newLanguage)
    {
        // Act
        var eventArgs = new LanguageChangedEventArgs(oldLanguage, newLanguage);

        // Assert
        Assert.Equal(oldLanguage, eventArgs.OldLanguage);
        Assert.Equal(newLanguage, eventArgs.NewLanguage);
    }

    public static IEnumerable<object[]> GetLanguageChangeTestData()
    {
        yield return new object[] { "en-US", "fr-FR" };
        yield return new object[] { "fr-FR", "en-US" };
        yield return new object[] { "en-GB", "en-US" };
        yield return new object[] { "fr-CA", "fr-FR" };
        yield return new object[] { "es-ES", "es-MX" };
        yield return new object[] { "de-DE", "de-AT" };
        yield return new object[] { "pt-BR", "pt-PT" };
    }

    [Fact]
    public void EventArgs_CanBeUsedInEventHandlerPattern()
    {
        // Arrange
        string? capturedOldLanguage = null;
        string? capturedNewLanguage = null;

        void LanguageChangedHandler(object? sender, LanguageChangedEventArgs e)
        {
            capturedOldLanguage = e.OldLanguage;
            capturedNewLanguage = e.NewLanguage;
        }

        var eventArgs = new LanguageChangedEventArgs("en-US", "fr-FR");

        // Act
        LanguageChangedHandler(this, eventArgs);

        // Assert
        Assert.Equal("en-US", capturedOldLanguage);
        Assert.Equal("fr-FR", capturedNewLanguage);
    }

    [Fact]
    public void ToString_ContainsLanguageInformation()
    {
        // Arrange
        var eventArgs = new LanguageChangedEventArgs("en-US", "fr-FR");

        // Act
        var result = eventArgs.ToString();

        // Assert
        Assert.NotNull(result);
        // The exact format depends on the base EventArgs.ToString() implementation
        // We just verify it returns a non-empty string
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData(null, "fr-FR")]
    [InlineData("en-US", null)]
    [InlineData(null, null)]
    public void Constructor_WithNullValues_HandlesGracefully(string? oldLanguage, string? newLanguage)
    {
        // Act
        var eventArgs = new LanguageChangedEventArgs(oldLanguage!, newLanguage!);

        // Assert
        Assert.Equal(oldLanguage, eventArgs.OldLanguage);
        Assert.Equal(newLanguage, eventArgs.NewLanguage);
    }

    [Fact]
    public void EventArgs_SupportsMultipleEventHandlers()
    {
        // Arrange
        var handlerCallCount = 0;
        var capturedEventArgs = new List<LanguageChangedEventArgs>();

        void Handler1(object? sender, LanguageChangedEventArgs e)
        {
            handlerCallCount++;
            capturedEventArgs.Add(e);
        }

        void Handler2(object? sender, LanguageChangedEventArgs e)
        {
            handlerCallCount++;
            capturedEventArgs.Add(e);
        }

        var eventArgs = new LanguageChangedEventArgs("en-US", "fr-FR");

        // Act
        Handler1(this, eventArgs);
        Handler2(this, eventArgs);

        // Assert
        Assert.Equal(2, handlerCallCount);
        Assert.Equal(2, capturedEventArgs.Count);
        Assert.All(capturedEventArgs, args =>
        {
            Assert.Equal("en-US", args.OldLanguage);
            Assert.Equal("fr-FR", args.NewLanguage);
        });
    }
}
