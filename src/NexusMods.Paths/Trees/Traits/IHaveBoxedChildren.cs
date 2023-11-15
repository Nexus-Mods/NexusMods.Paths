using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NexusMods.Paths.HighPerformance.CommunityToolkit;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by Tree implementations to indicate that they have a child.
/// </summary>
/// <typeparam name="TSelf">The type of the child stored in this FileTree.</typeparam>
public interface IHaveBoxedChildren<TSelf> where TSelf : struct, IHaveBoxedChildren<TSelf>
{
    /// <summary>
    ///     An array containing all the children of this node.
    /// </summary>
    public Box<TSelf>[] Children { get; }
}

/// <summary>
///     Trait methods for <see cref="IHaveBoxedChildren{TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveBoxedChildrenExtensions
{
    /// <summary>
    ///     True if the current node is a leaf (it has no children).
    /// </summary>
    /// <param name="item">The node to check.</param>
    public static bool IsLeaf<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.IsLeaf();

    /// <summary>
    ///     True if the current node is a leaf (it has no children).
    /// </summary>
    /// <param name="item">The node to check.</param>
    public static bool IsLeaf<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Children.Length == 0;

    /// <summary>
    ///     Enumerates all child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<Box<TSelf>> EnumerateChildrenBfs<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.EnumerateChildrenBfs();

    /// <summary>
    ///     Enumerates all child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<Box<TSelf>> EnumerateChildrenBfs<TSelf>(this TSelf item) where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        // Return the current item's immediate children first.
        foreach (var child in item.Children)
            yield return child;

        // Then return the children of those children.
        foreach (var child in item.Children)
        foreach (var grandChild in child.Item.EnumerateChildrenBfs())
            yield return grandChild;
    }

    /// <summary>
    ///     Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<Box<TSelf>> EnumerateChildrenDfs<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.EnumerateChildrenDfs();

    /// <summary>
    ///     Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<Box<TSelf>> EnumerateChildrenDfs<TSelf>(this TSelf item) where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        foreach (var child in item.Children)
        {
            yield return child;
            foreach (var grandChild in child.Item.EnumerateChildrenDfs())
                yield return grandChild;
        }
    }

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf>(this Box<TSelf> item) where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.CountChildren();

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf>(this TSelf item) where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        var result = 0;
        item.CountChildrenRecursive(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountChildrenRecursive<TSelf>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        accumulator += item.Children.Length;
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
            child.Item.CountChildrenRecursive(ref accumulator);
    }

    /// <summary>
    ///     Recursively returns all the children of this node.
    /// </summary>
    /// <param name="item">The node whose children to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all the children of this node.</returns>
    public static Box<TSelf>[] GetChildrenRecursive<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf> => item.Item.GetChildrenRecursive();

    /// <summary>
    ///     Recursively returns all the children of this node.
    /// </summary>
    /// <param name="item">The node whose children to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all the children of this node.</returns>
    public static Box<TSelf>[] GetChildrenRecursive<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        var totalChildren = item.CountChildren();
        var children = GC.AllocateUninitializedArray<Box<TSelf>>(totalChildren);
        var index = 0;
        GetChildrenRecursiveUnsafe(item, children, ref index);
        return children;
    }

    /// <summary>
    ///     Recursively returns all the children of this node (unsafe / no bounds checks).
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="childrenSpan">
    ///     The span representing the array to fill with children.
    ///     Should be at least as long as value returned by <see cref="CountChildren{TSelf}(Box{TSelf})"/>
    /// </param>
    /// <param name="index">The current index in the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetChildrenRecursiveUnsafe<TSelf>(this TSelf item, Span<Box<TSelf>> childrenSpan, ref int index)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
            childrenSpan.DangerousGetReferenceAt(index++) = child;

        foreach (var child in item.Children)
            GetChildrenRecursiveUnsafe(child.Item, childrenSpan, ref index);
    }

    /// <summary>
    ///     Counts the number of leaf nodes (nodes with no children) of the current node.
    /// </summary>
    /// <param name="item">The node whose leaf nodes are to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of leaf nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLeaves<TSelf>(this Box<TSelf> item) where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.CountLeaves();

    /// <summary>
    ///     Counts the number of leaf nodes (nodes with no children) of the current node.
    /// </summary>
    /// <param name="item">The node whose leaf nodes are to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of leaf nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLeaves<TSelf>(this TSelf item) where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        var leafCount = 0;
        CountLeavesRecursive(item, ref leafCount);
        return leafCount;
    }

    private static void CountLeavesRecursive<TSelf>(TSelf item, ref int leafCount)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        foreach (var child in item.Children)
        {
            if (child.IsLeaf())
                leafCount++;
            else
                CountLeavesRecursive(child.Item, ref leafCount);
        }
    }

    /// <summary>
    ///     Recursively returns all leaf nodes of this node.
    /// </summary>
    /// <param name="item">The node whose leaf nodes to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all leaf nodes of this node.</returns>
    public static Box<TSelf>[] GetLeaves<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf> => item.Item.GetLeaves();

    /// <summary>
    ///     Recursively returns all leaf nodes of this node.
    /// </summary>
    /// <param name="item">The node whose leaf nodes to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all leaf nodes of this node.</returns>
    public static Box<TSelf>[] GetLeaves<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        var totalLeaves = item.CountLeaves();
        var leaves = GC.AllocateUninitializedArray<Box<TSelf>>(totalLeaves);
        var index = 0;
        GetLeavesUnsafe(item, leaves, ref index);
        return leaves;
    }

    /// <summary>
    ///     Helper method to populate leaf nodes recursively (unsafe / no bounds checks).
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="leavesSpan">
    ///     The span to fill with leaf nodes.
    ///     Should be at least as long as value returned by a hypothetical <see cref="CountLeaves{TSelf}(Box{TSelf})"/> method.
    /// </param>
    /// <param name="index">The current index in the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetLeavesUnsafe<TSelf>(this TSelf item, Span<Box<TSelf>> leavesSpan, ref int index)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        foreach (var child in item.Children)
        {
            if (child.IsLeaf())
                leavesSpan.DangerousGetReferenceAt(index++) = child;
            else
                GetLeavesUnsafe(child.Item, leavesSpan, ref index);
        }
    }
}
