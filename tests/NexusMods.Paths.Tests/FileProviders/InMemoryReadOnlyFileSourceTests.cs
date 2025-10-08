using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NexusMods.Paths;
using NexusMods.Paths.FileProviders;
using Xunit;

namespace NexusMods.Paths.Tests.FileProviders;

public class InMemoryReadOnlyFileSourceTests
{
    [Fact]
    public void GetFileData_RespectsStartAndLength()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);
        var mountPoint = fs.FromUnsanitizedFullPath("/mnt");
        var rel = RelativePath.FromUnsanitizedInput("folder/file.bin");
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var src = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, data }
        });

        using var srcStream = src.OpenRead(rel); srcStream.Seek(1, SeekOrigin.Begin);
        var buf = new byte[3];
        { var read = 0; while (read < 3) { var n = srcStream.Read(buf, read, 3 - read); if (n == 0) break; read += n; } read.Should().Be(3); }
        buf[0].Should().Be(2); buf[1].Should().Be(3); buf[2].Should().Be(4);
    }

    [Fact]
    public void GetFileData_HandlesOverflow()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);
        var mountPoint = fs.FromUnsanitizedFullPath("/mnt");
        var rel = RelativePath.FromUnsanitizedInput("file.bin");
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var src = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, data }
        });

        using var srcStream = src.OpenRead(rel);
        srcStream.Seek(3, SeekOrigin.Begin);
        var buf = new byte[10];
        int total = 0; while (total < 10) { var n = srcStream.Read(buf, total, 10 - total); if (n == 0) break; total += n; } total.Should().Be(2);
        buf[0].Should().Be(4);
        buf[1].Should().Be(5);
    }

    [Fact]
    public void OpenRead_ReturnsReadOnlyStream()
    {
        var fs = new InMemoryFileSystem(OSInformation.FakeUnix);
        var mountPoint = fs.FromUnsanitizedFullPath("/mnt");
        var rel = RelativePath.FromUnsanitizedInput("file.bin");
        var data = new byte[] { 10, 20, 30, 40, 50 };
        var src = new InMemoryReadOnlyFileSource(mountPoint, new Dictionary<RelativePath, byte[]>
        {
            { rel, data }
        });

        using var s = src.OpenRead(rel);
        s.CanWrite.Should().BeFalse();
        var buffer = new byte[3];
        s.Read(buffer, 0, 3).Should().Be(3);
        buffer.Should().BeEquivalentTo(new byte[]{10,20,30});
        Action write = () => s.Write(new byte[]{1,2,3}, 0, 3);
        write.Should().Throw<NotSupportedException>();
    }
}




