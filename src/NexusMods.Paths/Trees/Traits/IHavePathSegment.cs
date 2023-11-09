using System;
using NexusMods.Paths.Extensions;
using NexusMods.Paths.HighPerformance.CommunityToolkit;

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

/// <summary>
///     Extensions tied to the <see cref="IHavePathSegment" />
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IHavePathSegmentExtensions
{
    /// <summary>
    ///     Reconstructs the full path of the current node by walking up to the root to the tree.
    /// </summary>
    /// <param name="item">The node for which to reconstruct the file path from.</param>
    public static RelativePath ReconstructPath<TSelf>(this TSelf item)
        where TSelf : struct, IHavePathSegment, IHaveParent<TSelf>
    {
        // Remember to follow the rules:
        // Pre-calculate the length of the path.
        var pathLength = item.Segment.Length;

        // Start with the current item and walk up the tree.
        var currentItem = item;
        while (currentItem.HasParent)
        {
            // Each parent adds its own segment length plus one for the separator.
            pathLength += currentItem.Parent!.Item.Segment.Length + 1;
            currentItem = currentItem.Parent;
        }

        // Create the string.
        static void Action(Span<char> span, TSelf item)
        {
            var currentItem = item;
            var position = span.Length;

            // Append the current (leaf) segment first.
            var segmentSpan = currentItem.Segment.Path.AsSpan();
            position -= segmentSpan.Length;
            segmentSpan.CopyTo(span.SliceFast(position));

            // Walk up the tree to build the path.
            while (currentItem.HasParent)
            {
                // Add the path separator.
                span.DangerousGetReferenceAt(--position) = '/';

                // Get the parent segment.
                currentItem = currentItem.Parent!.Item; // ignored because has parent
                segmentSpan = currentItem.Segment.Path.AsSpan();

                // Copy the parent segment.
                position -= segmentSpan.Length;
                segmentSpan.CopyTo(span.SliceFast(position));
            }
        }

        return string.Create(pathLength, item, Action);
    }
}
