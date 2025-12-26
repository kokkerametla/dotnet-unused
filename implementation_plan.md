# Implementation Plan - Dotnet Unused CLI Tool

## Goal
Build a high-performance, standalone CLI tool to detect unused code in .NET solutions.
**Guiding Principle**: "Fast, conservative, and trustworthy beats slow and theoretically perfect."

## Architecture

### 1. Core Components
- **SolutionLoader**: Responsible for loading `.sln` or `.csproj` files using `MSBuildWorkspace`.
    - *Key Requirement*: Must handle cross-project references and ignore generated files.
- **SymbolIndexer**: Scans all documents to find *declared* symbols (Methods, Properties, Fields).
    - *Optimization*: Parallel processing of syntax trees.
- **ReferenceWalker**: Scans all documents to find *referenced* symbols.
    - *Optimization*: Single-pass syntax tree traversal.
- **UnusedDetector**: Computes `Unused = Declared - Referenced`.
    - *Logic*: Applies heuristics to filter out false positives.

### 2. Data Structures
- `SymbolDefinition`: Wraps `ISymbol` with location data (File, Line).
- `AnalysisResult`: Contains the list of unused symbols.

## Implementation Phases

### Phase 1: Foundation & Loading
- [ ] **Project Setup**: Create `DotnetUnused` console app.
- [ ] **Dependencies**: Add `Microsoft.CodeAnalysis.Workspaces.MSBuild`, `Spectre.Console`, `System.CommandLine`.
- [ ] **SolutionLoader**: Implement `LoadAsync` with `MSBuildLocator` registration.
    - *Validation*: Ensure it loads complex solutions without crashing.

### Phase 2: The Analysis Pipeline (Parallel & Fast)
- [ ] **SymbolIndexer**:
    - Walk syntax trees to collect `IMethodSymbol`, `IPropertySymbol`, `IFieldSymbol`.
    - *Filter Early*: Ignore `abstract`, `extern`, `override`, and Interface members.
- [ ] **ReferenceWalker**:
    - Walk syntax trees to find usages (IdentifierName, GenericName, etc.).
    - Use `SemanticModel.GetSymbolInfo`.
    - *Concurrency*: Use `ConcurrentBag` or thread-safe collections.
- [ ] **UnusedDetector**:
    - Implement the subtraction logic.
    - Ensure `SymbolEqualityComparer.Default` is used correctly.

### Phase 3: Heuristics & Safety (The "Trustworthy" Part)
- [ ] **Entry Points**: Exclude `Main` methods.
- [ ] **Public API**: Option to exclude `public` members (default: true?).
- [ ] **Frameworks**:
    - Exclude ASP.NET Core Controllers / Endpoints.
    - Exclude Test methods (`[Fact]`, `[Test]`).
- [ ] **Attributes**: Exclude members with `[UsedImplicitly]`, `[Preserve]`.

### Phase 4: CLI & Reporting
- [ ] **CLI Commands**:
    - `scan <path>`
    - `--format <json|text>`
    - `--exclude-public`
- [ ] **Output**:
    - **Console**: Pretty table using `Spectre.Console`.
    - **JSON**: Machine-readable output for CI.

### Phase 5: Distribution
- [ ] **Publish Scripts**:
    - `dotnet publish -r win-x64 --self-contained -p:PublishSingleFile=true`
    - `dotnet publish -r linux-x64 --self-contained -p:PublishSingleFile=true`
    - `dotnet publish -r osx-x64 --self-contained -p:PublishSingleFile=true`

## Verification Plan
1.  **Test Solution**: Create a solution with:
    -   Class Library (Referenced)
    -   Console App (Entry point)
    -   Unused private methods
    -   Used public methods
    -   Interface implementations
2.  **Manual Verification**: Run the tool against the Test Solution and verify output.
3.  **Performance Test**: Run against a medium-sized open-source repo (if available) to check speed.
