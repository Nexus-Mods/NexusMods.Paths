namespace NexusMods.Paths.Tests;

public class RelativePathTests
{
    // ReSharper disable once InconsistentNaming
    private static IOSInformation CreateOSInformation(bool isUnix)
    {
        return isUnix ? OSInformation.FakeUnix : OSInformation.FakeWindows;
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("a/b", "a/b")]
    [InlineData("a/b/c", "a/b/c")]
    public void Test_FromStringExplicitCast(string input, string expected)
    {
        var path = (RelativePath)input;
        path.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("a/b", "a/b")]
    [InlineData("a/b/c", "a/b/c")]
    [InlineData("Images/748-0-1477626175.jpg", "Images/748-0-1477626175.jpg")]
    public void Test_FromStringImplicitCast(string input, string expected)
    {
        // A little roundabout, but I wanted to make sure the cast happens as part
        // of a method call.
        var basePath = Paths.FileSystem.Shared.EnumerateRootDirectories().First();
        var path = basePath.Combine(input).RelativeTo(basePath);
        path.ToString().Should().Be(expected);
    }


    [Theory]
    [InlineData("a", "a")]
    [InlineData("a/", "a")]
    [InlineData("a/b", "a/b")]
    [InlineData("a\\b", "a/b")]
    [InlineData("a\\b/c", "a/b/c")]
    [InlineData("a\\b\\c\\", "a/b/c")]
    public void Test_FromUnsanitizedInput(
        string inputPath,
        string expectedRelativePath)
    {
        var sanitizedPath = RelativePath.FromUnsanitizedInput(inputPath);
        sanitizedPath.Should().Be(expectedRelativePath);
    }

    [Theory]
    [InlineData(true, "", "")]
    [InlineData(false, "", "")]
    [InlineData(true, "foo/bar", "foo/bar")]
    [InlineData(false, "foo/bar", "foo\\bar")]
    public void Test_ToNativeSeparators(bool isUnix, string input, string expected)
    {
        var os = CreateOSInformation(isUnix);

        var path = new RelativePath(input);
        path.ToNativeSeparators(os).Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "foo")]
    [InlineData("foo/bar", "bar")]
    public void Test_Name(string input, string expected)
    {
        var path = new RelativePath(input);
        path.Name.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "")]
    [InlineData("foo.txt", ".txt")]
    public void Test_Extension(string input, string expectedExtension)
    {
        var path = new RelativePath(input);
        path.Extension.Should().Be(new Extension(expectedExtension));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "foo")]
    [InlineData("foo/bar/baz", "baz")]
    public void Test_FileName(string input, string expectedFileName)
    {
        var path = new RelativePath(input);
        path.FileName.Should().Be(expectedFileName);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("foo", 0)]
    [InlineData("foo/bar", 1)]
    [InlineData("foo/bar/baz", 2)]
    public void Test_Depth(string input, int expectedDepth)
    {
        var path = new RelativePath(input);
        path.Depth.Should().Be(expectedDepth);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "")]
    [InlineData("foo/bar", "foo")]
    [InlineData("foo/bar/baz", "foo/bar")]
    public void Test_Parent(string input, string expectedParent)
    {
        var path = new RelativePath(input);
        path.Parent.Should().Be(expectedParent);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "foo")]
    [InlineData("foo/bar", "foo")]
    [InlineData("foo/bar/baz", "foo")]
    public void Test_TopParent(string input, string expectedParent)
    {
        var path = new RelativePath(input);
        path.TopParent.Should().Be(expectedParent);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "")]
    [InlineData("foo/bar", "")]
    [InlineData("foo/bar/baz", "")]
    public void Test_GetRootComponent(string input, string expectedRootComponent)
    {
        var path = new RelativePath(input);
        path.GetRootComponent.Should().Be(expectedRootComponent);
    }

    [Theory]
    [InlineData("", new string[] { })]
    [InlineData("foo", new[] { "foo" })]
    [InlineData("foo/bar", new[] { "foo", "bar" })]
    [InlineData("foo/bar/baz", new[] { "foo", "bar", "baz" })]
    public void Test_Parts(string input, string[] expectedParts)
    {
        var path = new RelativePath(input);
        path.Parts.Should().BeEquivalentTo(expectedParts.Select(x => new RelativePath(x)),
            opts => opts.WithStrictOrdering());
    }

    [Theory]
    [InlineData("", new string[] { })]
    [InlineData("foo", new[] { "foo" })]
    [InlineData("foo/bar", new[] { "foo/bar", "foo" })]
    [InlineData("foo/bar/baz", new[] { "foo/bar/baz", "foo/bar", "foo" })]
    public void Test_GetAllParents(string input, string[] expectedParts)
    {
        var path = new RelativePath(input);
        path.GetAllParents().Should().BeEquivalentTo(expectedParts.Select(x => new RelativePath(x)));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "foo")]
    [InlineData("foo/bar", "foo/bar")]
    [InlineData("foo/bar/baz", "foo/bar/baz")]
    public void Test_GetNonRootPart(string input, string expected)
    {
        var path = new RelativePath(input);
        path.GetNonRootPart().Should().Be(expected);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("foo", false)]
    [InlineData("foo/bar", false)]
    public void Test_IsRooted(string input, bool expected)
    {
        var path = new RelativePath(input);
        path.IsRooted.Should().Be(expected);
    }

    [Theory]
    [InlineData("foo", ".txt", "foo.txt")]
    [InlineData("foo.txt", ".md", "foo.md")]
    public void Test_ReplaceExtension(string input, string extension, string expectedOutput)
    {
        var path = new RelativePath(input);
        var actualOutput = path.ReplaceExtension(new Extension(extension));
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("foo", ".txt", "foo.txt")]
    [InlineData("foo.txt", ".md", "foo.txt.md")]
    public void Test_WithExtension(string input, string extension, string expectedOutput)
    {
        var path = new RelativePath(input);
        var actualOutput = path.WithExtension(new Extension(extension));
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("foo", "bar", "foo/bar")]
    [InlineData("foo/bar", "baz", "foo/bar/baz")]
    [InlineData("foo", "bar/baz", "foo/bar/baz")]
    public void Test_Join(string left, string right, string expectedOutput)
    {
        var leftPath = new RelativePath(left);
        var rightPath = new RelativePath(right);
        var actualOutput = leftPath.Join(rightPath);
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("foo", "bar", false)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo/bar", "foo", true)]
    [InlineData("foo/bar", "bar", false)]
    [InlineData("foo/bar/baz", "bar/baz", false)]
    [InlineData("foo/bar/baz", "foo/bar", true)]
    public void Test_StartsWith(string left, string right, bool expected)
    {
        var path = new RelativePath(left);
        var actual = path.StartsWith(right);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("foo", "bar", false)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo/bar", "foo", false)]
    [InlineData("foo/bar", "bar", true)]
    [InlineData("foo/bar/baz", "bar/baz", true)]
    [InlineData("foo/bar/baz", "foo/bar", false)]
    public void Test_EndsWith(string left, string right, bool expected)
    {
        var path = new RelativePath(left);
        var actual = path.EndsWith(right);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("foo", "bar", false)]
    [InlineData("foo/bar", "foo", true)]
    [InlineData("foo/bar/baz", "foo/bar", true)]
    public void Test_InFolder(string left, string right, bool expected)
    {
        var leftPath = new RelativePath(left);
        var rightPath = new RelativePath(right);
        var actual = leftPath.InFolder(rightPath);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "", true)]
    [InlineData("", "foo", false)]
    [InlineData("foo", "bar", false)]
    [InlineData("foo", "", true)]
    [InlineData("foo/bar/baz", "", true)]
    [InlineData("foo/bar/baz", "foo", true)]
    [InlineData("foo/bar/baz", "foo/bar", true)]
    [InlineData("foobar", "foo", false)]
    [InlineData("foo/bar/baz", "foo/baz", false)]
    public void Test_StartsWithRelative(string child, string parent, bool expected)
    {
        var childPath = (RelativePath)child;
        var parentPath = (RelativePath)parent;
        var actual = childPath.StartsWith(parentPath);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "", true)]
    [InlineData("", "foo", false)]
    [InlineData("foo", "bar", false)]
    [InlineData("foo", "", true)]
    [InlineData("foo/bar/baz", "", true)]
    [InlineData("foo/bar/baz", "bar/baz", true)]
    [InlineData("foo/bar/baz", "foo/bar/baz", true)]
    [InlineData("foobar", "bar", false)]
    [InlineData("foo/bar/baz", "foo/baz", false)]
    public void Test_EndsWithRelative(string child, string parent, bool expected)
    {
        var childPath = (RelativePath)child;
        var parentPath = (RelativePath)parent;
        var actual = childPath.EndsWith(parentPath);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("foo/bar/baz", 0, "foo/bar/baz")]
    [InlineData("foo/bar/baz", 1, "bar/baz")]
    [InlineData("foo/bar/baz", 2, "baz")]
    [InlineData("foo/bar/baz", 3, "")]
    public void Test_DropFirst(string input, int count, string expectedOutput)
    {
        var path = new RelativePath(input);
        var output = path.DropFirst(count);
        output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("foo/bar/baz", "foo", "bar/baz")]
    [InlineData("foo/bar/baz", "foo/bar", "baz")]
    [InlineData("foo/bar/baz", "foo/bar/baz", "")]
    public void Test_RelativeTo(string left, string right, string expectedOutput)
    {
        var leftPath = new RelativePath(left);
        var rightPath = new RelativePath(right);
        var actualOutput = leftPath.RelativeTo(rightPath);
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("foo/bar/baz", "Foo/Bar/Baz", true)]
    [InlineData("foo/bar/baz", "foo/bar/baZ", true)]
    [InlineData("foo/bar/baz", "foo/bar/bazz", false)]
    [InlineData("foo/bar", "bar/foo", false)]
    public void Test_Equals_CaseInsensitive(string path1, string path2, bool expected)
    {
        var relativePath1 = new RelativePath(path1);
        var relativePath2 = new RelativePath(path2);

        relativePath1.Equals(relativePath2).Should().Be(expected);
    }

    [Theory]
    [InlineData("foo/bar/baz", "Foo/Bar/Baz")]
    [InlineData("foo/bar/baz", "foo/bar/baZ")]
    [InlineData("FOO/BAR/BAZ", "foo/bar/baz")]
    public void Test_GetHashCode_CaseInsensitive(string path1, string path2)
    {
        var relativePath1 = new RelativePath(path1);
        var relativePath2 = new RelativePath(path2);

        relativePath1.GetHashCode().Should().Be(relativePath2.GetHashCode());
    }
}
