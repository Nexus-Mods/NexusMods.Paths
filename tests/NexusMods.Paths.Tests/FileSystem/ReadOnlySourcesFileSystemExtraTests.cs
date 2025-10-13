using System.Text;
using System.Collections.Concurrent;
using NexusMods.Paths.FileProviders;

namespace NexusMods.Paths.Tests.FileSystem;

public sealed class ReadOnlySourcesFileSystemExtraTests
{
    [Fact]
    public async Task ReadOnlyEntries_UseFsTimestamp_And_TimestampUpdatesOnWrite()
    {
        var upstream = new InMemoryFileSystem(OSInformation.FakeUnix);
        var mount = upstream.FromUnsanitizedFullPath("/mnt");

        var rel1 = RelativePath.FromUnsanitizedInput("a/file1.txt");
        var rel2 = RelativePath.FromUnsanitizedInput("a/file2.txt");

        var src = new InMemoryReadOnlyFileSource(mount, new Dictionary<RelativePath, byte[]>
        {
            { rel1, Encoding.UTF8.GetBytes("one") },
            { rel2, Encoding.UTF8.GetBytes("two") },
        });

        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { src });
        var abs1 = (mount / rel1).WithFileSystem(fs);
        var abs2 = (mount / rel2).WithFileSystem(fs);

        var entry2Before = fs.GetFileEntry(abs2);
        var t0 = entry2Before.LastWriteTime;
        var c0 = entry2Before.CreationTime;

        // Ensure time can advance
        Thread.Sleep(20);

        // Modify a different file (triggers copy-on-write and FS timestamp bump)
        await fs.WriteAllTextAsync(abs1, "changed");

        var entry2After = fs.GetFileEntry(abs2);
        var t1 = entry2After.LastWriteTime;
        var c1 = entry2After.CreationTime;

        t1.Should().BeAfter(t0);
        c1.Should().BeAfter(c0);
    }
}
