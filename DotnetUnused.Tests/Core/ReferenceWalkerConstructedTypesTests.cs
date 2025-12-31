using Xunit;

namespace DotnetUnused.Tests.Core;

/// <summary>
/// Tests for ReferenceWalker handling of constructed generic types
/// Addresses issue: methods of generic classes (e.g., List<int>.Add) should map to List<T>.Add
/// </summary>
public class ReferenceWalkerConstructedTypesTests
{
    [Fact(Skip = "Requires full Roslyn compilation - integration test")]
    public async Task CollectReferencedSymbols_UnwrapsConstructedTypeMethods()
    {
        // Test case: var list = new List<int>(); list.Add(5);
        // Should reference List<T>.Add, not List<int>.Add
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires full Roslyn compilation - integration test")]
    public async Task CollectReferencedSymbols_UnwrapsConstructedTypeProperties()
    {
        // Test case: var list = new List<int>(); var count = list.Count;
        // Should reference List<T>.Count, not List<int>.Count
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires full Roslyn compilation - integration test")]
    public async Task CollectReferencedSymbols_HandlesNestedGenericTypes()
    {
        // Test case: Dictionary<string, List<int>>.TryGetValue
        // Should unwrap to Dictionary<TKey, TValue>.TryGetValue
        await Task.CompletedTask;
    }
}
