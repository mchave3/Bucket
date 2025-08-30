using Bucket.Core.Models;
using Bucket.Core.Services;

namespace Bucket.App.Tests.Services;

/// <summary>
/// Mock implementation of WindowsSystemLanguageDetectionService for testing
/// This mock simulates various Windows API behaviors without actually calling Windows APIs
/// </summary>
public class MockWindowsSystemLanguageDetectionService : ISystemLanguageDetectionService
{
    private readonly string? _simulatedSystemLanguage;
    private readonly bool _shouldThrowException;
    private readonly bool _shouldTimeout;

    public MockWindowsSystemLanguageDetectionService(
        string? simulatedSystemLanguage = "en-US",
        bool shouldThrowException = false,
        bool shouldTimeout = false)
    {
        _simulatedSystemLanguage = simulatedSystemLanguage;
        _shouldThrowException = shouldThrowException;
        _shouldTimeout = shouldTimeout;
    }

    public string GetSystemLanguageCode()
    {
        if (_shouldThrowException)
        {
            throw new InvalidOperationException("Simulated Windows API failure");
        }

        if (_shouldTimeout)
        {
            // Simulate timeout scenario - return fallback immediately
            return System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        // When testing with null or empty, return the default language directly
        // This simulates the behavior we want for unit tests
        if (string.IsNullOrEmpty(_simulatedSystemLanguage))
        {
            return SupportedLanguages.DefaultLanguage;
        }

        // Simulate successful Windows API call
        return _simulatedSystemLanguage;
    }

    public string GetBestMatchingLanguage()
    {
        var systemLanguage = GetSystemLanguageCode();
        return SupportedLanguages.MapOSLanguageToSupported(systemLanguage);
    }
}
