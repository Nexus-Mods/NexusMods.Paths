using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NexusMods.Paths.Extensions;
using NexusMods.Paths.HighPerformance.CommunityToolkit;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by Tree implementations to indicate that they have a key inside the node.
/// </summary>
/// <typeparam name="TKey">The type of the key used.</typeparam>
public interface IHaveKey<out TKey>
{
    /// <summary>
    ///     The key of the node.
    /// </summary>
    TKey Key { get; }
}

/// <summary>
///    Extensions for <see cref="IHaveBoxedChildren{TSelf}"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveKeyExtensionsForIHaveBoxedChildren
{
    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        => item.Item.EnumerateKeysBfs<TSelf, TKey>();

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
    {
        // First return all keys of the immediate children
        foreach (var child in item.Children)
            yield return child.Item.Key;

        // Then iterate over the immediate children and recursively enumerate their keys
        foreach (var child in item.Children)
        {
            foreach (var grandChildKey in child.Item.EnumerateKeysBfs<TSelf, TKey>())
                yield return grandChildKey;
        }
    }

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        => item.Item.EnumerateKeysDfs<TSelf, TKey>();

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
    {
        // Enumerate the key of each child and then recursively enumerate the keys of its children
        foreach (var child in item.Children)
        {
            yield return child.Item.Key;
            foreach (var grandChildKey in child.Item.EnumerateKeysDfs<TSelf, TKey>())
                yield return grandChildKey;
        }
    }

    /// <summary>
    ///     Recursively returns all the keys of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child keys to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An array of all the keys of the children of this node.</returns>
    public static TKey[] GetKeys<TSelf, TKey>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        => item.Item.GetKeys<TSelf, TKey>();

    /// <summary>
    ///     Recursively returns all the keys of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child keys to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An array of all the keys of the children of this node.</returns>
    public static TKey[] GetKeys<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
    {
        var totalKeys = item.CountChildren(); // Ensure this method counts all descendants.
        var keys = GC.AllocateUninitializedArray<TKey>(totalKeys);
        var index = 0;
        GetKeysUnsafe<TSelf, TKey>(item, keys, ref index);
        return keys;
    }

    /// <summary>
    ///     Helper method to populate keys recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="buffer">
    ///     The span to fill with keys.
    ///     Should be at least as big as <see cref="IHaveBoxedChildrenExtensions.CountChildren{TSelf}(Box{TSelf})"/>
    /// </param>
    /// <param name="index">The current index in the array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetKeysUnsafe<TSelf, TKey>(TSelf item, Span<TKey> buffer, ref int index)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
            buffer.DangerousGetReferenceAt(index++) = child.Item.Key;

        foreach (var child in item.Children)
            GetKeysUnsafe(child.Item, buffer, ref index);
    }

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the root node.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    public static Box<TSelf>? FindByKeyFromRoot<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        if (keys.IsEmpty || !EqualityComparer<TKey>.Default.Equals(root.Item.Key, keys.DangerousGetReferenceAt(0)))
            return null;

        return keys.Length == 1 ? root : FindNode(root, keys.SliceFast(1));
    }

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the root node.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static Box<TSelf>? FindByKeyFromRoot<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindByKeyFromRoot((Box<TSelf>)root, keys);

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the child nodes.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    public static Box<TSelf>? FindByKeyFromChild<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => keys.Length == 0 ? null : FindNode(root, keys);

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the child nodes.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static Box<TSelf>? FindByKeyFromChild<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindByKeyFromChild((Box<TSelf>)root, keys);

    private static Box<TSelf>? FindNode<TSelf, TKey>(Box<TSelf> node, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        foreach (var key in keys)
        {
            var found = false;
            foreach (var child in node.Item.Children)
            {
                if (!EqualityComparer<TKey>.Default.Equals(child.Item.Key, key))
                    continue;

                node = child;
                found = true;
                break;
            }

            if (!found)
                return null;
        }

        return node;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys, starting
    ///     the search from the child nodes of the given root.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    public static List<Box<TSelf>> FindSubPathsByKeyFromChild<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindSubPathsByKeyFromChild(root.Item, keys);

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys, starting the search
    ///     from the child nodes of the given root.
    /// </summary>
    /// <param name="node">The current node being examined.</param>
    /// <param name="keys">Span of keys representing the path/key segments to search.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    public static List<Box<TSelf>> FindSubPathsByKeyFromChild<TSelf, TKey>(this TSelf node, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var foundNodes = new List<Box<TSelf>>();
        if (keys.Length <= 0)
            return foundNodes;

        foreach (var child in node.Children)
            FindAllSubPathsByKeyRecursive(child, keys, foundNodes);

        return foundNodes;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys,
    ///     including the root node in the search.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    public static List<Box<TSelf>> FindSubPathsByKeyFromRoot<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var foundNodes = new List<Box<TSelf>>();
        if (keys.Length <= 0)
            return foundNodes;

        FindAllSubPathsByKeyRecursive(root, keys, foundNodes);
        return foundNodes;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys,
    ///     including the root node in the search.
    /// </summary>
    /// <param name="root">The current node being examined.</param>
    /// <param name="keys">Span of keys representing the path/key segments to search.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static List<Box<TSelf>> FindSubPathsByKeyFromRoot<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindSubPathsByKeyFromRoot((Box<TSelf>)root, keys);

    private static void FindAllSubPathsByKeyRecursive<TSelf, TKey>(Box<TSelf> node, Span<TKey> keys, List<Box<TSelf>> foundNodes)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var result = FindByKeyFromRoot(node, keys);
        if (result != null)
            foundNodes.Add(result);

        foreach (var child in node.Item.Children)
            FindAllSubPathsByKeyRecursive(child, keys, foundNodes);
    }

    /// <summary>
    ///     Searches for all nodes within a tree that, when traced upwards, match a specified sequence of keys.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys to be matched, starting from the leaf node and moving upwards.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>
    ///     A list of nodes where each node's path to the root (root being node at any depth)
    ///     matches the specified sequence of keys.
    /// </returns>
    public static List<Box<TSelf>> FindSubPathsByKeyUpward<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        var foundNodes = new List<Box<TSelf>>();
        FindSubPathsByKeyUpward(root, keys, foundNodes);
        return foundNodes;
    }

    /// <summary>
    ///     Initiates a search from a given node and finds all descendant nodes that, when traced upwards, match a specified sequence of keys.
    /// </summary>
    /// <param name="root">The starting node for the search.</param>
    /// <param name="keys">The sequence of keys to be matched, starting from each descendant node and moving upwards.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>
    ///     A list of nodes where each node's path to the root (root being node at any depth)
    ///     matches the specified sequence of keys.
    /// </returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static List<Box<TSelf>> FindSubPathsByKeyUpward<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        return FindSubPathsByKeyUpward((Box<TSelf>)root, keys);
    }

    private static void FindSubPathsByKeyUpward<TSelf, TKey>(Box<TSelf> node, Span<TKey> keys, List<Box<TSelf>> foundNodes)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        if (node.FindByKeysUpward(keys) != null)
            foundNodes.Add(node);

        foreach (var child in node.Item.Children)
            FindSubPathsByKeyUpward(child, keys, foundNodes);
    }
}

/// <summary>
///    Extensions for <see cref="IHaveObservableChildren{TSelf}"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveKeyExtensionsForIHaveObservableChildren
{
    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        => item.Item.EnumerateKeysBfs<TSelf, TKey>();

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
    {
        // First return all keys of the immediate children
        foreach (var child in item.Children)
            yield return child.Item.Key;

        // Then iterate over the immediate children and recursively enumerate their keys
        foreach (var child in item.Children)
        {
            foreach (var grandChildKey in child.Item.EnumerateKeysBfs<TSelf, TKey>())
                yield return grandChildKey;
        }
    }

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        => item.Item.EnumerateKeysDfs<TSelf, TKey>();

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
    {
        // Enumerate the key of each child and then recursively enumerate the keys of its children
        foreach (var child in item.Children)
        {
            yield return child.Item.Key;
            foreach (var grandChildKey in child.Item.EnumerateKeysDfs<TSelf, TKey>())
                yield return grandChildKey;
        }
    }

    /// <summary>
    ///     Recursively returns all the keys of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child keys to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An array of all the keys of the children of this node.</returns>
    public static TKey[] GetKeys<TSelf, TKey>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        => item.Item.GetKeys<TSelf, TKey>();

    /// <summary>
    ///     Recursively returns all the keys of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child keys to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An array of all the keys of the children of this node.</returns>
    public static TKey[] GetKeys<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
    {
        var totalKeys = item.CountChildren(); // Ensure this method counts all descendants.
        var keys = GC.AllocateUninitializedArray<TKey>(totalKeys);
        var index = 0;
        GetKeysUnsafe<TSelf, TKey>(item, keys, ref index);
        return keys;
    }

    /// <summary>
    ///     Helper method to populate keys recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="buffer">
    ///     The span to fill with keys.
    ///     If calling on root node, should be at least as big as <see cref="IHaveObservableChildrenExtensions.CountChildren{TSelf}(Box{TSelf})"/>
    /// </param>
    /// <param name="index">The current index in the array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetKeysUnsafe<TSelf, TKey>(TSelf item, Span<TKey> buffer, ref int index)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
            buffer.DangerousGetReferenceAt(index++) = child.Item.Key;

        foreach (var child in item.Children)
            GetKeysUnsafe(child.Item, buffer, ref index);
    }

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the root node.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    public static Box<TSelf>? FindByKeyFromRoot<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        if (keys.IsEmpty || !EqualityComparer<TKey>.Default.Equals(root.Item.Key, keys.DangerousGetReferenceAt(0)))
            return null;

        return keys.Length == 1 ? root : FindNode(root, keys.SliceFast(1));
    }

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the root node.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static Box<TSelf>? FindByKeyFromRoot<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindByKeyFromRoot((Box<TSelf>)root, keys);

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the child nodes.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    public static Box<TSelf>? FindByKeyFromChild<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => keys.Length == 0 ? null : FindNode(root, keys);

    /// <summary>
    ///     Finds a node in the tree based on a given sequence of keys, starting the search from the child nodes.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given sequence of keys, or null if not found.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static Box<TSelf>? FindByKeyFromChild<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindByKeyFromChild((Box<TSelf>)root, keys);

    private static Box<TSelf>? FindNode<TSelf, TKey>(Box<TSelf> node, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        foreach (var key in keys)
        {
            var found = false;
            foreach (var child in node.Item.Children)
            {
                if (!EqualityComparer<TKey>.Default.Equals(child.Item.Key, key))
                    continue;

                node = child;
                found = true;
                break;
            }

            if (!found)
                return null;
        }

        return node;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys, starting
    ///     the search from the child nodes of the given root.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    public static List<Box<TSelf>> FindSubPathsByKeyFromChild<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindSubPathsByKeyFromChild(root.Item, keys);

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys, starting the search
    ///     from the child nodes of the given root.
    /// </summary>
    /// <param name="node">The current node being examined.</param>
    /// <param name="keys">Span of keys representing the path/key segments to search.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    public static List<Box<TSelf>> FindSubPathsByKeyFromChild<TSelf, TKey>(this TSelf node, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var foundNodes = new List<Box<TSelf>>();
        if (keys.Length <= 0)
            return foundNodes;

        foreach (var child in node.Children)
            FindSubPathsByKeyRecursive(child, keys, foundNodes);

        return foundNodes;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys,
    ///     including the root node in the search.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    public static List<Box<TSelf>> FindSubPathsByKeyFromRoot<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var foundNodes = new List<Box<TSelf>>();
        if (keys.Length <= 0)
            return foundNodes;

        FindSubPathsByKeyRecursive(root, keys, foundNodes);
        return foundNodes;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys,
    ///     including the root node in the search.
    /// </summary>
    /// <param name="root">The current node being examined.</param>
    /// <param name="keys">Span of keys representing the path/key segments to search.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation is very slow as it has theoretical complexity of O(N^3), though in practice it's closer to O(N^2).
    ///     If possible, prefer using <see cref="FindSubPathsByKeyUpward{TSelf,TKey}(Box{TSelf},System.Span{TKey})"/>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static List<Box<TSelf>> FindSubPathsByKeyFromRoot<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindSubPathsByKeyFromRoot((Box<TSelf>)root, keys);

    private static void FindSubPathsByKeyRecursive<TSelf, TKey>(Box<TSelf> node, Span<TKey> keys, List<Box<TSelf>> foundNodes)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var result = FindByKeyFromRoot(node, keys);
        if (result != null)
            foundNodes.Add(result);

        foreach (var child in node.Item.Children)
            FindSubPathsByKeyRecursive(child, keys, foundNodes);
    }

    /// <summary>
    ///     Initiates a search from a given node and finds all descendant nodes that, when traced upwards, match a specified sequence of keys.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys to be matched, starting from the leaf node and moving upwards.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>
    ///     A list of nodes where each node's path to the root (root being node at any depth)
    ///     matches the specified sequence of keys.
    /// </returns>
    public static List<Box<TSelf>> FindSubPathsByKeyUpward<TSelf, TKey>(this Box<TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        var foundNodes = new List<Box<TSelf>>();
        FindSubPathsByKeyUpward(root, keys, foundNodes);
        return foundNodes;
    }

    /// <summary>
    ///     Initiates a search from a given node and finds all descendant nodes that, when traced upwards, match a specified sequence of keys.
    /// </summary>
    /// <param name="root">The starting node for the search.</param>
    /// <param name="keys">The sequence of keys to be matched, starting from each descendant node and moving upwards.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>
    ///     A list of nodes where each node's path to the root (root being node at any depth)
    ///     matches the specified sequence of keys.
    /// </returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static List<Box<TSelf>> FindSubPathsByKeyUpward<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
        => FindSubPathsByKeyUpward((Box<TSelf>)root, keys);

    private static void FindSubPathsByKeyUpward<TSelf, TKey>(Box<TSelf> node, Span<TKey> keys, List<Box<TSelf>> foundNodes)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        if (node.FindByKeysUpward(keys) != null)
            foundNodes.Add(node);

        foreach (var child in node.Item.Children)
            FindSubPathsByKeyUpward(child, keys, foundNodes);
    }
}

/// <summary>
///    Extensions for <see cref="IHaveBoxedChildrenWithKey{TKey,TSelf}"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveKeyExtensionsForIHaveBoxedChildrenWithKey
{
    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => item.Item.EnumerateKeysBfs<TSelf, TKey>();

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        // First return all keys of the immediate children
        foreach (var child in item.Children)
            yield return child.Value.Item.Key;

        // Then iterate over the immediate children and recursively enumerate their keys
        foreach (var child in item.Children)
        {
            foreach (var grandChildKey in child.Value.Item.EnumerateKeysBfs<TSelf, TKey>())
                yield return grandChildKey;
        }
    }

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => item.Item.EnumerateKeysDfs<TSelf, TKey>();

    /// <summary>
    ///     Enumerates all keys of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose child keys are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child keys of the current node.</returns>
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        // Enumerate the key of each child and then recursively enumerate the keys of its children
        foreach (var child in item.Children)
        {
            yield return child.Value.Item.Key;
            foreach (var grandChildKey in child.Value.Item.EnumerateKeysDfs<TSelf, TKey>())
                yield return grandChildKey;
        }
    }

    /// <summary>
    ///     Returns all the keys of the children of this node (using recursion).
    /// </summary>
    /// <param name="item">The node whose child keys to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An array of all the keys of the children of this node.</returns>
    public static TKey[] GetKeys<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull =>
        item.Item.GetKeys<TSelf, TKey>();

    /// <summary>
    ///     Returns all the keys of the children of this node (using recursion).
    /// </summary>
    /// <param name="item">The node whose child keys to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An array of all the keys of the children of this node.</returns>
    public static TKey[] GetKeys<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var totalChildren = item.CountChildren<TSelf, TKey>(); // Ensure CountChildren counts all descendants.
        var keys = GC.AllocateUninitializedArray<TKey>(totalChildren);
        var index = 0;
        GetKeysUnsafe<TSelf, TKey>(item, keys, ref index);
        return keys;
    }

    /// <summary>
    ///     Helper method to populate keys recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="buffer">
    ///     The span to fill with keys.
    ///     Should be at least as big as <see cref="IHaveBoxedChildrenWithKeyExtensions.CountChildren{TSelf,TKey}(KeyedBox{TKey,TSelf})"/>
    /// </param>
    /// <param name="index">The current index in the span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetKeysUnsafe<TSelf, TKey>(TSelf item, Span<TKey> buffer, ref int index)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        foreach (var pair in item.Children)
        {
            buffer.DangerousGetReferenceAt(index++) = pair.Key;
            GetKeysUnsafe(pair.Value.Item, buffer, ref index);
        }
    }

    /// <summary>
    ///     Finds a node in the tree based on a given relative path of keys, starting the search from the root node.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree wrapped in a ChildWithKeyBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given path of keys, or null if not found.</returns>
    public static KeyedBox<TKey, TSelf>? FindByKeyFromRoot<TSelf, TKey>(this KeyedBox<TKey, TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        if (keys.IsEmpty || !EqualityComparer<TKey>.Default.Equals(root.Item.Key, keys.DangerousGetReferenceAt(0)))
            return null;

        return keys.Length == 1 ? root : FindNode(root, keys.SliceFast(1));
    }

    /// <summary>
    ///     Finds a node in the tree based on a given relative path of keys, starting the search from the root node.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given path of keys, or null if not found.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static KeyedBox<TKey, TSelf>? FindByKeyFromRoot<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindByKeyFromRoot((KeyedBox<TKey, TSelf>) root, keys);

    /// <summary>
    ///     Finds a node in the tree based on a given relative path of keys, starting the search from the child nodes.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree wrapped in a ChildWithKeyBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given path of keys, or null if not found.</returns>
    public static KeyedBox<TKey, TSelf>? FindByKeyFromChild<TSelf, TKey>(this KeyedBox<TKey, TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => keys.Length == 0 ? null : FindNode(root, keys);

    /// <summary>
    ///     Finds a node in the tree based on a given relative path of keys, starting the search from the child nodes.
    /// </summary>
    /// <param name="root">The current (search root) node of the tree.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>The node that matches the given path of keys, or null if not found.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static KeyedBox<TKey, TSelf>? FindByKeyFromChild<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindByKeyFromChild((KeyedBox<TKey, TSelf>) root, keys);

    private static KeyedBox<TKey, TSelf>? FindNode<TSelf, TKey>(KeyedBox<TKey, TSelf> node, Span<TKey> keys) where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        while (true)
        {
            var currentKey = keys.DangerousGetReferenceAt(0);
            if (!node.Item.Children.TryGetValue(currentKey, out var childBox))
                return null;

            node = childBox;
            if (keys.Length == 1)
                return node;

            keys = keys.SliceFast(1);
        }
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys, starting
    ///     the search from the child nodes of the given root.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation has theoretical complexity of O(N^2), so worst case can be slow, though in practice it's closer to O(N).
    /// </remarks>
    public static List<KeyedBox<TKey, TSelf>> FindSubPathsByKeyFromChild<TSelf, TKey>(this KeyedBox<TKey, TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindSubPathsByKeyFromChild(root.Item, keys);

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys, starting the search
    ///     from the child nodes of the given root.
    /// </summary>
    /// <param name="node">The current node being examined.</param>
    /// <param name="keys">Span of keys representing the path/key segments to search.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation has theoretical complexity of O(N^2), so worst case can be slow, though in practice it's closer to O(N).
    /// </remarks>
    public static List<KeyedBox<TKey, TSelf>> FindSubPathsByKeyFromChild<TSelf, TKey>(this TSelf node, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var foundNodes = new List<KeyedBox<TKey, TSelf>>();
        if (keys.Length <= 0)
            return foundNodes;

        foreach (var child in node.Children)
            FindAllSubPathsByKeyRecursive(child.Value, keys, foundNodes);

        return foundNodes;
    }

    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys,
    ///     including the root node in the search.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys representing the path from this node to find.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation has theoretical complexity of O(N^2), so worst case can be slow, though in practice it's closer to O(N).
    /// </remarks>
    public static List<KeyedBox<TKey, TSelf>> FindSubPathsByKeyFromRoot<TSelf, TKey>(this KeyedBox<TKey, TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var foundNodes = new List<KeyedBox<TKey, TSelf>>();
        if (keys.Length <= 0)
            return foundNodes;

        FindAllSubPathsByKeyRecursive(root, keys, foundNodes);
        return foundNodes;
    }


    /// <summary>
    ///     Finds all nodes in the tree that match a given sequence of keys,
    ///     including the root node in the search.
    /// </summary>
    /// <param name="root">The current node being examined.</param>
    /// <param name="keys">Span of keys representing the path/key segments to search.</param>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <returns>A collection of nodes that match the given sequence of keys.</returns>
    /// <remarks>
    ///     This operation has theoretical complexity of O(N^2), so worst case can be slow, though in practice it's closer to O(N).
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static List<KeyedBox<TKey, TSelf>> FindSubPathsByKeyFromRoot<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
        => FindSubPathsByKeyFromRoot((KeyedBox<TKey, TSelf>) root, keys);

    private static void FindAllSubPathsByKeyRecursive<TSelf, TKey>(KeyedBox<TKey, TSelf> node, Span<TKey> keys, List<KeyedBox<TKey, TSelf>> foundNodes)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>
        where TKey : notnull
    {
        var result = FindByKeyFromRoot(node, keys);
        if (result != null)
            foundNodes.Add(result);

        foreach (var child in node.Item.Children)
            FindAllSubPathsByKeyRecursive(child.Value, keys, foundNodes);
    }

    /// <summary>
    ///     Searches for all nodes within a tree that, when traced upwards, match a specified sequence of keys.
    /// </summary>
    /// <param name="root">The root node of the tree, wrapped in a ChildBox.</param>
    /// <param name="keys">The sequence of keys to be matched, starting from the leaf node and moving upwards.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>
    ///     A list of nodes where each node's path to the root (root being node at any depth)
    ///     matches the specified sequence of keys.
    /// </returns>
    public static List<KeyedBox<TKey, TSelf>> FindSubPathsByKeyUpward<TSelf, TKey>(this KeyedBox<TKey, TSelf> root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        var foundNodes = new List<KeyedBox<TKey, TSelf>>();
        FindSubPathsByKeyUpward(root, keys, foundNodes);
        return foundNodes;
    }

    /// <summary>
    ///     Initiates a search from a given node and finds all descendant nodes that, when traced upwards, match a specified sequence of keys.
    /// </summary>
    /// <param name="root">The starting node for the search.</param>
    /// <param name="keys">The sequence of keys to be matched, starting from each descendant node and moving upwards.</param>
    /// <typeparam name="TSelf">The type of the node in the tree.</typeparam>
    /// <typeparam name="TKey">The type of the key used in the tree.</typeparam>
    /// <returns>
    ///     A list of nodes where each node's path to the root (root being node at any depth)
    ///     matches the specified sequence of keys.
    /// </returns>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method causes temporary boxing of object. Do not use unless you have no other way to access this item. Use this method via Box<TSelf> instead.")]
    public static List<KeyedBox<TKey, TSelf>> FindSubPathsByKeyUpward<TSelf, TKey>(this TSelf root, Span<TKey> keys)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
        => FindSubPathsByKeyUpward((KeyedBox<TKey, TSelf>)root, keys);

    private static void FindSubPathsByKeyUpward<TSelf, TKey>(KeyedBox<TKey, TSelf> node, Span<TKey> keys, List<KeyedBox<TKey, TSelf>> foundNodes)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveKey<TKey>, IHaveParent<TSelf>
        where TKey : notnull
    {
        if (node.FindByKeysUpward(keys) != null)
            foundNodes.Add(node);

        foreach (var child in node.Item.Children)
            FindSubPathsByKeyUpward(child.Value, keys, foundNodes);
    }
}
