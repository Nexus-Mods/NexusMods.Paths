using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHavePathSegmentTests
{
    [Fact]
    public void FindByPathFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create("child", grandchild);
        var root = TestTree.Create("root", child);

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
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create("child", grandchild);
        var root = TestTree.Create("root", child);

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
        var child = TestTree.Create("child");
        var root = TestTree.Create("root", child);

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
        var child = TestTree.Create("child", grandchild);
        var root = TestTree.Create("root", child);

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child/grandchild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("grandchild");
    }

    [Fact]
    public void FindByPathFromRoot_WithExactPath_IsCaseInsensitive()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create("child", grandchild);
        var root = TestTree.Create("root", child);

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("Root/Child/GrandChild"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("grandchild");
    }

    [Fact]
    public void FindByPathFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create("grandchild");
        var child = TestTree.Create("child", grandchild);
        var root = TestTree.Create("root", child);

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/child"));

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Segment.Path.Should().Be("child");
    }

    [Fact]
    public void FindByPathFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create("child");
        var root = TestTree.Create("root", child);

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("non/existing/path"));

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByPathFromRoot_WithNonExistingChild_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create("child");
        var root = TestTree.Create("root", child);

        // Act
        var foundNode = root.FindByPathFromRoot(new RelativePath("root/fakechild"));

        // Assert
        foundNode!.Should().BeNull();
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHavePathSegment
    {
        public Box<TestTree>[] Children { get; init; }
        public RelativePath Segment { get; init; }

        public static Box<TestTree> Create(string segment, Box<TestTree> child)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = new[] { child },
                Segment = segment,
            };
        }

        public static Box<TestTree> Create(string segment)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = Array.Empty<Box<TestTree>>(),
                Segment = segment,
            };
        }
    }
}
