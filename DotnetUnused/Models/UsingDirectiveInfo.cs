namespace DotnetUnused.Models;

/// <summary>
/// Represents an unused using directive with its location information
/// </summary>
public sealed class UsingDirectiveInfo
{
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Namespace { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public override string ToString() => $"{Namespace} at {FilePath}:{LineNumber}";
}
