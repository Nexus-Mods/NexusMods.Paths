using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Paths.Tests.Trees.Interfaces.BoxedChildren;

// ReSharper disable once InconsistentNaming
public class IHaveParentTests
{
    [Fact]
    public void GetSiblings_WithNoParent_ReturnsHasNoParent()
    {
        Box<TestTree> root = new TestTree();
        root.Item.HasParent().Should().Be(false);
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsIsTreeRoot()
    {
        Box<TestTree> root = new TestTree();
        root.Item.IsTreeRoot().Should().Be(true);
    }

    [Fact]
    public void EnumerateSiblings_WithNoParent_YieldsNoSiblings()
    {
        Box<TestTree> root = new TestTree();
        var siblings = root.EnumerateSiblings().ToArray();
        siblings.Should().BeEmpty();
    }

    [Fact]
    public void EnumerateSiblings_WithParent_YieldsAllButSelf()
    {
        var root = new Box<TestTree>();
        Box<TestTree> sibling1 = new TestTree(0) { Parent = root };
        Box<TestTree> sibling2 = new TestTree(1) { Parent = root };
        Box<TestTree> sibling3 = new TestTree(2) { Parent = root };
        root.Item.Children = new[]
        {
            new Box<TestTree> { Item = sibling1 },
            new Box<TestTree> { Item = sibling2 },
            new Box<TestTree> { Item = sibling3 },
        };

        var siblings = sibling3.Item.EnumerateSiblings().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().Contain(new Box<TestTree> { Item = sibling1 });
        siblings.Should().Contain(new Box<TestTree> { Item = sibling2 });
    }

    [Fact]
    public void EnumerateSiblings_WithParent_YieldsAllButSelf_WhenBoxed()
    {
        var root = new Box<TestTree>();
        Box<TestTree> sibling1 = new TestTree(0) { Parent = root };
        Box<TestTree> sibling2 = new TestTree(1) { Parent = root };
        Box<TestTree> sibling3 = new TestTree(2) { Parent = root };
        root.Item.Children = new[]
        {
            new Box<TestTree> { Item = sibling1 },
            new Box<TestTree> { Item = sibling2 },
            new Box<TestTree> { Item = sibling3 },
        };

        var sibling3Boxed = root.Item.Children.First(x => x.Item.Value == 2);
        var siblings = sibling3Boxed.EnumerateSiblings().ToArray();
        siblings.Should().HaveCount(2);
        siblings.Should().Contain(root.Item.Children.First(x => x.Item.Value == 0));
        siblings.Should().Contain(root.Item.Children.First(x => x.Item.Value == 1));
    }

    [Fact]
    public void GetSiblingCount_WithNoParent_ReturnsZero()
    {
        Box<TestTree> root = new TestTree();
        root.GetSiblingCount().Should().Be(0);
    }

    [Fact]
    public void GetSiblingCount_WithParent_ReturnsParentChildCountMinusOne()
    {
        var root = new Box<TestTree>();
        root.Item.Children = new Box<TestTree>[]
        {
            new TestTree() { Parent = root },
            new TestTree() { Parent = root }
        };

        var tree = root.Item.Children[0].Item;
        tree.GetSiblingCount().Should().Be(1);
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsEmpty_WhenBoxed()
    {
        Box<TestTree> root = new TestTree();
        root.GetSiblings().Should().BeEmpty();
    }

    [Fact]
    public void GetSiblings_WithNoParent_ReturnsEmpty()
    {
        var root = new TestTree();
        root.GetSiblings().Should().BeEmpty();
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsAllButSelf()
    {
        var root = new Box<TestTree>();
        Box<TestTree> sibling1 = new TestTree(0) { Parent = root };
        Box<TestTree> sibling2 = new TestTree(1) { Parent = root };
        Box<TestTree> sibling3 = new TestTree(2) { Parent = root };
        root.Item.Children = new[]
        {
            new Box<TestTree> { Item = sibling1 },
            new Box<TestTree> { Item = sibling2 },
            new Box<TestTree> { Item = sibling3 },
        };

        var siblings = sibling3.Item.GetSiblings();
        siblings.Should().Equal(sibling1.Item, sibling2.Item);
    }

    [Fact]
    public void GetSiblings_WithParent_ReturnsAllButSelf_WhenBoxed()
    {
        var root = new Box<TestTree>();
        Box<TestTree> sibling1 = new TestTree(0) { Parent = root };
        Box<TestTree> sibling2 = new TestTree(1) { Parent = root };
        Box<TestTree> sibling3 = new TestTree(2) { Parent = root };
        root.Item.Children = new[]
        {
            new Box<TestTree> { Item = sibling1 },
            new Box<TestTree> { Item = sibling2 },
            new Box<TestTree> { Item = sibling3 },
        };

        var sibling1Boxed = root.Item.Children.First(x => x.Item.Value == 0);
        var sibling2Boxed = root.Item.Children.First(x => x.Item.Value == 1);
        var sibling3Boxed = root.Item.Children.First(x => x.Item.Value == 2);
        var siblings = sibling3Boxed.GetSiblings();
        siblings.Should().Equal(sibling1Boxed, sibling2Boxed);
    }

    private struct TestTree : IHaveBoxedChildren<TestTree>, IHaveParent<TestTree>, IEquatable<TestTree>
    {
        public Box<TestTree>[] Children { get; internal set; } = Array.Empty<Box<TestTree>>();
        public Box<TestTree>? Parent { get; internal set; }
        public int Value { get; internal set; }
        public TestTree(int value)
        {
            Parent = null;
            Value = value;
        }

        public bool Equals(TestTree other) => Children.Equals(other.Children) && Equals(Parent, other.Parent) && Value == other.Value;
        public override bool Equals(object? obj) => obj is TestTree other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Children, Parent, Value);
    }
}
