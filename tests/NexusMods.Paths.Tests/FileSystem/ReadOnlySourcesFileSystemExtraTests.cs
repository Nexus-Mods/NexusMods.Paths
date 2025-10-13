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

    [Fact]
    public async Task Concurrent_CopyOnWrite_And_Reads_AreThreadSafe()
    {
        var upstream = new InMemoryFileSystem(OSInformation.FakeUnix);
        var mount = upstream.FromUnsanitizedFullPath("/mnt");
        var rel = RelativePath.FromUnsanitizedInput("a/file.txt");
        var src = new InMemoryReadOnlyFileSource(mount, new Dictionary<RelativePath, byte[]>
        {
            { rel, Encoding.UTF8.GetBytes("payload") },
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { src });
        var abs = (mount / rel).WithFileSystem(fs);

        var dir = abs.Parent;
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        var exceptions = new ConcurrentQueue<Exception>();

        Task Reader() => Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    _ = await fs.ReadAllBytesAsync(abs, cts.Token);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    // expected on cancellation
                }
                catch (Exception ex) { exceptions.Enqueue(ex); }
            }
        });

        // Perform a single write to trigger copy-on-write once
        Task WriterOnce() => Task.Run(async () =>
        {
            try { await fs.WriteAllTextAsync(abs, "updated", cts.Token); }
            catch (Exception ex) { exceptions.Enqueue(ex); }
        });

        Task Enumerator() => Task.Run(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    _ = fs.EnumerateFiles(dir, pattern: "*", recursive: false).ToArray();
                    _ = fs.EnumerateFileEntries(dir, pattern: "*", recursive: false).ToArray();
                }
                catch (Exception ex) { exceptions.Enqueue(ex); }
            }
        });

        var readers = Enumerable.Range(0, 4).Select(_ => Reader()).ToArray();
        var writer = WriterOnce();
        var enumerator = Enumerator();
        await writer;
        // allow readers/enumerator to run post-write a bit
        await Task.Delay(100);
        await cts.CancelAsync();
        await Task.WhenAll(readers.Append(enumerator));

        exceptions.Should().BeEmpty();
        fs.FileExists(abs).Should().BeTrue();
        var text = await fs.ReadAllTextAsync(abs);
        text.Should().Be("updated");
    }
}
