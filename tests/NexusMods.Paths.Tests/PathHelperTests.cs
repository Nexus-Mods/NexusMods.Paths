using System.Diagnostics.CodeAnalysis;
using System.Text;
using NexusMods.Paths.Utilities;

namespace NexusMods.Paths.Tests;

public class PathHelperTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("foo", "foo")]
    [InlineData("foo ", "foo")]
    [InlineData("foo/bar", "foo/bar")]
    [InlineData("foo/bar/", "foo/bar")]
    [InlineData("foo/bar/ ", "foo/bar")]
    [InlineData(@"foo\bar", "foo/bar")]
    [InlineData(@"foo\bar\", "foo/bar")]
    [InlineData("/", "/")]
    [InlineData("//", "/")]
    [InlineData("/foo", "/foo")]
    [InlineData("/foo/", "/foo")]
    [InlineData("/foo//bar//", "/foo/bar")]
    [InlineData(@"C:\", "C:/")]
    [InlineData(@"C:\foo", "C:/foo")]
    [InlineData(@"C:\foo\", "C:/foo")]
    [InlineData(@"\\Server\\foo", "//Server/foo")]
    [InlineData(@"\\.\C:\foo", "//./C:/foo")]
    [InlineData(@"\\?\C:\foo", "//?/C:/foo")]
    [InlineData(@"\\.\Volume{b75e2c83-0000-0000-0000-602f00000000}\foo", "//./Volume{b75e2c83-0000-0000-0000-602f00000000}/foo")]
    [InlineData(@"\\?\Volume{b75e2c83-0000-0000-0000-602f00000000}\foo", "//?/Volume{b75e2c83-0000-0000-0000-602f00000000}/foo")]
    public void Test_Sanitize(string input, string? expected)
    {
        var actual = PathHelpers.Sanitize(input);
        actual.Should().Be(expected);

        var isSanitized = PathHelpers.IsSanitized(actual);
        isSanitized.Should().BeTrue(because: "input was just sanitized");

        var again = PathHelpers.Sanitize(actual);
        again.Should().Be(actual, because: "already sanitized, no changes should be made");
    }

    [Theory]
    [MemberData(nameof(TestData_GetPathRoot))]
    public void Test_GetPathRoot(string input, string expectedRootPart, PathRootType expectedRootType)
    {
        var pathRoot = PathHelpers.GetPathRoot(input);
        pathRoot.Span.ToString().Should().Be(expectedRootPart);
        pathRoot.RootType.Should().Be(expectedRootType);
    }

    public static TheoryData<string, string, PathRootType> TestData_GetPathRoot()
    {
        return new TheoryData<string, string, PathRootType>
        {
            { "", "", PathRootType.None },
            { "foo", "", PathRootType.None },
            { "foo/bar", "", PathRootType.None },
            { "/", "/", PathRootType.Unix },
            { "/foo", "/", PathRootType.Unix },
            { "C:/", "C:/", PathRootType.DOS },
            { "C:/foo", "C:/", PathRootType.DOS },
            { "//A/" , "//A/", PathRootType.UNC },
            { "//A/foo" , "//A/", PathRootType.UNC },
            { "//Server/" , "//Server/", PathRootType.UNC },
            { "//Server/foo" , "//Server/", PathRootType.UNC },
            { "//./C:/", "//./C:/", PathRootType.DOSDeviceDrive },
            { "//?/C:/", "//?/C:/", PathRootType.DOSDeviceDrive },
            { "//./C:/foo", "//./C:/", PathRootType.DOSDeviceDrive },
            { "//?/C:/foo", "//?/C:/", PathRootType.DOSDeviceDrive },
            { "//./Volume{b75e2c83-0000-0000-0000-602f00000000}/", "//./Volume{b75e2c83-0000-0000-0000-602f00000000}/", PathRootType.DOSDeviceVolume },
            { "//?/Volume{b75e2c83-0000-0000-0000-602f00000000}/", "//?/Volume{b75e2c83-0000-0000-0000-602f00000000}/", PathRootType.DOSDeviceVolume },
            { "//./Volume{b75e2c83-0000-0000-0000-602f00000000}/foo", "//./Volume{b75e2c83-0000-0000-0000-602f00000000}/", PathRootType.DOSDeviceVolume },
            { "//?/Volume{b75e2c83-0000-0000-0000-602f00000000}/foo", "//?/Volume{b75e2c83-0000-0000-0000-602f00000000}/", PathRootType.DOSDeviceVolume },
        };
    }

    private static IOSInformation CreateOSInformation(bool isUnix)
    {
        return isUnix ? OSInformation.FakeUnix : OSInformation.FakeWindows;
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("/", true)]
    [InlineData("/foo", true)]
    [InlineData("/foo/bar", true)]
    [InlineData("/foo/bar.txt", true)]
    [InlineData("foo", true)]
    [InlineData("foo/bar", true)]
    [InlineData("foo/", false)]
    [InlineData("foo/bar/", false)]
    [InlineData("/foo/", false)]
    [InlineData( "/            ", false)]
    [InlineData( "C:/", true)]
    [InlineData("C:/foo", true)]
    [InlineData("C:/foo/bar", true)]
    [InlineData("C:/foo/bar.txt", true)]
    [InlineData("C:/foo/", false)]
    [InlineData("C:\\", false)]
    [InlineData("C:\\foo", false)]
    [InlineData("C:\\foo\\", false)]
    [InlineData("C:\\\\foo", false)]
    [InlineData("foo\\bar", false)]
    public void Test_IsSanitized(string input, bool expected)
    {
        var actual = PathHelpers.IsSanitized(input);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "", "")]
    [InlineData(false, "", "")]
    [InlineData(true, "/foo/bar", "/foo/bar")]
    [InlineData(false, "/foo/bar", "\\foo\\bar")]
    [InlineData(true, "foo/bar", "foo/bar")]
    [InlineData(false, "foo/bar", "foo\\bar")]
    public void Test_ToNativeSeparators(bool isUnix, string input, string expectedOutput)
    {
        var actualOutput = PathHelpers.ToNativeSeparators(input, CreateOSInformation(isUnix));
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("", "", true)]
    [InlineData("foo", "", false)]
    [InlineData("", "foo", false)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "FOO", true)]
    [InlineData("/foo", "/foo", true)]
    [InlineData("/foo", "/FOO", true)]
    [InlineData("C:/", "C:/", true)]
    [InlineData("C:/foo", "C:/foo", true)]
    [InlineData("C:/foo", "C:/FOO", true)]
    public void Test_Equals(string left, string right, bool expected)
    {
        var actual = PathHelpers.PathEquals(left, right);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "", 0)]
    [InlineData("", "foo", -1)]
    [InlineData("foo", "", 1)]
    [InlineData("foo", "foo", 0)]
    [InlineData("foo", "FOO", 0)]
    [InlineData("/foo", "/foo", 0)]
    [InlineData("/foo", "/FOO", 0)]
    [InlineData("/FOO", "/foo", 0)]
    [InlineData("/foo", "/bar", 1)]
    [InlineData("/bar", "/foo", -1)]
    [InlineData("C:/foo", "C:/foo", 0)]
    [InlineData("C:/foo", "C:/FOO", 0)]
    [InlineData("C:/FOO", "C:/foo", 0)]
    [InlineData("C:/foo", "C:/bar", 1)]
    [InlineData("C:/bar", "C:/foo", -1)]
    public void Test_Compare(string left, string right, int expected)
    {
        var actual = PathHelpers.Compare(left, right);
        actual = actual switch
        {
            0 => 0,
            > 0 => 1,
            < 0 => -1,
        };

        actual.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(TestData_IsValidWindowsDriveChar))]
    public void Test_IsValidWindowsDriveChar(char input, bool expectedResult)
    {
        var actualResult = PathHelpers.IsValidWindowsDriveChar(input);
        actualResult.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> TestData_IsValidWindowsDriveChar()
    {
        for (var i = (uint)'A'; i <= 'Z'; i++)
        {
            yield return new object[] { (char)i, true };
        }

        for (var i = (uint)'a'; i <= 'z'; i++)
        {
            yield return new object[] { (char)i, true };
        }

        for (var i = 0; i <= 9; i++)
        {
            yield return new object[] { i.ToString()[0], false };
        }
    }

    [Theory]
    [InlineData("/", 1)]
    [InlineData("/foo", 1)]
    [InlineData("/foo/", 1)]
    [InlineData("/foo/bar", 1)]
    [InlineData("foo", -1)]
    [InlineData("foo/bar", -1)]
    [InlineData("C:/", 3)]
    [InlineData("C:/foo", 3)]
    [InlineData("C:/foo/", 3)]
    [InlineData("C:/foo/bar", 3)]
    public void Test_GetRootLength(string path, int expectedRootLength)
    {
        var actualRootLength = PathHelpers.GetRootLength(path);
        actualRootLength.Should().Be(expectedRootLength);
    }

    [Theory]
    [InlineData("/", true)]
    [InlineData("/foo", true)]
    [InlineData("/foo/", true)]
    [InlineData("/foo/bar", true)]
    [InlineData("foo", false)]
    [InlineData("foo/bar", false)]
    [InlineData("C:/", true)]
    [InlineData("C:/foo", true)]
    [InlineData("C:/foo/", true)]
    [InlineData("C:/foo/bar", true)]
    public void Test_IsRooted(string path, bool expectedResult)
    {
        var actualResult = PathHelpers.IsRooted(path);
        actualResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData( "/", true)]
    [InlineData( "/foo", false)]
    [InlineData( "/foo/", false)]
    [InlineData( "/foo/bar", false)]
    [InlineData( "foo", false)]
    [InlineData( "foo/bar", false)]
    [InlineData( "C:/", true)]
    [InlineData( "C:/foo", false)]
    [InlineData( "C:/foo/", false)]
    [InlineData( "C:/foo/bar", false)]
    public void Test_IsRootDirectory(string path, bool expected)
    {
        var actual = PathHelpers.IsRootDirectory(path);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData( "/", "foo", "/foo")]
    [InlineData( "/foo", "bar", "/foo/bar")]
    [InlineData( "foo", "bar", "foo/bar")]
    [InlineData( "/", "foo/bar", "/foo/bar")]
    [InlineData( "", "foo", "foo")]
    [InlineData( "foo", "", "foo")]
    [InlineData( "C:/", "foo", "C:/foo")]
    [InlineData( "C:/foo", "bar", "C:/foo/bar")]
    [InlineData( "C:/", "foo/bar", "C:/foo/bar")]
    [InlineData( "", "", "")]
    public void Test_JoinParts(string left, string right, string expectedResult)
    {
        var actualResult1 = PathHelpers.JoinParts(left, right);
        actualResult1.Should().Be(expectedResult);

        var actualResult2 = PathHelpers.JoinParts(left.AsSpan(), right.AsSpan());
        actualResult2.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("", "", 1)]
    [InlineData("a", "", 2)]
    [InlineData("", "a", 2)]
    [InlineData("a", "a", 3)]
    public void Test_GetMaxJoinedPartLength(string left, string right, int expectedMaxLength)
    {
        var actualMaxLength = PathHelpers.GetMaxJoinedPartLength(left, right);
        actualMaxLength.Should().Be(expectedMaxLength);
    }

    [Theory]
    [InlineData("", "", 0)]
    [InlineData("foo", "", 3)]
    [InlineData("", "foo", 3)]
    [InlineData("foo", "bar", 7)]
    [InlineData("foo/", "bar", 7)]
    public void Test_GetExactJoinedPartLength(string left, string right, int expectedLength)
    {
        var actualLength = PathHelpers.GetExactJoinedPartLength(left, right);
        actualLength.Should().Be(expectedLength);
    }

    [Theory]
    [InlineData( "", "")]
    [InlineData( "foo", "foo")]
    [InlineData( "foo/bar", "bar")]
    [InlineData( "/", "")]
    [InlineData( "/foo", "foo")]
    [InlineData( "/foo/bar", "bar")]
    [InlineData( "C:/", "")]
    [InlineData( "C:/foo", "foo")]
    [InlineData( "C:/foo/bar", "bar")]
    public void Test_GetFileName(string path, string expectedFileName)
    {
        var actualFileName = PathHelpers.GetFileName(path).ToString();
        actualFileName.Should().Be(expectedFileName);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(".", "")]
    [InlineData("foo", "")]
    [InlineData("foo.txt", ".txt")]
    [InlineData("foo.bar.baz.txt", ".txt")]
    public void Test_GetExtension(string path, string expectedExtension)
    {
        var actualExtension = PathHelpers.GetExtension(path).ToString();
        actualExtension.Should().Be(expectedExtension);
    }

    [Theory]
    [InlineData("", ".txt", "")]
    [InlineData("foo", ".txt", "foo.txt")]
    [InlineData("foo.bar", ".txt", "foo.txt")]
    public void Test_ReplaceExtension(string input, string newExtension, string expectedOutput)
    {
        var actualOutput = PathHelpers.ReplaceExtension(input, newExtension);
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("foo.txt", 0)]
    [InlineData("foo/bar.txt", 1)]
    [InlineData("/", 1)]
    [InlineData("/foo.txt", 1)]
    [InlineData("/foo/bar.txt", 2)]
    [InlineData("C:/", 1)]
    [InlineData("C:/foo.txt", 1)]
    [InlineData("C:/foo/bar.txt", 2)]
    [InlineData("//Server/foo/bar.txt", 2)]
    [InlineData("//./C:/foo/bar", 2)]
    public void Test_GetDirectoryDepth(string input, int expectedDepth)
    {
        var actualDepth = PathHelpers.GetDirectoryDepth(input);
        actualDepth.Should().Be(expectedDepth);
    }

    [Theory]
    [InlineData("/", "/")]
    [InlineData("/foo", "/")]
    [InlineData("/foo/bar", "/foo")]
    [InlineData("", "")]
    [InlineData("foo", "")]
    [InlineData("foo/bar", "foo")]
    [InlineData("C:/", "C:/")]
    [InlineData("C:/foo", "C:/")]
    [InlineData("C:/foo/bar", "C:/foo")]
    public void Test_GetDirectoryName(string input, string expectedOutput)
    {
        var actualOutput = PathHelpers.GetDirectoryName(input).ToString();
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData( "", "", true)]
    [InlineData( "foo", "", true)]
    [InlineData( "", "foo", false)]
    [InlineData( "foo/bar", "foo", true)]
    [InlineData( "foo", "bar", false)]
    [InlineData( "/", "/", true)]
    [InlineData( "/foo", "/", true)]
    [InlineData( "/foo/bar/baz", "/", true)]
    [InlineData( "/foo/bar/baz", "/foo", true)]
    [InlineData( "/foo/bar/baz", "/foo/bar", true)]
    [InlineData( "/foobar", "/foo", false)]
    [InlineData( "C:/", "C:/", true)]
    [InlineData( "C:/foo", "C:/", true)]
    [InlineData( "C:/foo/bar/baz", "C:/", true)]
    [InlineData( "C:/foo/bar/baz", "C:/foo", true)]
    [InlineData( "C:/foo/bar/baz", "C:/foo/bar", true)]
    [InlineData( "C:/foobar", "C:/foo", false)]
    public void Test_InFolder(string child, string parent, bool expected)
    {
        var actual = PathHelpers.InFolder(child, parent);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData( "", "", "")]
    [InlineData( "foo/bar", "foo", "bar")]
    [InlineData( "/foo", "/", "foo")]
    [InlineData( "/foo/bar", "/foo", "bar")]
    [InlineData( "C:/foo", "C:/", "foo")]
    [InlineData( "C:/foo/bar", "C:/foo", "bar")]
    public void Test_RelativeTo(string child, string parent, string expectedOutput)
    {
        var actualOutput = PathHelpers.RelativeTo(child, parent).ToString();
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData( "", "")]
    [InlineData( "/", "/")]
    [InlineData( "C:/", "C:/")]
    [InlineData( "foo/bar", "foo")]
    [InlineData( "foo/bar/baz", "foo")]
    public void Test_GetTopParent(string path, string expectedOutput)
    {
        var actualOutput = PathHelpers.GetTopParent(path).ToString();
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("", 0, "")]
    [InlineData("/", 0, "/")]
    [InlineData("/", 1, "")]
    [InlineData("/foo", 1, "foo")]
    [InlineData("/foo/bar", 1, "foo/bar")]
    [InlineData("/foo/bar", 2, "bar")]
    [InlineData("/foo/bar", 3, "")]
    [InlineData("C:/", 0, "C:/")]
    [InlineData("C:/", 1, "")]
    [InlineData("C:/foo", 1, "foo")]
    [InlineData("C:/foo/bar", 1, "foo/bar")]
    [InlineData("C:/foo/bar", 2, "bar")]
    [InlineData("C:/foo/bar", 3, "")]
    [InlineData("//Server/foo/bar", 2, "bar")]
    [InlineData("//./C:/foo/bar", 2, "bar")]
    public void Test_DropParents(string path, int count, string expectedOutput)
    {
        var actualOutput = PathHelpers.DropParents(path, count).ToString();
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData( false, "", "")]
    [InlineData( false, "foo/bar", "foo+bar")]
    [InlineData( false,"/", "/")]
    [InlineData( false,"/foo", "/+foo")]
    [InlineData( false,"/foo/bar/baz", "/+foo+bar+baz")]
    [InlineData( false,"C:/", "C:/")]
    [InlineData( false,"C:/foo", "C:/+foo")]
    [InlineData( false,"C:/foo/bar/baz", "C:/+foo+bar+baz")]
    [InlineData( true, "", "")]
    [InlineData( true, "foo/bar", "bar+foo")]
    [InlineData( true,"/", "/")]
    [InlineData( true,"/foo", "foo+/")]
    [InlineData( true,"/foo/bar/baz", "baz+bar+foo+/")]
    [InlineData( true,"C:/", "C:/")]
    [InlineData( true,"C:/foo", "foo+C:/")]
    [InlineData( true,"C:/foo/bar/baz", "baz+bar+foo+C:/")]
    public void Test_WalkParts(bool isReverse, string path, string expectedOutput)
    {
        var sb = new StringBuilder();

        // ReSharper disable once InconsistentNaming
        PathHelpers.WalkParts(path, ref sb, (ReadOnlySpan<char> part, ref StringBuilder sb_) =>
        {
            if (sb_.Length != 0) sb_.Append('+');
            sb_.Append(part);
            return true;
        }, isReverse);

        var actualOutput = sb.ToString();
        actualOutput.Should().Be(expectedOutput);

        sb = new StringBuilder();
        PathHelpers.WalkParts(path, part =>
        {
            if (sb.Length != 0) sb.Append('+');
            sb.Append(part);
            return true;
        }, isReverse);

        actualOutput = sb.ToString();
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData( false, "/foo/bar/baz", 1, "/")]
    [InlineData( false, "/foo/bar/baz", 2, "/+foo")]
    [InlineData( false, "/foo/bar/baz", 3, "/+foo+bar")]
    [InlineData( false, "/foo/bar/baz", 4, "/+foo+bar+baz")]
    [InlineData( true, "/foo/bar/baz", 1, "baz")]
    [InlineData( true, "/foo/bar/baz", 2, "baz+bar")]
    [InlineData( true, "/foo/bar/baz", 3, "baz+bar+foo")]
    [InlineData( true, "/foo/bar/baz", 4, "baz+bar+foo+/")]
    [InlineData( false, "C:/foo/bar/baz", 1, "C:/")]
    [InlineData( false, "C:/foo/bar/baz", 2, "C:/+foo")]
    [InlineData( false, "C:/foo/bar/baz", 3, "C:/+foo+bar")]
    [InlineData( false, "C:/foo/bar/baz", 4, "C:/+foo+bar+baz")]
    [InlineData( true, "C:/foo/bar/baz", 1, "baz")]
    [InlineData( true, "C:/foo/bar/baz", 2, "baz+bar")]
    [InlineData( true, "C:/foo/bar/baz", 3, "baz+bar+foo")]
    [InlineData( true, "C:/foo/bar/baz", 4, "baz+bar+foo+C:/")]
    public void Test_WalkPartsPartially(bool isReverse, string path, int stopAfterN, string expectedOutput)
    {
        var sb = new StringBuilder();
        var counter = 0;

        PathHelpers.WalkParts(path, part =>
        {
            if (sb.Length != 0) sb.Append('+');
            sb.Append(part);
            counter++;
            return counter < stopAfterN;
        }, isReverse);

        var actualOutput = sb.ToString();
        actualOutput.Should().Be(expectedOutput);
    }

    [Theory]
    [MemberData(nameof(TestData_GetParts))]
    public void Test_GetParts(bool isReverse, string path, List<string> expectedOutput)
    {
        var actualOutput = PathHelpers.GetParts(path, isReverse);
        actualOutput.Should().Equal(expectedOutput);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static IEnumerable<object[]> TestData_GetParts => new[]
    {
        new object[] { false, "/foo/bar/baz", new List<string> { "/", "foo", "bar", "baz" } },
        new object[] { true, "/foo/bar/baz", new List<string> { "baz", "bar", "foo", "/" } },
        new object[] { false, "C:/foo/bar/baz", new List<string>{ "C:/", "foo", "bar", "baz" }},
        new object[] { true, "C:/foo/bar/baz", new List<string>{ "baz", "bar", "foo", "C:/" }},
    };
}
