using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHavePathSegmentWithKeyTests
{
    [Fact]
    public void FindByPathFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create(grandchild, "child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("child/grandchild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("grandchild");
    }

    [Fact]
    public void FindByPathFromChild_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create(grandchild, "child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("child"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("child");
    }

    [Fact]
    public void FindByPathFromChild_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create("child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromChild(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByPathFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create(grandchild, "child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child/grandchild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("grandchild");
    }

    /*
    // Broken because of bug: https://github.com/Nexus-Mods/NexusMods.Paths/issues/25
    [Fact]
    public void FindByPathFromRoot_WithExactPath_IsCaseInsensitive()
    {
        // Arrange
        var grandchild = TestTree.Create( "grandchild");
        var child = TestTree.Create(grandchild, "child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("Root/Child/GrandChild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("grandchild");
    }
    */

    [Fact]
    public void FindByPathFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create(grandchild, "child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("child");
    }

    [Fact]
    public void FindByPathFromRoot_WithSelfPath_ShouldReturnSelf()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create(grandchild, "child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("root");
    }

    [Fact]
    public void FindByPathFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create("child");
        var root = TestTree.Create(child, "root");

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<RelativePath, TestTree>, IHavePathSegment
    {
        public Dictionary<RelativePath, KeyedBox<RelativePath, TestTree>> Children { get; private init; }
        public RelativePath Segment { get; private init; }

        public static KeyedBox<RelativePath, TestTree> Create(string segment)
        {
            return (KeyedBox<RelativePath, TestTree>) new TestTree()
            {
                Children = new Dictionary<RelativePath, KeyedBox<RelativePath, TestTree>>(),
                Segment = new RelativePath(segment)
            };
        }

        public static KeyedBox<RelativePath, TestTree> Create(KeyedBox<RelativePath, TestTree> child, string segment)
        {
            return (KeyedBox<RelativePath, TestTree>) new TestTree()
            {
                Children = new Dictionary<RelativePath, KeyedBox<RelativePath, TestTree>> { { child.Item.Segment, child } },
                Segment = new RelativePath(segment)
            };
        }
    }
}
