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
        Box<TestTree> leaf1 = new TestTree(null);
        Box<TestTree> leaf2 = new TestTree(null);
        Box<TestTree> node = new TestTree(new Box<TestTree>[] { leaf1, leaf2 });
        Box<TestTree> root = new TestTree(node);

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
        Box<TestTree> grandChild = new TestTree(null);
        Box<TestTree> child = new TestTree(grandChild);
        Box<TestTree> root = new TestTree(child);

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
        Box<TestTree> grandChild = new TestTree(null);
        Box<TestTree> child = new TestTree(new[] { grandChild });
        Box<TestTree> root = new TestTree(new[] { child });

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
        var child1 = new TestTree(null);
        var child2 = new TestTree(null);
        Box<TestTree> root = new TestTree(new Box<TestTree>[] { child1, child2 });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void CountChildren_ShouldReturnZeroForLeafNode()
    {
        // Arrange
        Box<TestTree> leaf = new TestTree(null);

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
        var grandChild = new TestTree(null);
        var child = new TestTree(new Box<TestTree>[] { grandChild });
        Box<TestTree> root = new TestTree(child);

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void GetChildrenRecursive_ShouldReturnAllChildrenIncludingDescendants()
    {
        // Arrange
        var grandChild1 = new TestTree(null);
        var grandChild2 = new TestTree(null);
        var child1 = new TestTree(new Box<TestTree>[] { grandChild1 });
        var child2 = new TestTree(new Box<TestTree>[] { grandChild2 });
        var children = new Box<TestTree>[] { child1, child2 };
        Box<TestTree> root = new TestTree(children);

        // Act
        var allChildren = root.GetChildrenRecursive().ToArray();

        // Assert
        allChildren.Length.Should().Be(4); // Including child1, child2, grandChild1, grandChild2
        allChildren.Should().Contain(new Box<TestTree>[] { child1, child2, grandChild1, grandChild2 });
    }

    [Fact]
    public void CountLeaves_ShouldReturnCorrectNumberOfLeafNodes()
    {
        // Arrange
        var leaf1 = new TestTree(null);
        var leaf2 = new TestTree(null);
        var node = new TestTree(new Box<TestTree>[] { leaf1, leaf2 });
        Box<TestTree> root = new TestTree(new Box<TestTree>[] { node });

        // Act
        var leafCount = root.CountLeaves();

        // Assert
        leafCount.Should().Be(2); // leaf1 and leaf2 are leaves
    }

    [Fact]
    public void GetLeaves_ShouldReturnAllLeafNodes()
    {
        // Arrange
        Box<TestTree> leaf1 = new TestTree(null);
        Box<TestTree> leaf2 = new TestTree(null);
        Box<TestTree> node = new TestTree(new[] { leaf1, leaf2 });
        Box<TestTree> root = new TestTree(new[] { node });

        // Act
        var leaves = root.GetLeaves();

        // Assert
        leaves.Should().HaveCount(2).And.Contain(new[] { leaf1, leaf2 });
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>
    {
        public Box<TestTree>[] Children { get; }

        public TestTree(Box<TestTree>[]? children) => Children = children ?? Array.Empty<Box<TestTree>>();
        public TestTree(TestTree child) => Children = new Box<TestTree>[]{ child };
    }
}
