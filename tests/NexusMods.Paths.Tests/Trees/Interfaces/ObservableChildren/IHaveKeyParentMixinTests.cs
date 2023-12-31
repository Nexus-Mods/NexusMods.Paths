using System.Collections.ObjectModel;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyParentMixinTests
{
    [Fact]
    public void FindSubPathsByKeyUpward_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var deepChild2 = TestTree.Create(5); // Same key as deepChild1
        var grandChild1 = TestTree.Create(4, deepChild1);
        var grandChild2 = TestTree.Create(4, deepChild2); // Same key as grandChild1
        var child = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1, grandChild2 }, 3);
        var root = TestTree.Create(2, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(2);
        foundNodes.All(node => node.Item.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = TestTree.Create(4);
        var grandChild2 = TestTree.Create(5);
        var child1 = TestTree.Create(2, grandChild1);
        var child2 = TestTree.Create(3, grandChild2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 }, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Item.Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(null, 2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var deepChild2 = TestTree.Create(5); // Same key as deepChild1
        var grandChild1 = TestTree.Create(4, deepChild1);
        var grandChild2 = TestTree.Create(4, deepChild2); // Same key as grandChild1
        var child = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1, grandChild2 }, 3);
        var root = TestTree.Create(2, child);

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward<TestTree, int>(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(2);
        foundNodes.All(node => node.Item.Key == 4).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = TestTree.Create(4);
        var grandChild2 = TestTree.Create(5);
        var child1 = TestTree.Create(2, grandChild1);
        var child2 = TestTree.Create(3, grandChild2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 }, 1);

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward<TestTree, int>(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Item.Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward<TestTree, int>(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(null, 2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward<TestTree, int>(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveKey<int>, IHaveParent<TestTree>
    {
        public Box<TestTree>? Parent { get; private set; }
        public ObservableCollection<Box<TestTree>> Children { get; private set; }
        public int Key { get; private set; }

        public static Box<TestTree> Create(ObservableCollection<Box<TestTree>>? children, int key = default)
        {
            var tree = (Box<TestTree>) new TestTree();
            tree.Item.Key = key;
            tree.Item.Children = children ?? new ObservableCollection<Box<TestTree>>();
            foreach (var child in tree.Item.Children)
                child.Item.Parent = tree;

            return tree;
        }

        public static Box<TestTree> Create(int key = default, Box<TestTree>? child = null)
        {
            var tree = (Box<TestTree>) new TestTree();
            tree.Item.Key = key;
            if (child != null)
            {
                child.Item.Parent = tree;
                tree.Item.Children = new ObservableCollection<Box<TestTree>> { child };
            }
            else
            {
                tree.Item.Children = new ObservableCollection<Box<TestTree>>();
            }

            return tree;
        }
    }
}
