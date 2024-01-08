using System.Collections.ObjectModel;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;
using NexusMods.Paths.Trees.Traits.Interfaces;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class FilterAndSelectTests
{
    [Fact]
    public void GetChildItems_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1 }, 1);
        var child2 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild2 }, 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 });

        // Act
        var values = root.GetChildrenRecursive<TestTree, int, ValuesOverTwoFilter<TestTree>, ValueSelector<TestTree, int>>();

        // Assert
        values.Should().Equal(3, 4);
    }

    [Fact]
    public void GetChildItemsUnsafe_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1 }, 1);
        var child2 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild2 }, 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 });

        var buffer = new int[2];
        var index = 0;

        // Act
        root.GetChildrenRecursiveUnsafe<TestTree, int, ValuesOverTwoFilter<TestTree>, ValueSelector<TestTree, int>>(buffer, ref index);

        // Assert
        buffer.Should().Equal(3, 4);
    }

    [Fact]
    public void EnumerateChildrenDfs_WithFilterAndSelector_ShouldReturnTransformedValues()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 4);
        var grandChild2 = TestTree.Create(null, 5);
        var child1 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1 }, 3);
        var child2 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild2 }, 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 });

        // Act
        var transformedValues = root.EnumerateChildrenDfs<TestTree, int, ValueOverOneFilter<TestTree>, DoubleValueSelector<TestTree>>().ToList();

        // Assert
        transformedValues.Should().Equal(6, 8, 4, 10);
    }

    [Fact]
    public void EnumerateChildrenBfs_WithFilterAndSelector_ShouldReturnTransformedValues()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 4);
        var grandChild2 = TestTree.Create(null, 5);
        var child1 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild1 }, 3);
        var child2 = TestTree.Create(new ObservableCollection<Box<TestTree>> { grandChild2 }, 2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 });

        // Act
        var transformedValues = root.EnumerateChildrenBfs<TestTree, int, ValueOverOneFilter<TestTree>, DoubleValueSelector<TestTree>>().ToList();

        // Assert
        transformedValues.Should().Equal(6, 4, 8, 10);
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
    }

    private struct ValuesOverTwoFilter<TSelf> : IFilter<TSelf> where TSelf : struct, IHaveValue<int>
    {
        public static bool Match(TSelf item) => item.Value > 2;
    }

    private struct ValueSelector<TSelf, TValue> : ISelector<TSelf, TValue> where TSelf : struct, IHaveValue<TValue>
    {
        public static TValue Select(TSelf item) => item.Value;
    }

    private struct ValueOverOneFilter<TSelf> : IFilter<Box<TSelf>> where TSelf : struct, IHaveValue<int>
    {
        public static bool Match(Box<TSelf> item) => item.Item.Value > 1;
    }

    private struct DoubleValueSelector<TSelf> : ISelector<Box<TSelf>, int> where TSelf : struct, IHaveValue<int>
    {
        public static int Select(Box<TSelf> item) => item.Item.Value * 2;
    }
}
