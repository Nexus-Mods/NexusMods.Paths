using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveParentTests
{
    [Fact]
    public void EnumerateSiblings_WithParent_YieldsAllButSelf()
    {
        ParentBox<TestTree> root = new TestTree();
        var child1 = new TestTree(0) { Parent = root };
        var child2 = new TestTree(1) { Parent = root };
        var child3 = new TestTree(2) { Parent = root };
        root.Item.Children.Add(0, child1);
        root.Item.Children.Add(1, child2);
        root.Item.Children.Add(2, child3);

        var siblings = child1.EnumerateSiblings<TestTree, int>().Select(x => x.Item).ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().ContainEquivalentOf(child2);
        siblings.Should().ContainEquivalentOf(child3);
    }

    [Fact]
    public void EnumerateSiblings_WithParent_YieldsAllButSelf_WhenBoxed()
    {
        ParentBox<TestTree> root = new TestTree();
        ChildWithKeyBox<int, TestTree> child1 = new TestTree(0) { Parent = root };
        ChildWithKeyBox<int, TestTree> child2 = new TestTree(1) { Parent = root };
        ChildWithKeyBox<int, TestTree> child3 = new TestTree(2) { Parent = root };
        root.Item.Children.Add(0, child1);
        root.Item.Children.Add(1, child2);
        root.Item.Children.Add(2, child3);

        var siblings = child1.EnumerateSiblings().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().ContainEquivalentOf(child2);
        siblings.Should().ContainEquivalentOf(child3);
    }

    [Fact]
    public void GetSiblingCount_WithNoParent_ReturnsZero()
    {
        TestTree root = new TestTree(0);
        root.GetSiblingCount<TestTree, int>().Should().Be(0);
    }

    [Fact]
    public void GetSiblingCount_WithParent_ReturnsParentChildCountMinusOne()
    {
        ParentBox<TestTree> root = new TestTree();
        var child = new TestTree(0) { Parent = root };
        root.Item.Children.Add(0,child );
        root.Item.Children.Add(1, new TestTree(1) { Parent = root });

        child.GetSiblingCount<TestTree, int>().Should().Be(1);
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsEmpty_WhenBoxed()
    {
        ChildWithKeyBox<int, TestTree> root = new TestTree();
        root.GetSiblings().Should().BeEmpty();
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsEmpty()
    {
        var root = new TestTree();
        root.GetSiblings<TestTree, int>().Should().BeEmpty();
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsCorrectSiblings()
    {
        ParentBox<TestTree> root = new TestTree();
        var tree = new TestTree(0) { Parent = root };
        root.Item.Children.Add(0, tree);
        root.Item.Children.Add(1, new TestTree(1) { Parent = root });

        var siblings = tree.GetSiblings<TestTree, int>();
        siblings.Should().ContainSingle();
        siblings[0].Item.Value.Should().Be(1);
    }

    [Fact]
    public void GetSiblingCount_WithNoParent_ReturnsZero_WhenBoxed()
    {
        ChildWithKeyBox<int, TestTree> root = new TestTree(0);
        root.GetSiblingCount().Should().Be(0);
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsCorrectSiblings_WhenBoxed()
    {
        ParentBox<TestTree> parent = new TestTree();
        ChildWithKeyBox<int, TestTree> child1 = new TestTree(0) { Parent = parent };
        ChildWithKeyBox<int, TestTree> child2 = new TestTree(1) { Parent = parent };
        parent.Item.Children.Add(0, child1);
        parent.Item.Children.Add(1, child2);

        var siblings = child1.GetSiblings();
        siblings.Should().ContainSingle();
        siblings[0].Item.Value.Should().Be(1);
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveParent<TestTree>, IEquatable<TestTree>
    {
        public Dictionary<int, ChildWithKeyBox<int, TestTree>> Children { get; } = new();
        public ParentBox<TestTree>? Parent { get; internal set; }
        public int Value { get; internal set; }
        public TestTree(int value)
        {
            Parent = null;
            Value = value;
        }

        public TestTree()
        {
            Parent = null;
            Value = 0;
        }

        public bool Equals(TestTree other) => Children.Equals(other.Children) && Equals(Parent, other.Parent) && Value == other.Value;
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Children, Parent, Value);
    }
}
