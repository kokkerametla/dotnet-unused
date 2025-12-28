using DotnetUnused.Core;
using Xunit;

namespace DotnetUnused.Tests.Core;

/// <summary>
/// Additional edge case tests for FileFilter to ensure robust filtering
/// </summary>
public class FileFilterEdgeCasesTests
{
    [Theory]
    [InlineData("C:\\MyProject\\src\\MyClass.cs", true)]
    [InlineData("C:/MyProject/src/MyClass.cs", true)]
    [InlineData("C:\\MyProject/mixed\\path\\MyClass.cs", true)]
    [InlineData("/unix/style/path/MyClass.cs", true)]
    public void ShouldAnalyze_HandlesMultiplePathSeparatorStyles(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("C:\\bin\\MyClass.cs", false)] // bin at root
    [InlineData("C:\\obj\\MyClass.cs", false)] // obj at root
    [InlineData("bin\\MyClass.cs", false)] // relative path with bin
    [InlineData("obj\\MyClass.cs", false)] // relative path with obj
    public void ShouldAnalyze_ExcludesBinObjAtAnyLevel(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("MyForm.designer.CS", false)] // Mixed case
    [InlineData("MyForm.DESIGNER.cs", false)] // All caps DESIGNER
    [InlineData("MyForm.Designer.CS", false)] // Mixed extension
    [InlineData("MyWindow.G.cs", false)] // Capital G
    [InlineData("MyWindow.g.CS", false)] // Capital CS
    public void ShouldAnalyze_CaseInsensitiveForGeneratedFiles(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("C:\\Program Files\\MyApp\\src\\MyClass.cs", true)] // Space in path
    [InlineData("C:\\My Project (2024)\\src\\MyClass.cs", true)] // Parentheses and numbers
    [InlineData("C:\\Projects\\My-App_v2\\src\\MyClass.cs", true)] // Hyphens and underscores
    [InlineData("C:\\проект\\src\\MyClass.cs", true)] // Unicode characters
    public void ShouldAnalyze_HandlesSpecialCharactersInPath(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("MyClass.cshtml", true)] // Razor files should be analyzed
    [InlineData("MyClass.razor", true)] // Blazor files should be analyzed
    [InlineData("MyClass.cs.bak", true)] // Backup files should be analyzed
    [InlineData("MyClass.cs.old", true)] // Old files should be analyzed
    public void ShouldAnalyze_AllowsNonStandardButValidCSharpFiles(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("C:\\MyProject\\SomeDesigner.cs", true)] // Not ".Designer.cs"
    [InlineData("C:\\MyProject\\MyClass.gcs", true)] // Not ".g.cs"
    [InlineData("C:\\MyProject\\Test.Designer.txt", true)] // Designer but not .cs
    public void ShouldAnalyze_RequiresPreciseExtensionPatterns(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("\\\\network\\share\\project\\bin\\MyClass.cs", false)]
    [InlineData("\\\\network\\share\\project\\src\\MyClass.cs", true)]
    [InlineData("\\\\server\\obj\\MyClass.cs", false)]
    public void ShouldAnalyze_HandlesUNCPaths(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Theory]
    [InlineData("C:\\MyProject\\binaries\\MyClass.cs", true)] // "bin" substring but not directory
    [InlineData("C:\\MyProject\\objects\\MyClass.cs", true)] // "obj" substring but not directory
    [InlineData("C:\\MyProject\\robin\\MyClass.cs", true)] // contains "bin"
    [InlineData("C:\\MyProject\\objection\\MyClass.cs", true)] // contains "obj"
    public void ShouldAnalyze_DoesNotExcludePartialMatches(string filePath, bool expected)
    {
        // Act & Assert - only exact "\\bin\\" or "\\obj\\" should be excluded
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }

    [Fact]
    public void ShouldAnalyze_HandlesWhitespaceOnlyString()
    {
        // Arrange
        var filePath = "   ";

        // Act
        var result = FileFilter.ShouldAnalyze(filePath);

        // Assert - whitespace-only should be treated as invalid
        Assert.False(result);
    }

    [Theory]
    [InlineData("C:\\MyProject\\.Designer.cs", false)] // Starts with .Designer
    [InlineData("C:\\MyProject\\.g.cs", false)] // Starts with .g
    [InlineData("C:\\MyProject\\File.Designer.", true)] // Ends with Designer. but no .cs
    public void ShouldAnalyze_HandlesEdgeCaseExtensions(string filePath, bool expected)
    {
        // Act & Assert
        Assert.Equal(expected, FileFilter.ShouldAnalyze(filePath));
    }
}
