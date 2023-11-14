using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHavePathSegmentTests
{
    [Fact]
    public void ReconstructPath_WithSingleNode_ReturnsSingleSegment()
    {
        var root = new TestTree(Array.Empty<Box<TestTree>>())
        {
            Segment = new RelativePath("root")
        };

        var path = root.ReconstructPath();
        path.ToString().Should().Be("root");
    }

    [Fact]
    public void ReconstructPath_WithChildNode_ReturnsBothSegments()
    {
        var root = new TestTree(Array.Empty<Box<TestTree>>())
        {
            Segment = new RelativePath("root")
        };

        var child = new TestTree(Array.Empty<Box<TestTree>>())
        {
            Parent = new Box<TestTree> { Item = root },
            Segment = new RelativePath("child")
        };

        var path = child.ReconstructPath();
        path.ToString().Should().Be("root/child");
    }

    [Fact]
    public void ReconstructPath_WithNestedNodes_ReturnsAllSegments()
    {
        var root = new TestTree(Array.Empty<Box<TestTree>>())
        {
            Segment = new RelativePath("root")
        };

        var intermediate = new TestTree(Array.Empty<Box<TestTree>>())
        {
            Parent = new Box<TestTree> { Item = root },
            Segment = new RelativePath("intermediate")
        };

        var child = new TestTree(Array.Empty<Box<TestTree>>())
        {
            Parent = new Box<TestTree> { Item = intermediate },
            Segment = new RelativePath("child")
        };

        var path = child.ReconstructPath();
        path.ToString().Should().Be("root/intermediate/child");
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveParent<TestTree>, IHavePathSegment
    {
        public Box<TestTree>[] Children { get; }
        public Box<TestTree>? Parent { get; set; }
        public RelativePath Segment { get; set; }

        public TestTree(Box<TestTree>[] children)
        {
            Children = children;
        }
    }
}
