using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHavePathSegmentTests
{
    [Fact]
    public void FindByPathFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree(null, "grandchild");
        var child = new TestTree(grandchild, "child");
        ChildBox<TestTree> root = new TestTree(child, "root");

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
        var grandchild = new TestTree(null, "grandchild");
        var child = new TestTree(grandchild, "child");
        ChildBox<TestTree> root = new TestTree(child, "root");

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
        var child = new TestTree(null, "child");
        ChildBox<TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByPathFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree(null, "grandchild");
        var child = new TestTree(grandchild, "child");
        ChildBox<TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child/grandchild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("grandchild");
    }

    [Fact]
    public void FindByPathFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree(null, "grandchild");
        var child = new TestTree(grandchild, "child");
        ChildBox<TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Segment.Path.Should().Be("child");
    }

    [Fact]
    public void FindByPathFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree(null, "child");
        ChildBox<TestTree> root = new TestTree(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHavePathSegment
    {
        public ChildBox<TestTree>[] Children { get; set; }
        public RelativePath Segment { get; set; }

        public TestTree(ChildBox<TestTree>[]? children, string segment)
        {
            Children = children ?? Array.Empty<ChildBox<TestTree>>();
            Segment = segment;
        }

        public TestTree(TestTree child, string segment)
        {
            Children = new ChildBox<TestTree>[] {child };
            Segment = segment;
        }
    }
}