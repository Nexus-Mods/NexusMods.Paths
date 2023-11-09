using System.Collections.ObjectModel;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveObservableChildrenTests
{
    [Fact]
    public void EnumerateChildren_ShouldReturnAllChildrenRecursively()
    {
        // Arrange
        var leaf1 = new TestTree(new ObservableCollection<ChildBox<TestTree>>());
        var leaf2 = new TestTree(new ObservableCollection<ChildBox<TestTree>>());
        var node = new TestTree(new ObservableCollection<ChildBox<TestTree>> { leaf1, leaf2 });
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { node });

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
        var grandChild = new ChildBox<TestTree> { Item = new TestTree(new ObservableCollection<ChildBox<TestTree>>()) };
        var child = new ChildBox<TestTree> { Item = new TestTree(new ObservableCollection<ChildBox<TestTree>> { grandChild }) };
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child });

        // Act
        var allChildren = root.Item.EnumerateChildrenDfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(child.Item);
        allChildren[1].Should().BeEquivalentTo(grandChild.Item);
    }

    [Fact]
    public void EnumerateChildrenBfs_ShouldReturnAllChildrenInBreadthFirstOrder()
    {
        // Arrange
        var grandChild = new ChildBox<TestTree> { Item = new TestTree(new ObservableCollection<ChildBox<TestTree>>()) };
        var child = new ChildBox<TestTree> { Item = new TestTree(new ObservableCollection<ChildBox<TestTree>> { grandChild }) };
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child });

        // Act
        var allChildren = root.Item.EnumerateChildrenBfs().ToArray();

        // Assert
        allChildren.Should().HaveCount(2);
        allChildren[0].Should().BeEquivalentTo(child.Item);
        allChildren[1].Should().BeEquivalentTo(grandChild.Item);
    }

    [Fact]
    public void CountChildren_ShouldReturnCorrectNumberOfDirectChildren()
    {
        // Arrange
        var child1 = new TestTree(new ObservableCollection<ChildBox<TestTree>>());
        var child2 = new TestTree(new ObservableCollection<ChildBox<TestTree>>());
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child1, child2 });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void CountChildren_ShouldReturnZeroForLeafNode()
    {
        // Arrange
        ChildBox<TestTree> leaf = new TestTree(new ObservableCollection<ChildBox<TestTree>>());

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
        var grandChild = new TestTree(new ObservableCollection<ChildBox<TestTree>>());
        var child = new TestTree(new ObservableCollection<ChildBox<TestTree>> { grandChild });
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child });

        // Act
        var count = root.CountChildren();

        // Assert
        count.Should().Be(2);
    }

    private struct TestTree : IHaveObservableChildren<TestTree>
    {
        public ObservableCollection<ChildBox<TestTree>> Children { get; }

        public TestTree(ObservableCollection<ChildBox<TestTree>> children) => Children = children;
    }
}
