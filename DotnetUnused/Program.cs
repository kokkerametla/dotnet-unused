using System.Diagnostics;
using DotnetUnused.Core;
using DotnetUnused.Reporting;
using Spectre.Console;

namespace DotnetUnused;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Parse command line arguments
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return 0;
            }

            var path = args[0];
            var format = "text";
            string? outputPath = null;
            var excludePublic = true;

            // Parse options
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--format" or "-f":
                        if (i + 1 < args.Length)
                        {
                            format = args[++i];
                        }
                        break;
                    case "--output" or "-o":
                        if (i + 1 < args.Length)
                        {
                            outputPath = args[++i];
                        }
                        break;
                    case "--exclude-public":
                        if (i + 1 < args.Length)
                        {
                            excludePublic = bool.Parse(args[++i]);
                        }
                        break;
                }
            }

            await ScanAsync(path, format, outputPath, excludePublic);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static void ShowHelp()
    {
        AnsiConsole.Write(new FigletText("Dotnet Unused").Color(Color.Cyan1));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[cyan]Detect unused code in .NET solutions[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Usage:[/]");
        AnsiConsole.MarkupLine("  dotnet-unused [path] [options]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Arguments:[/]");
        AnsiConsole.MarkupLine("  [cyan]path[/]               Path to .sln or .csproj file");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Options:[/]");
        AnsiConsole.MarkupLine("  [cyan]--format, -f[/]       Output format: text or json (default: text)");
        AnsiConsole.MarkupLine("  [cyan]--output, -o[/]       Output file path (only for JSON format)");
        AnsiConsole.MarkupLine("  [cyan]--exclude-public[/]   Exclude public members (default: true)");
        AnsiConsole.MarkupLine("  [cyan]--help, -h[/]         Show help information");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Examples:[/]");
        AnsiConsole.MarkupLine("  dotnet-unused MySolution.sln");
        AnsiConsole.MarkupLine("  dotnet-unused MyProject.csproj --format json --output report.json");
        AnsiConsole.MarkupLine("  dotnet-unused MySolution.sln --exclude-public false");
    }

    private static async Task ScanAsync(string path, string format, string? outputPath, bool excludePublic)
    {
        var stopwatch = Stopwatch.StartNew();

        AnsiConsole.Write(new FigletText("Dotnet Unused").Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        var progress = new Progress<string>(msg => AnsiConsole.MarkupLine($"[grey]{msg}[/]"));

        // Load solution
        var loader = new SolutionLoader();
        var solution = await loader.LoadAsync(path, progress);

        // Index declared symbols
        var indexer = new SymbolIndexer();
        var declaredSymbols = await indexer.CollectDeclaredSymbolsAsync(solution, progress);

        // Find referenced symbols
        var walker = new ReferenceWalker();
        var referencedSymbols = await walker.CollectReferencedSymbolsAsync(solution, progress);

        // Detect unused
        var detector = new UnusedDetector(excludePublic);
        var result = detector.DetectUnused(declaredSymbols, referencedSymbols, progress);

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;

        // Output results
        var reporter = new ReportWriter();

        if (format.ToLowerInvariant() == "json")
        {
            var jsonPath = outputPath ?? "unused-code-report.json";
            await reporter.WriteJsonReportAsync(result, jsonPath);
        }
        else
        {
            reporter.WriteConsoleReport(result);
        }
    }
}
