using System;
using System.Diagnostics.CodeAnalysis;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Trees;

/// <summary>
///     A boxed structure.
/// </summary>
/// <remarks>
///     This is a helper class that boxes a constrained generic structure type.
///     While generic reference types share code (and are thus slower),
///     Generic structures can participate in devirtualization, and thus create
///     zero overhead abstractions.
/// </remarks>
[ExcludeFromCodeCoverage]
public class Box<TSelf> : IEquatable<Box<TSelf>> where TSelf : struct
{
    /// <summary>
    ///     Contains item deriving from <see cref="IHaveBoxedChildren{TSelf}" />
    /// </summary>
    public TSelf Item;

    /// <summary />
    public static implicit operator TSelf(Box<TSelf> box) => box.Item;

    /// <summary />
    public static implicit operator Box<TSelf>(TSelf item) => new() { Item = item };

    #region Autogenerated by R#
    /// <inheritdoc />
    public bool Equals(Box<TSelf>? other)
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
        return Equals((Box<TSelf>)obj);
    }

    /// <inheritdoc />
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => Item.GetHashCode();
    #endregion
}
