using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHavePathSegmentWithKeyTests
{
    [Fact]
    public void FindByPathFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree("grandchild");
        var child = new TestTree(grandchild, "child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("child/grandchild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("grandchild");
    }

    [Fact]
    public void FindByPathFromChild_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree("grandchild");
        var child = new TestTree(grandchild, "child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("child"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("child");
    }

    [Fact]
    public void FindByPathFromChild_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree("child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByPathFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree("grandchild");
        var child = new TestTree(grandchild, "child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child/grandchild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("grandchild");
    }

    /*
    // Broken because of bug: https://github.com/Nexus-Mods/NexusMods.Paths/issues/25
    [Fact]
    public void FindByPathFromRoot_WithExactPath_IsCaseInsensitive()
    {
        // Arrange
        var grandchild = new TestTree( "grandchild");
        var child = new TestTree(grandchild, "child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("Root/Child/GrandChild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("grandchild");
    }
    */

    [Fact]
    public void FindByPathFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree("grandchild");
        var child = new TestTree(grandchild, "child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("child");
    }

    [Fact]
    public void FindByPathFromRoot_WithSelfPath_ShouldReturnSelf()
    {
        // Arrange
        var grandchild = new TestTree("grandchild");
        var child = new TestTree(grandchild, "child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("root");
    }

    [Fact]
    public void FindByPathFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree("child");
        ChildWithKeyBox<RelativePath, TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<RelativePath, TestTree>, IHavePathSegment
    {
        public Dictionary<RelativePath, ChildWithKeyBox<RelativePath, TestTree>> Children { get; }
        public RelativePath Segment { get; }

        public TestTree(string segment)
        {
            Children = new Dictionary<RelativePath, ChildWithKeyBox<RelativePath, TestTree>>();
            Segment = new RelativePath(segment);
        }

        public TestTree(TestTree child, string segment)
        {
            Children = new Dictionary<RelativePath, ChildWithKeyBox<RelativePath, TestTree>>() { { child.Segment, child } };
            Segment = new RelativePath(segment);
        }
    }
}
