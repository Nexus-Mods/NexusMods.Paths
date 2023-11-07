using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveBoxedChildrenWithKeyTests
{
    [Fact]
    public void EnumerateChildren_ShouldReturnAllChildrenRecursively()
    {
        // Arrange
        var leaf1 = new TestTree(new());
        var leaf2 = new TestTree(new());
        var node = new TestTree(new()
        {
            [1] = leaf1,
            [2] = leaf2
        });
        var root = new TestTree(new() { [0] = node });

        // Act
        var allChildren = root.EnumerateChildren<TestTree, int>().ToArray();

        // Assert
        allChildren.Should().HaveCount(3)
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(0, node))
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(1, leaf1))
            .And.Contain(new KeyValuePair<int, ChildWithKeyBox<int, TestTree>>(2, leaf2));
    }

    [Fact]
    public void CountChildren_ShouldReturnCorrectNumberOfDirectChildren()
    {
        // Arrange
        var child1 = new TestTree(new());
        var child2 = new TestTree(new());
        var root = new TestTree(new()
        {
            [1] = child1,
            [2] = child2
        });

        // Act
        var count = root.CountChildren<TestTree, int>();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void CountChildren_ShouldReturnZeroForLeafNode()
    {
        // Arrange
        var leaf = new TestTree(new());

        // Act
        var count = leaf.CountChildren<TestTree, int>();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void CountChildrenRecursive_ShouldReturnTotalNumberOfAllChildren()
    {
        // Arrange
        var grandChild = new TestTree(new());
        var child = new TestTree(new() { [1] = grandChild });
        var root = new TestTree(new() { [0] = child });

        // Act
        var count = root.CountChildren<TestTree, int>();

        // Assert
        count.Should().Be(2); // Child + Grandchild
    }

    internal struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>> children) => Children = children;

        public bool Equals(TestTree other) => Children.Equals(other.Children);
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => Children.GetHashCode();
    }
}
