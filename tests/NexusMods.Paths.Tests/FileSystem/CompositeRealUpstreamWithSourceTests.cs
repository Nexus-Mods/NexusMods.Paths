using System.Text;
using NexusMods.Paths.FileProviders;

namespace NexusMods.Paths.Tests.FileSystem;

public sealed class CompositeRealUpstreamWithSourceTests : FileSystemBehaviorTestsBase
{
    protected override Ctx CreateContextWithInitialFile(string relativePath = "a/file.txt", string content = "payload")
    {
        var upstream = (NexusMods.Paths.FileSystem)NexusMods.Paths.FileSystem.Shared;
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var root = upstream.FromUnsanitizedFullPath(tempRoot);
        var mount = root / RelativePath.FromUnsanitizedInput("mnt");
        upstream.CreateDirectory(mount);
        var rel = RelativePath.FromUnsanitizedInput(relativePath);
        var src = new InMemoryReadOnlyFileSource(mount, new Dictionary<RelativePath, byte[]>
        {
            { rel, Encoding.UTF8.GetBytes(content) }
        });
        var fs = new ReadOnlySourcesFileSystem(upstream, new[] { src });
        var abs = (mount / rel).WithFileSystem(fs);
        return new Ctx(fs, mount, abs, () => { try { Directory.Delete(tempRoot, true); } catch { } });
    }
}
