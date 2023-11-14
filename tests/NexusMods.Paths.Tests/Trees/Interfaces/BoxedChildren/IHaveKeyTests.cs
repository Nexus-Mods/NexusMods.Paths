using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveKeyTests
{
    [Fact]
    public void EnumerateKeysBfs_ShouldReturnAllKeysInBreadthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, 0);

        // Act
        var keys = root.EnumerateKeysBfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 2, 3, 4); // Breadth-first order
    }

    [Fact]
    public void EnumerateKeysDfs_ShouldReturnAllKeysInDepthFirstOrder()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildBox<TestTree> root =  new TestTree(new ChildBox<TestTree>[] { child1, child2 }, 0);

        // Act
        var keys = root.EnumerateKeysDfs<TestTree, int>().ToArray();

        // Assert
        keys.Should().Equal(1, 3, 2, 4); // Depth-first order
    }

    [Fact]
    public void GetKeys_ShouldReturnAllKeysRecursively()
    {
        // Arrange
        var grandChild1 = new TestTree(null, 3);
        var grandChild2 = new TestTree(null, 4);
        var child1 = new TestTree(grandChild1, 1);
        var child2 = new TestTree(grandChild2, 2);
        ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 }, 0);

        // Act
        var keys = root.GetKeys<TestTree, int>();

        // Assert
        keys.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void FindByKeyFromChild_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild<TestTree, int>(new[] { 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromChild_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild<TestTree, int>(new[] { 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromChild_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromChild<TestTree, int>(new[] { 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    [Fact]
    public void FindByKeyFromRoot_WithExactPath_ShouldReturnCorrectNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot<TestTree, int>(new[] { 1, 2, 3 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(3);
    }

    [Fact]
    public void FindByKeyFromRoot_WithIncompletePath_ShouldReturnClosestNode()
    {
        // Arrange
        var grandchild = new TestTree(null, 3);
        var child = new TestTree(grandchild, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot<TestTree, int>(new[] { 1, 2 });

        // Assert
        foundNode!.Should().NotBeNull();
        foundNode!.Value.Key.Should().Be(2);
    }

    [Fact]
    public void FindByKeyFromRoot_WithNonExistingPath_ShouldReturnNull()
    {
        // Arrange
        var child = new TestTree(null, 2);
        ChildBox<TestTree> root = new TestTree(child, 1);

        // Act
        var foundNode = root.FindByKeyFromRoot<TestTree, int>(new[] { 99 });

        // Assert
        foundNode!.Should().BeNull();
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveKey<int>
    {
        public ChildBox<TestTree>[] Children { get; }
        public int Key { get; }

        public TestTree(ChildBox<TestTree>[]? children, int key = default)
        {
            Children = children ?? Array.Empty<ChildBox<TestTree>>();
            Key = key;
        }

        public TestTree(TestTree child, int key = default)
        {
            Children = new ChildBox<TestTree>[]{ child };
            Key = key;
        }
    }
}
