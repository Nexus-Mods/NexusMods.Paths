using System.Collections.Generic;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by Tree implementations to indicate that they have a value inside the node.
/// </summary>
/// <typeparam name="TValue">The type of the value used.</typeparam>
public interface IHaveValue<out TValue>
{
    /// <summary>
    ///     The value of the node.
    /// </summary>
    TValue Value { get; }
}

/// <summary>
///    Extensions for <see cref="IHaveBoxedChildren{TSelf}"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveValueExtensionsForIHaveBoxedChildren
{
    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesBfs<TSelf, TValue>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveValue<TValue>
        => item.Item.EnumerateValuesBfs<TSelf, TValue>();

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesBfs<TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveValue<TValue>
    {
        // First return all values of the immediate children
        foreach (var child in item.Children)
            yield return child.Item.Value;

        // Then iterate over the immediate children and recursively enumerate their values
        foreach (var child in item.Children)
        {
            foreach (var grandChildvalue in child.Item.EnumerateValuesBfs<TSelf, TValue>())
                yield return grandChildvalue;
        }
    }

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesDfs<TSelf, TValue>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveValue<TValue>
        => item.Item.EnumerateValuesDfs<TSelf, TValue>();

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesDfs<TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveValue<TValue>
    {
        // Enumerate the value of each child and then recursively enumerate the values of its children
        foreach (var child in item.Children)
        {
            yield return child.Item.Value;
            foreach (var grandChildvalue in child.Item.EnumerateValuesDfs<TSelf, TValue>())
                yield return grandChildvalue;
        }
    }
}

/// <summary>
///    Extensions for <see cref="IHaveObservableChildren{TSelf}"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveValueExtensionsForIHaveObservableChildren
{
    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesBfs<TSelf, TValue>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveValue<TValue>
        => item.Item.EnumerateValuesBfs<TSelf, TValue>();

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesBfs<TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveValue<TValue>
    {
        // First return all values of the immediate children
        foreach (var child in item.Children)
            yield return child.Item.Value;

        // Then iterate over the immediate children and recursively enumerate their values
        foreach (var child in item.Children)
        {
            foreach (var grandChildvalue in child.Item.EnumerateValuesBfs<TSelf, TValue>())
                yield return grandChildvalue;
        }
    }

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesDfs<TSelf, TValue>(this ChildBox<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveValue<TValue>
        => item.Item.EnumerateValuesDfs<TSelf, TValue>();

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesDfs<TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveValue<TValue>
    {
        // Enumerate the value of each child and then recursively enumerate the values of its children
        foreach (var child in item.Children)
        {
            yield return child.Item.Value;
            foreach (var grandChildvalue in child.Item.EnumerateValuesDfs<TSelf, TValue>())
                yield return grandChildvalue;
        }
    }
}

/// <summary>
///    Extensions for <see cref="IHaveBoxedChildrenWithKey{TValue,TSelf}"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveValueExtensionsForIHaveBoxedChildrenWithvalue
{
    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesBfs<TKey, TSelf, TValue>(this ChildWithKeyBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TValue, TSelf>, IHaveValue<TValue>
        where TValue : notnull
        where TKey : notnull => item.Item.EnumerateValuesBfs<TSelf, TValue>();

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a breadth-first manner.
    /// </summary>
    /// <param name="item">The node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesBfs<TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TValue, TSelf>, IHaveValue<TValue>
        where TValue : notnull
    {
        // First return all values of the immediate children
        foreach (var child in item.Children)
            yield return child.Value.Item.Value;

        // Then iterate over the immediate children and recursively enumerate their values
        foreach (var child in item.Children)
        {
            foreach (var grandChildvalue in child.Value.Item.EnumerateValuesBfs<TSelf, TValue>())
                yield return grandChildvalue;
        }
    }

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The boxed node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesDfs<TKey, TSelf, TValue>(this ChildWithKeyBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TValue, TSelf>, IHaveValue<TValue>
        where TValue : notnull
        where TKey : notnull => item.Item.EnumerateValuesDfs<TSelf, TValue>();

    /// <summary>
    ///     Enumerates all values of the child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose child values are to be enumerated.</param>
    /// <typeparam name="TSelf">The type of the child node.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <returns>An IEnumerable of all child values of the current node.</returns>
    public static IEnumerable<TValue> EnumerateValuesDfs<TSelf, TValue>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TValue, TSelf>, IHaveValue<TValue>
        where TValue : notnull
    {
        // Enumerate the value of each child and then recursively enumerate the values of its children
        foreach (var child in item.Children)
        {
            yield return child.Value.Item.Value;
            foreach (var grandChildvalue in child.Value.Item.EnumerateValuesDfs<TSelf, TValue>())
                yield return grandChildvalue;
        }
    }
}
