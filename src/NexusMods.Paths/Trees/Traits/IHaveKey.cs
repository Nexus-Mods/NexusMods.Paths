using System.Collections.Generic;

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
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this ChildBox<TSelf> item)
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
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this ChildBox<TSelf> item)
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
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this ChildBox<TSelf> item)
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
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this ChildBox<TSelf> item)
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
    public static IEnumerable<TKey> EnumerateKeysBfs<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item)
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
    public static IEnumerable<TKey> EnumerateKeysDfs<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item)
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
}
