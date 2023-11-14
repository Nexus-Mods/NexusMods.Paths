using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveKeyTests
{
    [Fact]
    public void EnumerateKeysBfs_ShouldReturnAllKeysInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(3);
        var grandChild2 = new TestTree(4);
        var child1 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 1, grandChild1 } }, 1);
        var child2 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 2, grandChild2 } }, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 1, child1 }, { 2, child2 } });

        // Act
        var keys = root.EnumerateKeysBfs().ToArray();

        // Assert
        keys.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateKeysDfs_ShouldReturnAllKeysInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(3);
        var grandChild2 = new TestTree(4);
        var child1 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 3, grandChild1 } }, 1);
        var child2 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 4, grandChild2 } }, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 1, child1 }, { 2, child2 } });

        // Act
        var keys = root.EnumerateKeysDfs().ToArray();

        // Assert
        keys.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetKeys_ShouldReturnAllKeysRecursively()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { {1, child1}, {2, child2} }) { Key = 0 };

        // Act
        var keys = root.GetKeys();

        // Assert
        keys.Should().Equal(1, 3, 2, 4);
    }

    [Fact]
    public void FindByKeyFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild(new[] { 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromChild_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild(new[] { 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromChild_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild(new[] { 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByKeyFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot(new[] { 1, 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot(new[] { 1, 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot(new[] { 99 });

        // Assert
        foundNode.Should().BeNull();
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveKey<int>
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public int Key { get; set; }

        public TestTree(int key)
        {
            Children = new Dictionary<int, ChildWithKeyBox<int, TestTree>>();
            Key = key;
        }

        public TestTree(TestTree child, int key)
        {
            Children = new Dictionary<int, ChildWithKeyBox<int, TestTree>>()
            {
                {child.Key, child}
            };
            Key = key;
        }


        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>>? children, int key = default)
        {
            Children = children ?? new Dictionary<int, ChildWithKeyBox<int, TestTree>>();
            Key = key;
        }
    }
}
