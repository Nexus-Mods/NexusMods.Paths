using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NexusMods.Paths.HighPerformance.CommunityToolkit;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Trees;

/// <summary>
///     Mixin extensions for combinations of various interfaces for <see cref="IHaveBoxedChildren{TSelf}"/>
/// </summary>
public static class MixinExtensionsForIHaveBoxedChildren
{
    /// <summary>
    ///     Recursively returns all the key-value pairs of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child key-value pairs to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An array of all the key-value pairs of the children of this node.</returns>
    public static KeyValuePair<TKey, TValue>[] GetKeyValues<TSelf, TKey, TValue>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>, IHaveValue<TValue> =>
        item.Item.GetKeyValues<TSelf, TKey, TValue>();

    /// <summary>
    ///     Recursively returns all the key-value pairs of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child key-value pairs to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An array of all the key-value pairs of the children of this node.</returns>
    public static KeyValuePair<TKey, TValue>[] GetKeyValues<TSelf, TKey, TValue>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>, IHaveValue<TValue>
    {
        var totalPairs = item.CountChildren(); // Ensure this method counts all descendants.
        var pairs = new KeyValuePair<TKey, TValue>[totalPairs];
        var index = 0;
        GetKeyValuesUnsafe<TSelf, TKey, TValue>(item, pairs, ref index);
        return pairs;
    }

    /// <summary>
    ///     Helper method to populate key-value pairs recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="buffer">
    ///     The span to fill with key-value pairs.
    ///     Should be at least as big as <see cref="IHaveBoxedChildrenExtensions.CountChildren{TSelf}(TSelf)"/>
    /// </param>
    /// <param name="index">The current index in the array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetKeyValuesUnsafe<TSelf, TKey, TValue>(TSelf item, Span<KeyValuePair<TKey, TValue>> buffer, ref int index)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveKey<TKey>, IHaveValue<TValue>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
        {
            var key = child.Item.Key;
            var value = child.Item.Value;
            buffer.DangerousGetReferenceAt(index++) = new KeyValuePair<TKey, TValue>(key, value);
        }

        foreach (var child in item.Children)
            GetKeyValuesUnsafe(child.Item, buffer, ref index);
    }
}

/// <summary>
///     Mixin extensions for combinations of various interfaces.
/// </summary>
public static class MixinExtensionsForIHaveBoxedChildrenWithKey
{
    /// <summary>
    ///     Recursively returns all the key-value pairs of the children of this node.
    /// </summary>
    /// <param name="item">The boxed node whose child key-value pairs to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An array of all the key-value pairs of the children of this node.</returns>
    public static KeyValuePair<TKey, TValue>[] GetKeyValues<TKey, TSelf, TValue>(this ChildWithKeyBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveValue<TValue>, IHaveKey<TKey>
        where TKey : notnull
        => item.Item.GetKeyValues<TKey, TSelf, TValue>();

    /// <summary>
    ///     Recursively returns all the key-value pairs of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child key-value pairs to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An array of all the key-value pairs of the children of this node.</returns>
    public static KeyValuePair<TKey, TValue>[] GetKeyValues<TKey, TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveValue<TValue>, IHaveKey<TKey>
        where TKey : notnull
    {
        var totalPairs = item.CountChildren<TSelf, TKey>(); // Ensure this method counts all descendants.
        var pairs = new KeyValuePair<TKey, TValue>[totalPairs];
        var index = 0;
        GetKeyValuesUnsafe<TKey, TSelf, TValue>(item, pairs, ref index);
        return pairs;
    }

    /// <summary>
    ///     Helper method to populate key-value pairs recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="buffer">
    ///     The span to fill with key-value pairs.
    ///     Should be at least as big as <see cref="IHaveBoxedChildrenWithKeyExtensions.CountChildren{TSelf,TKey}(TSelf)"/>
    /// </param>
    /// <param name="index">The current index in the array.</param>
    public static void GetKeyValuesUnsafe<TKey, TSelf, TValue>(TSelf item, Span<KeyValuePair<TKey, TValue>> buffer,
        ref int index)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveValue<TValue>, IHaveKey<TKey>
        where TKey : notnull
    {
        foreach (var pair in item.Children)
        {
            var key = pair.Value.Item.Key;
            var value = pair.Value.Item.Value;
            buffer.DangerousGetReferenceAt(index++) = new KeyValuePair<TKey, TValue>(key, value);
            GetKeyValuesUnsafe(pair.Value.Item, buffer, ref index);
        }
    }
}

/// <summary>
///     Mixin extensions for combinations of various interfaces.
/// </summary>
public static class MixinExtensionsForIHaveObservableChildren
{
    /// <summary>
    ///     Recursively returns all the key-value pairs of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child key-value pairs to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An array of all the key-value pairs of the children of this node.</returns>
    public static KeyValuePair<TKey, TValue>[] GetKeyValues<TSelf, TKey, TValue>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>, IHaveValue<TValue> =>
        item.Item.GetKeyValues<TSelf, TKey, TValue>();

    /// <summary>
    ///     Recursively returns all the key-value pairs of the children of this node.
    /// </summary>
    /// <param name="item">The node whose child key-value pairs to obtain.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An array of all the key-value pairs of the children of this node.</returns>
    public static KeyValuePair<TKey, TValue>[] GetKeyValues<TSelf, TKey, TValue>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>, IHaveValue<TValue>
    {
        var totalPairs = item.CountChildren(); // Ensure this method counts all descendants.
        var pairs = new KeyValuePair<TKey, TValue>[totalPairs];
        var index = 0;
        GetKeyValuesUnsafe<TSelf, TKey, TValue>(item, pairs, ref index);
        return pairs;
    }

    /// <summary>
    ///     Helper method to populate key-value pairs recursively.
    /// </summary>
    /// <param name="item">The current node.</param>
    /// <param name="buffer">
    ///     The span to fill with key-value pairs.
    ///     Should be at least as big as <see cref="IHaveObservableChildrenExtensions.CountChildren{TSelf}(TSelf)"/>
    /// </param>
    /// <param name="index">The current index in the array.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetKeyValuesUnsafe<TSelf, TKey, TValue>(TSelf item, Span<KeyValuePair<TKey, TValue>> buffer, ref int index)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveKey<TKey>, IHaveValue<TValue>
    {
        // Populate breadth first. Improved cache locality helps here.
        foreach (var child in item.Children)
        {
            var key = child.Item.Key;
            var value = child.Item.Value;
            buffer.DangerousGetReferenceAt(index++) = new KeyValuePair<TKey, TValue>(key, value);
        }

        foreach (var child in item.Children)
            GetKeyValuesUnsafe(child.Item, buffer, ref index);
    }
}
