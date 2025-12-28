using System.Diagnostics;
using DotnetUnused.Core;
using DotnetUnused.Reporting;
using Spectre.Console;

namespace DotnetUnused;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            AnsiConsole.MarkupLine("[yellow]Cancellation requested. Cleaning up...[/]");
        };

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
                            if (!bool.TryParse(args[++i], out excludePublic))
                            {
                                AnsiConsole.MarkupLine("[red]Invalid value for --exclude-public. Expected 'true' or 'false'.[/]");
                                return 1;
                            }
                        }
                        break;
                }
            }

            await ScanAsync(path, format, outputPath, excludePublic, cts.Token);
            return 0;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Operation cancelled by user.[/]");
            return 130; // Standard exit code for SIGINT
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

    private static async Task ScanAsync(string path, string format, string? outputPath, bool excludePublic, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        AnsiConsole.Write(new FigletText("Dotnet Unused").Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        var progress = new Progress<string>(msg => AnsiConsole.MarkupLine($"[grey]{msg}[/]"));

        // Load solution
        cancellationToken.ThrowIfCancellationRequested();
        var loader = new SolutionLoader();
        var solution = await loader.LoadAsync(path, progress, cancellationToken);

        // Index declared symbols
        cancellationToken.ThrowIfCancellationRequested();
        var indexer = new SymbolIndexer();
        var declaredSymbols = await indexer.CollectDeclaredSymbolsAsync(solution, progress, cancellationToken);

        // Find referenced symbols
        cancellationToken.ThrowIfCancellationRequested();
        var walker = new ReferenceWalker();
        var referencedSymbols = await walker.CollectReferencedSymbolsAsync(solution, progress, cancellationToken);

        // Detect unused
        cancellationToken.ThrowIfCancellationRequested();
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
