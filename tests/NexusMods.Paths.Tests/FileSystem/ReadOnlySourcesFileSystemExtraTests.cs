using System.Text;
using System.Collections.Concurrent;
using NexusMods.Paths.FileProviders;

namespace NexusMods.Paths.Tests.FileSystem;

public sealed class ReadOnlySourcesFileSystemExtraTests
{
    [Fact]
    public async Task ReadOnlyEntries_UseFsCreation_ForDefault_And_PerFileUpdatesOnWrite()
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

        // Modify a different file (triggers copy-on-write and per-file timestamp update)
        await fs.WriteAllTextAsync(abs1, "changed");

        var entry2After = fs.GetFileEntry(abs2);
        var t1 = entry2After.LastWriteTime;
        var c1 = entry2After.CreationTime;

        // File2 should remain unchanged
        t1.Should().Be(t0);
        c1.Should().Be(c0);

        // File1 should reflect an updated LastWriteTime
        var entry1After = fs.GetFileEntry(abs1);
        entry1After.LastWriteTime.Should().BeAfter(t0);
    }
}
