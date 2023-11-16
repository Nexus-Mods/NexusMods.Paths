using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyTests
{
    [Fact]
    public void EnumerateKeysBfs_ShouldReturnAllKeysInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create( 1, grandChild1);
        var child2 = TestTree.Create(2, grandChild2);
        var root = TestTree.Create(new[] { child1, child2 });

        // Act
        var keys = root.EnumerateKeysBfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateKeysDfs_ShouldReturnAllKeysInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create( 1, grandChild1);
        var child2 = TestTree.Create(2, grandChild2);
        var root = TestTree.Create(new[] { child1, child2 });

        // Act
        var keys = root.EnumerateKeysDfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetKeys_ShouldReturnAllKeysRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create( 1, grandChild1);
        var child2 = TestTree.Create(2, grandChild2);
        var root = TestTree.Create(new[] { child1, child2 });

        // Act
        var keys = root.GetKeys<TestTree, int>();

        // Assert
        keys.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void FindByKeyFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(2, grandchild);
        var root = TestTree.Create(1, child);

        // Act
        var foundNode = root.FindByKeyFromChild<TestTree, int>(new[] { 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromChild_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(2, grandchild);
        var root = TestTree.Create(1, child);

        // Act
        var foundNode = root.FindByKeyFromChild<TestTree, int>(new[] { 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromChild_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNode = root.FindByKeyFromChild<TestTree, int>(new[] { 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByKeyFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(2, grandchild);
        var root = TestTree.Create(1, child);

        // Act
        var foundNode = root.FindByKeyFromRoot<TestTree, int>(new[] { 1, 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(2, grandchild);
        var root = TestTree.Create(1, child);

        // Act
        var foundNode = root.FindByKeyFromRoot<TestTree, int>(new[] { 1, 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNode = root.FindByKeyFromRoot<TestTree, int>(new[] { 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindSubPathsByKeyFromRoot_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromRoot<TestTree, int>(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyFromChild_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var deepChild2 = TestTree.Create(5); // Same key as deepChild1
        var grandChild1 = TestTree.Create(4, deepChild1);
        var grandChild2 = TestTree.Create(4, deepChild2); // Same key as grandChild1
        var child = TestTree.Create(new[] { grandChild1, grandChild2 }, 3);
        var root = TestTree.Create(2, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromChild<TestTree, int>(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(2);
        foundNodes.All(node => node.Item.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyFromChild_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = TestTree.Create(4);
        var grandChild2 = TestTree.Create(5);
        var child1 = TestTree.Create(2, grandChild1);
        var child2 = TestTree.Create(3, grandChild2);
        var root = TestTree.Create(new[] { child1, child2 }, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromChild<TestTree, int>(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Item.Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathsByKeyFromChild_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromChild<TestTree, int>(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyFromChild_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromChild<TestTree, int>(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyFromRoot_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var deepChild2 = TestTree.Create(5); // Same key as deepChild1
        var grandChild1 = TestTree.Create(4, deepChild1);
        var grandChild2 = TestTree.Create(4, deepChild2); // Same key as grandChild1
        var child = TestTree.Create(new[] { grandChild1, grandChild2 }, 3);
        var root = TestTree.Create(2, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromRoot<TestTree, int>(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(2);
        foundNodes.All(node => node.Item.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyFromRoot_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = TestTree.Create(4);
        var grandChild2 = TestTree.Create(5);
        var child1 = TestTree.Create(2, grandChild1);
        var child2 = TestTree.Create(3, grandChild2);
        var root = TestTree.Create(new[] { child1, child2 }, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromRoot<TestTree, int>(new[] { 1, 2 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Item.Key.Should().Be(2);
    }

    [Fact]
    public void FindSubPathsByKeyFromRoot_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, child);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromRoot<TestTree, int>(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveKey<int>
    {
        public Box<TestTree>[] Children { get; private set; }
        public int Key { get; private set; }

        public static Box<TestTree> Create(Box<TestTree>[]? children, int key = default)
        {
            var tree = (Box<TestTree>) new TestTree();
            tree.Item.Key = key;
            tree.Item.Children = children ?? Array.Empty<Box<TestTree>>();
            return tree;
        }

        public static Box<TestTree> Create(int key = default, Box<TestTree>? child = null)
        {
            var tree = (Box<TestTree>) new TestTree();
            tree.Item.Key = key;
            tree.Item.Children = child != null ? new[] { child } : Array.Empty<Box<TestTree>>();
            return tree;
        }
    }
}
