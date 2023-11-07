namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     Represents nodes which have a path segment contained within.
///     A path segment being a sub-path for the given directory in a file tree.
/// </summary>
public interface IHavePathSegment
{
    /// <summary>
    ///     The path segment of the current node.
    /// </summary>
    public RelativePath Segment { get; }
}
