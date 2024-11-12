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
    public void Test_DeleteDirectoryShouldDeleteEmpty()
    {
        var fs = new Paths.FileSystem();

        var directory = fs.FromUnsanitizedFullPath(AppContext.BaseDirectory) / "TestDirectory";
        fs.CreateDirectory(directory);

        fs.DeleteDirectory(directory, recursive: false);

        fs.DirectoryExists(directory).Should().BeFalse();
    }

    [Fact]
    public void Test_DeleteDirectoryShouldNotDeleteNonEmpty()
    {
        var fs = new Paths.FileSystem();

        var directory = fs.FromUnsanitizedFullPath(AppContext.BaseDirectory) / ("TestDirectory");
        var child = directory / "Child";

        fs.CreateDirectory(directory);
        fs.CreateDirectory(child);

        var action = () => fs.DeleteDirectory(directory, recursive: false);
        action.Should().Throw<IOException>();

        // Cleanup to avoid breaking the other tests
        fs.DeleteDirectory(child, recursive: false);

        fs.DirectoryExists(child).Should().BeFalse();

        fs.DeleteDirectory(directory, recursive: false);

        fs.DirectoryExists(directory).Should().BeFalse();
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
        var file = fs.GetKnownPath(KnownPath.TempDirectory) / relativePath;
        await using (var stream = fs.CreateFile(file))
        {
            stream.Write(contents);
        }

        unsafe
        {
            using var mmf = fs.CreateMemoryMappedFile(file, FileMode.Open, MemoryMappedFileAccess.ReadWrite, 0);
            mmf.Should().NotBeNull();
            ((nuint)mmf.Pointer).Should().NotBe(0);
            mmf.Length.Should().Be((nuint)file.FileInfo.Size);

            // Assert that the contents are equal
            mmf.AsSpan().SequenceEqual(contents).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Test_CreateMemoryMappedFile_CanCreateAndWrite()
    {
        var fs = new Paths.FileSystem();
        var tempFile = fs.GetKnownPath(KnownPath.TempDirectory) / Path.GetRandomFileName();
        var contents = new byte[] { 1, 2, 3, 4, 5 };

        // Create a new MemoryMappedFile
        try
        {
            using (var mmf = fs.CreateMemoryMappedFile(tempFile, FileMode.CreateNew, MemoryMappedFileAccess.ReadWrite,
                       (ulong)contents.Length))
            {
                unsafe
                {
                    mmf.Should().NotBeNull();
                    ((nuint)mmf.Pointer).Should().NotBe(0);
                    mmf.Length.Should().Be((nuint)contents.Length);

                    // Write data to the MemoryMappedFile
                    contents.CopyTo(new Span<byte>(mmf.Pointer, contents.Length));
                }
            }
            
            /*
                Note(sewer)
                
                There's something weird going on in the Memory Mapped File abstraction,
                we open a FileStream with `FileShare.Read`, to create the MemoryMappedFile from under the hood,
                and we propagate this when opening the MemoryMappedFile.
                
                Below in `ReadAllBytesAsync`, we are opening a second FileStream with `FileAccess.Read` and `FileShare.Read`.
                It should technically work, but it does not. Chances are it's some not properly documented part
                of .NET's MemoryMappedFile abstraction for Win32.
            */

            // Verify the data was written correctly
            var writtenData = await fs.ReadAllBytesAsync(tempFile);
            writtenData.Should().BeEquivalentTo(contents);
        }
        finally
        {
            // Clean up
            if (fs.FileExists(tempFile))
                fs.DeleteFile(tempFile);
        }
    }

    [Fact]
    public void Test_ReadBytesRandom()
    {
        var fs = new Paths.FileSystem();
        var tempFile = fs.GetKnownPath(KnownPath.TempDirectory) / Path.GetRandomFileName();
        var contents = new byte[] { 1, 2, 3, 4, 5 };
        using (var stream = fs.CreateFile(tempFile))
        {
            stream.Write(contents);
        }

        var bytes = new byte[contents.Length];
        fs.ReadBytesRandomAccess(tempFile, bytes, 0);
        bytes.Should().BeEquivalentTo(contents);
    }

    [Fact]
    public void Test_ReadBytesRandomWithOffset()
    {
        var fs = new Paths.FileSystem();
        var tempFile = fs.GetKnownPath(KnownPath.TempDirectory) / Path.GetRandomFileName();
        var contents = new byte[] { 1, 2, 3, 4, 5 };
        using (var stream = fs.CreateFile(tempFile))
        {
            stream.Write(contents);
        }
        var offset = new Random().Next(1, contents.Length - 1);

        var bytes = new byte[contents.Length - offset];
        fs.ReadBytesRandomAccess(tempFile, bytes, offset);
        bytes.Should().BeEquivalentTo(contents.AsSpan(offset).ToArray());
    }

    [Fact]
    public async Task Test_ReadBytesRandomAsync()
    {
        var fs = new Paths.FileSystem();
        var tempFile = fs.GetKnownPath(KnownPath.TempDirectory) / Path.GetRandomFileName();
        var contents = new byte[] { 1, 2, 3, 4, 5 };
        await using (var stream = fs.CreateFile(tempFile))
        {
            await stream.WriteAsync(contents);
        }
        
        var bytes = new byte[contents.Length];
        await fs.ReadBytesRandomAccessAsync(tempFile, bytes, 0);
        bytes.Should().BeEquivalentTo(contents);
    }

    [Fact]
    public async Task Test_ReadBytesRandomAsyncWithOffset()
    {
        var fs = new Paths.FileSystem();
        var tempFile = fs.GetKnownPath(KnownPath.TempDirectory) / Path.GetRandomFileName();
        var contents = new byte[] { 1, 2, 3, 4, 5 };
        await using (var stream = fs.CreateFile(tempFile))
        {
            await stream.WriteAsync(contents);
        }
        var offset = new Random().Next(1, contents.Length - 1);

        var bytes = new byte[contents.Length - offset];
        await fs.ReadBytesRandomAccessAsync(tempFile, bytes, offset);
        bytes.Should().BeEquivalentTo(contents.AsSpan(offset).ToArray());
    }
}
