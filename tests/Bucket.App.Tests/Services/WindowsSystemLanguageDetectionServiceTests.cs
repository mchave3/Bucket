using Bucket.App.Services;

namespace Bucket.App.Tests.Services;

/// <summary>
/// Unit tests for WindowsSystemLanguageDetectionService class
/// Basic functionality tests
/// </summary>
public class WindowsSystemLanguageDetectionServiceTests
{
    private readonly WindowsSystemLanguageDetectionService _service;

    public WindowsSystemLanguageDetectionServiceTests()
    {
        _service = new WindowsSystemLanguageDetectionService();
    }

    [Fact]
    public void Constructor_CreatesValidService()
    {
        // Assert
        Assert.NotNull(_service);
    }

    [Fact]
    public void Service_HasCorrectType()
    {
        // Assert
        Assert.IsType<WindowsSystemLanguageDetectionService>(_service);
    }
}
