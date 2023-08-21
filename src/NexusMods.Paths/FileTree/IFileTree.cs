using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NexusMods.Paths.FileTree;

/// <summary>
/// Represents a generic tree of files.
/// </summary>
/// <typeparam name="TFileTree">Allows implementations to return concrete types</typeparam>
/// <typeparam name="TPath">The path type being used in the tree</typeparam>
[PublicAPI]
public interface IFileTree<TFileTree, TPath> where TFileTree : IFileTree<TFileTree, TPath>
    where TPath : struct, IPath<TPath>
{
    /// <summary>
    /// The complete path of the node with respect to the root of the tree.
    /// </summary>
    public TPath Path { get; }

    /// <summary>
    /// The file name for this node.
    /// </summary>
    public RelativePath Name { get; }

    /// <summary>
    /// Returns true if node is assumed to be a file.
    /// </summary>
    public bool IsFile { get; }

    /// <summary>
    /// Returns true if node is assumed to be a directory.
    /// </summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Returns true if node is the root of the tree.
    /// </summary>
    public bool IsTreeRoot { get; }

    /// <summary>
    /// Returns tre if node has a parent.
    /// </summary>
    public bool HasParent { get; }

    /// <summary>
    /// A Dictionary containing all the children of this node, both files and directories.
    /// The key is the <see cref="Name"/> of the child.
    /// </summary>
    public IDictionary<RelativePath, TFileTree> Children { get; }

    /// <summary>
    /// Returns the parent node if it exists, throws InvalidOperationException otherwise.
    /// </summary>
    public TFileTree Parent { get; }

    /// <summary>
    /// Returns the root node of the tree.
    /// </summary>
    public TFileTree Root { get; }

    /// <summary>
    /// A collection of all sibling nodes, excluding this one.
    /// Returns an empty collection if this node is the root.
    /// </summary>
    public IEnumerable<TFileTree> GetSiblings();

    /// <summary>
    /// A collection of all File nodes that descend from this one.
    /// Returns an empty collection if this node is a file.
    /// </summary>
    public IEnumerable<TFileTree> GetAllFileDescendants();
}
