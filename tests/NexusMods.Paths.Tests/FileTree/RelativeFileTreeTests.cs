using NexusMods.Paths.FileTree;

namespace NexusMods.Paths.Tests.FileTree;

public class RelativeFileTreeTests
{
    [Theory]
    [InlineData("file1.txt", true, 1)]
    [InlineData("file2.txt", false, 2)]
    [InlineData("foo/file2.txt", true, 2)]
    [InlineData("foo/file3.txt", true, 3)]
    [InlineData("foo/bar/file4.txt", true, 4)]
    [InlineData("baz/bazer/file5.txt", true, 5)]
    public void Test_FindNode(string path, bool found, int value)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        if (found)
        {
            node.Should().NotBeNull();
            var (testPath, testValue) = node!;
            testPath.Should().Be((RelativePath)path);
            testValue.Should().Be(value);
        }
        else
        {
            node.Should().BeNull();
        }
    }

    [Theory]
    [InlineData("file1.txt", "file1.txt")]
    [InlineData("foo", "foo")]
    [InlineData("foo/file2.txt", "file2.txt")]
    [InlineData("foo/file3.txt", "file3.txt")]
    [InlineData("foo/bar", "bar")]
    [InlineData("foo/bar/file4.txt", "file4.txt")]
    [InlineData("baz/bazer", "bazer")]
    [InlineData("baz/bazer/file5.txt", "file5.txt")]
    public void Test_Name(string path, string name)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.Name.Should().Be((RelativePath)name);
    }

    [Theory]
    [InlineData("file1.txt", true)]
    [InlineData("foo", false)]
    [InlineData("foo/file2.txt", true)]
    [InlineData("foo/file3.txt", true)]
    [InlineData("foo/bar", false)]
    [InlineData("foo/bar/file4.txt", true)]
    [InlineData("baz/bazer", false)]
    [InlineData("baz/bazer/file5.txt", true)]
    public void Test_IsFile(string path, bool isFile)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.IsFile.Should().Be(isFile);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("file1.txt", true)]
    [InlineData("foo", true)]
    [InlineData("foo/file2.txt", true)]
    public void Test_HasParent(string path, bool hasParent)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.HasParent.Should().Be(hasParent);
        if (hasParent)
        {
            node.Parent.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("file1.txt", false)]
    [InlineData("foo", false)]
    [InlineData("foo/file2.txt", false)]
    public void Test_IsTreeRoot(string path, bool isRoot)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.IsTreeRoot.Should().Be(isRoot);
        if (isRoot)
        {
            var act = () => node.Parent;
            act.Should().Throw<InvalidOperationException>();
        }

        node.Root.Path.Should().Be("");
    }


    [Theory]
    [InlineData("", new string[] { })]
    [InlineData("file1.txt", new[] { "foo", "baz" })]
    [InlineData("foo", new[] { "file1.txt", "baz" })]
    [InlineData("foo/bar/file4.txt", new string[] { })]
    public void Test_GetSiblings(string path, string[] expectedSiblingPaths)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.GetSiblings().Select(x => x.Path).Should()
            .BeEquivalentTo(expectedSiblingPaths.Select(x => (RelativePath)x));
    }

    [Theory]
    [InlineData("",
        new[]
            { "file1.txt", "foo/file2.txt", "foo/file3.txt", "foo/bar/file4.txt", "baz/bazer/file5.txt" })]
    [InlineData("file1.txt", new string[] { })]
    [InlineData("foo", new[] { "foo/file2.txt", "foo/file3.txt", "foo/bar/file4.txt" })]
    [InlineData("foo/file2.txt", new string[] { })]
    [InlineData("foo/bar", new[] { "foo/bar/file4.txt" })]
    [InlineData("baz", new[] { "baz/bazer/file5.txt" })]
    public void Test_GetAllDescendentFiles(string path, string[] expectedDescendentPaths)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.GetAllDescendentFiles().Select(x => x.Path).Should()
            .BeEquivalentTo(expectedDescendentPaths.Select(x => (RelativePath)x));
    }


    [Theory]
    [InlineData("",
        new[]
            { "file1.txt", "foo/file2.txt", "foo/file3.txt", "foo/bar/file4.txt", "baz/bazer/file5.txt" })]
    [InlineData("file1.txt", new string[] { })]
    [InlineData("foo", new[] { "foo/file2.txt", "foo/file3.txt", "foo/bar/file4.txt" })]
    [InlineData("foo/file2.txt", new string[] { })]
    [InlineData("foo/bar", new[] { "foo/bar/file4.txt" })]
    [InlineData("baz", new[] { "baz/bazer/file5.txt" })]
    public void Test_GetAllDescendentFilesDictionary(string path, string[] expectedDescendentPaths)
    {
        var tree = MakeTestTree();

        var node = tree.FindNode((RelativePath)path);
        node.Should().NotBeNull();
        node!.GetAllDescendentFilesDictionary().Select(x => x.Key).Should()
            .BeEquivalentTo(expectedDescendentPaths.Select(x => (RelativePath)x));
    }

    [Theory]
    [InlineData("bar", new [] {"foo/bar"}, new[] {2})]
    [InlineData("foo", new [] {"foo"}, new[] {1})]
    [InlineData("bazer", new [] {"baz/bazer"}, new[] {2})]

    public void Test_FindSubPath(string prefix, string[] paths, int[] depths)
    {
        var tree = MakeTestTree();

        var nodes = tree.FindSubPath((RelativePath)prefix).ToArray();
        nodes.Should().HaveCount(paths.Length);
        nodes.Should().HaveCount(depths.Length);

        nodes.Select(f => (string)f.Path).Should().Contain(paths);

        foreach (var (path, depth) in paths.Zip(depths))
        {
            var node = nodes.First(n => n.Path.StartsWith(path));
            node.Depth.Should().Be((ushort)depth);
        }

    }

    private static FileTreeNode<RelativePath, int> MakeTestTree()
    {
        Dictionary<RelativePath, int> fileEntries;

        fileEntries = new Dictionary<RelativePath, int>
        {
            { new RelativePath("file1.txt"), 1 },
            { new RelativePath("foo/file2.txt"), 2 },
            { new RelativePath("foo/file3.txt"), 3 },
            { new RelativePath("foo/bar/file4.txt"), 4 },
            { new RelativePath("baz/bazer/file5.txt"), 5 },
        };

        return FileTreeNode<RelativePath, int>.CreateTree(fileEntries);
    }
}
