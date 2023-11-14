using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NexusMods.Paths.HighPerformance.CommunityToolkit;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by FileTree implementations to indicate that they have a parent.
/// </summary>
/// <typeparam name="TSelf">The concrete type stored inside this interface.</typeparam>
public interface IHaveParent<TSelf> where TSelf : struct, IHaveParent<TSelf>
{
    /// <summary>
    ///     Returns the parent node if it exists. If not, the node is considered the root node.
    /// </summary>
    public Box<TSelf>? Parent { get; }

    /// <summary>
    ///     Returns true if the tree has a parent.
    /// </summary>
    bool HasParent => Parent != null;

    /// <summary>
    ///     Returns true if this is the root of the tree.
    /// </summary>
    bool IsTreeRoot => !HasParent;
}

/// <summary>
///     Trait methods for <see cref="IHaveBoxedChildren{TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveParentExtensionsForIHaveBoxedChildren
{
    /// <summary>
    ///      Returns the total number of siblings in this node.
    /// </summary>
    /// <param name="item">The 'this' item.</param>
    /// <returns>The total amount of siblings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingCount<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>
        => item.Item.GetSiblingCount();

    /// <summary>
    ///      Returns the total number of siblings in this node.
    /// </summary>
    /// <param name="item">The 'this' item.</param>
    /// <returns>The total amount of siblings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingCount<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>
    {
        var parent = item.Parent;
        if (parent != null) // <= do not invert branch, hot path
            return parent.Item.Children.Length - 1; // -1 to exclude self.

        return 0;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box<TSelf>[] GetSiblings<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var result = GC.AllocateUninitializedArray<Box<TSelf>>(item.GetSiblingCount());
        GetSiblingsUnsafe(item, result);
        return result;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box<TSelf>[] GetSiblings<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var result = GC.AllocateUninitializedArray<Box<TSelf>>(item.GetSiblingCount());
        GetSiblingsUnsafe(item, result);
        return result;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <param name="resultsBuf">
    ///     The buffer which holds the results.
    ///     Please use <see cref="GetSiblingCount{TSelf}(Box{TSelf})"/> to obtain the required size.
    /// </param>
    /// <returns>The amount of siblings inserted into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingsUnsafe<TSelf>(this Box<TSelf> item, Span<Box<TSelf>> resultsBuf)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        // Note: While this code is mostly duplicated from other overload, it is not the same.
        //       This compares reference equality, other compares value equality.
        var parent = item.Item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            var writeIndex = 0;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Equals(item))
                    resultsBuf.DangerousGetReferenceAt(writeIndex++) = child;
            }

            return item.GetSiblingCount();
        }

        return 0;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <param name="resultsBuf">
    ///     The buffer which holds the results.
    ///     Please use <see cref="GetSiblingCount{TSelf}(Box{TSelf})"/> to obtain the required size.
    /// </param>
    /// <returns>The amount of siblings inserted into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingsUnsafe<TSelf>(this TSelf item, Span<Box<TSelf>> resultsBuf)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var parent = item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            var writeIndex = 0;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Item.Equals(item))
                    resultsBuf.DangerousGetReferenceAt(writeIndex++) = child;
            }

            return item.GetSiblingCount();
        }

        return 0;
    }

    /// <summary>
    ///      Enumerates all of the siblings of this node.
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Box<TSelf>> EnumerateSiblings<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        // Note: While this code is mostly duplicated from other overload, it is not the same.
        //       This compares reference equality, other compares value equality.
        var parent = item.Item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Equals(item))
                    yield return child;
            }
        }
    }

    /// <summary>
    ///      Enumerates all of the siblings of this node.
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Box<TSelf>> EnumerateSiblings<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var parent = item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Item.Equals(item))
                    yield return child;
            }
        }
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveObservableChildren{TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveParentExtensionsForIHaveObservableChildren
{
    /// <summary>
    ///      Returns the total number of siblings in this node.
    /// </summary>
    /// <param name="item">The 'this' item.</param>
    /// <returns>The total amount of siblings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingCount<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>
        => item.Item.GetSiblingCount();

    /// <summary>
    ///      Returns the total number of siblings in this node.
    /// </summary>
    /// <param name="item">The 'this' item.</param>
    /// <returns>The total amount of siblings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingCount<TSelf>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>
    {
        var parent = item.Parent;
        if (parent != null) // <= do not invert branch, hot path
            return parent.Item.Children.Count - 1; // -1 to exclude self.

        return 0;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box<TSelf>[] GetSiblings<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var result = GC.AllocateUninitializedArray<Box<TSelf>>(item.GetSiblingCount());
        GetSiblingsUnsafe(item, result);
        return result;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box<TSelf>[] GetSiblings<TSelf>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var result = GC.AllocateUninitializedArray<Box<TSelf>>(item.GetSiblingCount());
        GetSiblingsUnsafe(item, result);
        return result;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <param name="resultsBuf">
    ///     The buffer which holds the results.
    ///     Please use <see cref="GetSiblingCount{TSelf}(Box{TSelf})"/> to obtain the required size.
    /// </param>
    /// <returns>The amount of siblings inserted into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingsUnsafe<TSelf>(this Box<TSelf> item, Span<Box<TSelf>> resultsBuf)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        // Note: While this code is mostly duplicated from other overload, it is not the same.
        //       This compares reference equality, other compares value equality.
        var parent = item.Item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            var writeIndex = 0;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Equals(item))
                    resultsBuf.DangerousGetReferenceAt(writeIndex++) = child;
            }

            return item.GetSiblingCount();
        }

        return 0;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <param name="resultsBuf">
    ///     The buffer which holds the results.
    ///     Please use <see cref="GetSiblingCount{TSelf}(Box{TSelf})"/> to obtain the required size.
    /// </param>
    /// <returns>The amount of siblings inserted into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingsUnsafe<TSelf>(this TSelf item, Span<Box<TSelf>> resultsBuf)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var parent = item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            var writeIndex = 0;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Item.Equals(item))
                    resultsBuf.DangerousGetReferenceAt(writeIndex++) = child;
            }

            return item.GetSiblingCount();
        }

        return 0;
    }

    /// <summary>
    ///      Enumerates all of the siblings of this node.
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Box<TSelf>> EnumerateSiblings<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        // Note: While this code is mostly duplicated from other overload, it is not the same.
        //       This compares reference equality, other compares value equality.
        var parent = item.Item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Equals(item))
                    yield return child;
            }
        }
    }

    /// <summary>
    ///      Enumerates all of the siblings of this node.
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Box<TSelf>> EnumerateSiblings<TSelf>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveParent<TSelf>, IEquatable<TSelf>
    {
        var parent = item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Item.Equals(item))
                    yield return child;
            }
        }
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveBoxedChildrenWithKey{TKey,TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveParentExtensionsForIHaveBoxedChildrenWithKey
{
    /// <summary>
    ///      Returns the total number of siblings in this node.
    /// </summary>
    /// <param name="item">The 'this' item.</param>
    /// <returns>The total amount of siblings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingCount<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf> where TKey : notnull
        => item.Item.GetSiblingCount<TSelf, TKey>();

    /// <summary>
    ///      Returns the total number of siblings in this node.
    /// </summary>
    /// <param name="item">The 'this' item.</param>
    /// <returns>The total amount of siblings.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingCount<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf> where TKey : notnull
    {
        var parent = item.Parent;
        if (parent != null) // <= do not invert branch, hot path
            return parent.Item.Children.Count - 1; // -1 to exclude self.

        return 0;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ChildWithKeyBox<TKey, TSelf>[] GetSiblings<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf>, IEquatable<TSelf> where TKey : notnull
    {
        var count = item.GetSiblingCount<TSelf, TKey>();
        var result = GC.AllocateUninitializedArray<ChildWithKeyBox<TKey, TSelf>>(count);
        GetSiblingsUnsafe<TSelf, TKey>(item, result);
        return result;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <param name="resultsBuf">
    ///     The buffer which holds the results.
    ///     Please use <see cref="GetSiblingCount{TSelf,TKey}(TSelf)"/> to obtain the required size.
    /// </param>
    /// <returns>The amount of siblings inserted into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingsUnsafe<TSelf, TKey>(this TSelf item, Span<ChildWithKeyBox<TKey, TSelf>> resultsBuf)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf>, IEquatable<TSelf> where TKey : notnull
    {
        var parent = item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            var writeIndex = 0;
            foreach (var child in parentChildren)
            {
                if (!child.Value.Item.Equals(item))
                    resultsBuf.DangerousGetReferenceAt(writeIndex++) = child.Value;
            }

            return item.GetSiblingCount<TSelf, TKey>();
        }

        return 0;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ChildWithKeyBox<TKey, TSelf>[] GetSiblings<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf>, IEquatable<TSelf> where TKey : notnull
    {
        var count = item.GetSiblingCount();
        var result = GC.AllocateUninitializedArray<ChildWithKeyBox<TKey, TSelf>>(count);
        GetSiblingsUnsafe(item, result);
        return result;
    }

    /// <summary>
    ///      Returns all of the siblings of this node (excluding itself).
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <param name="resultsBuf">
    ///     The buffer which holds the results.
    ///     Please use <see cref="GetSiblingCount{TSelf,TKey}(TSelf)"/> to obtain the required size.
    /// </param>
    /// <returns>The amount of siblings inserted into the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSiblingsUnsafe<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item, Span<ChildWithKeyBox<TKey, TSelf>> resultsBuf)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf>, IEquatable<TSelf> where TKey : notnull
    {
        var parent = item.Item.Parent;

        // Note: While this code is mostly duplicated from other overload, it is not the same.
        //       This compares reference equality, other compares value equality.
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            var writeIndex = 0;
            foreach (var child in parentChildren)
            {
                if (!child.Value.Equals(item))
                    resultsBuf.DangerousGetReferenceAt(writeIndex++) = child.Value;
            }

            return item.GetSiblingCount();
        }

        return 0;
    }

    /// <summary>
    ///      Enumerates all of the siblings of this node.
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ChildWithKeyBox<TKey, TSelf>> EnumerateSiblings<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf>, IEquatable<TSelf> where TKey : notnull
    {
        var parent = item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Value.Item.Equals(item))
                    yield return child.Value;
            }
        }
    }

    /// <summary>
    ///      Enumerates all of the siblings of this node.
    /// </summary>
    /// <param name="item">The item whose siblings to obtain.</param>
    /// <returns>All of the siblings of this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<ChildWithKeyBox<TKey, TSelf>> EnumerateSiblings<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveParent<TSelf>, IEquatable<TSelf> where TKey : notnull
    {
        var parent = item.Item.Parent;
        // ReSharper disable once InvertIf
        if (parent != null) // <= do not invert, hot path.
        {
            var parentChildren = parent.Item.Children;
            foreach (var child in parentChildren) // <= lowered to 'for'
            {
                if (!child.Value.Equals(item))
                    yield return child.Value;
            }
        }
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveParent{TSelf}"/>.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveParentExtensions
{
    /// <summary>
    ///      Returns true if the tree has a parent.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasParent<TSelf>(this TSelf item)
        where TSelf : struct, IHaveParent<TSelf>
        => item.HasParent;

    /// <summary>
    ///      Returns true if this is the root of the tree.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTreeRoot<TSelf>(this TSelf item)
        where TSelf : struct, IHaveParent<TSelf>
        => item.IsTreeRoot;

    /// <summary>
    ///     Finds a node by traversing up the parent nodes, matching keys in reverse order.
    /// </summary>
    /// <param name="node">The starting node for the search.</param>
    /// <param name="keys">The sequence of keys to match, in reverse order.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>The node that matches the sequence of keys from end to start, or null if no match is found.</returns>
    public static TSelf? FindByKeysUpward<TSelf, TKey>(this Box<TSelf> node, Span<TKey> keys)
        where TSelf : struct, IHaveParent<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => node.Item.FindByKeysUpward(keys);

    /// <summary>
    ///     Finds a node by traversing up the parent nodes, matching keys in reverse order.
    /// </summary>
    /// <param name="node">The starting node for the search.</param>
    /// <param name="keys">The sequence of keys to match, in reverse order.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>The node that matches the sequence of keys from end to start, or null if no match is found.</returns>
    [ExcludeFromCodeCoverage]
    public static TSelf? FindByKeysUpward<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> node, Span<TKey> keys)
        where TSelf : struct, IHaveParent<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => node.Item.FindByKeysUpward(keys);

    /// <summary>
    ///     Verifies the path of this node against a Span of keys (inverse FindByKey).
    /// </summary>
    /// <param name="node">The starting node for the search.</param>
    /// <param name="keys">The sequence of keys to match, in reverse order.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>The node that matches the sequence of keys from end to start, or null if no match is found.</returns>
    public static TSelf? FindByKeysUpward<TSelf, TKey>(this TSelf node, Span<TKey> keys)
        where TSelf : struct, IHaveParent<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => keys.Length == 0 ? null : node.FindByKeysUpwardWithNonZeroKey(keys);

    internal static TSelf? FindByKeysUpwardWithNonZeroKey<TSelf, TKey>(this TSelf node, Span<TKey> keys)
        where TSelf : struct, IHaveParent<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var keyIndex = keys.Length - 1;

        // Traverse upwards until you either exhaust keys or have no parent (root node)
        var currentNode = node;
        while (keyIndex >= 0)
        {
            if (!EqualityComparer<TKey>.Default.Equals(currentNode.Key, keys.DangerousGetReferenceAt(keyIndex)))
                return null;

            // Move to the parent node if not at the root
            keyIndex--;
            if (currentNode.HasParent)
                currentNode = currentNode.Parent!.Item;
            else
                break;
        }

        // Check if all keys have been matched
        return keyIndex < 0 ? node : null;
    }
}
