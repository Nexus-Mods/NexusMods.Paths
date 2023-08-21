using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NexusMods.Paths;

/// <summary>
/// Abstracts an individual path.
/// </summary>
public interface IPath
{
    /// <summary>
    /// Gets the extension of this path.
    /// </summary>
    Extension Extension { get; }

    /// <summary>
    /// Gets the file name of this path.
    /// </summary>
    RelativePath FileName { get; }
}

/// <summary>
/// Abstracts an individual path.
/// Allows methods to return specific path types.
/// </summary>
/// <typeparam name="TConcretePath">Concrete path type returned by method implementations</typeparam>
[PublicAPI]
public interface IPath<TConcretePath> : IPath where TConcretePath : struct, IPath<TConcretePath>, IEquatable<TConcretePath>
{
    /// <summary>
    /// The file name of this path.
    /// </summary>
    /// <remarks>
    /// Returns an empty <see cref="RelativePath"/> if this path is a root component.
    /// </remarks>
    RelativePath Name { get; }

    /// <summary>
    /// Gets the extension of this path.
    /// </summary>
    /// <remarks>
    /// Returns an empty <see cref="Extension"/> if no extension is present.
    /// </remarks>
    Extension Extension { get; }

    /// <summary>
    /// Traverses one directory up, returning the path of the parent.
    /// </summary>
    /// <remarks>
    /// If the path is a root component, returns the root component.
    /// If path is not rooted and there are no parent directories, returns an empty path.
    /// </remarks>
    TConcretePath Parent { get; }

    /// <summary>
    /// If this path is rooted, returns the root component, an empty path otherwise.
    /// </summary>
    TConcretePath GetRootComponent { get; }

    /// <summary>
    /// Returns a collection of parts that make up this path, excluding root components.
    /// NOTE: Root components like `C:/` are excluded and should be handled separately.
    /// </summary>
    IEnumerable<RelativePath> Parts { get; }

    /// <summary>
    /// Returns a collection of all parent paths, including this path.
    /// Order is from this path to the root.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TConcretePath> GetAllParents();

    /// <summary>
    /// Returns a <see cref="RelativePath"/> of the non-root part of this path.
    /// </summary>
    RelativePath GetNonRootPart();

    /// <summary>
    /// Returns whether this path is rooted.
    /// </summary>
    bool IsRooted { get; }
}
