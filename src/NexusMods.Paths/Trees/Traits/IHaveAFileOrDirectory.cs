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
