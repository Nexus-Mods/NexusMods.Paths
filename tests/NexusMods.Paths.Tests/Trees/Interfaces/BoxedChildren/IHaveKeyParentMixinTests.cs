using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyParentMixinTests
{
    [Fact]
    public void FindSubPathsByKeyUpward_WithNestedPath_ShouldReturnAllMatchingNodes()
    {
        // Arrange
        var deepChild1 = new TestTree(null, 5);
        var deepChild2 = new TestTree(null, 5); // Same key as deepChild1
        var grandChild1 = new TestTree(deepChild1, 4);
        var grandChild2 = new TestTree(deepChild2, 4); // Same key as grandChild1
        var child = new TestTree(new ChildBox<TestTree>[] { grandChild1, grandChild2 }, 3);
        ChildBox<TestTree> root = new TestTree(child, 2);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(new[] { 4, 5 });

        // Assert
        foundNodes.Count.Should().Be(2);
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
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(new[] { 4 });

        // Assert
        foundNodes.Count.Should().Be(1);
        foundNodes[0].Key.Should().Be(4);
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithNonExistingPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(new[] { 99 });

        // Assert
        foundNodes.Should().BeEmpty();
    }

    [Fact]
    public void FindSubPathsByKeyUpward_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNodes = root.FindSubPathsByKeyUpward<TestTree, int>(Array.Empty<int>());

        // Assert
        foundNodes.Should().BeEmpty();
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveKey<int>, IHaveParent<TestTree>
    {
        public ParentBox<TestTree>? Parent { get; private set; }
        public ChildBox<TestTree>[] Children { get; }
        public int Key { get; }

        public TestTree(ChildBox<TestTree>[]? children, int key = default)
        {
            Key = key;
            Children = children ?? Array.Empty<ChildBox<TestTree>>();
            foreach (var child in Children)
                child.Item.Parent = this;
        }

        public TestTree(TestTree child, int key = default)
        {
            Key = key;
            child.Parent = this;
            Children = new ChildBox<TestTree>[]{ child };
        }
    }
}
