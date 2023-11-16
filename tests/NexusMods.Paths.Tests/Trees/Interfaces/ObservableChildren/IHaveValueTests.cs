using System.Collections.ObjectModel;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveValueTests
{
    [Fact]
    public void EnumerateValuesBfs_ShouldReturnAllValuesInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(grandChild1, 1);
        var child2 = TestTree.Create(grandChild2, 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 });

        // Act
        var values = root.EnumerateValuesBfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateValuesDfs_ShouldReturnAllValuesInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(grandChild1, 1);
        var child2 = TestTree.Create(grandChild2 , 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 });

        // Act
        var values = root.EnumerateValuesDfs<TestTree, int>().ToArray();

        // Assert
        values.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetValues_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1 }, 1);
        var child2 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild2 }, 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1,child2 });

        // Act
        var values = root.GetValues<TestTree, int>();

        // Assert
        values.Should().Equal(1, 2, 3, 4);
    }

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveValue<int>
    {
        public ObservableCollection<Box<TestTree>> Children { get; private init; }
        public int Value { get; private init; }

        public static Box<TestTree> Create(ObservableCollection<Box<TestTree>>? children, int value = default)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = children ?? new ObservableCollection<Box<TestTree>>(),
                Value = value
            };
        }

        public static Box<TestTree> Create(TestTree child, int value = default)
        {
            return (Box<TestTree>)new TestTree()
            {
                Children = new ObservableCollection<Box<TestTree>> { new() { Item = child} },
                Value = value
            };
        }
    }
}
