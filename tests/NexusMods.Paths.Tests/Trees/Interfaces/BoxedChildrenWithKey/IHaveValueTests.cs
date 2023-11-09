using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveValueTests
{
    [Fact]
    public void EnumerateValuesBfs_ShouldReturnAllValuesInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(3);
        var grandChild2 = new TestTree(4);
        var child1 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 3, grandChild1 } }, 1);
        var child2 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 4, grandChild2 } }, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 1, child1 }, { 2, child2 } }, 0);

        // Act
        var values = root.EnumerateValuesBfs<int, TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateValuesDfs_ShouldReturnAllValuesInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(3);
        var grandChild2 = new TestTree(4);
        var child1 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 3, grandChild1 } }, 1);
        var child2 = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 4, grandChild2 } }, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 1, child1 }, { 2, child2 } }, 0);

        // Act
        var values = root.EnumerateValuesDfs<int, TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveValue<int>
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public int Value { get; }

        // Constructor for leaves
        public TestTree(int value)
        {
            Children = new Dictionary<int, ChildWithKeyBox<int, TestTree>>();
            Value = value;
        }

        // Constructor for non-leaves
        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>>? children, int value)
        {
            Children = children ?? new Dictionary<int, ChildWithKeyBox<int, TestTree>>();
            Value = value;
        }
    }
}
