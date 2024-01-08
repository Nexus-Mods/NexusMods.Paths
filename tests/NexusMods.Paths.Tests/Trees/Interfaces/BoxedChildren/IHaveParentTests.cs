using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveParentTests
{
    [Fact]
    public void GetSiblings_WithNoParent_ReturnsHasNoParent()
    {
        var root = TestTree.Create();
        root.HasParent().Should().Be(false);
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsIsTreeRoot()
    {
        var root = TestTree.Create();
        root.IsTreeRoot().Should().Be(true);
    }

    [Fact]
    public void EnumerateSiblings_WithNoParent_YieldsNoSiblings()
    {
        var root = TestTree.Create();
        var siblings = root.EnumerateSiblings().ToArray();
        siblings.Should().BeEmpty();

        // Unboxed
        siblings = root.Item.EnumerateSiblings().ToArray();
        siblings.Should().BeEmpty();
    }

    [Fact]
    public void EnumerateSiblings_WithParent_YieldsAllButSelf()
    {
        var root = new Box<TestTree>();
        var sibling1 = TestTree.Create(0, root);
        var sibling2 = TestTree.Create(1, root);
        var sibling3 = TestTree.Create(2, root);
        root.Item.Children = new[]
        {
            sibling1,
            sibling2,
            sibling3
        };

        var sibling3Boxed = root.Item.Children.First(x => x.Item.Value == 2);
        var siblings = sibling3Boxed.EnumerateSiblings().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().Contain(root.Item.Children.First(x => x.Item.Value == 0));
        siblings.Should().Contain(root.Item.Children.First(x => x.Item.Value == 1));

        // Unboxed
        siblings = sibling3Boxed.Item.EnumerateSiblings().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().Contain(root.Item.Children.First(x => x.Item.Value == 0));
        siblings.Should().Contain(root.Item.Children.First(x => x.Item.Value == 1));
    }

    [Fact]
    public void GetSiblingCount_WithNoParent_ReturnsZero()
    {
        var root = TestTree.Create();
        root.GetSiblingCount().Should().Be(0);
    }

    [Fact]
    public void GetSiblingCount_WithParent_ReturnsParentChildCountMinusOne()
    {
        var root = new Box<TestTree>();
        root.Item.Children = new[]
        {
            TestTree.Create(0, root),
            TestTree.Create(0, root)
        };

        var tree = root.Item.Children[0].Item;
        tree.GetSiblingCount().Should().Be(1);
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsEmpty()
    {
        var root = TestTree.Create();
        root.GetSiblings().Should().BeEmpty();
        root.Item.GetSiblings().Should().BeEmpty();
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsAllButSelf()
    {
        var root = new Box<TestTree>();
        var sibling1 = TestTree.Create(0, root);
        var sibling2 = TestTree.Create(1, root);
        var sibling3 = TestTree.Create(2, root);
        root.Item.Children = new[]
        {
            sibling1,
            sibling2,
            sibling3
        };

        var sibling1Boxed = root.Item.Children.First(x => x.Item.Value == 0);
        var sibling2Boxed = root.Item.Children.First(x => x.Item.Value == 1);
        var sibling3Boxed = root.Item.Children.First(x => x.Item.Value == 2);
        var siblings = sibling3Boxed.GetSiblings();
        siblings.Should().Equal(sibling1Boxed, sibling2Boxed);

        var siblingsUnboxed = sibling3Boxed.Item.GetSiblings();
        siblingsUnboxed.Should().Equal(sibling1Boxed, sibling2Boxed);
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveParent<TestTree>, IEquatable<TestTree>
    {
        public Box<TestTree>[] Children { get; internal set; }
        public Box<TestTree>? Parent { get; internal init; }
        public int Value { get; internal init; }
        public static Box<TestTree> Create(int value = default, Box<TestTree>? parent = null)
            => (Box<TestTree>) new TestTree
            {
                Parent = parent,
                Value = value,
                Children = Array.Empty<Box<TestTree>>()
            };

        public bool Equals(TestTree other) => Children.Equals(other.Children) && Equals(Parent, other.Parent) && Value == other.Value;
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Children, Parent, Value);
    }
}
