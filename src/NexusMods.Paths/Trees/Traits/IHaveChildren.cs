using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by Tree implementations to indicate that they have a child.
/// </summary>
/// <typeparam name="TSelf">The type of the child stored in this FileTree.</typeparam>
public interface IHaveChildren<out TSelf>
{
    /// <summary>
    ///     A Dictionary containing all the children of this node, both files and directories.
    /// </summary>
    public TSelf[] Children { get; }
}

/// <summary>
///     Trait methods for <see cref="IHaveChildren{T}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveChildrenExtensions
{
    /// <summary>
    ///     [DO NOT USE DIRECTLY, COPY THIS CODE INTO RELEVANT FILE TREE IMPLEMENTATION, DUE TO .NET DEVIRTUALIZATION ISSUES]
    ///     Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    /// <remarks>
    ///     Due to .NET devirtualization limitations, this method should not be used directly.
    ///     It's recommended to copy its implementation into the relevant FileTree implementations.
    ///     This code should be source generated in the future.
    /// </remarks>
    public static IEnumerable<TSelf> EnumerateChildren<TSelf>(this TSelf item) where TSelf : IHaveChildren<TSelf>
    {
        foreach (var child in item.Children)
        {
            yield return child;
            foreach (var grandChild in child.EnumerateChildren())
                yield return grandChild;
        }
    }

    /// <summary>
    ///     [DO NOT USE DIRECTLY, COPY THIS CODE INTO RELEVANT FILE TREE IMPLEMENTATION, DUE TO .NET DEVIRTUALIZATION ISSUES]
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    /// <remarks>
    ///     Due to .NET devirtualization limitations, this method should not be used directly.
    ///     It's recommended to copy its implementation into the relevant FileTree implementations.
    ///     This code should be source generated in the future.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf>(this TSelf item) where TSelf : IHaveChildren<TSelf>
    {
        var result = 0;
        item.CountChildrenRecursive(ref result);
        return result;
    }

    /// <summary>
    ///     [DO NOT USE DIRECTLY, COPY THIS CODE INTO RELEVANT FILE TREE IMPLEMENTATION, DUE TO .NET DEVIRTUALIZATION ISSUES]
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <param name="accumulator">Parameter that counts the running total.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    /// <remarks>
    ///     Due to .NET devirtualization limitations, this method should not be used directly.
    ///     It's recommended to copy its implementation into the relevant FileTree implementations.
    ///     This code should be source generated in the future.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountChildrenRecursive<TSelf>(this TSelf item, ref int accumulator)
        where TSelf : IHaveChildren<TSelf>
    {
        accumulator += item.Children.Length;
        foreach (var child in item.Children)
            child.CountChildrenRecursive(ref accumulator);
    }
}

