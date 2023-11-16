using System.Collections.ObjectModel;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.ObservableChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyValueMixinTests
{
    [Fact]
    public void GetKeyValues_ShouldReturnAllKeyValuesRecursively()
    {
        // Arrange
        var grandChild1 = TestTree.Create(null, "Key3", 3);
        var grandChild2 = TestTree.Create(null, "Key4", 4);
        var child1 = TestTree.Create("Key1", 1, grandChild1);
        var child2 = TestTree.Create("Key2", 2, grandChild2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 }, "RootKey");

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
        var grandChild1 = TestTree.Create(null, "Key3", 3);
        var grandChild2 = TestTree.Create(null, "Key4", 4);
        var child1 = TestTree.Create("Key1", 1, grandChild1);
        var child2 = TestTree.Create("Key2", 2, grandChild2);
        var root = TestTree.Create(new ObservableCollection<Box<TestTree>> { child1, child2 }, "RootKey");

        // Act
        var dictionary = root.ToDictionary<TestTree, string, int>();

        // Assert
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key1", 1));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key2", 2));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key3", 3));
        dictionary.Should().Contain(new KeyValuePair<string, int>("Key4", 4));
        dictionary.Count.Should().Be(4); // Ensure all pairs are included
    }

    private struct TestTree : IHaveObservableChildren<TestTree>, IHaveKey<string>, IHaveValue<int>
    {
        public ObservableCollection<Box<TestTree>> Children { get; private set; }
        public string Key { get; private set; }
        public int Value { get; private set; }

        public static Box<TestTree> Create(ObservableCollection<Box<TestTree>>? children, string key, int value = default)
        {
            var tree = (Box<TestTree>) new TestTree();
            tree.Item.Key = key;
            tree.Item.Value = value;
            tree.Item.Children = children ?? new ObservableCollection<Box<TestTree>>();
            return tree;
        }

        public static Box<TestTree> Create(string key, int value = default, Box<TestTree>? child = null)
        {
            var tree = (Box<TestTree>) new TestTree();
            tree.Item.Key = key;
            tree.Item.Value = value;
            tree.Item.Children = child != null ? new ObservableCollection<Box<TestTree>> { child } : new ObservableCollection<Box<TestTree>>();
            return tree;
        }
    }
}
