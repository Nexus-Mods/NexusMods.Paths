using System.Collections.ObjectModel;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveValueTests
{
    [Fact]
    public void EnumerateValuesBfs_ShouldReturnAllValuesInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(new ObservableCollection<ChildBox<TestTree>>(), 3);
        var grandChild2 = new TestTree(new ObservableCollection<ChildBox<TestTree>>(), 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        var rootChildren = new ObservableCollection<ChildBox<TestTree>>
        {
            child1,
            child2
        };
        ChildBox<TestTree> root = new TestTree(rootChildren, 0);

        // Act
        var values = root.EnumerateValuesBfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateValuesDfs_ShouldReturnAllValuesInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(new ObservableCollection<ChildBox<TestTree>>(), 3);
        var grandChild2 = new TestTree(new ObservableCollection<ChildBox<TestTree>>(), 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        var rootChildren = new ObservableCollection<ChildBox<TestTree>>
        {
            child1,
            child2
        };
        ChildBox<TestTree> root = new TestTree(rootChildren, 0);

        // Act
        var values = root.EnumerateValuesDfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveValue<int>
    {
        public ObservableCollection<ChildBox<TestTree>> Children { get; }
        public int Value { get; }

        public TestTree(ObservableCollection<ChildBox<TestTree>>? children, int value = default)
        {
            Children = children ?? new ObservableCollection<ChildBox<TestTree>>();
            Value = value;
        }

        public TestTree(TestTree child, int value = default) : this(new ObservableCollection<ChildBox<TestTree>> { child }, value)
        { }
    }
}
