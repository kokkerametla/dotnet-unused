# Dotnet unused CLI tool Requirement

## Role

You are a senior .NET static analysis engineer building a high-performance standalone CLI tool using .NET 8.0+ and Roslyn.

Your task is to design and implement a tool that analyzes an entire C# solution or project and lists unused code with a strong focus on speed, correctness, and low false positives.

## Objective

Build a standalone .NET CLI tool that detects and reports:

- Unused methods
- Unused properties
- Unused fields
- Unused using directives

The tool must analyze whole solutions, not individual files, and return results quickly even on large codebases.

## Non-Goals (Explicitly Out of Scope)

- Runtime analysis
- Reflection-based usage detection
- Dynamic code invocation
- Perfect 100% accuracy

The tool should be conservative and avoid false positives over aggressive deletion suggestions.

## Functional Requirements

### Input

Accept:
- .sln file
- .csproj file

CLI command example:
`dotnet-unused scan path/to/MySolution.sln`

### Output

For each unused item, report:
- Symbol type (method / property / field / using)
- Fully qualified name
- File path
- Line number
- Reason for classification

Output formats:
- Console (human-readable)
- JSON (machine-readable)

## Technical Requirements

### Platform
- .NET 8.0 or higher
- Standalone CLI (not a Roslyn Analyzer package)

### Core Technologies
```csharp
Microsoft.CodeAnalysis
Microsoft.CodeAnalysis.CSharp
Microsoft.CodeAnalysis.Workspaces.MSBuild
Microsoft.Build.Locator
```

## Architectural Requirements

### Solution Loading
- Load full solution or project using MSBuildWorkspace
- Handle:
    - Cross-project references
    - Partial classes
    - Internals across assemblies
- Exclude:
    - bin/, obj/
    - Generated files (*.g.cs, *.Designer.cs)

### Analysis Pipeline (Mandatory)
Implement the following single-pass, high-performance pipeline:
1. Load solution/projects
2. Build Roslyn Compilation objects
3. Index all declared symbols:
    - Methods
    - Properties
    - Fields
    - Using directives
4. Index all referenced symbols across the solution
5. Compute unused symbols:
    - Unused = Declared − Referenced
6. Apply heuristics and filters

### Symbol Declaration Indexing
Collect:
- IMethodSymbol
- IPropertySymbol
- IFieldSymbol

Exclude early:
- Interface members
- Abstract members
- Overrides
- Extern members

Use:
- SymbolEqualityComparer.Default

### Reference Detection
Detect symbol usage via:
- Method invocations
- Member access
- Object creation
- Attributes
- nameof(...)
- Pattern matching
- Event subscriptions

Use:
- SemanticModel.GetSymbolInfo()

**All syntax trees must be traversed once only.**

### Using Directive Analysis
- Perform syntax-level analysis for unused using statements
- Compare declared usings vs namespaces actually referenced

### Heuristics & Safety Rules
Exclude symbols from “unused” if:
- Public API (configurable)
- Entry points (Main)
- ASP.NET Controllers / Minimal API handlers
- Constructors used by dependency injection
- Serialization members
- Event handlers
- Test framework methods
- Members marked with:
    - `[UsedImplicitly]`
    - `[Preserve]`
    - Equivalent custom attributes

Allow:
- Suppression via comments or config file

## Performance Requirements
- Single compilation per project
- No repeated syntax tree traversal
- Parallel processing where safe
- Avoid LINQ in hot paths
- Cache semantic models
- Target performance:
    - Medium-sized solution (<500k LOC): < 10 seconds

## Quality & Reliability
- No crashes on malformed projects
- Clear error messages
- Deterministic output
- Minimal false positives
- Conservative reporting

## Deliverables
- CLI application source code
- Clear internal documentation
- Well-structured modules:
    - SolutionLoader
    - SymbolIndexer
    - ReferenceWalker
    - UnusedDetector
    - ReportWriter
- Example runs and sample output

## Success Criteria
The tool:
- Runs fast on large solutions
- Correctly identifies genuinely unused code
- Avoids obvious false positives
- Produces actionable, trustworthy results

## Guiding Principle
> “Fast, conservative, and trustworthy beats slow and theoretically perfect.”


What users need

Windows/macOS/Linux: nothing
No .NET runtime
No NuGet packages
No MSBuild install (you ship what you need)

How

dotnet publish -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true


You’d publish per OS:

win-x64
linux-x64
osx-x64 / osx-arm64

Pros
Zero friction
Enterprise-friendly
Predictable behavior
Fast startup on repeat runs


Cons
Larger binary (80–150 MB)
Multiple builds per OS
👉 This is what ReSharper / commercial tools effectively do.