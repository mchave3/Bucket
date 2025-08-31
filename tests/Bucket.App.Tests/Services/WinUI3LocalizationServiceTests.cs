using Bucket.App.Services;
using Bucket.Core.Services;
using Moq;

namespace Bucket.App.Tests.Services;

/// <summary>
/// Unit tests for WinUI3LocalizationService class
/// Uses mocks to avoid WinUI3/WinRT dependencies in CI environment
/// </summary>
public class WinUI3LocalizationServiceTests
{
    [Fact]
    public void WinUI3LocalizationService_HasCorrectType()
    {
        Assert.True(typeof(WinUI3LocalizationService).IsClass);
        Assert.False(typeof(WinUI3LocalizationService).IsAbstract);
    }

    [Fact]
    public void WinUI3LocalizationService_IsPublic()
    {
        Assert.True(typeof(WinUI3LocalizationService).IsPublic);
    }

    [Fact]
    public void WinUI3LocalizationService_ImplementsILocalizationService()
    {
        Assert.True(typeof(ILocalizationService).IsAssignableFrom(typeof(WinUI3LocalizationService)));
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var mockSystemLanguageDetection = new Mock<ISystemLanguageDetectionService>();
        var mockSaveLanguageToConfig = new Mock<Action<string>>();

        var exception = Record.Exception(() =>
            new WinUI3LocalizationService(
                mockSystemLanguageDetection.Object,
                mockSaveLanguageToConfig.Object.Invoke));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WithNullSystemLanguageDetection_ThrowsArgumentNullException()
    {
        var mockSaveLanguageToConfig = new Mock<Action<string>>();

        Assert.Throws<ArgumentNullException>(() =>
            new WinUI3LocalizationService(null, mockSaveLanguageToConfig.Object.Invoke));
    }

    [Fact]
    public void Constructor_WithNullSaveLanguageToConfig_ThrowsArgumentNullException()
    {
        var mockSystemLanguageDetection = new Mock<ISystemLanguageDetectionService>();

        Assert.Throws<ArgumentNullException>(() =>
            new WinUI3LocalizationService(mockSystemLanguageDetection.Object, null));
    }

    [Fact]
    public void CurrentLanguage_BeforeInitialization_ReturnsDefaultLanguage()
    {
        var mockSystemLanguageDetection = new Mock<ISystemLanguageDetectionService>();
        var mockSaveLanguageToConfig = new Mock<Action<string>>();
        var service = new WinUI3LocalizationService(
            mockSystemLanguageDetection.Object,
            mockSaveLanguageToConfig.Object.Invoke);

        var currentLanguage = service.CurrentLanguage;

        Assert.Equal(Core.Models.SupportedLanguages.DefaultLanguage, currentLanguage);
    }

    [Fact]
    public void SupportedLanguages_ReturnsAllSupportedLanguages()
    {
        var mockSystemLanguageDetection = new Mock<ISystemLanguageDetectionService>();
        var mockSaveLanguageToConfig = new Mock<Action<string>>();
        var service = new WinUI3LocalizationService(
            mockSystemLanguageDetection.Object,
            mockSaveLanguageToConfig.Object.Invoke);

        var supportedLanguages = service.SupportedLanguages;

        Assert.NotNull(supportedLanguages);
        Assert.Equal(Core.Models.SupportedLanguages.All.Count, supportedLanguages.Count);
    }

    [Theory]
    [InlineData("nonexistent.key")]
    [InlineData("")]
    public void GetString_WithoutInitialization_ReturnsKey(string key)
    {
        var mockSystemLanguageDetection = new Mock<ISystemLanguageDetectionService>();
        var mockSaveLanguageToConfig = new Mock<Action<string>>();
        var service = new WinUI3LocalizationService(
            mockSystemLanguageDetection.Object,
            mockSaveLanguageToConfig.Object.Invoke);

        var result = service.GetString(key);

        Assert.Equal(key, result);
    }

    [Fact]
    public void GetString_WithNullKey_ReturnsNull()
    {
        var mockSystemLanguageDetection = new Mock<ISystemLanguageDetectionService>();
        var mockSaveLanguageToConfig = new Mock<Action<string>>();
        var service = new WinUI3LocalizationService(
            mockSystemLanguageDetection.Object,
            mockSaveLanguageToConfig.Object.Invoke);

        var result = service.GetString(null!);

        Assert.Null(result);
    }
}
