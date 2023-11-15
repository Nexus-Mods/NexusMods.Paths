using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildrenWithKey;

// ReSharper disable once InconsistentNaming
public class IHaveParentTests
{
    [Fact]
    public void EnumerateSiblings_WithParent_YieldsAllButSelf()
    {
        var root = TestTree.Create();
        var child1 = TestTree.Create(0, root);
        var child2 = TestTree.Create(1, root);
        var child3 = TestTree.Create(2, root);
        root.Item.Children.Add(0, child1);
        root.Item.Children.Add(1, child2);
        root.Item.Children.Add(2, child3);

        var siblings = child1.EnumerateSiblings().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().ContainEquivalentOf(child2);
        siblings.Should().ContainEquivalentOf(child3);

        // Unboxed
        siblings = child1.Item.EnumerateSiblings<TestTree, int>().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().ContainEquivalentOf(child2);
        siblings.Should().ContainEquivalentOf(child3);
    }

    [Fact]
    public void GetSiblingCount_WithParent_ReturnsParentChildCountMinusOne()
    {
        Box<TestTree> root = TestTree.Create();
        var child = TestTree.Create(0, root);
        root.Item.Children.Add(0, child);
        root.Item.Children.Add(1, TestTree.Create(1, root));

        child.GetSiblingCount().Should().Be(1);
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsEmpty()
    {
        var root = TestTree.Create();
        root.GetSiblings().Should().BeEmpty();

        // Unboxed
        root.Item.GetSiblings<TestTree, int>().Should().BeEmpty();
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsCorrectSiblings()
    {
        var root = TestTree.Create();
        var tree = TestTree.Create(0, root);
        root.Item.Children.Add(0, tree);
        root.Item.Children.Add(1, TestTree.Create(1, root));

        var siblings = tree.GetSiblings();
        siblings.Should().ContainSingle();
        siblings[0].Item.Value.Should().Be(1);
    }

    [Fact]
    public void GetSiblingCount_WithNoParent_ReturnsZero()
    {
        var root = TestTree.Create(0);
        root.GetSiblingCount().Should().Be(0);
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsCorrectSiblings_WhenBoxed()
    {
        var parent = TestTree.Create();
        var child1 = TestTree.Create(0, parent);
        var child2 = TestTree.Create(1, parent);
        parent.Item.Children.Add(0, child1);
        parent.Item.Children.Add(1, child2);

        var siblings = child1.GetSiblings();
        siblings.Should().ContainSingle();
        siblings[0].Item.Value.Should().Be(1);

        var siblingsUnboxed = child1.Item.GetSiblings<TestTree, int>();
        siblingsUnboxed.Should().ContainSingle();
        siblingsUnboxed[0].Item.Value.Should().Be(1);
    }

    private struct TestTree : IHaveBoxedChildrenWithKey<int, TestTree>, IHaveParent<TestTree>, IEquatable<TestTree>
    {
        public Dictionary<int, KeyedBox<int, TestTree>> Children { get; private init; }
        public Box<TestTree>? Parent { get; private init; }
        public int Value { get; private init; }
        public static KeyedBox<int, TestTree> Create(int value, Box<TestTree>? parent = null)
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Parent = parent,
                Value = value,
                Children = new Dictionary<int, KeyedBox<int, TestTree>>(),
            };
        }

        public static KeyedBox<int, TestTree> Create()
        {
            return (KeyedBox<int, TestTree>) new TestTree()
            {
                Parent = null,
                Value = 0,
                Children = new Dictionary<int, KeyedBox<int, TestTree>>(),
            };
        }

        public bool Equals(TestTree other) => Children.Equals(other.Children) && Equals(Parent, other.Parent) && Value == other.Value;
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Children, Parent, Value);
    }
}
