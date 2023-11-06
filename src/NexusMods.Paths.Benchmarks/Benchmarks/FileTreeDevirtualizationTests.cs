using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using NexusMods.Paths.Benchmarks.Interfaces;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Benchmarks.Benchmarks;

[BenchmarkInfo("Tree Devirtualization Test", "Performance comparisons of virtualized and regular methods. One day hopefully, there will be no overhead.")]
[DisassemblyDiagnoser]
public class FileTreeDevirtualizationTests : IBenchmark
{
    private TestTree item = new TestTree().GenerateChildren(3);

    [Benchmark]
    public int CountChildren_ViaInterface()
    {
        return item.CountChildren<TestTree, RelativePath>();
    }

    [Benchmark]
    public int CountChildren_This()
    {
        return item.CountChildren();
    }
}

public struct TestTree : IHaveChildrenWithKey<RelativePath, TestTree>
{
    public TestTree() { }

    public Dictionary<RelativePath, ChildrenWithKeyBox<RelativePath, TestTree>> Children { get; } = new();

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

    public TestTree GenerateChildren(int depth, int itemsPerNode = 100)
    {
        if (depth <= 0)
            return this;

        for (var x = 0; x < itemsPerNode; x++)
        {
            var path = new RelativePath($"Child_{x}");
            Children[path] = new TestTree().GenerateChildren(depth - 1);
        }

        return this;
    }
}
