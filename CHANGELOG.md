# Changelog

All notable changes to DotnetUnused will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.1] - 2025-12-28

### Added
- GitVersion integration for automatic semantic versioning
- Unit test infrastructure with xUnit (85 passing tests)
- FileFilter utility class for shared file filtering logic
- Ctrl+C cancellation support with graceful shutdown
- VS Code extension automated release workflow
- Comprehensive test coverage for FileFilter edge cases
- AnalysisResult model tests
- GitVersion documentation (GitVersion.md)
- VS Code extension publishing documentation (vscode-extension/PUBLISHING.md)
- Test project documentation (DotnetUnused.Tests/README.md)

### Changed
- Improved CLI argument parsing with bool.TryParse
- Refactored SymbolIndexer, ReferenceWalker, and SolutionLoader to use FileFilter
- Enhanced GitHub Actions workflow with GitVersion
- Updated VS Code extension to use terminal output by default
- Synchronized versions between CLI tool and VS Code extension

### Fixed
- Command injection vulnerability in VS Code extension terminal mode
- Temp file cleanup in VS Code extension error paths
- Missing icon reference in VS Code extension
- FileFilter edge cases (whitespace handling, relative paths, exact pattern matching)
- Bool.Parse crash on invalid --exclude-public values

### Security
- Fixed command injection in VS Code extension (replaced string interpolation with ShellExecution)
- Added proper argument escaping for CLI execution

## [1.0.0] - 2025-12-26

### Added
- Initial release of DotnetUnused CLI tool
- Unused method detection
- Unused property detection
- Unused field detection
- Generic method support (correctly handles `Method<T>()` calls)
- Support for .NET Core 2.0+, .NET 5+, and .NET Framework 4.x projects
- Smart heuristics to avoid false positives:
  - Excludes entry points (Main methods)
  - Excludes public API (configurable)
  - Excludes ASP.NET Core controllers and endpoints
  - Excludes test methods (xUnit, NUnit, MSTest)
  - Excludes serialization members
  - Excludes DI constructors
  - Excludes interface/abstract/override/virtual members
  - Excludes members with `[UsedImplicitly]` or `[Preserve]` attributes
- Console output with beautiful tables (Spectre.Console)
- JSON output for CI/CD integration
- Parallel processing for fast analysis
- Single-pass syntax tree traversal
- Cross-project reference support
- Partial class support

### Performance
- Analyzes medium-sized solutions (<500k LOC) in under 10 seconds
- Single compilation per project
- Concurrent symbol indexing

### Compatibility
- Works with SDK-style projects
- Works with legacy .csproj format
- Requires Visual Studio MSBuild for .NET Framework projects

[Unreleased]: https://github.com/kokkerametla/dotnet-unused/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/kokkerametla/dotnet-unused/releases/tag/v1.0.0
