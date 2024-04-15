using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using NexusMods.Paths.Benchmarks.Interfaces;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Benchmarks.Benchmarks;

[BenchmarkInfo("Tree Devirtualization Test", "Performance comparisons of virtualized and regular methods. One day hopefully, there will be no overhead.")]
[DisassemblyDiagnoser]
public class FileTreeDevirtualizationTests : IBenchmark
{
    private TestTree _item = new TestTree().GenerateChildren(3);

    [Benchmark]
    public int CountChildren_ViaInterface() => _item.CountChildren<TestTree, RelativePath>();

    [Benchmark]
    public int CountChildren_This() => _item.CountChildren();

    [Benchmark]
    public int CountChildrenDepth_ViaInterface() => _item.SumChildrenDepth<TestTree, RelativePath>();

    [Benchmark]
    public int CountChildrenDepth_This() => _item.SumChildrenDepth();
}

public struct TestTree : IHaveBoxedChildrenWithKey<RelativePath, TestTree>, IHaveDepthInformation
{
    public TestTree() { }

    public Dictionary<RelativePath, KeyedBox<RelativePath, TestTree>> Children { get; } = new();
    public ushort Depth { get; private set;  }

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CountChildren()
    {
        var result = 0;
        CountChildrenRecursive(this, ref result);
        return result;
    }

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <param name="accumulator">Parameter that counts the running total.</param>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountChildrenRecursive(TestTree item, ref int accumulator)
    {
        accumulator += item.Children.Count;
        foreach (var child in item.Children)
            CountChildrenRecursive(child.Value.Item, ref accumulator);
    }

    /// <summary>
    ///     Test method for combining interfaces.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SumChildrenDepth()
    {
        var result = 0;
        SumChildrenDepthRecursive(ref result);
        return result;
    }

    /// <summary>
    ///     Sums the 'depth' field of all child nodes.
    ///     This is a test for when we combine interfaces with children.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SumChildrenDepthRecursive(ref int accumulator)
    {
        accumulator += Depth;
        foreach (var child in Children)
            child.Value.Item.SumChildrenDepthRecursive(ref accumulator);
    }

    public TestTree GenerateChildren(int depth, int itemsPerNode = 100)
    {
        if (depth <= 0)
            return this;

        for (var x = 0; x < itemsPerNode; x++)
        {
            var path = new RelativePath($"Child_{x}");
            Children[path] = (KeyedBox<RelativePath, TestTree>) new TestTree().GenerateChildren(depth - 1);
            Depth = (ushort)x;
        }

        return this;
    }
}

/// <summary>
///     Test for combining traits.
/// </summary>
// ReSharper disable once InconsistentNaming
internal static class IHaveChildrenWithKeyPrivateExtensions
{
    /// <summary>
    ///     Sums the 'depth' field of all child nodes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SumChildrenDepth<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveDepthInformation
        where TKey : notnull
    {
        var result = 0;
        item.SumChildrenDepthRecursive<TSelf, TKey>(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SumChildrenDepthRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveDepthInformation where TKey : notnull
    {
        accumulator += item.Depth;
        foreach (var child in item.Children)
            child.Value.Item.SumChildrenDepthRecursive<TSelf, TKey>(ref accumulator);
    }
}
