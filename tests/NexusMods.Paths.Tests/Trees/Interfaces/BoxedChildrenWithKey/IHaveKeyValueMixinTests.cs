using NexusMods.Paths.Trees.Traits;
using NexusMods.Paths.Trees;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveKeyValueWithKeyTests
{
    [Fact]
    public void GetKeyValues_ShouldReturnAllKeyValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3, "Key3");
        var grandChild2 = TestTree.Create(4, "Key4");
        var child1 = TestTree.Create(grandChild1, 1, "Key1");
        var child2 = TestTree.Create(grandChild2, 2, "Key2");
        var root = TestTree.Create(new Dictionary<string, KeyedBox<string, TestTree>>
        {
            { child1.Item.Key, child1 },
            { child2.Item.Key, child2 }
        }, 0, "RootKey");

        // Act
        var keyValuePairs = root.GetKeyValues<string, TestTree, int>();

        // Assert
        keyValuePairs.Should().Equal(
            new KeyValuePair<string, int>("Key1", 1),
            new KeyValuePair<string, int>("Key3", 3),
            new KeyValuePair<string, int>("Key2", 2),
            new KeyValuePair<string, int>("Key4", 4)
        );
    }

    [Fact]
    public void ToDictionary_ShouldReturnAllKeyValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(3, "Key3");
        var grandChild2 = TestTree.Create(4, "Key4");
        var child1 = TestTree.Create(grandChild1, 1, "Key1");
        var child2 = TestTree.Create(grandChild2, 2, "Key2");
        var root = TestTree.Create(new Dictionary<string, KeyedBox<string, TestTree>>
        {
            { "Key1", child1 },
            { "Key2", child2 }
        }, 0, "RootKey");

        // Act
        var dictionary = root.ToDictionary<string, TestTree, int>();

        // Assert
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key1", 1));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key2", 2));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key3", 3));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key4", 4));
        dictionary.Count.Should().Be(4); // Ensure all pairs are included
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<string, TestTree>, IHaveValue<int>, IHaveKey<string>
    {
        public Dictionary<string, KeyedBox<string, TestTree>> Children { get; private init; }
        public int Value { get; private init; }
        public string Key { get; private init; }

        public static KeyedBox<string, TestTree> Create(int value, string key)
        {
            return (KeyedBox<string, TestTree>)new TestTree()
            {
                Children = new Dictionary<string, KeyedBox<string, TestTree>>(),
                Value = value,
                Key = key
            };
        }

        public static KeyedBox<string, TestTree> Create(KeyedBox<string, TestTree> child, int value, string key)
        {
            return (KeyedBox<string, TestTree>)new TestTree()
            {
                Children = new Dictionary<string, KeyedBox<string, TestTree>>()
                {
                    { child.Item.Key, child }
                },
                Value = value,
                Key = key,
            };
        }

        public static KeyedBox<string, TestTree> Create(Dictionary<string, KeyedBox<string, TestTree>>? children, int value, string key)
        {
            return (KeyedBox<string, TestTree>)new TestTree()
            {
                Children = children ?? new Dictionary<string, KeyedBox<string, TestTree>>(),
                Value = value,
                Key = key
            };
        }
    }
}
