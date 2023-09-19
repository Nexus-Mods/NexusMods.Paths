using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using NexusMods.Paths.Benchmarks.Interfaces;
using NexusMods.Paths.FileTree;

namespace NexusMods.Paths.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[BenchmarkInfo("Improved File Tree Get Descendants", "Improved Version of GetDescendants for FileTree")]
public class FileTreeGetDescendants : IBenchmark
{
    private readonly FileTreeNode<RelativePath, int> _files;

    public FileTreeGetDescendants()
    {
        var files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "*", SearchOption.AllDirectories);
        Console.WriteLine("Files: " + files.Length);
        var entries = files.ToDictionary(x => new RelativePath(x.TrimStart('/')), x => x.Length);
        _files = FileTreeNode<RelativePath, int>.CreateTree(entries);
    }

    // We must enumerate the results to ensure we're not just returning a lazy enumerable.
    [Benchmark]
    public List<FileTreeNode<RelativePath, int>> GetDescendants()
    {
        return _files.GetAllDescendentFiles();
    }

    [Benchmark]
    public List<FileTreeNode<RelativePath, int>> OriginalGetDescendants()
    {
        return GetAllDescendentFilesOriginal(_files).ToList();
    }

    // Original Implementation
    public IEnumerable<FileTreeNode<TPath, TValue>> GetAllDescendentFilesOriginal<TPath, TValue>(FileTreeNode<TPath, TValue> node) where TPath : struct, IPath<TPath>, IEquatable<TPath>
    {
        if (node.IsFile) return Enumerable.Empty<FileTreeNode<TPath, TValue>>();
        if (!node.Children.Any()) return Enumerable.Empty<FileTreeNode<TPath, TValue>>();

        return node.Children.Values.SelectMany(x => { return x.IsFile ? new[] { x } : GetAllDescendentFilesOriginal(x); });
    }
}
