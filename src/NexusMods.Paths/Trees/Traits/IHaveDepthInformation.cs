namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     Represents trees with embedded depth information for each node.
/// </summary>
public interface IHaveDepthInformation
{
    /// <summary>
    /// Returns the depth of the node in the tree, with the root node having a depth of 0.
    /// So, in a FileTree `bar` in `/foo/bar/baz` will have a depth of 2 due to the `/` having a depth of 0.
    /// </summary>
    public ushort Depth { get; }
}
