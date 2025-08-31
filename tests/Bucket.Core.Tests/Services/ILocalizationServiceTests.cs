using Bucket.Core.Models;
using Bucket.Core.Services;
using Moq;

namespace Bucket.Core.Tests.Services;

/// <summary>
/// Unit tests for ILocalizationService interface contract and mock behavior
/// </summary>
public class ILocalizationServiceTests
{
    [Fact]
    public void CurrentLanguage_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.CurrentLanguage).Returns("fr-FR");

        // Act
        var result = mockService.Object.CurrentLanguage;

        // Assert
        Assert.Equal("fr-FR", result);
    }

    [Fact]
    public void SupportedLanguages_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        var supportedLanguages = new List<LanguageItem>
        {
            new("en-US", "English"),
            new("fr-FR", "Français")
        }.AsReadOnly();

        mockService.Setup(s => s.SupportedLanguages).Returns(supportedLanguages);

        // Act
        var result = mockService.Object.SupportedLanguages;

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, lang => lang.Code == "en-US");
        Assert.Contains(result, lang => lang.Code == "fr-FR");
    }

    [Fact]
    public async Task SetLanguageAsync_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.SetLanguageAsync("fr-FR")).ReturnsAsync(true);

        // Act
        var result = await mockService.Object.SetLanguageAsync("fr-FR");

        // Assert
        Assert.True(result);
        mockService.Verify(s => s.SetLanguageAsync("fr-FR"), Times.Once);
    }

    [Theory]
    [InlineData("en-US", true)]
    [InlineData("fr-FR", true)]
    [InlineData("invalid-lang", false)]
    [InlineData("", false)]
    public async Task SetLanguageAsync_WithDifferentLanguageCodes_ReturnsExpectedResults(string languageCode, bool expectedResult)
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.SetLanguageAsync(languageCode)).ReturnsAsync(expectedResult);

        // Act
        var result = await mockService.Object.SetLanguageAsync(languageCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task InitializeAsync_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.InitializeAsync("en-US")).Returns(Task.CompletedTask);

        // Act
        await mockService.Object.InitializeAsync("en-US");

        // Assert
        mockService.Verify(s => s.InitializeAsync("en-US"), Times.Once);
    }

    [Fact]
    public async Task InitializeWithAutoDetectionAsync_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.InitializeWithAutoDetectionAsync("en-US", true)).Returns(Task.CompletedTask);

        // Act
        await mockService.Object.InitializeWithAutoDetectionAsync("en-US", true);

        // Assert
        mockService.Verify(s => s.InitializeWithAutoDetectionAsync("en-US", true), Times.Once);
    }

    [Theory]
    [InlineData("HelloWorld", "Hello World")]
    [InlineData("AppTitle", "My Application")]
    [InlineData("NonExistentKey", "NonExistentKey")]
    public void GetString_WithDifferentKeys_ReturnsExpectedResults(string key, string expectedResult)
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.GetString(key)).Returns(expectedResult);

        // Act
        var result = mockService.Object.GetString(key);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void LanguageChanged_EventCanBeRaised()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        var eventRaised = false;
        LanguageChangedEventArgs? capturedEventArgs = null;

        // Act
        mockService.Object.LanguageChanged += (sender, args) =>
        {
            eventRaised = true;
            capturedEventArgs = args;
        };

        // Simulate raising the event
        mockService.Raise(s => s.LanguageChanged += null, new LanguageChangedEventArgs("en-US", "fr-FR"));

        // Assert
        Assert.True(eventRaised);
        Assert.NotNull(capturedEventArgs);
        Assert.Equal("en-US", capturedEventArgs.OldLanguage);
        Assert.Equal("fr-FR", capturedEventArgs.NewLanguage);
    }

    [Fact]
    public async Task ServiceWorkflow_SimulatesCompleteLocalizationFlow()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        var eventFired = false;

        // Setup initial state
        mockService.Setup(s => s.CurrentLanguage).Returns("en-US");
        mockService.Setup(s => s.SupportedLanguages).Returns(SupportedLanguages.All);

        // Setup initialization
        mockService.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Setup language change
        mockService.Setup(s => s.SetLanguageAsync("fr-FR"))
               .Callback(() => mockService.Setup(s => s.CurrentLanguage).Returns("fr-FR"))
               .ReturnsAsync(true);

        // Setup string retrieval
        mockService.Setup(s => s.GetString("Welcome"))
               .Returns(() => mockService.Object.CurrentLanguage == "fr-FR" ? "Bienvenue" : "Welcome");

        // Subscribe to events
        mockService.Object.LanguageChanged += (sender, args) => eventFired = true;

        // Act
        await mockService.Object.InitializeAsync("en-US");
        var initialWelcome = mockService.Object.GetString("Welcome");
        var changeResult = await mockService.Object.SetLanguageAsync("fr-FR");
        mockService.Raise(s => s.LanguageChanged += null, new LanguageChangedEventArgs("en-US", "fr-FR"));
        var newWelcome = mockService.Object.GetString("Welcome");
        var currentLanguage = mockService.Object.CurrentLanguage;

        // Assert
        Assert.Equal("Welcome", initialWelcome);
        Assert.True(changeResult);
        Assert.True(eventFired);
        Assert.Equal("Bienvenue", newWelcome);
        Assert.Equal("fr-FR", currentLanguage);
    }

    [Fact]
    public async Task InitializeAsync_WithNullParameter_HandlesGracefully()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.InitializeAsync(null)).Returns(Task.CompletedTask);

        // Act
        await mockService.Object.InitializeAsync(null);

        // Assert
        mockService.Verify(s => s.InitializeAsync(null), Times.Once);
    }

    [Fact]
    public async Task InitializeWithAutoDetectionAsync_WithDefaultParameters_WorksCorrectly()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.InitializeWithAutoDetectionAsync(null, false)).Returns(Task.CompletedTask);

        // Act
        await mockService.Object.InitializeWithAutoDetectionAsync();

        // Assert
        mockService.Verify(s => s.InitializeWithAutoDetectionAsync(null, false), Times.Once);
    }

    [Fact]
    public void GetString_WithNullOrEmptyKey_CanBeMocked()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.GetString(null!)).Returns(string.Empty);
        mockService.Setup(s => s.GetString("")).Returns("");

        // Act
        var resultNull = mockService.Object.GetString(null!);
        var resultEmpty = mockService.Object.GetString("");

        // Assert
        Assert.Equal(string.Empty, resultNull);
        Assert.Equal("", resultEmpty);
    }

    [Fact]
    public async Task SetLanguageAsync_CanThrowExceptions()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.SetLanguageAsync("invalid"))
               .ThrowsAsync(new ArgumentException("Invalid language code"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => mockService.Object.SetLanguageAsync("invalid"));
    }

    [Fact]
    public void MultipleEventHandlers_CanBeAttachedToLanguageChanged()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        var handler1Called = false;
        var handler2Called = false;

        // Act
        mockService.Object.LanguageChanged += (sender, args) => handler1Called = true;
        mockService.Object.LanguageChanged += (sender, args) => handler2Called = true;

        mockService.Raise(s => s.LanguageChanged += null, new LanguageChangedEventArgs("en-US", "fr-FR"));

        // Assert
        Assert.True(handler1Called);
        Assert.True(handler2Called);
    }

    [Fact]
    public async Task AsyncOperations_CanBeCanceled()
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        using var cts = new CancellationTokenSource();

        mockService.Setup(s => s.SetLanguageAsync("fr-FR"))
               .Returns(async () =>
               {
                   await Task.Delay(100, cts.Token);
                   return true;
               });

        // Act
        var task = mockService.Object.SetLanguageAsync("fr-FR");
        cts.Cancel();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }

    [Theory]
    [MemberData(nameof(GetLanguageSetupTestData))]
    public async Task SetLanguageAsync_WithMemberData_WorksCorrectly(string languageCode, bool expectedResult)
    {
        // Arrange
        var mockService = new Mock<ILocalizationService>();
        mockService.Setup(s => s.SetLanguageAsync(languageCode)).ReturnsAsync(expectedResult);

        // Act
        var result = await mockService.Object.SetLanguageAsync(languageCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    public static IEnumerable<object[]> GetLanguageSetupTestData()
    {
        yield return new object[] { "en-US", true };
        yield return new object[] { "fr-FR", true };
        yield return new object[] { "es-ES", false };  // Unsupported
        yield return new object[] { "invalid", false };
        yield return new object[] { "", false };
    }
}
