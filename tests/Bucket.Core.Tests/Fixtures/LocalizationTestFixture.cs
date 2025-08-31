using Bucket.Core.Models;

namespace Bucket.Core.Tests.Fixtures;

/// <summary>
/// Shared test fixture for localization tests
/// Demonstrates XUnit IClassFixture and ICollectionFixture patterns
/// </summary>
public class LocalizationTestFixture : IDisposable
{
    public IReadOnlyList<LanguageItem> TestLanguages { get; }
    public string DefaultLanguageCode { get; }
    public Dictionary<string, string> TestResourceKeys { get; }

    public LocalizationTestFixture()
    {
        // Initialize test data that can be shared across multiple tests
        TestLanguages = new List<LanguageItem>
        {
            new("en-US", "English"),
            new("fr-FR", "Français"),
            new("de-DE", "Deutsch"), // Not actually supported, but useful for testing
            new("es-ES", "Español")  // Not actually supported, but useful for testing
        }.AsReadOnly();

        DefaultLanguageCode = SupportedLanguages.DefaultLanguage;

        TestResourceKeys = new Dictionary<string, string>
        {
            { "Welcome", "Welcome" },
            { "Goodbye", "Goodbye" },
            { "Settings", "Settings" },
            { "Language", "Language" },
            { "Cancel", "Cancel" },
            { "Save", "Save" }
        };
    }

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Collection definition for sharing fixture across multiple test classes
/// </summary>
[CollectionDefinition("Localization Collection")]
public class LocalizationTestCollection : ICollectionFixture<LocalizationTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
