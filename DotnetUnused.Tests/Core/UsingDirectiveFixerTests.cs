using DotnetUnused.Core;
using DotnetUnused.Models;
using Xunit;

namespace DotnetUnused.Tests.Core;

public class UsingDirectiveFixerTests
{
    [Fact]
    public async Task ApplyFixesAsync_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var fixer = new UsingDirectiveFixer();
        var solution = CreateEmptySolution();
        var unusedUsings = new List<UsingDirectiveInfo>();

        // Act
        var result = await fixer.ApplyFixesAsync(solution, unusedUsings);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ApplyFixesAsync_RespectsProgress()
    {
        // Arrange
        var fixer = new UsingDirectiveFixer();
        var solution = CreateEmptySolution();
        var unusedUsings = new List<UsingDirectiveInfo>
        {
            new() { FilePath = "test.cs", LineNumber = 1, Namespace = "System.Linq" }
        };
        var progressMessages = new List<string>();
        var progress = new Progress<string>(msg => progressMessages.Add(msg));

        // Act
        await fixer.ApplyFixesAsync(solution, unusedUsings, progress);

        // Assert - Progress should report something (warning about file not found)
        Assert.NotEmpty(progressMessages);
    }

    [Fact(Skip = "Requires real file system access - integration test")]
    public async Task ApplyFixesAsync_UsesAtomicWrite()
    {
        // This would test that files are written atomically using temp files
        // Requires actual file system operations
        await Task.CompletedTask;
    }

    private static Microsoft.CodeAnalysis.Solution CreateEmptySolution()
    {
        return new Microsoft.CodeAnalysis.AdhocWorkspace().CurrentSolution;
    }
}
