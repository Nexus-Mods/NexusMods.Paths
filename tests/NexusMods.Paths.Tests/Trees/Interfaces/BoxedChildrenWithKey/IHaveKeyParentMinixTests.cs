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
        var deepChild1 = new TestTree(null, 5);
        var grandChild1 = new TestTree(deepChild1, 4);
        var child = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>>
        {
            { grandChild1.Key, grandChild1 }
        }, 3);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes.All(node => node.Key == 5).Should().BeTrue();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithPartialMatchingPath_ShouldReturnPartialMatches()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 4);
        var grandChild2 = new TestTree(null, 5);
        var child1 = new TestTree(grandChild1, 2);
        var child2 = new TestTree(grandChild2, 3);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>>
        {
            { 2, child1 },
            { 3, child2 }
        }, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(new Dictionary<int, ChildWithKeyBox<int, TestTree>> { { 2, child } }, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildWithKeyBox<int, TestTree> root = new TestTree(child, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveKey<int>, IHaveParent<TestTree>
    {
        public Box<TestTree>? Parent { get; private set; }
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; }
        public int Key { get; }

        public TestTree(Dictionary<int, ChildWithKeyBox<int, TestTree>>? children, int key = default)
        {
            Key = key;
            Children = children ?? new Dictionary<int, ChildWithKeyBox<int, TestTree>>();
            foreach (var child in Children.Values)
            {
                child.Item.Parent = this;
            }
        }

        public TestTree(TestTree child, int key = default)
        {
            Key = key;
            child.Parent = this;
            Children = new Dictionary<int, ChildWithKeyBox<int, TestTree>>
            {
                { child.Key, child }
            };
        }
    }
}
