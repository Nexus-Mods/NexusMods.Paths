using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveBoxedChildrenWithKeyTests
{
    [Fact]
    public void EnumerateChildrenBfsAndDfs_ShouldReturnAllChildrenRecursively()
    {
        // Arrange
        var leaf1 = TestTree.Create();
        var leaf2 = TestTree.Create();
        var node = TestTree.Create(new()
        {
            [1] = leaf1,
            [2] = leaf2
        });
        var root = TestTree.Create(new() { [0] = node });

        // Act
        var allChildrenBfs = root.EnumerateChildrenBfs().ToArray();
        var allChildrenDfs = root.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildrenBfs.Should().HaveCount(3)
            .And.Contain(new KeyValuePair<int, KeyedBox<int, TestTree>>(0, node))
            .And.Contain(new KeyValuePair<int, KeyedBox<int, TestTree>>(1, leaf1))
            .And.Contain(new KeyValuePair<int, KeyedBox<int, TestTree>>(2, leaf2));
        allChildrenDfs.Should().HaveCount(3)
            .And.Contain(new KeyValuePair<int, KeyedBox<int, TestTree>>(0, node))
            .And.Contain(new KeyValuePair<int, KeyedBox<int, TestTree>>(1, leaf1))
            .And.Contain(new KeyValuePair<int, KeyedBox<int, TestTree>>(2, leaf2));
    }

    [Fact]
    public void EnumerateChildrenDfs_ShouldReturnAllChildrenInDepthFirstOrder()
    {
        // Arrange
        var grandChild = TestTree.Create();
        var child = TestTree.Create(new() { [1] = grandChild });
        var root = TestTree.Create(new() { [0] = child });

        // Act
        var allChildren = root.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(new KeyValuePair<int, KeyedBox<int, TestTree>>(0, child));
        allChildren[1].Should().BeEquivalentTo(new KeyValuePair<int, KeyedBox<int, TestTree>>(1, grandChild));
    }

    [Fact]
    public void EnumerateChildrenBfs_ShouldReturnAllChildrenInBreadthFirstOrder()
    {
        // Arrange
        var grandChild = TestTree.Create();
        var child = TestTree.Create(new() { [1] = grandChild });
        var root = TestTree.Create(new() { [0] = child });

        // Act
        var allChildren = root.EnumerateChildrenBfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(new KeyValuePair<int, KeyedBox<int, TestTree>>(0, child));
        allChildren[1].Should().BeEquivalentTo(new KeyValuePair<int, KeyedBox<int, TestTree>>(1, grandChild));
    }

    [Fact]
    public void CountChildren_ShouldReturnCorrectNumberOfDirectChildren()
    {
        // Arrange
        var child1 = TestTree.Create();
        var child2 = TestTree.Create();
        var root = TestTree.Create(new()
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
        var leaf = TestTree.Create();

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
        var grandChild = TestTree.Create();
        var child = TestTree.Create(new() { [1] = grandChild });
        var root = TestTree.Create(new() { [0] = child });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2); // Child + Grandchild
    }

    [Fact]
    public void GetChildrenRecursive_ShouldReturnAllChildrenIncludingDescendants()
    {
        // Arrange
        var grandChild1 = TestTree.Create(new ());
        var grandChild2 = TestTree.Create(new ());
        var child1 = TestTree.Create(new (){ {0, grandChild1} });
        var child2 = TestTree.Create(new (){ {0, grandChild2} });
        var children = new Dictionary<int, KeyedBox<int, TestTree>> { {0, child1}, {1, child2} };
        var root = TestTree.Create(children);

        // Act
        var allChildren = root.GetChildrenRecursive().ToArray();

        // Assert
        allChildren.Length.Should().Be(4); // Including child1, child2, grandChild1, grandChild2
        allChildren.Should().Contain(new[]{ child1, child2, grandChild1, grandChild2 });
    }

    [Fact]
    public void CountLeaves_ShouldReturnCorrectNumberOfLeafNodes()
    {
        // Arrange
        var leaf1 = TestTree.Create();
        var leaf2 = TestTree.Create();
        var node = TestTree.Create(new()
        {
            [1] = leaf1,
            [2] = leaf2
        });
        var root = TestTree.Create(new() { [0] = node });

        // Act
        var leafCount = root.CountLeaves();

        // Assert
        leafCount.Should().Be(2);
    }

    [Fact]
    public void GetLeaves_ShouldReturnAllLeafNodesRecursively()
    {
        // Arrange
        var leaf1 = TestTree.Create();
        var leaf2 = TestTree.Create();
        var node1 = TestTree.Create(new() { [1] = leaf1 });
        var node2 = TestTree.Create(new() { [2] = leaf2 });
        var root = TestTree.Create(new() { [0] = node1, [3] = node2 });

        // Act
        var leaves = root.GetLeaves().ToArray();

        // Assert
        leaves.Should().HaveCount(2)
            .And.Contain(new[]{ leaf1, leaf2 });
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>
    {
        public Dictionary<int, KeyedBox<int, TestTree>> Children { get; private init; }

        public static KeyedBox<int, TestTree> Create(Dictionary<int, KeyedBox<int, TestTree>>? children = null)
            => (KeyedBox<int, TestTree>) new TestTree()
        {
            Children = children ?? new Dictionary<int, KeyedBox<int, TestTree>>()
        };

        public bool Equals(TestTree other) => Children.Equals(other.Children);
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => Children.GetHashCode();
    }
}
