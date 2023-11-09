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

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveKey<int>
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public int Key { get; }

        public TestTree(int key)
        {
            Children = new Dictionary<int, ChildWithKeyBox<int, TestTree>>();
            Key = key;
        }

        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>> children, int key = default)
        {
            Children = children;
            Key = key;
        }
    }
}
