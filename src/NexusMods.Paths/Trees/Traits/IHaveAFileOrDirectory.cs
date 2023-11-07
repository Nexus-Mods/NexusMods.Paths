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
        // Branchless increment.
        bool isFileBool = item.IsFile;
        accumulator += Unsafe.As<bool, byte>(ref isFileBool);
        foreach (var child in item.Children)
            child.Value.Item.CountFilesRecursive<TSelf, TKey>(ref accumulator);
    }

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
        // Branchless increment.
        bool isFileBool = item.IsDirectory;
        accumulator += Unsafe.As<bool, byte>(ref isFileBool);
        foreach (var child in item.Children)
            child.Value.Item.CountFilesRecursive<TSelf, TKey>(ref accumulator);
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
        // Branchless increment.
        bool isFileBool = item.IsFile;
        accumulator += Unsafe.As<bool, byte>(ref isFileBool);
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
            child.CountFilesRecursive(ref accumulator);
    }

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
        bool isFileBool = item.IsDirectory;
        accumulator += Unsafe.As<bool, byte>(ref isFileBool);
        foreach (var child in item.Children) // <= lowered to 'for loop' because array.
            child.CountFilesRecursive(ref accumulator);
    }
}
