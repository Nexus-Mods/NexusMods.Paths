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
        var grandChild1 = new TestTree(3, "Key3");
        var grandChild2 = new TestTree(4, "Key4");
        var child1 = new TestTree(grandChild1, 1, "Key1");
        var child2 = new TestTree(grandChild2, 2, "Key2");
        ChildWithKeyBox<string, TestTree> root = new TestTree(new Dictionary<string, ChildWithKeyBox<string, TestTree>> { { child1.Key, child1 }, { child2.Key, child2 } }, 0, "RootKey");

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
        var grandChild1 = new TestTree(3, "Key3");
        var grandChild2 = new TestTree(4, "Key4");
        var child1 = new TestTree(grandChild1, 1, "Key1");
        var child2 = new TestTree(grandChild2, 2, "Key2");
        ChildWithKeyBox<string, TestTree> root = new TestTree(new Dictionary<string, ChildWithKeyBox<string, TestTree>> { { "Key1", child1 }, { "Key2", child2 } }, 0, "RootKey");

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
        public Dictionary<string, ChildWithKeyBox<string, TestTree>> Children { get; }
        public int Value { get; }
        public string Key { get; }

        public TestTree(int value, string key)
        {
            Children = new Dictionary<string, ChildWithKeyBox<string, TestTree>>();
            Value = value;
            Key = key;
        }

        public TestTree(TestTree child, int value, string key)
        {
            Children = new Dictionary<string, ChildWithKeyBox<string, TestTree>>()
            {
                { child.Key, child }
            };
            Value = value;
            Key = key;
        }

        public TestTree(Dictionary<string, ChildWithKeyBox<string, TestTree>>? children, int value, string key)
        {
            Children = children ?? new Dictionary<string, ChildWithKeyBox<string, TestTree>>();
            Value = value;
            Key = key;
        }
    }
}
