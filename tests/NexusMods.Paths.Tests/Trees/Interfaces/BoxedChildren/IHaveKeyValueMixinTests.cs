using NexusMods.Paths.Trees.Traits;
using NexusMods.Paths.Trees;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyValueMixinTests
{
    [Fact]
    public void GetKeyValues_ShouldReturnAllKeyValuesRecursively()
    {
        // Arrange
        var grandChild1 = new TestTree(null, "Key3", 3);
        var grandChild2 = new TestTree(null, "Key4", 4);
        var child1 = new TestTree(grandChild1, "Key1", 1);
        var child2 = new TestTree(grandChild2, "Key2", 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, "RootKey", 0);

        // Act
        var keyValuePairs = root.GetKeyValues<TestTree, string, int>();

        // Assert
        keyValuePairs.Should().Equal(
            new KeyValuePair<string, int>("Key1", 1),
            new KeyValuePair<string, int>("Key2", 2),
            new KeyValuePair<string, int>("Key3", 3),
            new KeyValuePair<string, int>("Key4", 4)
        );
    }

    [Fact]
    public void ToDictionary_ShouldReturnAllKeyValuesRecursively()
    {
        // Arrange
        var grandChild1 = new TestTree(null, "Key3", 3);
        var grandChild2 = new TestTree(null, "Key4", 4);
        var child1 = new TestTree(grandChild1, "Key1", 1);
        var child2 = new TestTree(grandChild2, "Key2", 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, "RootKey", 0);

        // Act
        var dictionary = root.ToDictionary<TestTree, string, int>();

        // Assert
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key1", 1));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key2", 2));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key3", 3));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key4", 4));
        dictionary.Count.Should().Be(4); // Ensure all pairs are included
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveKey<string>, IHaveValue<int>
    {
        public ChildBox<TestTree>[] Children { get; }
        public string Key { get; }
        public int Value { get; set; }

        public TestTree(ChildBox<TestTree>[]? children, string key, int value = default)
        {
            Children = children ?? Array.Empty<ChildBox<TestTree>>();
            Key = key;
            Value = value;
        }

        public TestTree(TestTree child, string key, int value = default)
        {
            Children = new[] { new ChildBox<TestTree> { Item = child } };
            Key = key;
            Value = value;
        }
    }
}