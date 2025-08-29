using Bucket.Core.Services;

namespace Bucket.App.Tests.Services;

/// <summary>
/// Mock implementation of ISystemLanguageDetectionService for testing
/// </summary>
public class MockSystemLanguageDetectionService : ISystemLanguageDetectionService
{
    private readonly string _systemLanguageCode;

    public MockSystemLanguageDetectionService(string systemLanguageCode = "en-US")
    {
        _systemLanguageCode = systemLanguageCode;
    }

    public string GetSystemLanguageCode()
    {
        return _systemLanguageCode;
    }

    public string GetBestMatchingLanguage()
    {
        return Core.Models.SupportedLanguages.MapOSLanguageToSupported(_systemLanguageCode);
    }
}
