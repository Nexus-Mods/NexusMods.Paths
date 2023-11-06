namespace NexusMods.Paths.Trees.Traits;

/// <summary>
///     An interface used by FileTree implementations to indicate that they have a parent.
/// </summary>
/// <typeparam name="TSelf">The concrete type stored inside this interface.</typeparam>
public interface IHaveParent<out TSelf> where TSelf : IHaveParent<TSelf>
{
    /// <summary>
    ///     Returns the parent node if it exists. If not, the node is considered the root node.
    /// </summary>
    public TSelf? Parent { get; }

    /// <summary>
    ///     Returns true if the tree has a parent.
    /// </summary>
    public bool HasParent => Parent != null;

    /// <summary>
    ///     Returns true if this is the root of the tree.
    /// </summary>
    public bool IsTreeRoot => !HasParent;
}
