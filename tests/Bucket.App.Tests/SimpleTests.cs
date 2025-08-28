namespace Bucket.App.Tests;

public class SimpleTests
{
    [Fact]
    public void Test_ReturnTrue_ShouldReturnTrue()
    {
        // Arrange
        var result = true;

        // Act & Assert
        Assert.True(result);
    }

    [Fact]
    public void Test_ReturnFalse_ShouldReturnFalse()
    {
        // Arrange
        var result = false;

        // Act & Assert
        Assert.False(result);
    }

    [Fact]
    public void Test_SimpleAddition_ShouldReturnCorrectResult()
    {
        // Arrange
        int a = 2;
        int b = 3;
        int expected = 5;

        // Act
        int actual = a + b;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Test_StringComparison_ShouldReturnTrue()
    {
        // Arrange
        string text1 = "Hello";
        string text2 = "Hello";

        // Act
        bool areEqual = text1 == text2;

        // Assert
        Assert.True(areEqual);
    }
}
