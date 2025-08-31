using Bucket.Core.Models;
using Bucket.Core.Services;

namespace Bucket.App.Tests.Services;

/// <summary>
/// Mock implementation of ISystemLanguageDetectionService for testing
/// Avoids WinRT API calls that can hang in CI environments
/// </summary>
public class MockSystemLanguageDetectionService : ISystemLanguageDetectionService
{
    private readonly string _mockSystemLanguage;

    public MockSystemLanguageDetectionService(string mockSystemLanguage = "en-US")
    {
        _mockSystemLanguage = mockSystemLanguage;
    }

    public string GetSystemLanguageCode()
    {
        return _mockSystemLanguage;
    }

    public string GetBestMatchingLanguage()
    {
        return SupportedLanguages.MapOSLanguageToSupported(_mockSystemLanguage);
    }
}
