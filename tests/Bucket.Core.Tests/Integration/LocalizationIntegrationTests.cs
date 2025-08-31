using Bucket.Core.Helpers;
using Bucket.Core.Models;
using Bucket.Core.Services;
using Moq;

namespace Bucket.Core.Tests.Integration;

/// <summary>
/// Integration tests for localization functionality without WinRT dependencies
/// </summary>
public class LocalizationIntegrationTests
{
    [Fact]
    public async Task LocalizationWorkflow_FirstStartupAutoDetection_WorksCorrectly()
    {
        // Arrange
        var mockSystemLanguageService = new Mock<ISystemLanguageDetectionService>();
        var mockLocalizationService = new Mock<ILocalizationService>();

        // Setup system language detection to return French Canadian
        mockSystemLanguageService.Setup(s => s.GetSystemLanguageCode()).Returns("fr-CA");
        mockSystemLanguageService.Setup(s => s.GetBestMatchingLanguage()).Returns("fr-FR");

        // Setup localization service behavior
        mockLocalizationService.Setup(s => s.InitializeWithAutoDetectionAsync(null, true))
                       .Returns(Task.CompletedTask);
        mockLocalizationService.Setup(s => s.CurrentLanguage).Returns("fr-FR");

        // Act
        var detectedLanguage = mockSystemLanguageService.Object.GetBestMatchingLanguage();
        await mockLocalizationService.Object.InitializeWithAutoDetectionAsync(null, true);
        var currentLanguage = mockLocalizationService.Object.CurrentLanguage;

        // Assert
        Assert.Equal("fr-FR", detectedLanguage);
        Assert.Equal("fr-FR", currentLanguage);
        mockSystemLanguageService.Verify(s => s.GetBestMatchingLanguage(), Times.Once);
        mockLocalizationService.Verify(s => s.InitializeWithAutoDetectionAsync(null, true), Times.Once);
    }

    [Fact]
    public async Task LocalizationWorkflow_SubsequentStartupWithSavedLanguage_UsesExistingLanguage()
    {
        // Arrange
        var mockSystemLanguageService = new Mock<ISystemLanguageDetectionService>();
        var mockLocalizationService = new Mock<ILocalizationService>();

        var savedLanguage = "en-US";

        // Setup localization service to use saved language (no auto-detection)
        mockLocalizationService.Setup(s => s.InitializeAsync(savedLanguage))
                       .Returns(Task.CompletedTask);
        mockLocalizationService.Setup(s => s.CurrentLanguage).Returns(savedLanguage);

        // Act
        await mockLocalizationService.Object.InitializeAsync(savedLanguage);
        var currentLanguage = mockLocalizationService.Object.CurrentLanguage;

        // Assert
        Assert.Equal("en-US", currentLanguage);
        mockLocalizationService.Verify(s => s.InitializeAsync(savedLanguage), Times.Once);
        // System language service should not be called for subsequent startups
        mockSystemLanguageService.Verify(s => s.GetBestMatchingLanguage(), Times.Never);
    }

    [Theory]
    [InlineData("en-GB", "en-US")]
    [InlineData("fr-CA", "fr-FR")]
    [InlineData("es-MX", "en-US")]
    [InlineData("de-DE", "en-US")]
    public void LanguageMapping_WithSystemLanguageDetection_MapsCorrectly(string osLanguage, string expectedSupportedLanguage)
    {
        // Arrange
        var mockSystemLanguageService = new Mock<ISystemLanguageDetectionService>();
        mockSystemLanguageService.Setup(s => s.GetSystemLanguageCode()).Returns(osLanguage);
        mockSystemLanguageService.Setup(s => s.GetBestMatchingLanguage())
                          .Returns(() => SupportedLanguages.MapOSLanguageToSupported(osLanguage));

        // Act
        var systemLanguage = mockSystemLanguageService.Object.GetSystemLanguageCode();
        var mappedLanguage = mockSystemLanguageService.Object.GetBestMatchingLanguage();

        // Assert
        Assert.Equal(osLanguage, systemLanguage);
        Assert.Equal(expectedSupportedLanguage, mappedLanguage);
    }

    [Fact]
    public async Task LanguageChangeWorkflow_WithEventNotification_WorksCorrectly()
    {
        // Arrange
        var mockLocalizationService = new Mock<ILocalizationService>();
        var eventReceived = false;
        LanguageChangedEventArgs? receivedEventArgs = null;

        mockLocalizationService.Setup(s => s.SetLanguageAsync("fr-FR")).ReturnsAsync(true);
        mockLocalizationService.Setup(s => s.CurrentLanguage).Returns("en-US");

        // Subscribe to language change event
        mockLocalizationService.Object.LanguageChanged += (sender, args) =>
        {
            eventReceived = true;
            receivedEventArgs = args;
        };

        // Act
        var changeResult = await mockLocalizationService.Object.SetLanguageAsync("fr-FR");

        // Simulate the event being raised by the service
        mockLocalizationService.Raise(s => s.LanguageChanged += null, new LanguageChangedEventArgs("en-US", "fr-FR"));

        // Assert
        Assert.True(changeResult);
        Assert.True(eventReceived);
        Assert.NotNull(receivedEventArgs);
        Assert.Equal("en-US", receivedEventArgs.OldLanguage);
        Assert.Equal("fr-FR", receivedEventArgs.NewLanguage);
    }

    [Fact]
    public void ErrorHandling_WhenLanguageChangeValidation_WorksCorrectly()
    {
        // Arrange
        var invalidLanguage = "invalid-lang";
        var currentLanguage = "en-US";

        var validationResult = LocalizationHelper.ValidateLanguageCode(invalidLanguage);
        var shouldChange = LocalizationHelper.ShouldChangeLanguage(currentLanguage, invalidLanguage);

        // Act & Assert
        Assert.Equal(SupportedLanguages.DefaultLanguage, validationResult);
        Assert.True(shouldChange); // Should change because codes are different, but validation will handle the invalid code
    }

    [Fact]
    public void LocalizationHelper_Integration_WithSupportedLanguages_WorksCorrectly()
    {
        // Test the integration between LocalizationHelper and SupportedLanguages

        // Test valid languages
        foreach (var supportedLang in SupportedLanguages.All)
        {
            var validatedCode = LocalizationHelper.ValidateLanguageCode(supportedLang.Code);
            var languageItem = LocalizationHelper.GetLanguageItem(supportedLang.Code);
            var isSupported = SupportedLanguages.IsSupported(supportedLang.Code);

            Assert.Equal(supportedLang.Code, validatedCode);
            Assert.Equal(supportedLang, languageItem);
            Assert.True(isSupported);
        }

        // Test invalid language handling
        var invalidCode = "xyz-123";
        var validatedInvalid = LocalizationHelper.ValidateLanguageCode(invalidCode);
        var invalidLanguageItem = LocalizationHelper.GetLanguageItem(invalidCode);
        var isInvalidSupported = SupportedLanguages.IsSupported(invalidCode);

        Assert.Equal(SupportedLanguages.DefaultLanguage, validatedInvalid);
        Assert.Equal(SupportedLanguages.GetDefault(), invalidLanguageItem);
        Assert.False(isInvalidSupported);
    }

    [Fact]
    public async Task CompleteApplicationStartup_SimulatesRealWorldScenario()
    {
        // Arrange - Simulate a complete application startup scenario
        var mockSystemLanguageService = new Mock<ISystemLanguageDetectionService>();
        var mockLocalizationService = new Mock<ILocalizationService>();

        var isFirstStartup = true;
        string? savedLanguage = null; // No saved language for first startup

        // Setup system language detection
        mockSystemLanguageService.Setup(s => s.GetSystemLanguageCode()).Returns("fr-CA");
        mockSystemLanguageService.Setup(s => s.GetBestMatchingLanguage()).Returns("fr-FR");

        // Setup localization service initialization
        mockLocalizationService.Setup(s => s.InitializeWithAutoDetectionAsync(savedLanguage, isFirstStartup))
                       .Returns(Task.CompletedTask);
        mockLocalizationService.Setup(s => s.CurrentLanguage).Returns("fr-FR");
        mockLocalizationService.Setup(s => s.SupportedLanguages).Returns(SupportedLanguages.All);

        // Act - Simulate application startup logic
        string languageToUse;
        if (isFirstStartup)
        {
            languageToUse = mockSystemLanguageService.Object.GetBestMatchingLanguage();
            await mockLocalizationService.Object.InitializeWithAutoDetectionAsync(savedLanguage, isFirstStartup);
        }
        else
        {
            languageToUse = savedLanguage ?? SupportedLanguages.DefaultLanguage;
            await mockLocalizationService.Object.InitializeAsync(languageToUse);
        }

        var currentLanguage = mockLocalizationService.Object.CurrentLanguage;
        var supportedLanguages = mockLocalizationService.Object.SupportedLanguages;

        // Assert
        Assert.Equal("fr-FR", languageToUse);
        Assert.Equal("fr-FR", currentLanguage);
        Assert.NotNull(supportedLanguages);
        Assert.Contains(supportedLanguages, lang => lang.Code == "fr-FR");

        mockSystemLanguageService.Verify(s => s.GetBestMatchingLanguage(), Times.Once);
        mockLocalizationService.Verify(s => s.InitializeWithAutoDetectionAsync(savedLanguage, isFirstStartup), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetLanguageChangeScenarios))]
    public async Task LanguageChangeScenarios_WithDifferentStartingLanguages_WorkCorrectly(
        string initialLanguage,
        string newLanguage,
        bool expectedChangeResult,
        bool shouldEventFire)
    {
        // Arrange
        var mockLocalizationService = new Mock<ILocalizationService>();
        var eventFired = false;

        mockLocalizationService.Setup(s => s.CurrentLanguage).Returns(initialLanguage);
        mockLocalizationService.Setup(s => s.SetLanguageAsync(newLanguage)).ReturnsAsync(expectedChangeResult);

        mockLocalizationService.Object.LanguageChanged += (sender, args) => eventFired = true;

        // Act
        var shouldChange = LocalizationHelper.ShouldChangeLanguage(initialLanguage, newLanguage);
        var changeResult = await mockLocalizationService.Object.SetLanguageAsync(newLanguage);

        if (shouldEventFire && expectedChangeResult)
        {
            mockLocalizationService.Raise(s => s.LanguageChanged += null,
                new LanguageChangedEventArgs(initialLanguage, newLanguage));
        }

        // Assert
        Assert.Equal(expectedChangeResult, changeResult);
        Assert.Equal(shouldChange, !initialLanguage.Equals(newLanguage, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(newLanguage));
        Assert.Equal(shouldEventFire && expectedChangeResult, eventFired);
    }

    public static IEnumerable<object[]> GetLanguageChangeScenarios()
    {
        yield return new object[] { "en-US", "fr-FR", true, true };   // Valid change
        yield return new object[] { "en-US", "en-US", true, false };  // Same language
        yield return new object[] { "fr-FR", "en-US", true, true };   // Valid reverse change
        yield return new object[] { "en-US", "", false, false };      // Empty new language
        yield return new object[] { "en-US", "invalid", false, false }; // Invalid language
        yield return new object[] { "fr-FR", "FR-FR", true, false };  // Case difference only
    }

    [Fact]
    public void ErrorRecovery_WhenSystemLanguageDetectionFails_FallsBackCorrectly()
    {
        // Arrange
        var mockSystemLanguageService = new Mock<ISystemLanguageDetectionService>();

        // Simulate system language detection failure
        mockSystemLanguageService.Setup(s => s.GetSystemLanguageCode())
                          .Throws(new InvalidOperationException("System language unavailable"));

        // Simulate fallback behavior
        mockSystemLanguageService.Setup(s => s.GetBestMatchingLanguage())
                          .Returns(SupportedLanguages.DefaultLanguage);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mockSystemLanguageService.Object.GetSystemLanguageCode());

        var fallbackLanguage = mockSystemLanguageService.Object.GetBestMatchingLanguage();
        Assert.Equal(SupportedLanguages.DefaultLanguage, fallbackLanguage);
    }
}
