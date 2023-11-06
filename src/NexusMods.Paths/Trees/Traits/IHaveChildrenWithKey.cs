using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NexusMods.Paths.Trees.Traits;

/*
   Note: This is constrained to Dictionary<T> because doing it with 'where' constraints will cause
   a recursive generic definition.

   public interface IHaveChildrenWithKey<TKey, TSelf, out TDictionary>
     where TDictionary : IDictionary<TKey, CwkBox<TKey, TSelf, TDictionary>>
     where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf, TDictionary>

   TDictionary recurses on itself.
*/

/// <summary>
///     An interface used by Tree implementations to indicate that they have a keyed child.
/// </summary>
/// <typeparam name="TKey">The name of the key used in the File Tree.</typeparam>
/// <typeparam name="TSelf">The type of the child stored in this FileTree.</typeparam>
public interface IHaveChildrenWithKey<TKey, TSelf>
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    /// <summary>
    ///     A Dictionary containing all the children of this node.
    /// </summary>
    /// <remarks>
    ///     This should point to an empty dictionary if there are no items.
    /// </remarks>
    public Dictionary<TKey, ChildrenWithKeyBox<TKey, TSelf>> Children { get; }
}

/// <summary>
///     A boxed element that implements <see cref="IHaveChildrenWithKey{TKey,TSelf}" />
/// </summary>
/// <remarks>
///     This is a helper class that boxes a constrained generic structure type.
///     While generic reference types share code (and are thus slower),
///     Generic structures can participate in devirtualization, and thus create
///     zero overhead abstractions.
/// </remarks>
public class ChildrenWithKeyBox<TKey, TSelf>
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    /// <summary>
    ///     Contains item deriving from <see cref="IHaveChildrenWithKey{TKey,TSelf}" />
    /// </summary>
    public TSelf Item;

    /// <summary />
    public static implicit operator TSelf(ChildrenWithKeyBox<TKey, TSelf> box)
    {
        return box.Item;
    }

    /// <summary />
    public static implicit operator ChildrenWithKeyBox<TKey, TSelf>(TSelf item)
    {
        return new ChildrenWithKeyBox<TKey, TSelf> { Item = item };
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveChildrenWithKey{TKey,TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveChildrenWithKeyExtensions
{
    /// <summary>
    ///     Enumerates all child nodes of the current node in a depth-first manner.
    /// </summary>
    /// <param name="item">The node whose children are to be enumerated.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>An IEnumerable of all child nodes of the current node.</returns>
    public static IEnumerable<KeyValuePair<TKey, ChildrenWithKeyBox<TKey, TSelf>>> EnumerateChildren<TSelf, TKey>(
        this TSelf item)
        where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        foreach (var child in item.Children)
        {
            yield return child;
            foreach (var tuple in child.Value.Item.EnumerateChildren<TSelf, TKey>())
                yield return tuple;
        }
    }

    /// <summary>
    ///     Counts the number of direct child nodes of the current node.
    /// </summary>
    /// <param name="item">The node whose children are to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The count of direct child nodes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountChildren<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
        where TKey : notnull
    {
        var result = 0;
        item.CountChildrenRecursive<TSelf, TKey>(ref result);
        return result;
    }

    /// <summary>
    ///     Enumerates all child nodes of this current node.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountChildrenRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf> where TKey : notnull
    {
        accumulator += item.Children.Count;
        foreach (var child in item.Children)
            child.Value.Item.CountChildrenRecursive<TSelf, TKey>(ref accumulator);
    }
}
