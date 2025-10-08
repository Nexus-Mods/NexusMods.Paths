using System.Text;
using NexusMods.Paths.FileProviders;

namespace NexusMods.Paths.Tests.FileSystem;

public sealed class CompositeInMemoryUpstreamWithSourceTests : FileSystemBehaviorTestsBase
{
    protected override Ctx CreateContextWithInitialFile(string relativePath = "a/file.txt", string content = "payload")
    {
        var upstream = new NexusMods.Paths.InMemoryFileSystem(OSInformation.FakeUnix);
        var mount = upstream.FromUnsanitizedFullPath("/mnt");
        var rel = RelativePath.FromUnsanitizedInput(relativePath);
        var src = new InMemoryReadOnlyFileSource(mount, new Dictionary<RelativePath, byte[]>
        {
            { rel, Encoding.UTF8.GetBytes(content) }
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { src });
        var abs = (mount / rel).WithFileSystem(fs);
        return new Ctx(fs, mount, abs, () => { });
    }
}