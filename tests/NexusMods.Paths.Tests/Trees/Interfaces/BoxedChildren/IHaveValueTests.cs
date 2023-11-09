using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveValueTests
{
    [Fact]
    public void EnumerateValuesBfs_ShouldReturnAllValuesInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, 0);

        // Act
        var values = root.EnumerateValuesBfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateValuesDfs_ShouldReturnAllValuesInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2 , 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, 0);

        // Act
        var values = root.EnumerateValuesDfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetValues_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(new ChildBox<TestTree>[] { grandChild1 }, 1);
        var child2 = new TestTree(new ChildBox<TestTree>[] { grandChild2 }, 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1,child2 }, 0);

        // Act
        var values = root.GetValues<TestTree, int>();

        // Assert
        values.Should().Equal(1, 2, 3, 4);
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveValue<int>
    {
        public ChildBox<TestTree>[] Children { get; }
        public int Value { get; set; }

        public TestTree(ChildBox<TestTree>[]? children, int value = default)
        {
            Children = children ?? Array.Empty<ChildBox<TestTree>>();
            Value = value;
        }

        public TestTree(TestTree child, int value = default)
        {
            Children = new[] { new ChildBox<TestTree> { Item = child} };
            Value = value;
        }
    }
}
