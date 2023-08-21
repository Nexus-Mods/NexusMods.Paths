using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NexusMods.Paths.Extensions;
using NexusMods.Paths.Utilities;

namespace NexusMods.Paths;

/// <summary>
/// A path that represents a partial path to a file or directory.
/// </summary>
[PublicAPI]
public readonly struct RelativePath : IPath<RelativePath>, IEquatable<RelativePath>, IComparable<RelativePath>
{
    // NOTE(erri120): since relative paths are not rooted, the operating system
    // shouldn't matter. The OS is usually only relevant to determine the root part
    // of a path.
    private static readonly IOSInformation OS = OSInformation.Shared;

    /// <summary>
    /// Gets the comparer used for sorting.
    /// </summary>
    public static readonly RelativePathComparer Comparer;

    /// <summary>
    /// Represents an empty path.
    /// </summary>
    public static RelativePath Empty => new(string.Empty);

    /// <summary>
    /// Contains the relative path stored in this instance.
    /// </summary>
    public readonly string Path;

    /// <inheritdoc />
    public Extension Extension => Extension.FromPath(Path);

    /// <summary>
    /// Returns the file name of this path.
    /// </summary>
    public RelativePath FileName => Name;

    /// <inheritdoc />
    public RelativePath Name => new(PathHelpers.GetFileName(Path, OS).ToString());

    /// <summary>
    /// Amount of directories contained within this relative path.
    /// </summary>
    public int Depth => PathHelpers.GetDirectoryDepth(Path, OS);

    /// <summary>
    /// Traverses one directory up.
    /// </summary>
    public RelativePath Parent
    {
        get
        {
            var directoryName = PathHelpers.GetDirectoryName(Path, OS);
            return directoryName.IsEmpty ? Empty : new RelativePath(directoryName.ToString());
        }
    }

    /// <summary>
    /// Always returns an empty path as relative paths are not rooted.
    /// </summary>
    public RelativePath GetRootComponent => RelativePath.Empty;

    /// <inheritdoc/>
    public IEnumerable<RelativePath> Parts
    {
        get
        {
            var currentPath = this;
            while (currentPath.Path != Empty)
            {
                yield return currentPath.Name;
                currentPath = currentPath.Parent;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<RelativePath> GetAllParents()
    {
        var parentPath = this;
        while (parentPath != Empty)
        {
            yield return parentPath;
            parentPath = parentPath.Parent;
        }
    }

    /// <summary>
    /// Always returns itself as relative paths are not rooted.
    /// </summary>
    /// <returns></returns>
    public RelativePath GetNonRootPart()
    {
        return this;
    }

    /// <inheritdoc />
    public bool IsRooted => false;


    /// <summary>
    /// Obtains the name of the first folder stored in this path.
    /// </summary>
    /// <remarks>
    /// This will return empty string if there are no child directories.
    /// </remarks>
    public RelativePath TopParent
    {
        get
        {
            var topParent = PathHelpers.GetTopParent(Path, OS);
            return topParent.IsEmpty ? Empty : new RelativePath(topParent.ToString());
        }
    }

    /// <summary>
    /// Creates a relative path given a string.
    /// </summary>
    /// <param name="path">The relative path to use.</param>
    public RelativePath(string path)
    {
        PathHelpers.DebugAssertIsSanitized(path, OS, isRelative: true);
        Path = path;
    }

    /// <summary>
    /// Creates a new <see cref="RelativePath"/> from a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="path"></param>
    public static RelativePath FromUnsanitizedInput(ReadOnlySpan<char> path)
    {
        return new RelativePath(PathHelpers.Sanitize(path, OS));
    }

    /// <summary>
    /// Returns the path with the directory separators native to the passed operating system.
    /// </summary>
    public string ToNativeSeparators(IOSInformation os)
    {
        return PathHelpers.ToNativeSeparators(Path, os);
    }

    /// <summary>
    /// Returns a new path that is this path with the extension changed.
    /// </summary>
    /// <param name="newExtension">The extension to replace the old extension.</param>
    public RelativePath ReplaceExtension(Extension newExtension)
    {
        return new RelativePath(PathHelpers.ReplaceExtension(Path, newExtension.ToString()));
    }

    /// <summary>
    /// Adds an extension to the relative path.
    /// </summary>
    /// <param name="ext">The extension to add.</param>
    public RelativePath WithExtension(Extension ext) => new(Path + ext);

    /// <summary>
    /// Appends another path to an existing path.
    /// </summary>
    /// <param name="other">The path to append.</param>
    /// <returns>Combinations of both paths.</returns>
    [Pure]
    public RelativePath Join(RelativePath other)
    {
        return new RelativePath(PathHelpers.JoinParts(Path, other.Path, OS));
    }

    /// <summary>
    /// Returns true if the relative path starts with a given string.
    /// </summary>
    public bool StartsWith(ReadOnlySpan<char> other)
    {
        return Path.AsSpan().StartsWith(other, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true if the relative path ends with a given string.
    /// </summary>
    public bool EndsWith(ReadOnlySpan<char> other)
    {
        return Path.AsSpan().EndsWith(other, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true if this path is a child of this path.
    /// </summary>
    /// <param name="other">The path to verify.</param>
    /// <returns>True if this is a child path of the parent path; else false.</returns>
    public bool InFolder(RelativePath other)
    {
        return PathHelpers.InFolder(Path, other.Path, OS);
    }

    /// <summary>
    /// Drops first X directories of a path.
    /// </summary>
    /// <param name="numDirectories">Number of directories to drop.</param>
    public RelativePath DropFirst(int numDirectories = 1)
    {
        var res = PathHelpers.DropParents(Path, numDirectories, OS);
        return res.IsEmpty ? Empty : new RelativePath(res.ToString());
    }

    /// <summary>
    /// Returns a path relative to the sub-path specified.
    /// </summary>
    /// <param name="basePath">The sub-path specified.</param>
    public RelativePath RelativeTo(RelativePath basePath)
    {
        var other = basePath.Path;
        if (other.Length == 0) return this;

        var res = PathHelpers.RelativeTo(Path, other, OS);
        if (!res.IsEmpty) return new RelativePath(res.ToString());

        ThrowHelpers.PathException($"Path '{Path}' is not relative to '{other}'");
        return default;
    }

    /// <inheritdoc />
    public override string ToString() => Path;

    #region Equals & GetHashCode

    /// <inheritdoc />
    public bool Equals(RelativePath other) => PathHelpers.PathEquals(Path, other.Path, OS);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is RelativePath other && Equals(other);
    }

    /// <inheritdoc />
    [SkipLocalsInit]
    public override int GetHashCode()
    {
        return Path.AsSpan().GetNonRandomizedHashCode32();
    }

    #endregion

    /// <summary/>
    public static implicit operator string(RelativePath d) => d.Path;

    /// <summary/>
    public static implicit operator ReadOnlySpan<char>(RelativePath d) => d.Path;

    /// <summary/>
    public static implicit operator RelativePath(string b) => new(b);

    /// <summary/>
    public static bool operator ==(RelativePath lhs, RelativePath rhs) => lhs.Equals(rhs);

    /// <summary/>
    public static bool operator !=(RelativePath lhs, RelativePath rhs) => !(lhs == rhs);

    /// <inheritdoc />
    public int CompareTo(RelativePath other) => PathHelpers.Compare(Path, other.Path, OS);
}

/// <summary>
/// Compares two relative paths for sorting purposes.
/// </summary>
[PublicAPI]
public readonly struct RelativePathComparer : IComparer<RelativePath>
{
    /// <inheritdoc />
    public int Compare(RelativePath x, RelativePath y) => x.CompareTo(y);
}
