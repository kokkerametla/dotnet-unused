using System.Text.Json;
using DotnetUnused.Models;
using Spectre.Console;

namespace DotnetUnused.Reporting;

/// <summary>
/// Formats and outputs analysis results
/// </summary>
public sealed class ReportWriter
{
    /// <summary>
    /// Writes results to console in a human-readable format
    /// </summary>
    public void WriteConsoleReport(AnalysisResult result)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Unused Code Analysis Results[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();

        // Summary
        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");

        summaryTable.AddRow("Total Symbols Analyzed", result.TotalSymbolsAnalyzed.ToString());
        summaryTable.AddRow("Total References Found", result.TotalReferencesFound.ToString());
        summaryTable.AddRow("Unused Symbols", $"[red]{result.UnusedSymbols.Count}[/]");
        summaryTable.AddRow("Analysis Duration", $"{result.Duration.TotalSeconds:F2}s");

        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        if (result.UnusedSymbols.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No unused symbols found![/]");
            return;
        }

        // Group by symbol kind
        var grouped = result.UnusedSymbols
            .GroupBy(s => s.Kind)
            .OrderBy(g => g.Key.ToString());

        foreach (var group in grouped)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[cyan]Unused {group.Key}s ({group.Count()})[/]").RuleStyle("grey").LeftJustified());
            AnsiConsole.WriteLine();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Name")
                .AddColumn("Location");

            foreach (var symbol in group.OrderBy(s => s.FilePath).ThenBy(s => s.LineNumber))
            {
                var name = symbol.FullyQualifiedName;
                var location = $"{symbol.FilePath}:{symbol.LineNumber}";
                table.AddRow(name, location);
            }

            AnsiConsole.Write(table);
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes results to a JSON file
    /// </summary>
    public async Task WriteJsonReportAsync(AnalysisResult result, string outputPath)
    {
        var jsonData = new
        {
            Summary = new
            {
                result.TotalSymbolsAnalyzed,
                result.TotalReferencesFound,
                UnusedCount = result.UnusedSymbols.Count,
                DurationSeconds = result.Duration.TotalSeconds
            },
            UnusedSymbols = result.UnusedSymbols.Select(s => new
            {
                Kind = s.Kind.ToString(),
                s.FullyQualifiedName,
                s.FilePath,
                s.LineNumber
            }).OrderBy(s => s.FilePath).ThenBy(s => s.LineNumber).ToList()
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(jsonData, options);
        await File.WriteAllTextAsync(outputPath, json);

        AnsiConsole.MarkupLine($"[green]JSON report written to: {outputPath}[/]");
    }
}
