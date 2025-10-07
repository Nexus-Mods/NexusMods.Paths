using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
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

        using var chunk = src.GetFileData(rel, 1, 3);
        chunk.DataLength.Should().Be(3ul);
        var span = chunk.Data.Span;
        span[0].Should().Be(2);
        span[1].Should().Be(3);
        span[2].Should().Be(4);
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

        using var chunk = src.GetFileData(rel, 3, 10);
        chunk.DataLength.Should().Be(2ul);
        chunk.Data.Span[0].Should().Be(4);
        chunk.Data.Span[1].Should().Be(5);
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
