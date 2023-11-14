using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces;

// ReSharper disable once InconsistentNaming
public class IHaveParentWithKeyTests
{
    [Fact]
    public void FindByKeysUpward_WithCorrectSequence_ShouldReturnCorrectNode()
    {
        // Arrange
        var root = new Box<TestTree>() { Item = new TestTree(1) };
        Box<TestTree> child2 = new TestTree(2) { Parent = root };
        Box<TestTree> grandChild = new TestTree(3) { Parent = child2.Item };
        child2.Item.Children = new[] { grandChild };
        root.Item.Children = new[] { child2 };

        // Act
        var foundNode = grandChild.FindByKeysUpward<TestTree, int>(new[] { 1, 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeysUpward_WithIncorrectSequence_ShouldReturnNull()
    {
        // Arrange
        var root = new Box<TestTree>();
        Box<TestTree> child1 = new TestTree(1) { Parent = root };
        Box<TestTree> child2 = new TestTree(2) { Parent = root };
        Box<TestTree> grandChild = new TestTree(3) { Parent = child2.Item };
        child2.Item.Children = new[] { grandChild };
        root.Item.Children = new[] { child1, child2 };

        // Act
        var foundNode = grandChild.FindByKeysUpward<TestTree, int>(new[] { 3, 2, 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByKeysUpward_WithPartialSequence_ShouldReturnResult()
    {
        // Arrange
        var root = new Box<TestTree>();
        var child1 = new TestTree(1) { Parent = root };
        var child2 = new TestTree(2) { Parent = root };
        Box<TestTree> grandChild = new TestTree(3) { Parent = child2 };
        child2.Children = new[] { grandChild };
        root.Item.Children = new Box<TestTree>[] { child1, child2 };

        // Act
        var foundNode = grandChild.FindByKeysUpward<TestTree, int>(new[] { 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveParent<TestTree>, IHaveKey<int>, IEquatable<TestTree>
    {
        public Box<TestTree>[] Children { get; internal set; } = Array.Empty<Box<TestTree>>();
        public Box<TestTree>? Parent { get; internal set; }
        public int Key { get; }
        public TestTree(int key)
        {
            Parent = null;
            Key = key;
        }

        public bool Equals(TestTree other) => Children.Equals(other.Children) && Equals(Parent, other.Parent) && Key == other.Key;
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Children, Parent, Key);

    }
}
