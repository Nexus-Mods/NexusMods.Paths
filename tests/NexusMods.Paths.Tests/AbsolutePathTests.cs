using System.Runtime.InteropServices;
using FluentAssertions;
using NexusMods.Paths.Utilities;

namespace NexusMods.Paths.Tests;

public class AbsolutePathTests
{
    [Theory]
    [InlineData(true, "/", "/", "")]
    [InlineData(true, "/foo", "/", "foo")]
    [InlineData(true, "/foo/bar", "/foo", "bar")]
    [InlineData(false, "C:/", "C:/", "")]
    [InlineData(false, "C:/foo", "C:/", "foo")]
    [InlineData(false, "C:/foo/bar", "C:/foo", "bar")]
    public void Test_FromSanitizedFullPath(bool isUnix, string input, string expectedDirectory, string expectedFileName)
    {
        var os = CreateOSInformation(isUnix);
        var fs = new InMemoryFileSystem(os);
        var actualPath = AbsolutePath.FromSanitizedFullPath(input, fs);
        actualPath.Directory.Should().Be(expectedDirectory);
        actualPath.FileName.Should().Be(expectedFileName);
        actualPath.GetFullPath().Should().Be(input);
    }

    [Theory]
    [InlineData(true, "/", "", "/", "", "/")]
    [InlineData(true, "/", "foo", "/", "foo", "/foo")]
    [InlineData(true, "/foo", "bar", "/foo", "bar", "/foo/bar")]
    [InlineData(false, "C:\\", "", "C:/", "", "C:/")]
    [InlineData(false, "C:\\", "foo", "C:/", "foo", "C:/foo")]
    [InlineData(false, "C:\\foo", "bar", "C:/foo", "bar", "C:/foo/bar")]
    public void Test_FromUnsanitizedDirectoryAndFileName(
        bool isUnix,
        string directory, string fileName,
        string expectedDirectory, string expectedFileName, string expectedFullPath)
    {
        var os = CreateOSInformation(isUnix);
        var fs = new InMemoryFileSystem(os);
        var actualPath = AbsolutePath.FromUnsanitizedDirectoryAndFileName(directory, fileName, fs);
        actualPath.Directory.Should().Be(expectedDirectory);
        actualPath.FileName.Should().Be(expectedFileName);
        actualPath.GetFullPath().Should().Be(expectedFullPath);
    }

    [Theory]
    [InlineData(true, "/", "")]
    [InlineData(true, "/foo", "foo")]
    [InlineData(true, "/foo/bar", "bar")]
    [InlineData(false, "C:/", "")]
    [InlineData(false, "C:/foo", "foo")]
    [InlineData(false, "C:/foo/bar", "bar")]
    public void Test_Name(bool isUnix, string input, string expected)
    {
        var path = CreatePath(input, isUnix);
        var actual = path.Name;
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "", "")]
    [InlineData(false, "", "")]
    [InlineData(true, "foo/bar", "foo/bar")]
    [InlineData(false, "foo/bar", "foo\\bar")]
    public void Test_ToNativeSeparators(bool isUnix, string input, string expected)
    {
        var os = CreateOSInformation(isUnix);
        var fs = new InMemoryFileSystem(os);

        var absolutePath = AbsolutePath.FromUnsanitizedFullPath(input, fs);
        var actual = absolutePath.ToNativeSeparators(os);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("/", "")]
    [InlineData("/foo", "")]
    [InlineData("/foo.txt", ".txt")]
    public void Test_Extension(string input, string expectedExtension)
    {
        var path = CreatePath(input, isUnix: true);
        var actualExtension = path.Extension;
        actualExtension.ToString().Should().Be(expectedExtension);
    }

    [Theory]
    [InlineData(true, "/", new string[] { })]
    [InlineData(true, "/foo", new string[] { "foo" })]
    [InlineData(true, "/foo/bar", new string[] { "foo", "bar" })]
    [InlineData(false, "C:/", new string[] { })]
    [InlineData(false, "C:/foo", new string[] { "foo" })]
    [InlineData(false, "C:/foo/bar", new string[] { "foo", "bar" })]
    public void Test_Parts(bool isUnix, string input, string[] expectedParts)
    {
        var path = CreatePath(input, isUnix);
        var actualParts = path.Parts;
        actualParts.Should().BeEquivalentTo(expectedParts.Select(p => new RelativePath(p)),
            opts => opts.WithStrictOrdering());
    }

    [Theory]
    [InlineData(true, "/", new string[] { "/" })]
    [InlineData(true, "/foo", new string[] { "/foo", "/" })]
    [InlineData(true, "/foo/bar", new string[] { "/foo/bar", "/foo", "/" })]
    [InlineData(false, "C:/", new string[] { "C:/" })]
    [InlineData(false, "C:/foo", new string[] { "C:/foo", "C:/" })]
    [InlineData(false, "C:/foo/bar", new string[] { "C:/foo/bar", "C:/foo", "C:/" })]
    public void Test_GetAllParents(bool isUnix, string input, string[] expectedParts)
    {
        var path = CreatePath(input, isUnix);
        var actualParents = path.GetAllParents();
        actualParents.Should().BeEquivalentTo(expectedParts.Select(p => CreatePath(p, isUnix)));
    }

    [Theory]
    [InlineData(true, "/", "")]
    [InlineData(true, "/foo", "foo")]
    [InlineData(true, "/foo/bar", "foo/bar")]
    [InlineData(false, "C:/", "")]
    [InlineData(false, "C:/foo", "foo")]
    [InlineData(false, "C:/foo/bar", "foo/bar")]
    public void TestGetNonRootPart(bool isUnix, string input, string expected)
    {
        var path = CreatePath(input, isUnix);
        var actual = path.GetNonRootPart();
        actual.Should().Be(expected);
    }

    [Fact]
    public void Test_IsRooted()
    {
        var path = CreatePath("/");
        path.IsRooted.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, "/", "/", "", "/")]
    [InlineData(true, "/foo", "/", "", "/")]
    [InlineData(true, "/foo/bar", "/", "foo", "/foo")]
    [InlineData(false, "C:/", "C:/", "", "C:/")]
    [InlineData(false, "C:/foo", "C:/", "", "C:/")]
    [InlineData(false, "C:/foo/bar", "C:/", "foo", "C:/foo")]
    public void Test_Parent(bool isUnix, string input, string expectedDirectory, string expectedFileName,
        string expectedFullPath)
    {
        var path = CreatePath(input, isUnix);
        var actualParent = path.Parent;
        actualParent.Directory.Should().Be(expectedDirectory);
        actualParent.FileName.Should().Be(expectedFileName);
        actualParent.GetFullPath().Should().Be(expectedFullPath);
    }

    [Theory]
    [InlineData("/", "")]
    [InlineData("/foo", "foo")]
    [InlineData("/foo.txt", "foo")]
    public void Test_GetFileNameWithoutExtension(string input, string expectedFileName)
    {
        var path = CreatePath(input);
        var actualFileName = path.GetFileNameWithoutExtension();
        actualFileName.Should().Be(expectedFileName);
    }

    [Theory]
    [InlineData("/foo", ".txt", "/foo.txt")]
    public void Test_AppendExtension(string input, string extension, string expectedFullPath)
    {
        var path = CreatePath(input);
        var actualPath = path.AppendExtension(new Extension(extension));
        actualPath.GetFullPath().Should().Be(expectedFullPath);
    }

    [Theory]
    [InlineData("/foo.txt", ".md", "/foo.md")]
    public void Test_ReplaceExtension(string input, string extension, string expectedFullPath)
    {
        var path = CreatePath(input);
        var newPath = path.ReplaceExtension(new Extension(extension));
        newPath.GetFullPath().Should().Be(expectedFullPath);
    }

    [Theory]
    [InlineData(true, "/", "/")]
    [InlineData(true, "/foo/bar/baz", "/")]
    [InlineData(false, "C:/", "C:/")]
    [InlineData(false, "C:/foo/bar/baz", "C:/")]
    public void Test_GetRootDirectory(bool isUnix, string input, string expectedRootDirectory)
    {
        var path = CreatePath(input, isUnix);
        var actualRootDirectory = path.GetRootDirectory();
        actualRootDirectory.GetFullPath().Should().Be(expectedRootDirectory);
    }

    [Theory]
    [InlineData(true, "/foo", "/foo", "")]
    [InlineData(true, "/foo", "/", "foo")]
    [InlineData(true, "/foo/bar/baz", "/", "foo/bar/baz")]
    [InlineData(true, "/foo/bar/baz", "/foo", "bar/baz")]
    [InlineData(false, "C:/foo", "C:/", "foo")]
    [InlineData(false, "C:/foo/bar/baz", "C:/", "foo/bar/baz")]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo", "bar/baz")]
    public void Test_RelativeTo(bool isUnix, string child, string parent, string expectedOutput)
    {
        var childPath = CreatePath(child, isUnix);
        var parentPath = CreatePath(parent, isUnix);
        var actualOutput = childPath.RelativeTo(parentPath);
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("/foo/bar/baz", "/f")]
    public void Test_RelativeTo_PathException(string child, string parent)
    {
        var childPath = CreatePath(child);
        var parentPath = CreatePath(parent);

        Action act = () => childPath.RelativeTo(parentPath);
        act.Should().ThrowExactly<PathException>();
    }

    [Theory]
    [InlineData(true, "/", "/", true)]
    [InlineData(true, "/foo", "/", true)]
    [InlineData(true, "/foo/bar/baz", "/", true)]
    [InlineData(true, "/foo/bar/baz", "/foo", true)]
    [InlineData(true, "/foo/bar/baz", "/foo/bar", true)]
    [InlineData(true, "/foobar", "/foo", false)]
    [InlineData(true, "/foo/bar/baz", "/foo/baz", false)]
    [InlineData(false, "C:/", "C:/", true)]
    [InlineData(false, "C:/foo", "C:/", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo/bar", true)]
    [InlineData(false, "C:/foobar", "C:/foo", false)]
    public void Test_InFolder(bool isUnix, string child, string parent, bool expected)
    {
        var childPath = CreatePath(child, isUnix);
        var parentPath = CreatePath(parent, isUnix);
        var actual = childPath.InFolder(parentPath);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "/", "/", true)]
    [InlineData(true, "/", "/foo", false)]
    [InlineData(true, "/foo", "/bar", false)]
    [InlineData(true, "/foo", "/", true)]
    [InlineData(true, "/foo/bar/baz", "/", true)]
    [InlineData(true, "/foo/bar/baz", "/foo", true)]
    [InlineData(true, "/foo/bar/baz", "/foo/bar", true)]
    [InlineData(true, "/foobar", "/foo", false)]
    [InlineData(true, "/foo/bar/baz", "/foo/baz", false)]
    [InlineData(false, "C:/", "C:/", true)]
    [InlineData(false, "C:/foo", "C:/", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo/bar", true)]
    [InlineData(false, "C:/foobar", "C:/foo", false)]
    public void Test_StartsWith(bool isUnix, string child, string parent, bool expected)
    {
        var childPath = CreatePath(child, isUnix);
        var parentPath = CreatePath(parent, isUnix);
        var actual = childPath.StartsWith(parentPath);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "/", "", true)]
    [InlineData(true, "/", "foo", false)]
    [InlineData(true, "/foo", "bar", false)]
    [InlineData(true, "/foo", "", true)]
    [InlineData(true, "/foo/bar/baz", "", true)]
    [InlineData(true, "/foo/bar/baz", "bar/baz", true)]
    [InlineData(true, "/foo/bar/baz", "foo/bar/baz", true)]
    [InlineData(true, "/foobar", "bar", false)]
    [InlineData(true, "/foo/bar/baz", "foo/baz", false)]
    [InlineData(false, "C:/", "", true)]
    [InlineData(false, "C:/", "foo", false)]
    [InlineData(false, "C:/foo", "bar", false)]
    [InlineData(false, "C:/foo", "", true)]
    [InlineData(false, "C:/foo/bar/baz", "", true)]
    [InlineData(false, "C:/foo/bar/baz", "bar/baz", true)]
    [InlineData(false, "C:/foo/bar/baz", "foo/bar/baz", true)]
    [InlineData(false, "C:/foobar", "bar", false)]
    [InlineData(false, "C:/foo/bar/baz", "foo/baz", false)]
    public void Test_EndsWith(bool isUnix, string path, string end, bool expected)
    {
        var startingPath = CreatePath(path, isUnix);
        var endPath = (RelativePath)end;
        var actual = startingPath.EndsWith(endPath);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "/foo/bar/baz", "/FOO/BAR/BAZ", true)]
    [InlineData(true, "/foo/bar/baz", "/foo/bar/baz", true)]
    [InlineData(true, "/foo/bar/baz", "/foo/bar/bazz", false)]
    [InlineData(false, "C:/foo/bar/baz", "C:/FOO/BAR/BAZ", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo/bar/baz", true)]
    [InlineData(false, "C:/foo/bar/baz", "C:/foo/bar/bazz", false)]
    public void Test_Equals_CaseInsensitive(bool isUnix, string path1, string path2, bool expected)
    {
        var absolutePath1 = CreatePath(path1, isUnix);
        var absolutePath2 = CreatePath(path2, isUnix);

        absolutePath1.Equals(absolutePath2).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "/foo/bar/baz", "/FOO/BAR/BAZ")]
    [InlineData(false, "C:/foo/bar/baz", "C:/FOO/BAR/BAZ")]
    public void Test_GetHashCode_CaseInsensitive(bool isUnix, string path1, string path2)
    {
        var absolutePath1 = CreatePath(path1, isUnix);
        var absolutePath2 = CreatePath(path2, isUnix);

        absolutePath1.GetHashCode().Should().Be(absolutePath2.GetHashCode());
    }

    private static AbsolutePath CreatePath(string input, bool isUnix = true)
    {
        var os = CreateOSInformation(isUnix);
        var fs = new InMemoryFileSystem(os);
        var path = AbsolutePath.FromSanitizedFullPath(input, fs);
        return path;
    }

    private static IOSInformation CreateOSInformation(bool isUnix)
    {
        return isUnix ? new OSInformation(OSPlatform.Linux) : new OSInformation(OSPlatform.Windows);
    }
}
