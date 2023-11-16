using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Reloaded.Memory.Extensions;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by Tree implementations to indicate that they have a keyed child.
/// </summary>
/// <typeparam name="TKey">The name of the key used in the File Tree.</typeparam>
/// <typeparam name="TSelf">The type of the child stored in this FileTree.</typeparam>
public interface IHaveBoxedChildrenWithKey<TKey, TSelf>
    where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    /// <summary>
    ///     A Dictionary containing all the children of this node.
    /// </summary>
    /// <remarks>
    ///     This should point to an empty dictionary if there are no items.
    /// </remarks>
    public Dictionary<TKey, KeyedBox<TKey, TSelf>> Children { get; }
}

/// <summary>
///     Trait methods for <see cref="IHaveBoxedChildrenWithKey{TKey,TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveBoxedChildrenWithKeyExtensions
{
    /// <summary>
    ///     True if the current node is a leaf (it has no children).
    /// </summary>
    /// <param name="item">The node to check.</param>
    public static bool IsLeaf<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Item.IsLeaf<TSelf, TKey>();

    /// <summary>
    ///     True if the current node is a leaf (it has no children).
    /// </summary>
    /// <param name="item">The node to check.</param>
    public static bool IsLeaf<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Children.Count == 0;

    /// <summary>
    /// Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<KeyValuePair<TKey, KeyedBox<TKey, TSelf>>> EnumerateChildrenDfs<TSelf, TKey>(
        this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Item.EnumerateChildrenDfs<TSelf, TKey>();

    /// <summary>
    /// Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<KeyValuePair<TKey, KeyedBox<TKey, TSelf>>> EnumerateChildrenDfs<TSelf, TKey>(
        this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        foreach (var child in item.Children)
        {
            yield return child;
            foreach (var grandChild in child.Value.Item.EnumerateChildrenDfs<TSelf, TKey>())
            {
                yield return grandChild;
            }
        }
    }

    /// <summary>
    /// Enumerates all child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<KeyValuePair<TKey, KeyedBox<TKey, TSelf>>> EnumerateChildrenBfs<TSelf, TKey>(
        this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Item.EnumerateChildrenBfs<TSelf, TKey>();

    /// <summary>
    /// Enumerates all child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<KeyValuePair<TKey, KeyedBox<TKey, TSelf>>> EnumerateChildrenBfs<TSelf, TKey>(
        this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        // Return the current item's immediate children first.
        foreach (var child in item.Children)
            yield return child;

        // Then return the children of those children.
        foreach (var child in item.Children)
        foreach (var grandChild in child.Value.Item.EnumerateChildrenBfs<TSelf, TKey>())
            yield return grandChild;
    }

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Item.CountChildren<TSelf, TKey>();

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        var result = 0;
        item.CountChildrenRecursive<TSelf, TKey>(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountChildrenRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf> where TKey : notnull
    {
        accumulator += item.Children.Count;
        foreach (var child in item.Children)
            child.Value.Item.CountChildrenRecursive<TSelf, TKey>(ref accumulator);
    }

    /// <summary>
    ///     Recursively returns all the children of this node.
    /// </summary>
    /// <param name="item">The node whose children to obtain.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all the children of this node.</returns>
    public static KeyedBox<TKey, TSelf>[] GetChildrenRecursive<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull => item.Item.GetChildrenRecursive<TSelf, TKey>();

    /// <summary>
    ///     Recursively returns all the children of this node.
    /// </summary>
    /// <param name="item">The node whose children to obtain.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all the children of this node.</returns>
    public static KeyedBox<TKey, TSelf>[] GetChildrenRecursive<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        var totalChildren = item.CountChildren<TSelf, TKey>();
        var children = GC.AllocateUninitializedArray<KeyedBox<TKey, TSelf>>(totalChildren);
        var index = 0;
        GetChildrenRecursiveUnsafe<TSelf, TKey>(item, children, ref index);
        return children;
    }

    /// <summary>
    ///     Recursively returns all the children of this node (unsafe / no bounds checks).
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="childrenSpan">
    ///     The span representing the array to fill with children.
    ///     Should be at least as long as value returned by <see cref="CountChildren{TSelf,TKey}(TSelf)"/>
    /// </param>
    /// <param name="index">The current index in the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetChildrenRecursiveUnsafe<TSelf, TKey>(TSelf item, Span<KeyedBox<TKey, TSelf>> childrenSpan, ref int index)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        foreach (var child in item.Children)
        {
            childrenSpan.DangerousGetReferenceAt(index++) = child.Value;
            GetChildrenRecursiveUnsafe(child.Value.Item, childrenSpan, ref index);
        }
    }

    /// <summary>
    ///     Counts the number of leaf nodes (nodes with no children) of the current node.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <param name="item">The boxed node whose leaf nodes are to be counted.</param>
    /// <returns>The count of leaf nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLeaves<TKey, TSelf>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Item.CountLeaves<TKey, TSelf>();

    /// <summary>
    ///     Counts the number of leaf nodes (nodes with no children) under the current node.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <param name="item">The node whose leaf nodes are to be counted.</param>
    /// <returns>The total count of leaf nodes under the current node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLeaves<TKey, TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        var leafCount = 0;
        CountLeavesRecursive<TKey, TSelf>(item, ref leafCount);
        return leafCount;
    }

    private static void CountLeavesRecursive<TKey, TSelf>(TSelf item, ref int leafCount)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        foreach (var pair in item.Children)
        {
            if (pair.Value.IsLeaf())
                leafCount++;
            else
                CountLeavesRecursive<TKey, TSelf>(pair.Value.Item, ref leafCount);
        }
    }

    /// <summary>
    ///     Recursively finds and returns all leaf nodes under the current node.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <param name="item">The boxed node whose leaf nodes are to be found.</param>
    /// <returns>An array of all leaf nodes under the current node.</returns>
    public static KeyedBox<TKey, TSelf>[] GetLeaves<TKey, TSelf>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
        => item.Item.GetLeaves<TKey, TSelf>();

    /// <summary>
    ///     Recursively finds and returns all leaf nodes under the current node.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <param name="item">The node whose leaf nodes are to be found.</param>
    /// <returns>An array of all leaf nodes under the current node.</returns>
    public static KeyedBox<TKey, TSelf>[] GetLeaves<TKey, TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        var totalLeaves = item.CountLeaves<TKey, TSelf>();
        var leaves = GC.AllocateUninitializedArray<KeyedBox<TKey, TSelf>>(totalLeaves);
        var index = 0;
        GetLeavesUnsafe<TKey, TSelf>(item, leaves, ref index);
        return leaves;
    }

    /// <summary>
    ///     Helper method to populate leaf nodes recursively without bounds checking.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <param name="item">The current node to find leaf nodes from.</param>
    /// <param name="leavesSpan">The span to fill with leaf nodes.</param>
    /// <param name="index">Current index in the span, used internally for recursion.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetLeavesUnsafe<TKey, TSelf>(this TSelf item, Span<KeyedBox<TKey, TSelf>> leavesSpan, ref int index)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        foreach (var pair in item.Children)
        {
            if (pair.Value.IsLeaf())
                leavesSpan.DangerousGetReferenceAt(index++) = pair.Value;
            else
                GetLeavesUnsafe(pair.Value.Item, leavesSpan, ref index);
        }
    }
}
