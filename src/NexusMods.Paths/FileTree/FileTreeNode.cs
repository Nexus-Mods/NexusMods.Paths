using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace NexusMods.Paths.FileTree;

/// <summary>
/// Represents a generic tree of files with some associated value.
/// </summary>
/// <typeparam name="TPath">The path type used in the tree</typeparam>
/// <typeparam name="TValue"></typeparam>
[PublicAPI]
public class FileTreeNode<TPath, TValue> : IFileTree<FileTreeNode<TPath, TValue>>
    where TPath : struct, IPath<TPath>, IEquatable<TPath>
{
    private readonly bool _isFile;

    private readonly Dictionary<RelativePath, FileTreeNode<TPath, TValue>> _children;
    private FileTreeNode<TPath, TValue>? _parent;

    /// <summary>
    /// Constructs a new <see cref="FileTreeNode{TPath,TValue}"/> with the given path, name, parent and value.
    /// </summary>
    /// <remarks>If the value is null, this node is assumed to be a directory, a file otherwise.</remarks>
    /// <param name="path">The complete path for the node with respect to the root of the tree</param>
    /// <param name="name">The file name for the node</param>
    /// <param name="value">The associated value to be stored along a file entry. Should be null for directories.</param>
    public FileTreeNode(TPath path, RelativePath name, TValue? value)
    {
        Path = path;
        Name = name;
        _isFile = value != null;
        Value = value;
        _children = new Dictionary<RelativePath, FileTreeNode<TPath, TValue>>();
    }

    /// <summary>
    /// The complete path of the node with respect to the root of the tree.
    /// </summary>
    public TPath Path { get; }

    /// <inheritdoc />
    public RelativePath Name { get; }

    /// <inheritdoc />
    public bool IsFile => _isFile;

    /// <inheritdoc />
    public bool IsDirectory => !_isFile;

    /// <inheritdoc />
    public bool HasParent => _parent != null;

    /// <inheritdoc />
    public bool IsTreeRoot => !HasParent;

    /// <summary>
    /// A value associated with a File entry, null for directories.
    /// </summary>
    public TValue? Value { get; }

    /// <inheritdoc />
    public IDictionary<RelativePath, FileTreeNode<TPath, TValue>> Children => _children;

    /// <inheritdoc />
    public FileTreeNode<TPath, TValue> Parent
    {
        get
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("Root node has no parent");
            }

            return _parent;
        }
    }

    /// <inheritdoc />
    public FileTreeNode<TPath, TValue> Root
    {
        get
        {
            var current = this;
            while (current.HasParent)
            {
                current = current.Parent;
            }

            return current;
        }
    }

    /// <inheritdoc />
    public IEnumerable<FileTreeNode<TPath, TValue>> GetSiblings()
    {
        if (IsTreeRoot) return Enumerable.Empty<FileTreeNode<TPath, TValue>>();

        return Parent.Children.Values.Where(x => x != this);
    }

    /// <inheritdoc />
    public IEnumerable<FileTreeNode<TPath, TValue>> GetAllDescendentFiles()
    {
        if (IsFile) return Enumerable.Empty<FileTreeNode<TPath, TValue>>();

        return Children.Values.SelectMany(x => x.GetAllDescendentFiles());
    }

    /// <summary>
    /// Returns a dictionary of all the file entries under the current node.
    /// </summary>
    /// <returns>A dictionary of with <see cref="Path"/> as keys and <see cref="Value"/> as values.</returns>
    public Dictionary<TPath, TValue> GetAllDescendentFilesDictionary()
    {
        return GetAllDescendentFiles().ToDictionary(file => file.Path, file => file.Value!);
    }

    /// <summary>
    /// Recursively searches the subtree for a node with the given path.
    /// </summary>
    /// <param name="searchedPath">A complete path with respect to the tree root</param>
    /// <returns>null if not found</returns>
    public FileTreeNode<TPath, TValue>? FindNode(TPath searchedPath)
    {
        var currentNode = this;

        while (searchedPath.InFolder(currentNode.Path))
        {
            if (currentNode.Path.Equals(searchedPath)) return currentNode;

            currentNode = currentNode.Children.Values.FirstOrDefault(x => searchedPath.InFolder(x.Path));

            if (currentNode == null) return null;
        }

        return null;
    }

    /// <summary>
    /// Adds a collection of nodes as children to this node.
    /// </summary>
    /// <remarks>
    /// Sets the parent of the children to this node.
    /// </remarks>
    /// <param name="children">The collection of nodes to be added</param>
    public void AddChildren(IEnumerable<FileTreeNode<TPath, TValue>> children)
    {
        foreach (var child in children)
        {
            child._parent = this;
            AddChild(child);
        }
    }

    /// <summary>
    /// Adds a node as a child to this node.
    /// </summary>
    /// <remarks>
    /// Sets the parent of the child to this node.
    /// </remarks>
    /// <param name="child">The node to be added</param>
    public void AddChild(FileTreeNode<TPath, TValue> child)
    {
        child._parent = this;
        _children.Add(child.Name, child);
    }


    /// <summary>
    /// Creates a tree structure from a collection of file entries.
    /// </summary>
    /// <remarks>
    /// If the file paths are rooted, they all need to be sharing the same root component.
    /// If the paths are not rooted, they are assumed to be relative to the same unknown root.
    /// </remarks>
    /// <param name="files">A non empty collection of unique file entries in the form of TPath,TValue pairs.</param>
    /// <returns>The root node of the generated tree.</returns>
    /// <throws><see cref="ArgumentException"/> if the collection is empty.</throws>
    public static FileTreeNode<TPath, TValue> CreateTree(IEnumerable<KeyValuePair<TPath, TValue>> files)
    {
        if (!files.Any()) throw new ArgumentException("Collection of files cannot be empty");

        var fileArray = files.ToArray();
        var rootComponent = fileArray.First().Key.GetRootComponent;

        // If paths are rooted, we assume all the passed paths share the same root
        // If paths are not rooted, we assume they are all relative to the same unknown root (RelativePath.Empty)
        var rootNode = new FileTreeNode<TPath, TValue>(rootComponent, RelativePath.Empty, default);

        rootNode.PopulateTree(fileArray);

        return rootNode;
    }

    /// <summary>
    /// Populates the tree with the given collection of file entries.
    /// </summary>
    /// <remarks>
    /// The current node is assumed to be the root of the tree.
    /// All file paths are assumed to be relative to the root of the tree.
    /// </remarks>
    /// <param name="files">A collection of unique files in the form of TPath,TValue pairs.</param>
    /// <exception cref="InvalidOperationException">If there are duplicate file entries.</exception>
    protected void PopulateTree(IEnumerable<KeyValuePair<TPath, TValue>> files)
    {
        foreach (var fileEntry in files)
        {
            var parentNode = this;
            var currentFullPath = fileEntry.Key;

            var parentPaths = currentFullPath.GetAllParents().Reverse().ToArray();

            // traverse the path from root to leaf and add missing nodes
            for (var i = 0; i < parentPaths.Length; i++)
            {
                // if the path is rooted, skip the first path as it is the root component, which we already have
                if (Path.IsRooted && i == 0) continue;

                var subPath = parentPaths[i];
                var subPathName = subPath.Name;

                if (parentNode.Children.TryGetValue(subPathName, out var existing))
                {
                    // if we are at the last path, this is the file
                    if (i == parentPaths.Length - 1)
                    {
                        throw new InvalidOperationException($"Duplicate path found for file: {subPath}");
                    }

                    parentNode = existing;
                    continue;
                }

                // if we are at the last path, this is the file
                var value = i == parentPaths.Length - 1 ? fileEntry.Value : default;

                var node = new FileTreeNode<TPath, TValue>(subPath, subPathName, value);

                parentNode.AddChild(node);

                parentNode = node;
            }
        }
    }
}
