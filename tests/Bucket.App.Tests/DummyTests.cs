using Xunit;

namespace Bucket.App.Tests;

/// <summary>
/// Tests de base pour s'assurer que le projet de test fonctionne correctement.
/// </summary>
public class DummyTests
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MathTest_ShouldCalculateCorrectly()
    {
        // Arrange
        var a = 2;
        var b = 3;
        var expected = 5;

        // Act
        var actual = a + b;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void Addition_ShouldReturnCorrectSum(int a, int b, int expected)
    {
        // Act
        var result = a + b;

        // Assert
        Assert.Equal(expected, result);
    }
}
