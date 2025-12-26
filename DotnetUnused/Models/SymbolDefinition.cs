using Microsoft.CodeAnalysis;

namespace DotnetUnused.Models;

/// <summary>
/// Represents a declared symbol with its location information
/// </summary>
public sealed class SymbolDefinition
{
    public ISymbol Symbol { get; }
    public string FilePath { get; }
    public int LineNumber { get; }
    public SymbolKind Kind { get; }
    public string FullyQualifiedName { get; }

    public SymbolDefinition(ISymbol symbol, Location location)
    {
        Symbol = symbol;
        var lineSpan = location.GetLineSpan();
        FilePath = lineSpan.Path;
        LineNumber = lineSpan.StartLinePosition.Line + 1; // Convert to 1-based
        Kind = symbol.Kind;
        FullyQualifiedName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public override string ToString() => $"{Kind} {FullyQualifiedName} at {FilePath}:{LineNumber}";
}
