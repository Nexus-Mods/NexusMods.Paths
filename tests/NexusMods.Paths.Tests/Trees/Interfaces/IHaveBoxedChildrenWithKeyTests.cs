using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveBoxedChildrenWithKeyTests
{
    [Fact]
    public void EnumerateChildrenBfsAndDfs_ShouldReturnAllChildrenRecursively()
    {
        // Arrange
        var leaf1 = new TestTree(new());
        var leaf2 = new TestTree(new());
        var node = new TestTree(new()
        {
            [1] = leaf1 ,
            [2] = leaf2
        });
        ChildWithKeyBox<int, TestTree> root = new TestTree(new() { [0] = node });

        // Act
        var allChildrenBfs = root.EnumerateChildrenBfs().ToArray();
        var allChildrenDfs = root.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildrenBfs.Should().HaveCount(3)
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(0, node))
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(1, leaf1))
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(2, leaf2));
        allChildrenDfs.Should().HaveCount(3)
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(0, node))
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(1, leaf1))
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(2, leaf2));
    }

    [Fact]
    public void EnumerateChildrenDfs_ShouldReturnAllChildrenInDepthFirstOrder()
    {
        // Arrange
        var grandChild = new TestTree(new());
        var child = new TestTree(new() { [1] = grandChild });
        ChildWithKeyBox<int, TestTree> root = new TestTree(new() { [0] = child });

        // Act
        var allChildren = root.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(0, child));
        allChildren[1].Should().BeEquivalentTo(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(1, grandChild));
    }

    [Fact]
    public void EnumerateChildrenBfs_ShouldReturnAllChildrenInBreadthFirstOrder()
    {
        // Arrange
        var grandChild = new TestTree(new());
        var child = new TestTree(new() { [1] = grandChild });
        ChildWithKeyBox<int, TestTree> root = new TestTree(new() { [0] = child });

        // Act
        var allChildren = root.EnumerateChildrenBfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(0, child));
        allChildren[1].Should().BeEquivalentTo(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(1, grandChild));
    }

    [Fact]
    public void CountChildren_ShouldReturnCorrectNumberOfDirectChildren()
    {
        // Arrange
        var child1 = new TestTree(new());
        var child2 = new TestTree(new());
        ChildWithKeyBox<int, TestTree> root = new TestTree(new()
        {
            [1] = child1,
            [2] = child2
        });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void CountChildren_ShouldReturnZeroForLeafNode()
    {
        // Arrange
        ChildWithKeyBox<int, TestTree> leaf = new TestTree(new());

        // Act
        var count = leaf.CountChildren();

        // Assert
        count.Should().Be(0);
        leaf.IsLeaf().Should().BeTrue();
    }

    [Fact]
    public void CountChildrenRecursive_ShouldReturnTotalNumberOfAllChildren()
    {
        // Arrange
        var grandChild = new TestTree(new());
        var child = new TestTree(new() { [1] = grandChild });
        ChildWithKeyBox<int, TestTree> root = new TestTree(new() { [0] = child });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2); // Child + Grandchild
    }

    [Fact]
    public void GetChildrenRecursive_ShouldReturnAllChildrenIncludingDescendants()
    {
        // Arrange
        var grandChild1 = new TestTree(new ());
        var grandChild2 = new TestTree(new ());
        var child1 = new TestTree(new (){ {0, grandChild1} });
        var child2 = new TestTree(new (){ {0, grandChild2} });
        var children = new Dictionary<int, ChildWithKeyBox<int, TestTree>> { {0, child1}, {1, child2} };
        ChildWithKeyBox<int, TestTree> root = new TestTree(children);

        // Act
        var allChildren = root.GetChildrenRecursive().ToArray();

        // Assert
        allChildren.Length.Should().Be(4); // Including child1, child2, grandChild1, grandChild2
        allChildren.Should().Contain(new ChildWithKeyBox<int, TestTree>[]{ child1, child2, grandChild1, grandChild2 });
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>> children) => Children = children;

        public bool Equals(TestTree other) => Children.Equals(other.Children);
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => Children.GetHashCode();
    }
}
