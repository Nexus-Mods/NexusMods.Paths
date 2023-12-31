using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;
using NexusMods.Paths.Trees.Traits.Interfaces;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class FilterAndSelectTests
{
    [Fact]
    public void GetChildItems_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = grandChild1 }, 1);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [2] = grandChild2 }, 2);
        var root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = child1, [2] = child2 }, 0);

        // Act
        var values = root.GetChildrenRecursive<TestTree, int, int, ValuesOverTwoFilter<TestTree>, ValueSelector<TestTree, int>>();

        // Assert
        values.Should().Equal(3, 4);
    }

    [Fact]
    public void GetChildItemsUnsafe_ShouldReturnAllValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 3);
        var grandChild2 = TestTree.Create(null, 4);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = grandChild1 }, 1);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [2] = grandChild2 }, 2);
        var root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = child1, [2] = child2 }, 0);

        var buffer = new int[2];
        var index = 0;

        // Act
        root.GetChildrenRecursiveUnsafe<TestTree, int, int, ValuesOverTwoFilter<TestTree>, ValueSelector<TestTree, int>>(buffer, ref index);

        // Assert
        buffer.Should().Equal(3, 4);
    }

    [Fact]
    public void EnumerateChildrenDfs_WithFilterAndSelector_ShouldReturnTransformedValues()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 4);
        var grandChild2 = TestTree.Create(null, 5);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = grandChild1 }, 3);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [2] = grandChild2 }, 2);
        var root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = child1, [2] = child2 }, 0);

        // Act
        var transformedValues = root.EnumerateChildrenDfs<TestTree, int, int, ValueOverOneFilter<int, TestTree>, DoubleValueSelector<int, TestTree>>().ToList();

        // Assert
        transformedValues.Should().Equal(6, 8, 4, 10);
    }

    [Fact]
    public void EnumerateChildrenBfs_WithFilterAndSelector_ShouldReturnTransformedValues()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, 4);
        var grandChild2 = TestTree.Create(null, 5);
        var child1 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = grandChild1 }, 3);
        var child2 = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [2] = grandChild2 }, 2);
        var root = TestTree.Create(new Dictionary<int, KeyedBox<int, TestTree>> { [1] = child1, [2] = child2 }, 0);

        // Act
        var transformedValues = root.EnumerateChildrenBfs<TestTree, int, int, ValueOverOneFilter<int, TestTree>, DoubleValueSelector<int, TestTree>>().ToList();

        // Assert
        transformedValues.Should().Equal(6, 4, 8, 10);
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveValue<int>
    {
        public Dictionary<int, KeyedBox<int, TestTree>> Children { get; private init; }
        public int Value { get; private init; }

        public static KeyedBox<int, TestTree> Create(Dictionary<int, KeyedBox<int, TestTree>>? children, int value)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Children = children ?? new Dictionary<int, KeyedBox<int, TestTree>>(),
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

    private struct ValueOverOneFilter<TKey, TSelf> : IFilter<KeyValuePair<TKey, KeyedBox<TKey, TSelf>>>
        where TSelf : struct, IHaveValue<int>
        where TKey : notnull
    {
        public static bool Match(KeyValuePair<TKey, KeyedBox<TKey, TSelf>> item) => item.Value.Item.Value > 1;
    }

    private struct DoubleValueSelector<TKey, TSelf> : ISelector<KeyValuePair<TKey, KeyedBox<TKey, TSelf>>, int> where TSelf : struct, IHaveValue<int>
        where TKey : notnull
    {
        public static int Select(KeyValuePair<TKey, KeyedBox<TKey, TSelf>> item) => item.Value.Item.Value * 2;
    }
}
