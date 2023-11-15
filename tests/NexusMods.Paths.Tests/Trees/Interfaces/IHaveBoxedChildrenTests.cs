using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveBoxedChildrenTests
{
    [Fact]
    public void EnumerateChildren_ShouldReturnAllChildrenRecursively()
    {
        // Arrange
        var leaf1 = TestTree.Create();
        var leaf2 = TestTree.Create();
        var node = TestTree.Create(new[] { leaf1, leaf2 });
        var root = TestTree.Create(node);

        // Act
        var allChildrenBfs = root.EnumerateChildrenBfs().ToArray();
        var allChildrenDfs = root.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildrenBfs.Should().HaveCount(3).And.Contain(new[] { node, leaf1, leaf2 });
        allChildrenDfs.Should().HaveCount(3).And.Contain(new[] { node, leaf1, leaf2 });
    }

    [Fact]
    public void EnumerateChildrenDfs_ShouldReturnAllChildrenInDepthFirstOrder()
    {
        // Arrange
        var grandChild = TestTree.Create();
        var child = TestTree.Create(grandChild);
        var root = TestTree.Create(child);

        // Act
        var allChildren = root.Item.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(child);
        allChildren[1].Should().BeEquivalentTo(grandChild);
    }

    [Fact]
    public void EnumerateChildrenBfs_ShouldReturnAllChildrenInBreadthFirstOrder()
    {
        // Arrange
        var grandChild = TestTree.Create();
        var child = TestTree.Create(new[] { grandChild });
        var root = TestTree.Create(new[] { child });

        // Act
        var allChildren = root.Item.EnumerateChildrenBfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(child);
        allChildren[1].Should().BeEquivalentTo(grandChild);
    }

    [Fact]
    public void CountChildren_ShouldReturnCorrectNumberOfDirectChildren()
    {
        // Arrange
        var child1 = TestTree.Create();
        var child2 = TestTree.Create();
        var root = TestTree.Create(new[] { child1, child2 });

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
        var child = TestTree.Create(grandChild);
        var root = TestTree.Create(child);

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void GetChildrenRecursive_ShouldReturnAllChildrenIncludingDescendants()
    {
        // Arrange
        var grandChild1 = TestTree.Create();
        var grandChild2 = TestTree.Create();
        var child1 = TestTree.Create(grandChild1);
        var child2 = TestTree.Create(grandChild2);
        var children = new[] { child1, child2 };
        var root = TestTree.Create(children);

        // Act
        var allChildren = root.GetChildrenRecursive().ToArray();

        // Assert
        allChildren.Length.Should().Be(4); // Including child1, child2, grandChild1, grandChild2
        allChildren.Should().Contain(new[] { child1, child2, grandChild1, grandChild2 });
    }

    [Fact]
    public void CountLeaves_ShouldReturnCorrectNumberOfLeafNodes()
    {
        // Arrange
        var leaf1 = TestTree.Create();
        var leaf2 = TestTree.Create();
        var node = TestTree.Create(new[] { leaf1, leaf2 });
        var root = TestTree.Create(new[] { node });

        // Act
        var leafCount = root.CountLeaves();

        // Assert
        leafCount.Should().Be(2); // leaf1 and leaf2 are leaves
    }

    [Fact]
    public void GetLeaves_ShouldReturnAllLeafNodes()
    {
        // Arrange
        var leaf1 = TestTree.Create();
        var leaf2 = TestTree.Create();
        var node = TestTree.Create(new[] { leaf1, leaf2 });
        var root = TestTree.Create(new[] { node });

        // Act
        var leaves = root.GetLeaves();

        // Assert
        leaves.Should().HaveCount(2).And.Contain(new[] { leaf1, leaf2 });
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>
    {
        public Box<TestTree>[] Children { get; private init; }

        public static Box<TestTree> Create(Box<TestTree>[]? children) => (Box<TestTree>) new TestTree()
        {
            Children = children ?? Array.Empty<Box<TestTree>>()
        };

        public static Box<TestTree> Create(Box<TestTree>? child) => (Box<TestTree>) new TestTree()
        {
            Children = child != null ? new[] { child } : Array.Empty<Box<TestTree>>()
        };

        public static Box<TestTree> Create() => (Box<TestTree>) new TestTree()
        {
            Children = Array.Empty<Box<TestTree>>()
        };
    }
}
