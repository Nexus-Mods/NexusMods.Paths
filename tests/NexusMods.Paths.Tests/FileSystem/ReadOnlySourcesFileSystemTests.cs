using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NexusMods.Paths.FileProviders;
using Xunit;

namespace NexusMods.Paths.Tests.FileSystem;

public class ReadOnlySourcesFileSystemTests
{
    private static (InMemoryFileSystem upstream, IReadOnlyFileSource source, ReadOnlySourcesFileSystem fs, AbsolutePath abs, RelativePath rel)
        SetupWithSingleFile(string mount = "/mnt", string relPath = "a/b.txt", string content = "hello")
    {
        var upstream = new InMemoryFileSystem(OSInformation.FakeUnix);
        var mountPoint = upstream.FromUnsanitizedFullPath(mount);
        var rel = RelativePath.FromUnsanitizedInput(relPath);
        var fileBytes = Encoding.UTF8.GetBytes(content);
        var source = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, fileBytes }
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { source });
        var abs = mountPoint / rel;
        abs = abs.WithFileSystem(fs);
        return (upstream, source, fs, abs, rel);
    }

    [Fact]
    public async Task Read_FallsBackToReadOnlySource()
    {
        var (_, _, fs, abs, _) = SetupWithSingleFile();
        var text = await fs.ReadAllTextAsync(abs);
        text.Should().Be("hello");
    }

    [Fact]
    public void Enumerations_IncludeReadOnlySourceFilesAndDirs()
    {
        var (_, _, fs, abs, _) = SetupWithSingleFile();
        var dir = abs.Parent.Parent; // /mnt/a
        var files = fs.EnumerateFiles(dir, "*", true).ToArray();
        files.Should().Contain(abs);
        fs.DirectoryExists(dir).Should().BeTrue();
        fs.FileExists(abs).Should().BeTrue();
    }

    [Fact]
    public void Delete_ReadOnlySourceFile_Throws()
    {
        var (_, _, fs, abs, _) = SetupWithSingleFile();
        Action act = () => fs.DeleteFile(abs);
        act.Should().Throw<IOException>();
    }

    [Fact]
    public void Write_TriggersCopyOnWrite_ThenUsesUpstream()
    {
        var (upstream, _, fs, abs, _) = SetupWithSingleFile();
        using (var stream = fs.OpenFile(abs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
        {
            var buf = new byte[5];
            stream.Read(buf, 0, buf.Length).Should().Be(5);
            Encoding.UTF8.GetString(buf).Should().Be("hello");

            stream.Seek(0, SeekOrigin.Begin);
            var newBytes = Encoding.UTF8.GetBytes("world");
            stream.Write(newBytes, 0, newBytes.Length);
        }

        upstream.FileExists(abs.WithFileSystem(upstream)).Should().BeTrue();
        fs.ReadAllTextAsync(abs).Result.Should().Be("world");
    }
}
