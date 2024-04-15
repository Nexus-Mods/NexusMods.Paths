using System.IO.MemoryMappedFiles;
using System.Reflection;
using NexusMods.Paths.TestingHelpers;

namespace NexusMods.Paths.Tests.FileSystem;

public class FileSystemTests
{
    [Fact]
    public void Test_EnumerateFiles()
    {
        var fs = new Paths.FileSystem();

        var file = fs.FromUnsanitizedFullPath(Assembly.GetExecutingAssembly().Location);
        var directory = fs.FromUnsanitizedFullPath(AppContext.BaseDirectory);
        fs.EnumerateFiles(directory, recursive: false)
            .Should()
            .Contain(file);
    }

    [Fact]
    public void Test_EnumerateDirectories()
    {
        var fs = new Paths.FileSystem();

        var directory = fs.FromUnsanitizedFullPath(AppContext.BaseDirectory);
        var parentDirectory = directory.Parent;

        fs.EnumerateDirectories(parentDirectory, recursive: false)
            .Should()
            .Contain(directory);
    }

    [Fact]
    public void Test_EnumerateFileEntries()
    {
        var fs = new Paths.FileSystem();

        var file = fs.FromUnsanitizedFullPath(Assembly.GetExecutingAssembly().Location);
        var directory = fs.FromUnsanitizedFullPath(AppContext.BaseDirectory);

        fs.EnumerateFileEntries(directory, recursive: false)
            .Should()
            .Contain(x => x.Path == file);
    }

    [Theory, AutoFileSystem]
    public async Task Test_CreateMemoryMappedFile_CanOpen(RelativePath relativePath, byte[] contents)
    {
        var fs = new Paths.FileSystem();
        var file = fs.GetKnownPath(KnownPath.TempDirectory).Combine(relativePath);
        await using (var stream = fs.CreateFile(file))
        {
            stream.Write(contents);
        }

        unsafe
        {
            using var mmf = fs.CreateMemoryMappedFile(file, FileMode.Open, MemoryMappedFileAccess.ReadWrite);
            mmf.Should().NotBeNull();
            ((nuint)mmf.Pointer).Should().NotBe(0);
            mmf.Length.Should().Be((nuint)file.FileInfo.Size);

            // Assert that the contents are equal
            mmf.AsSpan().SequenceEqual(contents).Should().BeTrue();
        }
    }
}
