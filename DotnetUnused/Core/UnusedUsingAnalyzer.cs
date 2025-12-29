using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using DotnetUnused.Models;

namespace DotnetUnused.Core;

/// <summary>
/// Analyzes solutions for unused using directives using Roslyn's IDE0005 analyzer
/// </summary>
public sealed class UnusedUsingAnalyzer
{
    /// <summary>
    /// Analyzes the solution for unused using directives
    /// </summary>
    public async Task<List<UsingDirectiveInfo>> AnalyzeAsync(
        Solution solution,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<UsingDirectiveInfo>();
        var projects = solution.Projects.ToList();

        progress?.Report($"Analyzing unused usings in {projects.Count} projects...");

        // Process projects in parallel
        await Parallel.ForEachAsync(projects, cancellationToken, async (project, ct) =>
        {
            if (!project.SupportsCompilation)
            {
                return;
            }

            var compilation = await project.GetCompilationAsync(ct);
            if (compilation is null)
            {
                return;
            }

            try
            {
                // Get IDE analyzers (includes IDE0005)
                var analyzerReferences = project.AnalyzerReferences;
                if (!analyzerReferences.Any())
                {
                    return;
                }

                var analyzers = analyzerReferences
                    .SelectMany(r => r.GetAnalyzers(project.Language))
                    .Where(a => a != null)
                    .ToImmutableArray();

                if (analyzers.IsEmpty)
                {
                    return;
                }

                var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, options: null);
                var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync(ct);

                // Filter for CS8019 (unused usings - compiler diagnostic) or IDE0005 (IDE analyzer)
                var unusedUsingDiagnostics = diagnostics.Where(d =>
                    (d.Id == "CS8019" || d.Id == "IDE0005") &&
                    d.Location.IsInSource &&
                    d.Location.SourceTree != null);

                foreach (var diagnostic in unusedUsingDiagnostics)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    var filePath = lineSpan.Path;

                    // Skip external dependencies (NuGet packages, SDK files)
                    if (filePath.Contains(".nuget") || filePath.Contains("Microsoft.NET.") ||
                        filePath.Contains("\\packages\\") || filePath.Contains("/packages/"))
                    {
                        continue;
                    }

                    // Apply FileFilter to skip generated files
                    if (!FileFilter.ShouldAnalyze(filePath))
                    {
                        continue;
                    }

                    results.Add(new UsingDirectiveInfo
                    {
                        FilePath = filePath,
                        LineNumber = lineSpan.StartLinePosition.Line + 1,
                        Namespace = ExtractNamespaceFromDiagnostic(diagnostic),
                        Message = diagnostic.GetMessage()
                    });
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the entire analysis
                progress?.Report($"Warning: Failed to analyze usings in {project.Name}: {ex.Message}");
            }
        });

        progress?.Report($"Found {results.Count} unused using directives");
        return results.OrderBy(u => u.FilePath).ThenBy(u => u.LineNumber).ToList();
    }

    /// <summary>
    /// Extracts the namespace from the IDE0005 diagnostic message
    /// </summary>
    private static string ExtractNamespaceFromDiagnostic(Diagnostic diagnostic)
    {
        // IDE0005 message format: "Using directive is unnecessary."
        // The actual namespace is in the source span
        var sourceTree = diagnostic.Location.SourceTree;
        if (sourceTree != null)
        {
            var span = diagnostic.Location.SourceSpan;
            var text = sourceTree.GetText().GetSubText(span).ToString();

            // Extract namespace from "using System.Linq;" -> "System.Linq"
            text = text.Replace("using", "").Replace(";", "").Trim();

            // Handle "using static" -> remove "static"
            if (text.StartsWith("static "))
            {
                text = text.Substring(7).Trim();
            }

            // Handle "global using" -> remove "global"
            if (text.StartsWith("global "))
            {
                text = text.Substring(7).Trim();
            }

            return text;
        }

        return diagnostic.GetMessage();
    }
}
