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
    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { SetupWithSingleFile_InMemory() };
        yield return new object[] { SetupWithSingleFile_Real() };
    }

    public record Setup(NexusMods.Paths.InMemoryFileSystem? upstreamMem, NexusMods.Paths.FileSystem? upstreamReal, IReadOnlyFileSource source, ReadOnlySourcesFileSystem fs, AbsolutePath abs, RelativePath rel, Action Cleanup);

    private static Setup SetupWithSingleFile_InMemory(string mount = "/mnt", string relPath = "a/b.txt", string content = "hello")
    {
        var upstream = new NexusMods.Paths.InMemoryFileSystem(OSInformation.FakeUnix);
        var mountPoint = upstream.FromUnsanitizedFullPath(mount);
        var rel = RelativePath.FromUnsanitizedInput(relPath);
        var fileBytes = Encoding.UTF8.GetBytes(content);
        var source = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, fileBytes }
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { source });
        var abs = (mountPoint / rel).WithFileSystem(fs);
        return new Setup(upstream, null, source, fs, abs, rel, () => { });
    }

    private static Setup SetupWithSingleFile_Real(string relPath = "a/b.txt", string content = "hello")
    {
        var upstream = (NexusMods.Paths.FileSystem)NexusMods.Paths.FileSystem.Shared;
        var tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var root = upstream.FromUnsanitizedFullPath(tempRoot);
        var mountPoint = root / RelativePath.FromUnsanitizedInput("mnt");
        upstream.CreateDirectory(mountPoint);
        var rel = RelativePath.FromUnsanitizedInput(relPath);
        var fileBytes = Encoding.UTF8.GetBytes(content);
        var source = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, fileBytes }
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { source });
        var abs = (mountPoint / rel).WithFileSystem(fs);
        return new Setup(null!, upstream, source, fs, abs, rel, () => { try { Directory.Delete(tempRoot, true); } catch { } });
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Read_FallsBackToReadOnlySource(Setup s)
    {
        try
        {
            var text = await s.fs.ReadAllTextAsync(s.abs);
            text.Should().Be("hello");
        }
        finally { s.Cleanup(); }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Enumerations_IncludeReadOnlySourceFilesAndDirs(Setup s)
    {
        try
        {
            var dir = s.abs.Parent.Parent; // /mnt/a
            var files = s.fs.EnumerateFiles(dir, "*", true).ToArray();
            files.Should().Contain(s.abs);
            s.fs.DirectoryExists(dir).Should().BeTrue();
            s.fs.FileExists(s.abs).Should().BeTrue();
        }
        finally { s.Cleanup(); }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Delete_ReadOnlySourceFile_MarkedDeleted(Setup s)
    {
        try
        {
            s.fs.DeleteFile(s.abs);
            s.fs.FileExists(s.abs).Should().BeFalse();
        }
        finally { s.Cleanup(); }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Write_TriggersCopyOnWrite_ThenUsesUpstream(Setup s)
    {
        try
        {
            using (var stream = s.fs.OpenFile(s.abs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                var buf = new byte[5];
                stream.Read(buf, 0, buf.Length).Should().Be(5);
                Encoding.UTF8.GetString(buf).Should().Be("hello");

                stream.Seek(0, SeekOrigin.Begin);
                var newBytes = Encoding.UTF8.GetBytes("world");
                stream.Write(newBytes, 0, newBytes.Length);
            }

            if (s.upstreamMem is not null)
            {
                s.upstreamMem.FileExists(s.abs.WithFileSystem(s.upstreamMem)).Should().BeTrue();
            }
            if (s.upstreamReal is not null)
            {
                s.upstreamReal.FileExists(s.abs.WithFileSystem(s.upstreamReal)).Should().BeTrue();
            }
            s.fs.ReadAllTextAsync(s.abs).Result.Should().Be("world");
        }
        finally { s.Cleanup(); }
    }
}



