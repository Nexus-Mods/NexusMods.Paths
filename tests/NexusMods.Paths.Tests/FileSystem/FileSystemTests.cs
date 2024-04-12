using System.IO.MemoryMappedFiles;
using System.Reflection;
using FluentAssertions;

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

    [Fact]
    public async Task Test_CreateMemoryMappedFile_CanOpen()
    {
        var fs = new Paths.FileSystem();
        var file = fs.FromUnsanitizedFullPath(Assembly.GetExecutingAssembly().Location);
        var expectedData = await file.ReadAllBytesAsync();

        unsafe
        {
            using var mmf = fs.CreateMemoryMappedFile(file, FileMode.Open, MemoryMappedFileAccess.ReadWrite);
            mmf.Should().NotBeNull();
            ((nuint)mmf.Pointer).Should().NotBe(0);
            mmf.Length.Should().Be((nuint)file.FileInfo.Size);

            // Assert that the contents are equal
            mmf.AsSpan().SequenceEqual(expectedData).Should().BeTrue();
        }
    }
}
