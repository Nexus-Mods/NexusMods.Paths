using System.Collections.ObjectModel;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyTests
{
    [Fact]
    public void EnumerateKeysBfs_ShouldReturnAllKeysInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child1, child2 }, 0);

        // Act
        var keys = root.EnumerateKeysBfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateKeysDfs_ShouldReturnAllKeysInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child1, child2 }, 0);
        // Act
        var keys = root.EnumerateKeysDfs<TestTree, int>().ToArray();

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
        ChildBox<TestTree> root = new TestTree(new ObservableCollection<ChildBox<TestTree>> { child1, child2 }, 0);

        // Act
        var keys = root.GetKeys<TestTree, int>();

        // Assert
        keys.Should().Equal(1, 2, 3, 4);
    }

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveKey<int>
    {
        public ObservableCollection<ChildBox<TestTree>> Children { get; }
        public int Key { get; set; }

        public TestTree(ObservableCollection<ChildBox<TestTree>>? children, int key = default)
        {
            Children = children ?? new ObservableCollection<ChildBox<TestTree>>();
            Key = key;
        }

        public TestTree(TestTree child, int key = default)
        {
            Children = new ObservableCollection<ChildBox<TestTree>> { child };
            Key = key;
        }
    }
}
