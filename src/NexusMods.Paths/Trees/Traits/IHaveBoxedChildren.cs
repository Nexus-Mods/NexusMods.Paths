using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NexusMods.Paths.Trees.Traits.Interfaces;
using Reloaded.Memory.Extensions;

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
    ///     Enumerates all child nodes of the current node in a breadth-first manner that satisfy a given filter.
    /// </summary>
    /// <param name="item">The node, wrapped in a Box, whose children are to be enumerated.</param>
    /// <param name="filter">The filter struct to be applied to each child node.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TFilter">The type of the filter struct.</typeparam>
    /// <returns>An IEnumerable of all filtered child nodes of the current node.</returns>
    [ExcludeFromCodeCoverage] // Wrapper
    public static IEnumerable<Box<TSelf>> EnumerateChildrenFilteredBfs<TSelf, TFilter>(this Box<TSelf> item, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<Box<TSelf>>
        => item.Item.EnumerateChildrenFilteredBfs(filter);

    /// <summary>
    ///     Enumerates all child nodes of the current node in a breadth-first manner that satisfy a given filter.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <param name="filter">The filter struct to be applied to each child node.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TFilter">The type of the filter struct.</typeparam>
    /// <returns>An IEnumerable of all filtered child nodes of the current node.</returns>
    public static IEnumerable<Box<TSelf>> EnumerateChildrenFilteredBfs<TSelf, TFilter>(this TSelf item, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<Box<TSelf>>
    {
        // Return the current item's immediate children first if they match the filter.
        foreach (var child in item.Children)
            if (filter.Match(child))
                yield return child;

        // Then return the filtered children of those children.
        foreach (var child in item.Children)
        foreach (var grandChild in child.Item.EnumerateChildrenFilteredBfs(filter))
            yield return grandChild;
    }

    /// <summary>
    ///     Enumerates all child nodes of the current node in a depth-first manner that satisfy a given filter.
    /// </summary>
    /// <param name="item">The node, wrapped in a Box, whose children are to be enumerated.</param>
    /// <param name="filter">The filter struct to be applied to each child node.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TFilter">The type of the filter struct.</typeparam>
    /// <returns>An IEnumerable of all filtered child nodes of the current node.</returns>
    [ExcludeFromCodeCoverage] // Wrapper
    public static IEnumerable<Box<TSelf>> EnumerateChildrenFilteredDfs<TSelf, TFilter>(this Box<TSelf> item, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<Box<TSelf>>
        => item.Item.EnumerateChildrenFilteredDfs(filter);

    /// <summary>
    ///     Enumerates all child nodes of the current node in a depth-first manner that satisfy a given filter.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <param name="filter">The filter struct to be applied to each child node.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TFilter">The type of the filter struct.</typeparam>
    /// <returns>An IEnumerable of all filtered child nodes of the current node.</returns>
    public static IEnumerable<Box<TSelf>> EnumerateChildrenFilteredDfs<TSelf, TFilter>(this TSelf item, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<Box<TSelf>>
    {
        foreach (var child in item.Children)
        {
            if (filter.Match(child))
                yield return child;

            foreach (var grandChild in child.Item.EnumerateChildrenFilteredDfs(filter))
                yield return grandChild;
        }
    }

    /// <summary>
    ///     Counts the number of children that match the given filter under this node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <param name="filter">The filter to apply to each child.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <returns>The total count of children that match the filter under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ExcludeFromCodeCoverage]
    public static int CountChildren<TSelf, TFilter>(this Box<TSelf> item, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<TSelf>
        => item.Item.CountChildren(filter);

    /// <summary>
    ///     Counts the number of children that match the given filter under this node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <param name="filter">The filter to apply to each child.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <returns>The total count of children that match the filter under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf, TFilter>(this TSelf item, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<TSelf>
    {
        var result = 0;
        item.CountChildrenRecursive(ref result, filter);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountChildrenRecursive<TSelf, TFilter>(this TSelf item, ref int accumulator, TFilter filter)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TFilter : struct, IFilter<TSelf>
    {
        foreach (var child in item.Children)
        {
            var matchesFilter = filter.Match(child.Item);
            accumulator += Unsafe.As<bool, byte>(ref matchesFilter); // Branchless increment.
            child.Item.CountChildrenRecursive(ref accumulator, filter);
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
    ///     Recursively returns all the child items of this node selected by the given selector.
    /// </summary>
    /// <param name="item">The boxed node whose child items to obtain.</param>
    /// <param name="selector">The selector to determine which child items to return.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TSelector">The type of the selector.</typeparam>
    /// <returns>An array of all the selected child items of this node.</returns>
    [ExcludeFromCodeCoverage]
    public static TResult[] GetChildItems<TSelf, TResult, TSelector>(this Box<TSelf> item, TSelector selector)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TSelector : struct, ISelector<TSelf, TResult>
        => item.Item.GetChildItems<TSelf, TResult, TSelector>(selector);

    /// <summary>
    ///     Recursively returns all the child items of this node selected by the given selector.
    /// </summary>
    /// <param name="item">The node whose child items to obtain.</param>
    /// <param name="selector">The selector to determine which child items to return.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TSelector">The type of the selector.</typeparam>
    /// <returns>An array of all the selected child items of this node.</returns>
    public static TResult[] GetChildItems<TSelf, TResult, TSelector>(this TSelf item, TSelector selector)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TSelector : struct, ISelector<TSelf, TResult>
    {
        var totalValues = item.CountChildren(); // Ensure this method counts all descendants.
        var results = GC.AllocateUninitializedArray<TResult>(totalValues);
        var index = 0;
        GetChildItemsUnsafe<TSelf, TResult, TSelector>(item, selector, results, ref index);
        return results;
    }

    /// <summary>
    ///     Helper method to populate child items recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="selector">The selector to determine which child items to return.</param>
    /// <param name="buffer">
    ///     The span to fill with child items.
    ///     Should be at least as big as <see cref="IHaveBoxedChildrenExtensions.CountChildren{TSelf}(Box{TSelf})"/>
    /// </param>
    /// <param name="index">The current index in the array.</param>
    public static void GetChildItemsUnsafe<TSelf, TResult, TSelector>(this TSelf item, TSelector selector, Span<TResult> buffer, ref int index)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        where TSelector : struct, ISelector<TSelf, TResult>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
            buffer.DangerousGetReferenceAt(index++) = selector.Select(child.Item);

        foreach (var child in item.Children)
            GetChildItemsUnsafe(child.Item, selector, buffer, ref index);
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

    /// <summary>
    ///     Retrieves the direct children of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be retrieved.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of the direct children of the current node.</returns>
    [ExcludeFromCodeCoverage] // Wrapper
    public static Box<TSelf>[] Children<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.Children;
}
