using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyTests
{
    [Fact]
    public void EnumerateKeysBfs_ShouldReturnAllKeysInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Key = 3 };
        var grandChild2 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Key = 4 };
        var child1 = new TestTree(grandChild1) { Key = 1 };
        var child2 = new TestTree(grandChild2) { Key = 2 };
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }) { Key = 0 };

        // Act
        var keys = root.EnumerateKeysBfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateKeysDfs_ShouldReturnAllKeysInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Key = 3 };
        var grandChild2 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Key = 4 };
        var child1 = new TestTree(grandChild1) { Key = 1 };
        var child2 = new TestTree(grandChild2) { Key = 2 };
        ChildBox<TestTree> root =  new TestTree(new ChildBox<TestTree>[] { child1, child2 }) { Key = 0 };

        // Act
        var keys = root.EnumerateKeysDfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveKey<int>
    {
        public ChildBox<TestTree>[] Children { get; }
        public int Key { get; set; }

        public TestTree(ChildBox<TestTree>[] children, int key = default)
        {
            Children = children;
            Key = key;
        }

        public TestTree(TestTree child, int key = default)
        {
            Children = new ChildBox<TestTree>[]{ child };
            Key = key;
        }
    }
}
