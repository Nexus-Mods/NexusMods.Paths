using System.Globalization;
using System.Runtime.InteropServices;
using FluentAssertions;
using NexusMods.Paths.TestingHelpers;
using NexusMods.Paths.Utilities;

namespace NexusMods.Paths.Tests.FileSystem;

public class BaseFileSystemTests
{
    [Theory, AutoFileSystem]
    public void Test_PathMapping(InMemoryFileSystem fs, AbsolutePath originalPath, AbsolutePath mappedPath)
    {
        var overlayFileSystem = (BaseFileSystem)fs.CreateOverlayFileSystem(new Dictionary<AbsolutePath, AbsolutePath>
        {
            { originalPath, mappedPath }
        }, new Dictionary<KnownPath, AbsolutePath>());

        overlayFileSystem.GetMappedPath(originalPath).Should().Be(mappedPath);
    }

    [Theory, AutoFileSystem]
    public void Test_PathMapping_WithDirectory(InMemoryFileSystem fs,
        AbsolutePath originalDirectoryPath, AbsolutePath newDirectoryPath, string fileName)
    {
        var originalFilePath = originalDirectoryPath.Combine(fileName);
        var newFilePath = newDirectoryPath.Combine(fileName);

        var overlayFileSystem = (BaseFileSystem)fs.CreateOverlayFileSystem(
            new Dictionary<AbsolutePath, AbsolutePath>
            {
                { originalDirectoryPath, newDirectoryPath }
            },
            new Dictionary<KnownPath, AbsolutePath>());

        overlayFileSystem.GetMappedPath(originalFilePath).Should().Be(newFilePath);
    }

    [Fact]
    public void Test_PathMappings_SpecialCases()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);

        var overlayFileSystem = (BaseFileSystem)fs.CreateOverlayFileSystem(
            new Dictionary<AbsolutePath, AbsolutePath>
            {
                { fs.FromUnsanitizedFullPath("/c"), fs.FromUnsanitizedFullPath("/foo") },
                { fs.FromUnsanitizedFullPath("/z"), fs.FromUnsanitizedFullPath("/") },
            },
            new Dictionary<KnownPath, AbsolutePath>());

        overlayFileSystem.GetMappedPath(fs.FromUnsanitizedFullPath("/c/a")).Should().Be(fs.FromUnsanitizedFullPath("/foo/a"));
        overlayFileSystem.GetMappedPath(fs.FromUnsanitizedFullPath("/z/a")).Should().Be(fs.FromUnsanitizedFullPath("/a"));
    }

    [Theory, AutoFileSystem]
    public async Task Test_ReadAllBytesAsync(InMemoryFileSystem fs, AbsolutePath path, byte[] contents)
    {
        fs.AddFile(path, contents);
        var result = await fs.ReadAllBytesAsync(path);
        result.Should().BeEquivalentTo(contents);
    }

    [Theory, AutoFileSystem]
    public async Task Test_ReadAllTextAsync(InMemoryFileSystem fs, AbsolutePath path, string contents)
    {
        fs.AddFile(path, contents);
        var result = await fs.ReadAllTextAsync(path);
        result.Should().BeEquivalentTo(contents);
    }

    [Theory]
    [InlineData("C:/", "/c")]
    [InlineData("C:/foo/bar", "/c/foo/bar")]
    public void Test_ConvertCrossPlatformPath(string input, string output)
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix)
            .CreateOverlayFileSystem(
            new Dictionary<AbsolutePath, AbsolutePath>(),
            new Dictionary<KnownPath, AbsolutePath>(),
            true);

        var path = fs.FromUnsanitizedFullPath(input);
        path.GetFullPath().Should().Be(output);
    }

    [Fact]
    public void Test_KnownPathMappings()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);

        var knownPathMappings = new Dictionary<KnownPath, AbsolutePath>();
        var values = Enum.GetValues<KnownPath>();
        foreach (var knownPath in values)
        {
            var newPath = fs.FromUnsanitizedFullPath($"/{Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture)}");
            knownPathMappings[knownPath] = newPath;
        }

        var overlayFileSystem = fs.CreateOverlayFileSystem(new Dictionary<AbsolutePath, AbsolutePath>(), knownPathMappings);
        foreach (var knownPath in values)
        {
            var actualPath = overlayFileSystem.GetKnownPath(knownPath);
            var expectedPath = knownPathMappings[knownPath];

            actualPath.Should().Be(expectedPath);
        }
    }

    [Fact]
    public void Test_EnumerateRootDirectories_Windows()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeWindows);

        var rootDirectory = fs.FromUnsanitizedFullPath("C:/");
        var pathMappings = Enumerable.Range('A', 'Z' - 'A')
            .Select(iDriveLetter =>
            {
                var driveLetter = (char)iDriveLetter;
                var originalPath = fs.FromUnsanitizedFullPath($"{driveLetter}:/");
                var newPath = rootDirectory.Combine(Guid.NewGuid().ToString("D"));
                return (originalPath, newPath);
            }).ToDictionary(x => x.originalPath, x => x.newPath);

        var overlayFileSystem = fs.CreateOverlayFileSystem(
            pathMappings,
            new Dictionary<KnownPath, AbsolutePath>(),
            convertCrossPlatformPaths: false);

        var expectedRootDirectories = pathMappings
            .Select(kv => kv.Value)
            .ToArray();

        foreach (var expectedRootDirectory in expectedRootDirectories)
        {
            overlayFileSystem.CreateDirectory(expectedRootDirectory);
        }

        var actualRootDirectories = overlayFileSystem
            .EnumerateRootDirectories()
            .ToArray();

        actualRootDirectories.Should().BeEquivalentTo(expectedRootDirectories);
    }

    [Fact]
    public void Test_EnumerateRootDirectories_Linux()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);

        var rootDirectory = fs.FromUnsanitizedFullPath("/");
        var expectedRootDirectories = new[] { rootDirectory };
        var actualRootDirectories = fs
            .EnumerateRootDirectories()
            .ToArray();

        actualRootDirectories.Should().BeEquivalentTo(expectedRootDirectories);
    }

    [Fact]
    public void Test_EnumerateRootDirectories_WithCrossPlatformPathMappings()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);

        var rootDirectory = fs.FromUnsanitizedFullPath("/");

        var pathMappings = Enumerable.Range('a', 'z' - 'a')
            .Select(iDriveLetter =>
            {
                var driveLetter = (char)iDriveLetter;
                var originalPath = fs.FromUnsanitizedDirectoryAndFileName("/", driveLetter.ToString());
                var newPath = rootDirectory.Combine(Guid.NewGuid().ToString("D"));
                return (originalPath, newPath);
            }).ToDictionary(x => x.originalPath, x => x.newPath);

        var overlayFileSystem = fs.CreateOverlayFileSystem(
            pathMappings,
            new Dictionary<KnownPath, AbsolutePath>(),
            convertCrossPlatformPaths: true);

        var expectedRootDirectories = pathMappings
            .Select(kv => kv.Value)
            .Append(rootDirectory)
            .ToArray();

        foreach (var expectedRootDirectory in expectedRootDirectories)
        {
            overlayFileSystem.CreateDirectory(expectedRootDirectory);
        }

        var actualRootDirectories = overlayFileSystem
            .EnumerateRootDirectories()
            .ToArray();

        actualRootDirectories.Should().BeEquivalentTo(expectedRootDirectories);
    }

    [SkippableTheory]
    [InlineData("Linux", KnownPath.EntryDirectory, true)]
    [InlineData("Linux", KnownPath.CurrentDirectory, true)]
    [InlineData("Linux", KnownPath.CommonApplicationDataDirectory, true)]
    [InlineData("Linux", KnownPath.ProgramFilesDirectory, false)]
    [InlineData("Linux", KnownPath.ProgramFilesX86Directory, false)]
    [InlineData("Linux", KnownPath.CommonProgramFilesDirectory, false)]
    [InlineData("Linux", KnownPath.CommonProgramFilesX86Directory, false)]
    [InlineData("Linux", KnownPath.TempDirectory, true)]
    [InlineData("Linux", KnownPath.HomeDirectory, true)]
    [InlineData("Linux", KnownPath.ApplicationDataDirectory, true)]
    [InlineData("Linux", KnownPath.LocalApplicationDataDirectory, true)]
    [InlineData("Linux", KnownPath.MyDocumentsDirectory, true)]
    [InlineData("Linux", KnownPath.MyGamesDirectory, true)]

    [InlineData("OSX", KnownPath.EntryDirectory, true)]
    [InlineData("OSX", KnownPath.CurrentDirectory, true)]
    [InlineData("OSX", KnownPath.CommonApplicationDataDirectory, true)]
    // For OSX .GetKnownPath(KnownPath.ProgramFilesDirectory) returns a value, but .HasPath returns false so this
    // test is disabled as it's all nonsense
    // [InlineData("OSX", KnownPath.ProgramFilesDirectory, true)]
    [InlineData("OSX", KnownPath.ProgramFilesX86Directory, false)]
    [InlineData("OSX", KnownPath.CommonProgramFilesDirectory, false)]
    [InlineData("OSX", KnownPath.CommonProgramFilesX86Directory, false)]
    [InlineData("OSX", KnownPath.TempDirectory, true)]
    [InlineData("OSX", KnownPath.HomeDirectory, true)]
    [InlineData("OSX", KnownPath.ApplicationDataDirectory, true)]
    [InlineData("OSX", KnownPath.LocalApplicationDataDirectory, true)]
    [InlineData("OSX", KnownPath.MyDocumentsDirectory, true)]
    [InlineData("OSX", KnownPath.MyGamesDirectory, true)]

    [InlineData("Windows", KnownPath.EntryDirectory, true)]
    [InlineData("Windows", KnownPath.CurrentDirectory, true)]
    [InlineData("Windows", KnownPath.CommonApplicationDataDirectory, true)]
    [InlineData("Windows", KnownPath.ProgramFilesDirectory, true)]
    [InlineData("Windows", KnownPath.ProgramFilesX86Directory, true)]
    [InlineData("Windows", KnownPath.CommonProgramFilesDirectory, true)]
    [InlineData("Windows", KnownPath.CommonProgramFilesX86Directory, true)]
    [InlineData("Windows", KnownPath.TempDirectory, true)]
    [InlineData("Windows", KnownPath.HomeDirectory, true)]
    [InlineData("Windows", KnownPath.ApplicationDataDirectory, true)]
    [InlineData("Windows", KnownPath.LocalApplicationDataDirectory, true)]
    [InlineData("Windows", KnownPath.MyDocumentsDirectory, true)]
    [InlineData("Windows", KnownPath.MyGamesDirectory, true)]
    public void Test_KnownPath(string os, KnownPath knownPath, bool expected)
    {
        var platform = OSPlatform.Create(os);
        Skip.If(platform != OSInformation.Shared.Platform);
        var fs = new InMemoryFileSystem();

        var actual = fs.HasKnownPath(knownPath);
        actual.Should().Be(expected);

        Action act = () => fs.GetKnownPath(knownPath);
        if (expected) act.Should().NotThrow();
        else act.Should().Throw<Exception>();
    }

    [Fact]
    public void TryResolveRealFilesystemPath_WithRealFileSystem_ReturnsTrue()
    {
        // Arrange
        var fs = new Paths.FileSystem();
        using var tempManager = new TemporaryFileManager(fs);
        using var tempFile = tempManager.CreateFile();

        // Act
        var result = FileSystemMarshal.TryResolveRealFilesystemPath(tempFile, out var resolvedPath);

        // Assert
        result.Should().BeTrue();
        resolvedPath.Should().Be(tempFile.Path.GetFullPath());
    }

    [Fact]
    public void TryResolveRealFilesystemPath_WithInMemoryFileSystem_ReturnsFalse()
    {
        // Arrange
        var fs = new InMemoryFileSystem();
        using var tempManager = new TemporaryFileManager(fs);
        using var tempFile = tempManager.CreateFile();

        // Act
        var result = FileSystemMarshal.TryResolveRealFilesystemPath(tempFile, out var resolvedPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryResolveRealFilesystemPath_WithMappedPath_ResolvesCorrectly()
    {
        // Arrange
        var fs = new Paths.FileSystem();
        using var tempManager = new TemporaryFileManager(fs);
        using var tempFile1 = tempManager.CreateFile();
        using var tempFile2 = tempManager.CreateFile();
        var originalPath = tempFile1.Path;
        var mappedPath = tempFile2.Path;

        var overlayFileSystem = fs.CreateOverlayFileSystem(new Dictionary<AbsolutePath, AbsolutePath>
        {
            { originalPath, mappedPath }
        }, new Dictionary<KnownPath, AbsolutePath>());

        // Act
        var source = overlayFileSystem.FromUnsanitizedFullPath(originalPath.GetFullPath());
        var result = FileSystemMarshal.TryResolveRealFilesystemPath(source, out var resolvedPath);

        // Assert
        result.Should().BeTrue();
        resolvedPath.Should().Be(mappedPath.GetFullPath());
    }
}
