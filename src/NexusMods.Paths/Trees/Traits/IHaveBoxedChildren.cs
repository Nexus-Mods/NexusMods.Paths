using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public ChildBox<TSelf>[] Children { get; }
}

/// <summary>
///     A boxed element that implements <see cref="IHaveBoxedChildren{TSelf}" />
/// </summary>
/// <remarks>
///     This is a helper class that boxes a constrained generic structure type.
///     While generic reference types share code (and are thus slower),
///     Generic structures can participate in devirtualization, and thus create
///     zero overhead abstractions.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ChildBox<TSelf> : IEquatable<ChildBox<TSelf>> where TSelf : struct
{
    /// <summary>
    ///     Contains item deriving from <see cref="IHaveBoxedChildren{TSelf}" />
    /// </summary>
    public TSelf Item;

    /// <summary />
    public static implicit operator TSelf(ChildBox<TSelf> box) => box.Item;

    /// <summary />
    public static implicit operator ChildBox<TSelf>(TSelf item) => new() { Item = item };

    #region Autogenerated by R#
    /// <inheritdoc />
    public bool Equals(ChildBox<TSelf>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Item.Equals(other.Item);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ChildBox<TSelf>)obj);
    }

    /// <inheritdoc />
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => Item.GetHashCode();
    #endregion
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
    public static bool IsLeaf<TSelf>(this ChildBox<TSelf> item)
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
    public static IEnumerable<TSelf> EnumerateChildrenBfs<TSelf>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.EnumerateChildrenBfs();

    /// <summary>
    ///     Enumerates all child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<TSelf> EnumerateChildrenBfs<TSelf>(this TSelf item) where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        // Return the current item's immediate children first.
        foreach (var child in item.Children)
            yield return child.Item;

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
    public static IEnumerable<TSelf> EnumerateChildrenDfs<TSelf>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
        => item.Item.EnumerateChildrenDfs();

    /// <summary>
    ///     Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<TSelf> EnumerateChildrenDfs<TSelf>(this TSelf item) where TSelf : struct, IHaveBoxedChildren<TSelf>
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
    public static int CountChildren<TSelf>(this ChildBox<TSelf> item) where TSelf : struct, IHaveBoxedChildren<TSelf>
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
    public static ChildBox<TSelf>[] GetChildrenRecursive<TSelf>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf> => item.Item.GetChildrenRecursive();

    /// <summary>
    ///     Recursively returns all the children of this node.
    /// </summary>
    /// <param name="item">The node whose children to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An array of all the children of this node.</returns>
    public static ChildBox<TSelf>[] GetChildrenRecursive<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        int totalChildren = item.CountChildren();
        var children = GC.AllocateUninitializedArray<ChildBox<TSelf>>(totalChildren);
        int index = 0;
        GetChildrenRecursiveUnsafe(item, children, ref index);
        return children;
    }

    /// <summary>
    ///     Recursively returns all the children of this node (unsafe / no bounds checks).
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="childrenSpan">
    ///     The span representing the array to fill with children.
    ///     Should be at least as long as value returned by <see cref="CountChildren{TSelf}(NexusMods.Paths.Trees.Traits.ChildBox{TSelf})"/>
    /// </param>
    /// <param name="index">The current index in the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetChildrenRecursiveUnsafe<TSelf>(TSelf item, Span<ChildBox<TSelf>> childrenSpan, ref int index)
        where TSelf : struct, IHaveBoxedChildren<TSelf>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
            childrenSpan.DangerousGetReferenceAt(index++) = child;

        foreach (var child in item.Children)
            GetChildrenRecursiveUnsafe(child.Item, childrenSpan, ref index);
    }
}
