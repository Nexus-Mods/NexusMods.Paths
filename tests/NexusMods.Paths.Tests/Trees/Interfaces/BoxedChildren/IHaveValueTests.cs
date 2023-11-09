using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveValueTests
{
    [Fact]
    public void EnumerateValuesBfs_ShouldReturnAllValuesInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Value = 3 };
        var grandChild2 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Value = 4 };
        var child1 = new TestTree(grandChild1) { Value = 1 };
        var child2 = new TestTree(grandChild2) { Value = 2 };
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }) { Value = 0 };

        // Act
        var values = root.EnumerateValuesBfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateValuesDfs_ShouldReturnAllValuesInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Value = 3 };
        var grandChild2 = new TestTree(Array.Empty<ChildBox<TestTree>>()) { Value = 4 };
        var child1 = new TestTree(grandChild1) { Value = 1 };
        var child2 = new TestTree(grandChild2) { Value = 2 };
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }) { Value = 0 };

        // Act
        var values = root.EnumerateValuesDfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveValue<int>
    {
        public ChildBox<TestTree>[] Children { get; }
        public int Value { get; set;  }

        public TestTree(ChildBox<TestTree>[] children, int value = default)
        {
            Children = children;
            Value = value;
        }

        public TestTree(TestTree child, int value = default)
        {
            Children = new[] { new ChildBox<TestTree> { Item = child} };
            Value = value;
        }
    }
}
