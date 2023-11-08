using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveBoxedChildrenTests
{
    [Fact]
    public void EnumerateChildren_ShouldReturnAllChildrenRecursively()
    {
        // Arrange
        var leaf1 = new TestTree(Array.Empty<ChildBox<TestTree>>());
        var leaf2 = new TestTree(Array.Empty<ChildBox<TestTree>>());
        var node = new TestTree(new ChildBox<TestTree>[] { leaf1, leaf2 });
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { node });

        // Act
        var allChildren = root.EnumerateChildren().ToArray();

        // Assert
        allChildren.Should().HaveCount(3)
            .And.Contain(new[] { node, leaf1, leaf2 });
    }

    [Fact]
    public void CountChildren_ShouldReturnCorrectNumberOfDirectChildren()
    {
        // Arrange
        var child1 = new TestTree(Array.Empty<ChildBox<TestTree>>());
        var child2 = new TestTree(Array.Empty<ChildBox<TestTree>>());
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void CountChildren_ShouldReturnZeroForLeafNode()
    {
        // Arrange
        ChildBox<TestTree> leaf = new TestTree(Array.Empty<ChildBox<TestTree>>());

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
        var grandChild = new TestTree(Array.Empty<ChildBox<TestTree>>());
        var child = new TestTree(new ChildBox<TestTree>[] { grandChild });
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>
    {
        public ChildBox<TestTree>[] Children { get; }

        public TestTree(ChildBox<TestTree>[] children) => Children = children;
    }
}
