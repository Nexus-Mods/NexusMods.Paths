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
        var root = TestTree.Create(1);
        var child2 = TestTree.Create(2, root);
        var grandChild = TestTree.Create(3, child2);
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
        var child1 = TestTree.Create(1, root);
        var child2 = TestTree.Create(2, root);
        var grandChild = TestTree.Create(3, child2);
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
        var root = TestTree.Create(0);
        var child1 = TestTree.Create(1, root);
        var child2 = TestTree.Create(2, root);
        var grandChild = TestTree.Create(3, child2);
        child2.Item.Children = new[] { grandChild };
        root.Item.Children = new[] { child1, child2 };

        // Act
        var foundNode = grandChild.FindByKeysUpward<TestTree, int>(new[] { 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveParent<TestTree>, IHaveKey<int>, IEquatable<TestTree>
    {
        public Box<TestTree>[] Children { get; internal set; }
        public Box<TestTree>? Parent { get; internal init; }
        public int Key { get; private init; }
        public static Box<TestTree> Create(int key, Box<TestTree>? parent = null) =>
            (Box<TestTree>) new TestTree()
            {
                Parent = parent,
                Children = Array.Empty<Box<TestTree>>(),
                Key = key
            };

        public bool Equals(TestTree other) => Children.Equals(other.Children) && Equals(Parent, other.Parent) && Key == other.Key;
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Children, Parent, Key);
    }
}
