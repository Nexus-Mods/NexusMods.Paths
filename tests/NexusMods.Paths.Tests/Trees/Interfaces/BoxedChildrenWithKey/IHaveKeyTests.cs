using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveKeyTests
{
    [Fact]
    public void EnumerateKeysBfs_ShouldReturnAllKeysInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 1, grandChild1 } }, 1);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 2, grandChild2 } }, 2);
        var root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 1, child1 }, { 2, child2 } });

        // Act
        var keys = root.EnumerateKeysBfs().ToArray();

        // Assert
        keys.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateKeysDfs_ShouldReturnAllKeysInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 3, grandChild1 } }, 1);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 4, grandChild2 } }, 2);
        KeyedBox<int, TestTree> root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 1, child1 }, { 2, child2 } });

        // Act
        var keys = root.EnumerateKeysDfs().ToArray();

        // Assert
        keys.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetKeys_ShouldReturnAllKeysRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create(grandChild1, 1);
        var child2 = TestTree.Create(grandChild2, 2);
        var root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { {1, child1}, {2, child2} });

        // Act
        var keys = root.GetKeys();

        // Assert
        keys.Should().Equal(1, 3, 2, 4);
    }

    [Fact]
    public void FindByKeyFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(grandchild, 2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild(new[] { 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromChild_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(grandchild, 2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild(new[] { 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromChild_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild(new[] { 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByKeyFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(grandchild, 2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot(new[] { 1, 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = TestTree.Create(3);
        var child = TestTree.Create(grandchild, 2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot(new[] { 1, 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Item.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot(new[] { 99 });

        // Assert
        foundNode.Should().BeNull();
    }

    [Fact]
    public void FindSubPathsByKeyFromChild_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var grandChild1 = TestTree.Create(deepChild1, 4);
        var child = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>>()
        {
            { 4, grandChild1 }
        }, 3);
        var root = TestTree.Create(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromChild(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes.All(node => node.Item.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyFromRoot_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var grandChild1 = TestTree.Create(deepChild1, 4);
        var child = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>>()
        {
            { 4, grandChild1 }
        }, 3);
        var root = TestTree.Create(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromRoot(new[] { 2, 3, 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes.All(node => node.Item.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyFromChild_WithEmptyPath_ReturnsEmpty()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var grandChild1 = TestTree.Create(deepChild1, 4);
        var child = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>>()
        {
            { 4, grandChild1 }
        }, 3);
        var root = TestTree.Create(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromChild(Array.Empty<int>());

        // Assert
        foundNodes.Count.Should().Be(0);
    }

    [Fact]
    public void FindSubPathsByKeyFromRoot_WithEmptyPath_ReturnsEmpty()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var grandChild1 = TestTree.Create(deepChild1, 4);
        var child = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>>()
        {
            { 4, grandChild1 }
        }, 3);
        var root = TestTree.Create(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyFromRoot(Array.Empty<int>());

        // Assert
        foundNodes.Count.Should().Be(0);
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveKey<int>
    {
        public Dictionary<int, KeyedBox<int, TestTree>> Children { get; private init; }
        public int Key { get; init; }

        public static KeyedBox<int, TestTree> Create(int key)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = new Dictionary<int, KeyedBox<int, TestTree>>(),
                Key = key
            };
        }

        public static KeyedBox<int, TestTree> Create(KeyedBox<int, TestTree> child, int key)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = new Dictionary<int, KeyedBox<int, TestTree>>()
                {
                    {child.Item.Key, child}
                },
                Key = key
            };
        }


        public static KeyedBox<int, TestTree> Create(Dictionary<int, KeyedBox<int, TestTree>>? children, int key = default)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = children ?? new Dictionary<int, KeyedBox<int, TestTree>>(),
                Key = key
            };
        }
    }
}
