# Dotnet Unused

A high-performance CLI tool for detecting unused code in .NET solutions and projects using Roslyn static analysis.

## Features

- **Fast Analysis**: Single-pass syntax tree traversal with parallel processing
- **Conservative Detection**: Minimal false positives with smart heuristics
- **Framework Aware**: Recognizes ASP.NET Core, test frameworks, serialization, and DI patterns
- **Multiple Formats**: Console output and JSON export
- **Comprehensive**: Detects unused methods, properties, and fields
- **Broad Compatibility**: Works with both .NET Core/.NET 5+ and .NET Framework projects
- **Generic Method Support**: Correctly handles generic methods and constructed generic types

## Installation

### Build from Source

```bash
cd DotnetUnused
dotnet build -c Release
```

### Create Self-Contained Executable

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# Linux
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true

# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
```

The executable will be in `bin/Release/net8.0/{runtime}/publish/`

## Usage

### Basic Usage

```bash
dotnet run --project DotnetUnused -- path/to/MySolution.sln
```

Or using the built executable:

```bash
DotnetUnused MySolution.sln
```

### Options

- `--format, -f`: Output format (`text` or `json`, default: `text`)
- `--output, -o`: Output file path (for JSON format)
- `--exclude-public`: Exclude public members from detection (default: `true`)
- `--help, -h`: Show help information

### Examples

```bash
# Analyze a .NET solution with console output
DotnetUnused MySolution.sln

# Analyze a .NET Framework solution
DotnetUnused LegacySolution.sln

# Generate JSON report
DotnetUnused MySolution.sln --format json --output report.json

# Include public members in analysis
DotnetUnused MySolution.sln --exclude-public false

# Analyze a single project
DotnetUnused MyProject.csproj
```

## How It Works

### Analysis Pipeline

1. **Load Solution/Project**: Uses MSBuildWorkspace to load the full solution with all dependencies
2. **Index Declared Symbols**: Scans all syntax trees to collect declared methods, properties, and fields
3. **Find References**: Single-pass traversal to find all symbol usages
4. **Compute Unused**: Subtracts referenced symbols from declared symbols
5. **Apply Heuristics**: Filters out false positives using smart rules

### Exclusion Heuristics

The tool automatically excludes:

- **Entry Points**: `Main` methods
- **Public API**: Public members (configurable)
- **Framework Code**:
  - ASP.NET Core controllers and endpoints
  - Test methods (`[Fact]`, `[Test]`, `[TestMethod]`)
  - Serialization members
- **Compiler-Generated**:
  - Interface members
  - Abstract members
  - Overrides
  - Virtual members
  - Extern members
- **Explicitly Marked**: `[UsedImplicitly]`, `[Preserve]` attributes
- **DI Constructors**: Non-private constructors (used by dependency injection)

## Architecture

### Core Components

- **SolutionLoader**: Loads .sln or .csproj files using MSBuildWorkspace
- **SymbolIndexer**: Collects declared symbols with parallel processing
- **ReferenceWalker**: Finds symbol references using CSharpSyntaxWalker
- **UnusedDetector**: Computes unused symbols and applies heuristics
- **ReportWriter**: Formats output (console tables or JSON)

### Performance Optimizations

- Single compilation per project
- No repeated syntax tree traversal
- Parallel project processing
- Concurrent collections for thread-safe symbol tracking
- Efficient SymbolEqualityComparer usage

## Output

### Console Output

```
╭──────────────────────────────────────────╮
│ Unused Code Analysis Results            │
╰──────────────────────────────────────────╯

╭─────────────────────────┬───────╮
│ Metric                  │ Value │
├─────────────────────────┼───────┤
│ Total Symbols Analyzed  │ 1234  │
│ Total References Found  │ 1100  │
│ Unused Symbols          │ 15    │
│ Analysis Duration       │ 3.45s │
╰─────────────────────────┴───────╯

╭──── Unused Methods (12) ─────────────────╮
│ Name                    │ Location        │
├─────────────────────────┼─────────────────┤
│ MyClass.UnusedMethod    │ MyClass.cs:42   │
│ ...                     │ ...             │
╰─────────────────────────┴─────────────────╯
```

### JSON Output

```json
{
  "summary": {
    "totalSymbolsAnalyzed": 1234,
    "totalReferencesFound": 1100,
    "unusedCount": 15,
    "durationSeconds": 3.45
  },
  "unusedSymbols": [
    {
      "kind": "Method",
      "fullyQualifiedName": "MyNamespace.MyClass.UnusedMethod()",
      "filePath": "C:\\Path\\To\\MyClass.cs",
      "lineNumber": 42
    }
  ]
}
```

## Requirements

- .NET 8.0 SDK or higher (for running the tool itself)
- MSBuild (automatically located via Microsoft.Build.Locator)
- **For .NET Framework projects**: Visual Studio installation (provides MSBuild for .NET Framework)

## Compatibility

### Supported Project Types
- ✅ .NET Core 2.0+
- ✅ .NET 5+
- ✅ .NET 6/7/8/9+
- ✅ .NET Framework 4.x (requires Visual Studio MSBuild)
- ✅ SDK-style projects
- ✅ Legacy .csproj format

### Supported C# Features
- ✅ Generic methods and types
- ✅ Extension methods
- ✅ Partial classes
- ✅ Properties (auto-properties and full properties)
- ✅ Fields (instance and static)
- ✅ Methods (regular, async, local functions excluded)
- ✅ Cross-project references
- ✅ Internal accessibility across assemblies

## Limitations

- **Static Analysis Only**: Does not detect runtime or reflection-based usage
- **Conservative Approach**: May miss some unused code to avoid false positives
- **No Incremental Analysis**: Analyzes the entire solution each time

## Contributing

This tool follows the principle: **"Fast, conservative, and trustworthy beats slow and theoretically perfect."**

## License

See LICENSE file for details.

## Acknowledgments

Built with:
- [Roslyn](https://github.com/dotnet/roslyn) - .NET Compiler Platform
- [Spectre.Console](https://spectreconsole.net/) - Beautiful console UI
