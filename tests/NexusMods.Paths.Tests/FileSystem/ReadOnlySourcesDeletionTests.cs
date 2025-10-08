using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NexusMods.Paths.FileProviders;
using Xunit;

namespace NexusMods.Paths.Tests.FileSystem;

public class ReadOnlySourcesDeletionTests
{
    public record Setup(IFileSystem Fs, AbsolutePath AbsPath, AbsolutePath Dir, string InitialContent, Action Cleanup);

    private static Setup SetupInMemory()
    {
        var fs = new NexusMods.Paths.InMemoryFileSystem(OSInformation.FakeUnix);
        var root = fs.FromUnsanitizedFullPath("/mnt");
        var abs = root / RelativePath.FromUnsanitizedInput("a/file.txt");
        fs.CreateDirectory(abs.Parent);
        fs.WriteAllText(abs, "payload");
        return new Setup(fs, abs, abs.Parent, "payload", () => { });
    }

    private static Setup SetupCompositeSourceOnly()
    {
        var upstream = new NexusMods.Paths.InMemoryFileSystem(OSInformation.FakeUnix);
        var mountPoint = upstream.FromUnsanitizedFullPath("/mnt");
        var rel = RelativePath.FromUnsanitizedInput("a/file.txt");
        var source = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, Encoding.UTF8.GetBytes("payload") }
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { source });
        var abs = (mountPoint / rel).WithFileSystem(fs);
        return new Setup(fs, abs, abs.Parent, "payload", () => { });
    }

    private static Setup SetupReal()
    {
        var fs = (NexusMods.Paths.FileSystem)NexusMods.Paths.FileSystem.Shared;
        var tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var root = fs.FromUnsanitizedFullPath(tempRoot);
        var abs = root / RelativePath.FromUnsanitizedInput("a/file.txt");
        fs.CreateDirectory(abs.Parent);
        fs.WriteAllText(abs, "payload");
        return new Setup(fs, abs, abs.Parent, "payload", () => { try { Directory.Delete(tempRoot, true); } catch { } });
    }

    private static Setup SetupCompositeSourceOnlyReal()
    {
        var fsUp = (NexusMods.Paths.FileSystem)NexusMods.Paths.FileSystem.Shared;
        var tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var root = fsUp.FromUnsanitizedFullPath(tempRoot);
        var mountPoint = root / RelativePath.FromUnsanitizedInput("mnt");
        fsUp.CreateDirectory(mountPoint);
        var rel = RelativePath.FromUnsanitizedInput("a/file.txt");
        var source = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, Encoding.UTF8.GetBytes("payload") }
        });
        var fs = new ReadOnlySourcesFileSystem(fsUp, new[] { source });
        var abs = (mountPoint / rel).WithFileSystem(fs);
        return new Setup(fs, abs, abs.Parent, "payload", () => { try { Directory.Delete(tempRoot, true); } catch { } });
    }

    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { SetupInMemory() };
        yield return new object[] { SetupCompositeSourceOnly() };
        yield return new object[] { SetupReal() };
        yield return new object[] { SetupCompositeSourceOnlyReal() };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Delete_HidesFile_And_Enumeration(Setup setup)
    {
        try
        {
            var fs = setup.Fs;
            var abs = setup.AbsPath;
            var dir = setup.Dir;

            fs.FileExists(abs).Should().BeTrue();
            fs.EnumerateFiles(dir, recursive: false).Should().Contain(abs);

            fs.DeleteFile(abs);

            fs.FileExists(abs).Should().BeFalse();
            fs.EnumerateFiles(dir, recursive: false).Should().NotContain(abs);
            Action read = () => fs.ReadFile(abs);
            read.Should().Throw<FileNotFoundException>();
        }
        finally { setup.Cleanup(); }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Recreate_ClearsDeletion_And_IsEmpty(Setup setup)
    {
        try
        {
            var fs = setup.Fs;
            var abs = setup.AbsPath;

            fs.DeleteFile(abs);
            fs.FileExists(abs).Should().BeFalse();

            using (fs.CreateFile(abs)) { }

            fs.FileExists(abs).Should().BeTrue();
            fs.ReadAllTextAsync(abs).Result.Should().BeEmpty();
        }
        finally { setup.Cleanup(); }
    }

    [Fact]
    public void Composite_CopyOnWrite_WhenNotDeleted()
    {
        var s = SetupCompositeSourceOnly();
        try
        {
            var fs = s.Fs;
            var abs = s.AbsPath;

            using (var stream = fs.OpenFile(abs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                var buffer = new byte[7];
                stream.Read(buffer, 0, buffer.Length).Should().Be(7);
                Encoding.UTF8.GetString(buffer).Should().Be("payload");

                stream.Seek(0, SeekOrigin.Begin);
                var newBytes = Encoding.UTF8.GetBytes("abc");
                stream.Write(newBytes, 0, newBytes.Length);
            }

            fs.ReadAllTextAsync(abs).Result.Should().StartWith("abc");
        }
        finally { s.Cleanup(); }
    }
}

