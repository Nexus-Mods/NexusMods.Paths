using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveValueTests
{
    [Fact]
    public void EnumerateValuesBfs_ShouldReturnAllValuesInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 3, grandChild1 } }, 1);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 4, grandChild2 } }, 2);
        KeyedBox<int, TestTree> root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 1, child1 }, { 2, child2 } }, 0);

        // Act
        var values = root.EnumerateValuesBfs<int, TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateValuesDfs_ShouldReturnAllValuesInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create(grandChild1, 1);
        var child2 = TestTree.Create(grandChild2, 2);
        KeyedBox<int, TestTree> root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 1, child1 }, { 2, child2 } }, 0);

        // Act
        var values = root.EnumerateValuesDfs<int, TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetValues_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3);
        var grandChild2 = TestTree.Create(4);
        var child1 = TestTree.Create(grandChild1, 1);
        var child2 = TestTree.Create(grandChild2, 2);
        KeyedBox<int, TestTree> root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { { 1, child1 }, { 2, child2 } }, 0);

        // Act
        var values = root.GetValues<int, TestTree, int>();

        // Assert
        values.Should().Equal(1, 3, 2, 4);
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveValue<int>
    {
        public Dictionary<int, KeyedBox<int, TestTree>> Children { get; private init; }
        public int Value { get; private init; }

        public static KeyedBox<int, TestTree> Create(int value)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = new Dictionary<int, KeyedBox<int, TestTree>>(),
                Value = value
            };
        }

        public static KeyedBox<int, TestTree> Create(KeyedBox<int, TestTree> child, int value)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = new Dictionary<int, KeyedBox<int, TestTree>>()
                {
                    { child.Item.Value, child }
                },
                Value = value
            };
        }

        public static KeyedBox<int, TestTree> Create(Dictionary<int, KeyedBox<int, TestTree>>? children, int value)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = children ?? new Dictionary<int, KeyedBox<int, TestTree>>(),
                Value = value
            };
        }
    }
}
