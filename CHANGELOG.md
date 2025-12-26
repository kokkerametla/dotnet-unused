# Changelog

All notable changes to DotnetUnused will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
