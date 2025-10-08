using System.IO.MemoryMappedFiles;

namespace NexusMods.Paths.Tests.FileSystem;

public abstract class FileSystemBehaviorTestsBase
{
    public record Ctx(IFileSystem Fs, AbsolutePath Root, AbsolutePath FilePath, Action Cleanup);

    protected abstract Ctx CreateContextWithInitialFile(string relativePath = "a/file.txt", string content = "payload");

    [Fact]
    public async Task Read_ReturnsPayload()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            var text = await ctx.Fs.ReadAllTextAsync(ctx.FilePath);
            text.Should().Be("payload");
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public void Enumerate_ContainsFile()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            var files = ctx.Fs.EnumerateFiles(ctx.FilePath.Parent, "*", recursive: false).ToArray();
            files.Should().Contain(ctx.FilePath);
            ctx.Fs.DirectoryExists(ctx.FilePath.Parent).Should().BeTrue();
            ctx.Fs.FileExists(ctx.FilePath).Should().BeTrue();
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public void Delete_HidesFile_And_Enumeration()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            ctx.Fs.DeleteFile(ctx.FilePath);
            ctx.Fs.FileExists(ctx.FilePath).Should().BeFalse();
            ctx.Fs.EnumerateFiles(ctx.FilePath.Parent, recursive: false).Should().NotContain(ctx.FilePath);
            Action read = () => ctx.Fs.ReadFile(ctx.FilePath);
            read.Should().Throw<FileNotFoundException>();
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public void Recreate_ClearsDeletion_And_IsEmpty()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            ctx.Fs.DeleteFile(ctx.FilePath);
            ctx.Fs.FileExists(ctx.FilePath).Should().BeFalse();
            using (ctx.Fs.CreateFile(ctx.FilePath)) { }
            ctx.Fs.FileExists(ctx.FilePath).Should().BeTrue();
            ctx.Fs.ReadAllTextAsync(ctx.FilePath).Result.Should().BeEmpty();
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public void Write_OpenOrCreate_ReadWrite_ReplacesContent()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            ctx.Fs.WriteAllTextAsync(ctx.FilePath, "world").Wait();
            ctx.Fs.ReadAllTextAsync(ctx.FilePath).Result.Should().Be("world");
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public void RandomAccess_ReadPartial()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            Span<byte> bytes = stackalloc byte[4];
            var n = ctx.Fs.ReadBytesRandomAccess(ctx.FilePath, bytes, offset: 3);
            n.Should().BeGreaterThan(0);
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public async Task RandomAccess_ReadPartialAsync()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            var mem = new Memory<byte>(new byte[4]);
            var n = await ctx.Fs.ReadBytesRandomAccessAsync(ctx.FilePath, mem, offset: 2, CancellationToken.None);
            n.Should().BeGreaterThan(0);
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public unsafe void MemoryMapped_Read()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            var handle = ctx.Fs.CreateMemoryMappedFile(ctx.FilePath, FileMode.Open, MemoryMappedFileAccess.Read, 0);
            try
            {
                ((nuint)handle.Length).Should().BeGreaterThan(0);
                var span = new Span<byte>(handle.Pointer, (int)handle.Length);
                span.Length.Should().BeGreaterThan(0);
            }
            finally { handle.Dispose(); }
        }
        finally { ctx.Cleanup(); }
    }

    [Fact]
    public void MoveFile_Works()
    {
        var ctx = CreateContextWithInitialFile();
        try
        {
            var dest = ctx.FilePath.Parent / RelativePath.FromUnsanitizedInput("renamed.txt");
            ctx.Fs.MoveFile(ctx.FilePath, dest, overwrite: true);
            ctx.Fs.FileExists(dest).Should().BeTrue();
        }
        finally { ctx.Cleanup(); }
    }
}
