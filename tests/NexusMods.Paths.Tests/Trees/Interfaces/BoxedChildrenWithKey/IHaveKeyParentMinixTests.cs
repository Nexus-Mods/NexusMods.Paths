using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveKeyParentMixinTests
{
    [Fact]
    public void FindSubPathsByKeyUpward_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var grandChild1 = TestTree.Create(deepChild1, 4);
        var child = TestTree.Create(3, new Dictionary<int, KeyedBox<int, TestTree>>
        {
            { 4, grandChild1 }
        });
        var root = TestTree.Create(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes.All(node => node.Item.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = TestTree.Create(4);
        var grandChild2 = TestTree.Create(5);
        var child1 = TestTree.Create(grandChild1, 2);
        var child2 = TestTree.Create(grandChild2, 3);
        var root = TestTree.Create(1, new Dictionary<int, KeyedBox<int, TestTree>>
        {
            { 2, child1 },
            { 3, child2 }
        });

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Item.Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, new Dictionary<int, KeyedBox<int, TestTree>> { { 2, child } });

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = TestTree.Create(5);
        var grandChild1 = TestTree.Create(deepChild1, 4);
        var child = TestTree.Create(3, new Dictionary<int, KeyedBox<int, TestTree>>
        {
            { 4, grandChild1 }
        });
        var root = TestTree.Create(child, 2);

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes.All(node => node.Item.Key == 4).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = TestTree.Create(4);
        var grandChild2 = TestTree.Create(5);
        var child1 = TestTree.Create(grandChild1, 2);
        var child2 = TestTree.Create(grandChild2, 3);
        var root = TestTree.Create(1, new Dictionary<int, KeyedBox<int, TestTree>>
        {
            { 2, child1 },
            { 3, child2 }
        });

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Item.Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(1, new Dictionary<int, KeyedBox<int, TestTree>> { { 2, child } });

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathRootsByKeyUpward_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = TestTree.Create(2);
        var root = TestTree.Create(child, 1);

        // Act
        var foundNodes = root.FindSubPathRootsByKeyUpward(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveKey<int>, IHaveParent<TestTree>
    {
        public Box<TestTree>? Parent { get; private set; }
        public Dictionary<int, KeyedBox<int, TestTree>> Children { get; private init; }
        public int Key { get; private init; }

        public static KeyedBox<int, TestTree> Create(int key = default, Dictionary<int, KeyedBox<int, TestTree>>? children = null)
        {
            var result = (KeyedBox<int, TestTree>)new TestTree()
            {
                Key = key,
                Children = children ?? new Dictionary<int, KeyedBox<int, TestTree>>()
            };

            foreach (var child in result.Item.Children.Values)
                child.Item.Parent = result;

            return result;
        }

        public static KeyedBox<int, TestTree> Create(KeyedBox<int, TestTree> child, int key = default)
        {
            var result = (KeyedBox<int, TestTree>) new TestTree()
            {
                Key = key,
                Children = new Dictionary<int, KeyedBox<int, TestTree>>
                {
                    { child.Item.Key, child }
                }
            };

            child.Item.Parent = result;
            return result;
        }
    }
}
