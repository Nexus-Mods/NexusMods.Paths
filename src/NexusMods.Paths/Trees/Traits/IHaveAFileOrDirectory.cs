using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by Tree implementations to indicate whether an item is a file/directory.
/// </summary>
public interface IHaveAFileOrDirectory
{
    /// <summary>
    ///     Returns true if this item represents a file.
    /// </summary>
    public bool IsFile { get; }

    /// <summary>
    ///     Returns true if this item represents a directory.
    /// </summary>
    public bool IsDirectory => !IsFile;
}

/// <summary>
///     Trait methods for <see cref="IHaveBoxedChildrenWithKey{TKey,TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveAFileOrDirectoryExtensionsForIHaveBoxedChildrenWithKey
{
    /// <summary>
    ///     Counts the number of files present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total file count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountFiles<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveAFileOrDirectory
        where TKey : notnull =>
        item.Item.CountFiles<TSelf, TKey>();

    /// <summary>
    ///     Counts the number of files present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total file count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountFiles<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveAFileOrDirectory
        where TKey : notnull
    {
        var result = 0;
        item.CountFilesRecursive<TSelf, TKey>(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountFilesRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveAFileOrDirectory where TKey : notnull
    {
        foreach (var child in item.Children)
        {
            var isDir = child.Value.Item.IsFile; // <= branchless increment.
            accumulator += Unsafe.As<bool, byte>(ref isDir);
            child.Value.Item.CountFilesRecursive<TSelf, TKey>(ref accumulator);
        }
    }

    /// <summary>
    ///     Counts the number of directories present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior directory count is to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total directory count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDirectories<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveAFileOrDirectory
        where TKey : notnull  => item.Item.CountDirectories<TSelf, TKey>();

    /// <summary>
    ///     Counts the number of directories present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior directory count is to be counted.</param>
    /// <typeparam name="TKey">The type of key used to identify children.</typeparam>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total directory count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDirectories<TSelf, TKey>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveAFileOrDirectory
        where TKey : notnull
    {
        var result = 0;
        item.CountDirectoriesRecursive<TSelf, TKey>(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountDirectoriesRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>, IHaveAFileOrDirectory where TKey : notnull
    {
        foreach (var child in item.Children)
        {
            var isDir = child.Value.Item.IsDirectory; // <= branchless increment.
            accumulator += Unsafe.As<bool, byte>(ref isDir);
            child.Value.Item.CountDirectoriesRecursive<TSelf, TKey>(ref accumulator);
        }
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveBoxedChildren{TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveAFileOrDirectoryExtensionsForIHaveBoxedChildren
{
    /// <summary>
    ///      Counts the number of files present under this node.
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total file count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountFiles<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveAFileOrDirectory =>
        item.Item.CountFiles();

    /// <summary>
    ///      Counts the number of files present under this node.
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total file count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountFiles<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveAFileOrDirectory
    {
        var result = 0;
        item.CountFilesRecursive(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountFilesRecursive<TSelf>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveAFileOrDirectory
    {
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
        {
            var isFile = child.Item.IsFile; // <= branchless increment.
            accumulator += Unsafe.As<bool, byte>(ref isFile);
            child.Item.CountFilesRecursive(ref accumulator);
        }
    }

    /// <summary>
    ///      Counts the number of directories present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total directory count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDirectories<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveAFileOrDirectory =>
        item.Item.CountDirectories();

    /// <summary>
    ///      Counts the number of directories present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total directory count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDirectories<TSelf>(this TSelf item)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveAFileOrDirectory
    {
        var result = 0;
        item.CountDirectoriesRecursive(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountDirectoriesRecursive<TSelf>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveBoxedChildren<TSelf>, IHaveAFileOrDirectory
    {
        // Branchless increment.
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
        {
            var isDir = child.Item.IsDirectory; // <= branchless increment.
            accumulator += Unsafe.As<bool, byte>(ref isDir);
            child.Item.CountDirectoriesRecursive(ref accumulator);
        }
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveObservableChildren{TSelf}" />.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHaveAFileOrDirectoryExtensionsForIHaveObservableChildren
{
    /// <summary>
    ///      Counts the number of files present under this node.
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total file count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountFiles<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveAFileOrDirectory =>
        item.Item.CountFiles();

    /// <summary>
    ///      Counts the number of files present under this node.
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total file count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountFiles<TSelf>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveAFileOrDirectory
    {
        var result = 0;
        item.CountFilesRecursive(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountFilesRecursive<TSelf>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveAFileOrDirectory
    {
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
        {
            var isFile = child.Item.IsFile; // <= branchless increment.
            accumulator += Unsafe.As<bool, byte>(ref isFile);
            child.Item.CountFilesRecursive(ref accumulator);
        }
    }

    /// <summary>
    ///      Counts the number of directories present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total directory count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDirectories<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveAFileOrDirectory =>
        item.Item.CountDirectories();

    /// <summary>
    ///      Counts the number of directories present under this node (directory).
    /// </summary>
    /// <param name="item">The node (directory) whose interior file count is to be counted.</param>
    /// <typeparam name="TSelf">The type of child node.</typeparam>
    /// <returns>The total directory count under this node.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDirectories<TSelf>(this TSelf item)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveAFileOrDirectory
    {
        var result = 0;
        item.CountDirectoriesRecursive(ref result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountDirectoriesRecursive<TSelf>(this TSelf item, ref int accumulator)
        where TSelf : struct, IHaveObservableChildren<TSelf>, IHaveAFileOrDirectory
    {
        // Branchless increment.
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
        {
            var isDir = child.Item.IsDirectory; // <= branchless increment.
            accumulator += Unsafe.As<bool, byte>(ref isDir);
            child.Item.CountDirectoriesRecursive(ref accumulator);
        }
    }
}

/// <summary>
///     Trait methods for <see cref="IHaveAFileOrDirectory"/>.
/// </summary>
[ExcludeFromCodeCoverage] // Wrapper
// ReSharper disable once InconsistentNaming
public static class IHaveAFileOrDirectoryExtensions
{
    /// <summary>
    ///     Checks if the item in the keyed box is a file.
    /// </summary>
    /// <param name="item">The keyed box containing the item to check.</param>
    /// <returns>True if the item is a file; otherwise, false.</returns>
    public static bool IsFile<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveAFileOrDirectory
        where TKey : notnull
        => item.Item.IsFile;

    /// <summary>
    ///     Checks if the item in the box is a file.
    /// </summary>
    /// <param name="item">The box containing the item to check.</param>
    /// <returns>True if the item is a file; otherwise, false.</returns>
    public static bool IsFile<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveAFileOrDirectory
        => item.Item.IsFile;

    /// <summary>
    ///     Checks if the item in the keyed box is a directory.
    /// </summary>
    /// <param name="item">The keyed box containing the item to check.</param>
    /// <returns>True if the item is a directory; otherwise, false.</returns>
    public static bool IsDirectory<TSelf, TKey>(this KeyedBox<TKey, TSelf> item)
        where TSelf : struct, IHaveAFileOrDirectory
        where TKey : notnull
        => item.Item.IsDirectory;

    /// <summary>
    ///     Checks if the item in the box is a directory.
    /// </summary>
    /// <param name="item">The box containing the item to check.</param>
    /// <returns>True if the item is a directory; otherwise, false.</returns>
    public static bool IsDirectory<TSelf>(this Box<TSelf> item)
        where TSelf : struct, IHaveAFileOrDirectory
        => item.Item.IsDirectory;
}
