using System.Runtime.InteropServices;
using NexusMods.Paths.FileTree;

namespace NexusMods.Paths.Tests.FileTree;

public class AbsoluteFileTreeTests
{
    [Theory]
    [InlineData("/file1.txt", true, true, 1)]
    [InlineData("/file2.txt", true, false, 2)]
    [InlineData("/foo/file2.txt", true, true, 2)]
    [InlineData("/foo/file3.txt", true, true, 3)]
    [InlineData("/foo/bar/file4.txt", true, true, 4)]
    [InlineData("/baz/bazer/file5.txt", true, true, 5)]
    [InlineData("/bazer/file5.txt", true, false, 5)]
    [InlineData("C:/file1.txt", false, true, 1)]
    [InlineData("C:/file2.txt", false, false, 2)]
    [InlineData("C:/foo/file2.txt", false, true, 2)]
    [InlineData("C:/foo/file3.txt", false, true, 3)]
    [InlineData("C:/foo/bar/file4.txt", false, true, 4)]
    [InlineData("C:/baz/bazer/file5.txt", false, true, 5)]
    [InlineData("C:/bazer/file5.txt", false, false, 5)]
    public void Test_FindNode(string path, bool isUnix, bool found, int value)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        if (found)
        {
            node.Should().NotBeNull();
            var (testPath, testValue) = node!;
            testPath.Should().Be(CreateAbsPath(path, isUnix));
            testValue.Should().Be(value);
        }
        else
        {
            node.Should().BeNull();
        }
    }

    [Theory]
    [InlineData("/file1.txt", true, "file1.txt")]
    [InlineData("/foo", true, "foo")]
    [InlineData("/foo/file2.txt", true, "file2.txt")]
    [InlineData("/foo/file3.txt", true, "file3.txt")]
    [InlineData("/foo/bar", true, "bar")]
    [InlineData("/foo/bar/file4.txt", true, "file4.txt")]
    [InlineData("/baz/bazer", true, "bazer")]
    [InlineData("/baz/bazer/file5.txt", true, "file5.txt")]
    [InlineData("C:/file1.txt", false, "file1.txt")]
    [InlineData("C:/foo", false, "foo")]
    [InlineData("C:/foo/file2.txt", false, "file2.txt")]
    [InlineData("C:/foo/file3.txt", false, "file3.txt")]
    [InlineData("C:/foo/bar", false, "bar")]
    public void Test_Name(string path, bool isUnix, string name)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.Name.Should().Be((RelativePath)name);
    }

    [Theory]
    [InlineData("/file1.txt", true, true)]
    [InlineData("/foo", true, false)]
    [InlineData("/foo/file2.txt", true, true)]
    [InlineData("/foo/file3.txt", true, true)]
    [InlineData("/foo/bar", true, false)]
    [InlineData("/foo/bar/file4.txt", true, true)]
    [InlineData("/baz/bazer", true, false)]
    [InlineData("/baz/bazer/file5.txt", true, true)]
    [InlineData("C:/file1.txt", false, true)]
    [InlineData("C:/foo", false, false)]
    [InlineData("C:/foo/file2.txt", false, true)]
    [InlineData("C:/foo/file3.txt", false, true)]
    [InlineData("C:/foo/bar", false, false)]
    public void Test_IsFile(string path, bool isUnix, bool isFile)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.IsFile.Should().Be(isFile);
    }



    [Theory]
    [InlineData("/", true, false)]
    [InlineData("/file1.txt", true, true)]
    [InlineData("/foo", true, true)]
    [InlineData("/foo/file2.txt", true, true)]
    [InlineData("C:/", false, false)]
    [InlineData("C:/file1.txt", false, true)]
    [InlineData("C:/foo", false, true)]
    [InlineData("C:/foo/file2.txt", false, true)]
    public void Test_HasParent(string path, bool isUnix, bool hasParent)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.HasParent.Should().Be(hasParent);
        if (hasParent)
        {
            node.Parent.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("/", true, true)]
    [InlineData("/file1.txt", true, false)]
    [InlineData("/foo", true, false)]
    [InlineData("/foo/file2.txt", true, false)]
    [InlineData("C:/", false, true)]
    [InlineData("C:/file1.txt", false, false)]
    [InlineData("C:/foo", false, false)]
    [InlineData("C:/foo/file2.txt", false, false)]
    public void Test_IsTreeRoot(string path, bool isUnix, bool isRoot)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.IsTreeRoot.Should().Be(isRoot);
        if (isRoot)
        {
            var act = () => node.Parent;
            act.Should().Throw<InvalidOperationException>();
        }

        node.Root.Path.Should().Be(isUnix ? CreateAbsPath("/", isUnix) : CreateAbsPath("C:/", isUnix));
    }

    [Theory]
    [InlineData("/", true, new string[] { })]
    [InlineData("/file1.txt", true, new[] { "/foo", "/baz" })]
    [InlineData("/foo", true, new[] { "/file1.txt", "/baz" })]
    [InlineData("/foo/bar/file4.txt", true, new string[] { })]
    [InlineData("C:/", false, new string[] { })]
    [InlineData("C:/file1.txt", false, new[] { "C:/foo", "C:/baz" })]
    [InlineData("C:/foo", false, new[] { "C:/file1.txt", "C:/baz" })]
    [InlineData("C:/foo/bar/file4.txt", false, new string[] { })]
    public void Test_GetSiblings(string path, bool isUnix, string[] expectedSiblingPaths)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.GetSiblings().Select(x => x.Path).Should()
            .BeEquivalentTo(expectedSiblingPaths.Select(x => CreateAbsPath(x, isUnix)));
    }

    [Theory]
    [InlineData("/", true,
        new[]
            { "/file1.txt", "/foo/file2.txt", "/foo/file3.txt", "/foo/bar/file4.txt", "/baz/bazer/file5.txt" })]
    [InlineData("/file1.txt", true, new string[] { })]
    [InlineData("/foo", true, new[] { "/foo/file2.txt", "/foo/file3.txt", "/foo/bar/file4.txt" })]
    [InlineData("/foo/file2.txt", true, new string[] { })]
    [InlineData("/foo/bar", true, new[] { "/foo/bar/file4.txt" })]
    [InlineData("/baz", true, new[] { "/baz/bazer/file5.txt" })]
    [InlineData("C:/", false,
        new[]
            { "C:/file1.txt", "C:/foo/file2.txt", "C:/foo/file3.txt", "C:/foo/bar/file4.txt", "C:/baz/bazer/file5.txt" })]
    [InlineData("C:/file1.txt", false, new string[] { })]
    [InlineData("C:/foo", false, new[] { "C:/foo/file2.txt", "C:/foo/file3.txt", "C:/foo/bar/file4.txt" })]
    [InlineData("C:/foo/file2.txt", false, new string[] { })]
    [InlineData("C:/foo/bar", false, new[] { "C:/foo/bar/file4.txt" })]
    [InlineData("C:/baz", false, new[] { "C:/baz/bazer/file5.txt" })]
    public void Test_GetAllDescendentFiles(string path, bool isUnix, string[] expectedDescendentPaths)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.GetAllDescendentFiles().Select(x => x.Path).Should()
            .BeEquivalentTo(expectedDescendentPaths.Select(x => CreateAbsPath(x, isUnix)));
    }


    [Theory]
    [InlineData("/", true,
        new[]
            { "/file1.txt", "/foo/file2.txt", "/foo/file3.txt", "/foo/bar/file4.txt", "/baz/bazer/file5.txt" })]
    [InlineData("/file1.txt", true, new string[] { })]
    [InlineData("/foo", true, new[] { "/foo/file2.txt", "/foo/file3.txt", "/foo/bar/file4.txt" })]
    [InlineData("/foo/file2.txt", true, new string[] { })]
    [InlineData("/foo/bar", true, new[] { "/foo/bar/file4.txt" })]
    [InlineData("/baz", true, new[] { "/baz/bazer/file5.txt" })]
    [InlineData("C:/", false,
        new[]
            { "C:/file1.txt", "C:/foo/file2.txt", "C:/foo/file3.txt", "C:/foo/bar/file4.txt", "C:/baz/bazer/file5.txt" })]
    [InlineData("C:/file1.txt", false, new string[] { })]
    [InlineData("C:/foo", false, new[] { "C:/foo/file2.txt", "C:/foo/file3.txt", "C:/foo/bar/file4.txt" })]
    [InlineData("C:/foo/file2.txt", false, new string[] { })]
    [InlineData("C:/foo/bar", false, new[] { "C:/foo/bar/file4.txt" })]
    [InlineData("C:/baz", false, new[] { "C:/baz/bazer/file5.txt" })]
    public void Test_GetAllDescendentFilesDictionary(string path, bool isUnix, string[] expectedDescendentPaths)
    {
        var tree = MakeTestTree(isUnix);

        var node = tree.FindNode(CreateAbsPath(path, isUnix));
        node.Should().NotBeNull();
        node!.GetAllDescendentFilesDictionary().Select(x => x.Key).Should()
            .BeEquivalentTo(expectedDescendentPaths.Select(x => CreateAbsPath(x, isUnix)));
    }

    private static FileTreeNode<AbsolutePath, int> MakeTestTree(bool isUnix = true)
    {
        Dictionary<AbsolutePath, int> fileEntries;
        if (isUnix)
        {
            fileEntries = new Dictionary<AbsolutePath, int>
            {
                { CreateAbsPath("/file1.txt", isUnix), 1 },
                { CreateAbsPath("/foo/file2.txt", isUnix), 2 },
                { CreateAbsPath("/foo/file3.txt", isUnix), 3 },
                { CreateAbsPath("/foo/bar/file4.txt", isUnix), 4 },
                { CreateAbsPath("/baz/bazer/file5.txt", isUnix), 5 },
            };
        }
        else
        {
            fileEntries = new Dictionary<AbsolutePath, int>
            {
                { CreateAbsPath("c:/file1.txt", isUnix), 1 },
                { CreateAbsPath("c:/foo/file2.txt", isUnix), 2 },
                { CreateAbsPath("c:/foo/file3.txt", isUnix), 3 },
                { CreateAbsPath("c:/foo/bar/file4.txt", isUnix), 4 },
                { CreateAbsPath("c:/baz/bazer/file5.txt", isUnix), 5 },
            };
        }

        return FileTreeNode<AbsolutePath, int>.CreateTree(fileEntries);
    }

    private static AbsolutePath CreateAbsPath(string input, bool isUnix = true)
    {
        var os = CreateOSInformation(isUnix);
        var fs = new InMemoryFileSystem(os);
        var path = AbsolutePath.FromSanitizedFullPath(input, fs);
        return path;
    }

    // ReSharper disable once InconsistentNaming
    private static IOSInformation CreateOSInformation(bool isUnix)
    {
        return isUnix ? new OSInformation(OSPlatform.Linux) : new OSInformation(OSPlatform.Windows);
    }
}
