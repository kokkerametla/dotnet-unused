using DotnetUnused.Models;
using Xunit;

namespace DotnetUnused.Tests.Models;

public class UsingDirectiveInfoTests
{
    [Fact]
    public void Constructor_InitializesPropertiesWithDefaultValues()
    {
        // Act
        var info = new UsingDirectiveInfo();

        // Assert
        Assert.Equal(string.Empty, info.FilePath);
        Assert.Equal(0, info.LineNumber);
        Assert.Equal(string.Empty, info.Namespace);
        Assert.Equal(string.Empty, info.Message);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var info = new UsingDirectiveInfo
        {
            FilePath = "C:\\MyProject\\Program.cs",
            LineNumber = 5,
            Namespace = "System.Linq",
            Message = "Using directive is unnecessary"
        };

        // Assert
        Assert.Equal("C:\\MyProject\\Program.cs", info.FilePath);
        Assert.Equal(5, info.LineNumber);
        Assert.Equal("System.Linq", info.Namespace);
        Assert.Equal("Using directive is unnecessary", info.Message);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var info = new UsingDirectiveInfo
        {
            FilePath = "C:\\MyProject\\Program.cs",
            LineNumber = 3,
            Namespace = "System.Collections"
        };

        // Act
        var result = info.ToString();

        // Assert
        Assert.Equal("System.Collections at C:\\MyProject\\Program.cs:3", result);
    }

    [Theory]
    [InlineData("System.Linq")]
    [InlineData("System.Collections.Generic")]
    [InlineData("MyApp.Services")]
    [InlineData("")]
    public void Namespace_AcceptsVariousValues(string namespace_)
    {
        // Arrange & Act
        var info = new UsingDirectiveInfo { Namespace = namespace_ };

        // Assert
        Assert.Equal(namespace_, info.Namespace);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(0)]
    [InlineData(-1)] // Shouldn't happen but test edge case
    public void LineNumber_AcceptsVariousValues(int lineNumber)
    {
        // Arrange & Act
        var info = new UsingDirectiveInfo { LineNumber = lineNumber };

        // Assert
        Assert.Equal(lineNumber, info.LineNumber);
    }
}
